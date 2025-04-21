import asyncio
import grpc
import sys
import os
import re

GENCLIENT_PYTHON_PATH = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../GeneratedClients/python"))

print(f"Adding to sys.path: {GENCLIENT_PYTHON_PATH}")  # Debugging
sys.path.insert(0, GENCLIENT_PYTHON_PATH)

# Check if the file exists
print(f"Files in {GENCLIENT_PYTHON_PATH}: {os.listdir(GENCLIENT_PYTHON_PATH)}")

# noinspection PyUnresolvedReferences
import filter_pb2 # works even though IDE shows error (suppressing the warning)
# noinspection PyUnresolvedReferences
import filter_pb2_grpc # works even though IDE shows error (suppressing the warning)

# Import functions from common.py
COMMON_PATH = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../Common/Python"))
sys.path.insert(0, COMMON_PATH)

# noinspection PyUnresolvedReferences
from common import init_filter, get_next_url, read_params, send_to_next_url, get_data, dd_warning, dd_info, dd_error

class FilterServicer(filter_pb2_grpc.FilterServicer):
    async def FilterCall(self, request, context):
        dd_info("FilterCall", "Received FilterCall request")
        
        # Get data from the request
        raw_data = request.raw_data
        format_type = request.format
        dd_info("Format", f"Processing data with format type: {format_type}")
        
        (strings, raw) = get_data(raw_data, format_type)
        dd_info("Data Processing", f"Parsed strings: {strings}")
        
        (json_parameters, parameters) = read_params(request.parameters.split(";"), "regex")
        dd_info("Parameters", f"Parsed parameters: {json_parameters}")

        match = None
        for string in strings:
            if re.search(json_parameters["regex"], string):
                match = string
                dd_info("Regex Match", f"Found match: {match}")

        (next_url, urls) = get_next_url(request.next_urls.split(";"))
        dd_info("Routing", f"Next URL: {next_url}")

        if not raw:
            dd_warning("Data Processing", "No raw data received")
            data_to_send = b''
        else:
            data_to_send = raw[0]
        data_to_send += b'magic'
        data_to_send += bytes(match, encoding='utf-8') if match else b''
        dd_info("Data Processing", f"Prepared data to send: {len(data_to_send)} bytes")

        (success, msg) = await send_to_next_url(next_url, data_to_send, parameters, urls)
        dd_info("Data Transfer", f"Send result: success={success}, message={msg}")

        return filter_pb2.FilterReply(success=success, err_msg=msg)

async def serve():
    dd_info("Server Startup", "Initializing gRPC server")
    server = grpc.aio.server()
    
    dd_info("Server Config", "Adding FilterServicer to server")
    filter_pb2_grpc.add_FilterServicer_to_server(
        FilterServicer(), server
    )

    port = 50051
    server.add_insecure_port(f'[::]:{port}')
    await server.start()
    dd_info("Server Status", f"Server started on port {port}")
    await server.wait_for_termination()

if __name__ == '__main__':
    dd_info("Service Startup", "Starting RegexFilter service")
    parameters = {
        "regex": ".*"
    }
    dd_info("Parameters", f"Initial parameters: {parameters}")
    
    loop = asyncio.new_event_loop()
    loop.create_task(init_filter(loop, parameters))
    asyncio.set_event_loop(loop)
    dd_info("Event Loop", "Running server event loop")
    loop.run_until_complete(serve())
