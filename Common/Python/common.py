import aiohttp
import asyncio
import os

from azure.identity.aio import DefaultAzureCredential
from azure.storage.blob.aio import BlobServiceClient

async def send_heartbeat():
    baseurl = os.getenv("BASE_URL", "http://do.jonaskaad.com")
    parser_name = os.getenv("PARSER_NAME")
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

async def register_parser():
    async with aiohttp.ClientSession() as session:
        baseurl = os.getenv("BASE_URL", "http://do.jonaskaad.com")
        parser_name = os.getenv("PARSER_NAME")
        url = f"{baseurl}/Parser/{parser_name}/register"
        try:
            async with session.post(url, json=os.getenv("PARSER_URL")) as response:
                return response.status == 200
        except aiohttp.ClientConnectorError:
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

def azure_cred_checker():
    credential = DefaultAzureCredential()
    account_url = "https://parserstorage.blob.core.windows.net/"
    blob_client = BlobServiceClient(account_url, credential)
    return blob_client