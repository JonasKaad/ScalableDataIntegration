import asyncio
import grpc
import sys
import os
import json
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
# noinspection PyUnresolvedReferences
import parser_pb2
# noinspection PyUnresolvedReferences
import parser_pb2_grpc

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

        if next_url:
            if next_url.startswith("http://"):
                next_url = next_url[7:]
            elif next_url.startswith("https://"):
                next_url = next_url[8:]

        if ":" not in next_url:
            next_url = next_url + ":80"
        try:
            # Create channel with timeout options
            options = [
                ('grpc.enable_retries', 1),
                ('grpc.keepalive_time_ms', 10000),
                ('grpc.dns_resolution_timeout_ms', 5000)
            ]
            async with grpc.aio.insecure_channel(next_url, options=options) as channel:
                if len(parameters) > 0:
                    stub = filter_pb2_grpc.FilterStub(channel)
                    response = await stub.FilterCall(
                        filter_pb2.FilterRequest(raw_data=raw_data, parameters=parameters, next_urls=urls))
                else:
                    stub = parser_pb2_grpc.ParserStub(channel)
                    data_to_send = raw[0]
                    data_to_send += b'magic'
                    data_to_send += bytes(match, encoding='utf-8')
                    response = await stub.ParseCall(parser_pb2.ParseRequest(raw_data=data_to_send))

                if response.success:
                    success = True
                    msg = "Filter succeeded sending data to next url"
                else:
                    success = False
                    msg = "Filter failed sending data to next url"
        except Exception as e:
            print(f"gRPC connection error: {e}")
            success = False
            msg = "gRPC connection error"

        return filter_pb2.FilterReply(success=success, err_msg=msg)

def get_data(raw_data, format_type):
    data_list = raw_data.split(b'magic')
    strings = []
    raw = []
    for data in data_list:
        if data:  # Skip empty data chunks
            if format_type == "str":
                try:
                    #TODO: FIX ME
                    strings = data.decode('utf-8').split(";")
                except UnicodeDecodeError:
                    print(f"Warning: Could not decode part of data as UTF-8")
                    raw.append(data)  # Keep as bytes
            else:
                raw.append(data)

    return strings, raw


def read_params(params_list, relevant_string=""):
    relevant = {}
    for parameter in params_list:
        json_acceptable_string = parameter.replace("'", "\"")
        if relevant_string in json_acceptable_string:
            relevant = json.loads(json_acceptable_string)
            params_list.remove(parameter)
    return relevant, ";".join(params_list)

def get_next_url(urls):
    if len(urls) == 0:
        return None
    url = urls.pop(0)
    return url, ";".join(urls)


async def serve():
    # Create a gRPC server
    server = grpc.aio.server()
    global reader

    # Add the servicer to the server
    filter_pb2_grpc.add_FilterServicer_to_server(
        FilterServicer(), server
    )

    # Listen on port 50051
    server.add_insecure_port('[::]:50052')
    await server.start()
    print(f"Starting server on {50052}")
    await server.wait_for_termination()


if __name__ == '__main__':
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    loop.run_until_complete(serve())
