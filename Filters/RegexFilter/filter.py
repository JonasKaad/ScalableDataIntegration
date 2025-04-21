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
from common import init_filter, get_next_url, read_params, send_to_next_url, get_data

class FilterServicer(filter_pb2_grpc.FilterServicer):
    async def FilterCall(self, request, context):
        # Get data from the request
        raw_data = request.raw_data
        format_type = request.format
        (strings, raw) = get_data(raw_data, format_type)
        if format_type != "str":
            return filter_pb2.FilterResponse(success=False, err_msg="Invalid format")

        (json_parameters, parameters) = read_params(request.parameters.split(";"), "regex")

        for string in strings:
            if re.search(json_parameters["regex"], string):
                match = string

        (next_url, urls) = get_next_url(request.next_urls.split(";"))

        data_to_send = raw[0]
        data_to_send += b'magic'
        data_to_send += bytes(match, encoding='utf-8')

        (success, msg) = await send_to_next_url(next_url, data_to_send, parameters, urls)

        return filter_pb2.FilterReply(success=success, err_msg=msg)

async def serve():
    # Create a gRPC server
    server = grpc.aio.server()
    global reader

    # Add the servicer to the server
    filter_pb2_grpc.add_FilterServicer_to_server(
        FilterServicer(), server
    )

    # Listen on port 50051
    server.add_insecure_port('[::]:50051')
    await server.start()
    print(f"Starting server on {50051}")
    await server.wait_for_termination()


if __name__ == '__main__':
    parameters = {
        "regex": ".*"
    }
    loop = asyncio.new_event_loop()
    loop.create_task(init_filter(loop, parameters))
    asyncio.set_event_loop(loop)
    loop.run_until_complete(serve())
