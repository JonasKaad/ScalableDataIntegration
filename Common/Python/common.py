import aiohttp
import asyncio
import datetime
import json
import os
import sys
from datadog import initialize, api
import logging
initialize()
logging.basicConfig(stream=sys.stdout, level=logging.INFO)

import grpc
from azure.identity.aio import DefaultAzureCredential
from azure.storage.blob.aio import BlobServiceClient

GENCLIENT_PYTHON_PATH = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../GeneratedClients/python"))


print(f"Adding to sys.path: {GENCLIENT_PYTHON_PATH}")  # Debugging
sys.path.insert(0, GENCLIENT_PYTHON_PATH)

# Check if the file exists
print(f"Files in {GENCLIENT_PYTHON_PATH}: {os.listdir(GENCLIENT_PYTHON_PATH)}")

# noinspection PyUnresolvedReferences
import filter_pb2 # works even though IDE shows error (suppressing the warning)
# noinspection PyUnresolvedReferences
import filter_pb2_grpc # works even though IDE shows error (suppressing the warning)
# noinspection PyUnresolvedReferences
import parser_pb2
# noinspection PyUnresolvedReferences
import parser_pb2_grpc

### REGISTRY
async def send_heartbeat(type="Parser"):
    baseurl = os.getenv("BASE_URL")
    parser_name = os.getenv("PARSER_NAME")
    url = f"{baseurl}/{type}/{parser_name}/heartbeat"
    async with aiohttp.ClientSession() as session:
        async with session.post(url) as response:
            return response.status == 200

async def heartbeat_scheduler(type="Parser", filter_params=None):
    alive = True
    while True:
        await asyncio.sleep(1 * 60)
        if alive:
            try:
                alive = await send_heartbeat(type)
            except Exception as e:

                dd_warning("Failed sending heartbeat", str(e))
        else:
            try:
                alive = await register(type, filter_params)
            except Exception as e:
                dd_warning("Failed to re-register", str(e))

async def register(type="Parser", filter_params=None):
    async with aiohttp.ClientSession() as session:
        baseurl = os.getenv("BASE_URL")
        name = os.getenv("PARSER_NAME")
        url = f"{baseurl}/{type}/{name}/register"
        if type == "Filter":
            data = {
                "name": name,
                "parameters": filter_params,
                "url": os.getenv("PARSER_URL")
            }
        else:
            data = {
                "url": os.getenv("PARSER_URL"),
            }
        print(f"Registering at {url} with data: {json.dumps(data)}")
        try:
            headers = {'Content-Type': 'application/json'}
            async with session.post(url, json=data, headers=headers) as response:
                print(f"Response status: {response.status}")
                if response.status != 200:
                    dd_error("Failed to register", f"Tried to register at {url} with data: {json.dumps(data)}")
                return response.status == 200
        except aiohttp.ClientConnectorError:
            print("failed to connect")
            return False

async def init_parser(injected_loop):
    registered = False
    while not registered:
        registered = await register("Parser")
        if registered:
            injected_loop.create_task(heartbeat_scheduler("Parser"))
            return
        else:
            print("Failed to register, retrying in 10 seconds...")
            await asyncio.sleep(10)

async def init_filter(injected_loop, parameters):
    registered = False
    while not registered:
        registered = await register("Filter", parameters)
        if registered:
            injected_loop.create_task(heartbeat_scheduler("Filter", parameters))
            return
        else:
            print("Failed to register, retrying in 10 seconds...")
            await asyncio.sleep(10)

### DATA

def get_data(raw_data, format_type):
    data_list = raw_data.split(b'magic')
    relevant = []
    raw = []
    for data in data_list:
        if data:
            if format_type == "str":
                try:
                    relevant.append(data.decode('utf-8').split(";"))
                except UnicodeDecodeError as e:
                    raw.append(data)
            elif format_type == "img":
                ##TODO: Do some more checks to see if data is actually image
                relevant.append(data)
            else:
                raw.append(data)
    flattened_relevant = [item for sublist in relevant for item in sublist]
    if flattened_relevant:
        return flattened_relevant, raw
    else:
        return [], raw

