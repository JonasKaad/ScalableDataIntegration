import asyncio
import json
import grpc
import sys
import os

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

# Import functions from common.py
COMMON_PATH = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../Common/Python"))
sys.path.insert(0, COMMON_PATH)

# noinspection PyUnresolvedReferences
from common import send_heartbeat, heartbeat_scheduler, init_parser, azure_cred_checker, get_data, save_data_to_azure

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
            (strings, rest) = get_data(raw_data, format_type="str")

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

        raw_data_to_save = raw_data
        parsed_data_to_save = json.dumps(tafs, cls=TAFEncoder)
        container_name = os.getenv("PARSER_NAME", "python-taf")
        await save_data_to_azure(raw_data_to_save, parsed_data_to_save, container_name)
        return response

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
