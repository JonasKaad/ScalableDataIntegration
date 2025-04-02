import aiohttp
import asyncio
import datetime
import json
import grpc
import sys
import os

from aiohttp import ClientConnectorError
from azure.identity.aio import DefaultAzureCredential
from azure.storage.blob.aio import BlobServiceClient
from metar_taf_parser.parser.parser import TAFParser
from metar_taf_parser.model.model import TAF

GENCLIENT_PYTHON_PATH = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../GeneratedClients/python"))


print(f"Adding to sys.path: {GENCLIENT_PYTHON_PATH}")  # Debugging
sys.path.insert(0, GENCLIENT_PYTHON_PATH)

# Check if the file exists
print(f"Files in {GENCLIENT_PYTHON_PATH}: {os.listdir(GENCLIENT_PYTHON_PATH)}")

# noinspection PyUnresolvedReferences
import parser_pb2 # works even though IDE shows error (suppressing the warning)
# noinspection PyUnresolvedReferences
import parser_pb2_grpc # works even though IDE shows error (suppressing the warning)

class TAFEncoder(json.JSONEncoder):
    def default(self, o):
        if isinstance(o, TAF):
            clouds = '['
            for cloud in o.clouds:
                clouds += '{'
                clouds += '"height":' + str(cloud.height) + ','
                clouds += '"quantity":"' + cloud.quantity.value + '",'
                clouds += '"type":"' + str(cloud.type) + '"'
                clouds += '},'
            clouds += ']'
            return {
                    "station": o.station,
                    "validity": {
                        "start_day": o.validity.start_day,
                        "start_hour": o.validity.start_hour,
                        "end_day": o.validity.end_day,
                        "end_hour": o.validity.end_hour
                    },
                    "visibility": {
                        "distance": o.visibility.distance,
                    },
                    "clouds": clouds,
                    "wind": {
                        "speed": o.wind.speed,
                        "direction": o.wind.direction,
                        "gust": o.wind.gust,
                        "degrees": o.wind.degrees,
                        "unit": o.wind.unit
                    }
                    }
        return json.JSONEncoder.default(self, o)

class ParserServicer(parser_pb2_grpc.ParserServicer):
    async def ParseCall(self, request, context):
        raw_data = request.raw_data
        format_type = request.format
        if(format_type == "str"):
            (strings, raw_data) = get_data(raw_data, format_type="str")

        tafstrings = []
        for string in strings:
            string = string.split("\n")
            for d in string:
                if d == "" or d == "magic":
                    continue
                tafstrings.append("TAF " + d)

        tafs = []
        for taf in tafstrings:
            tafs.append(TAFParser().parse(taf))

        if len(tafs) != len(tafstrings):
            response = parser_pb2.ParseResponse(success=False, err_msg="Error parsing all TAFs")
        else:
            response = parser_pb2.ParseResponse(success=True, err_msg=f"Parsed {len(tafstrings)} TAFs")

        client = self.azure_cred_checker();
        container_name = os.getenv("PARSER_NAME", "python-taf")
        container_client = client.get_container_client(container_name)
        if not await container_client.exists():
            await container_client.create_container()

        now = datetime.datetime.now(datetime.timezone.utc)
        raw_file_name = f"{now.year}/{now.strftime('%m')}/{now.day}/{now.strftime('%H%M')}-raw.txt"
        parsed_file_name = f"{now.year}/{now.strftime('%m')}/{now.day}/{now.strftime('%H%M')}-parsed.txt"

        taf_json = json.dumps(tafs, cls=TAFEncoder)
        await container_client.upload_blob(name=raw_file_name, data=raw_data.decode('utf-8'))
        await container_client.upload_blob(name=parsed_file_name, data=taf_json)

        await client.close()
        return response

    def azure_cred_checker(self):
        credential = DefaultAzureCredential()
        account_url = "https://parserstorage.blob.core.windows.net/"
        blob_client = BlobServiceClient(account_url, credential)
        return blob_client



async def send_heartbeat():
    baseurl = os.getenv("BASE_URL", "http://do.jonaskaad.com")
    parser_name = os.getenv("PARSER_NAME", "python-taf")
    url = f"{baseurl}/{parser_name}/Parser/heartbeat"
    async with aiohttp.ClientSession() as session:
        async with session.post(url) as response:
            return response.status == 200

async def heartbeat_scheduler():
    alive = True
    while True:
        await asyncio.sleep(1 * 60)  # Sleep for 30 minutes
        if alive:
            try:
                alive = await send_heartbeat()
            except Exception as e:
                print(e)
        else:
            try:
                alive = await register_parser()
            except Exception as e:
                print(e)

def get_data(raw_data, format_type):
    data_list = raw_data.split(b'magic')
    strings = []
    raw = []
    for data in data_list:
        if data:
            if format_type == "str":
                try:
                    strings.append(data.decode('utf-8').split(";")[0])
                except UnicodeDecodeError:
                    print(f"Warning: Could not decode part of data as UTF-8")
                    raw.append(data)
            else:
                raw.append(data)

    return strings, raw


async def register_parser():
    async with aiohttp.ClientSession() as session:
        baseurl = os.getenv("BASE_URL", "http://do.jonaskaad.com")
        parser_name = os.getenv("PARSER_NAME", "python-taf")
        url = f"{baseurl}/Parser/{parser_name}/register"
        try:
            async with session.post(url, json=os.getenv("PARSER_URL", "http://pythonparser.jonaskaad.com")) as response:
                return response.status == 200
        except ClientConnectorError:
            return False

async def init_parser(injected_loop):
    registered = False
    while not registered:
        registered = await register_parser()
        if registered:
            injected_loop.create_task(heartbeat_scheduler())
            return
        else:
            print("Failed to register, retrying in 10 seconds...")
            await asyncio.sleep(10)

async def serve():
    # Create a gRPC server
    server = grpc.aio.server()

    # Add the servicer to the server
    parser_pb2_grpc.add_ParserServicer_to_server(
        ParserServicer(), server
    )

    # Listen on port 50051
    server.add_insecure_port('[::]:50051')
    await server.start()
    print(f"Starting server on {50051}")
    await server.wait_for_termination()


if __name__ == '__main__':
    loop = asyncio.new_event_loop()
    loop.create_task(init_parser(loop))
    asyncio.set_event_loop(loop)
    loop.run_until_complete(serve())