def get_next_url(urls):
    if len(urls) == 0:
        return None
    url = urls.pop(0)
    if url:
        if url.startswith("http://"):
            url = url[7:]
        elif url.startswith("https://"):
            url = url[8:]
    return url, ";".join(urls)

def read_params(params_list, relevant_string=""):
    relevant = {}
    try:
        for parameter in params_list:
            json_acceptable_string = parameter.replace("'", "\"")
            if relevant_string in json_acceptable_string:
                relevant = json.loads(json_acceptable_string)
                params_list.remove(parameter)
    except Exception as e:
        dd_error("Failed to read parameters", str(e))
    return relevant, ";".join(params_list)

### GRPC

async def send_to_next_url(next_url, raw_data, parameters, urls):
    try:
        # Create channel with timeout options
        options = [
            ('grpc.enable_retries', 1),
            ('grpc.keepalive_time_ms', 10000),
            ('grpc.keepalive_timeout_ms', 5000),
            ('grpc.dns_resolution_timeout_ms', 5000)
        ]
        credentials = grpc.ssl_channel_credentials()
        async with grpc.aio.secure_channel(next_url, options=options, credentials=credentials) as channel:
            if len(parameters) > 0:
                response = await send_to_filter(channel, raw_data, parameters, urls)
            else:
                response = await send_to_parser(channel, raw_data)

            if response.success:
                success = True
                msg = "Filter succeeded sending data to next url"
            else:
                success = False
                msg = "Filter failed sending data to next url"
                dd_error("Filter succeeded sending data to next url, but received error", str(response.err_msg))
    except grpc.aio.AioRpcError as e:
        print(f"gRPC connection error: {e.code()} - {e.details()}")
        success = False
        msg = f"gRPC connection error: {e.details()}"
        dd_error("Failed to send data to next URL", f"gRPC Error: {e.code()} - {e.details()}")
    except Exception as e:
        print(f"Unexpected error during gRPC call: {e}")
        success = False
        msg = f"Unexpected error: {e}"
        dd_error("Failed to send data to next URL", f"Unexpected Error: {str(e)}")

    return success, msg

async def send_to_filter(channel, raw_data, parameters, urls):
    stub = filter_pb2_grpc.FilterStub(channel)
    return await stub.FilterCall(
        filter_pb2.FilterRequest(raw_data=raw_data, parameters=parameters, next_urls=urls, format="str"))

async def send_to_parser(channel, raw_data):
    stub = parser_pb2_grpc.ParserStub(channel)
    return await stub.ParseCall(parser_pb2.ParseRequest(raw_data=raw_data))

### AZURE
async def save_data_to_azure(raw_file, parsed_file, name):
    client = azure_cred_checker()
    container_name = os.getenv("PARSER_NAME", name)
    container_client = client.get_container_client(container_name)
    if not await container_client.exists():
        await container_client.create_container()

    now = datetime.datetime.now(datetime.timezone.utc)
    parsed_file_name = f"{now.year}/{now.strftime('%m')}/{now.strftime('%d')}/{now.strftime('%H%M')}-parsed.txt"

    await container_client.upload_blob(name=parsed_file_name, data=parsed_file)

    await client.close()

def azure_cred_checker():
    credential = DefaultAzureCredential()
    account_url = "https://parserstorage.blob.core.windows.net/"
    blob_client = BlobServiceClient(account_url, credential)
    return blob_client


## Datadog

def dd_error(title: str, text: str):
    title = f"{os.getenv('PARSER_NAME')} - {title}"
    tags = {"service": os.getenv("PARSER_NAME")}
    api.Event.create(title=title, text=text, tags=tags, alert_type="error", priority="normal")
    logging.error(text)

def dd_warning(title: str, text: str):
    title = f"{os.getenv('PARSER_NAME')} - {title}"
    tags = {"service": os.getenv("PARSER_NAME")}
    api.Event.create(title=title, text=text, tags=tags, alert_type="warning", priority="normal")
    logging.warning(text)

def dd_info(title: str, text: str):
    title = f"{os.getenv('PARSER_NAME')} - {title}"
    tags = {"service": os.getenv("PARSER_NAME")}
    api.Event.create(title=title, text=text, tags=tags, alert_type="info", priority="normal")
    logging.info(text)