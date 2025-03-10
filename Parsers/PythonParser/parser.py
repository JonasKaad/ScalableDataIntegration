import grpc
from concurrent import futures
import time
import sys
import os

GENCLIENT_PYTHON_PATH = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../GeneratedClients/python"))


print(f"Adding to sys.path: {GENCLIENT_PYTHON_PATH}")  # Debugging
sys.path.insert(0, GENCLIENT_PYTHON_PATH)

# Check if the file exists
print(f"Files in {GENCLIENT_PYTHON_PATH}: {os.listdir(GENCLIENT_PYTHON_PATH)}")

# noinspection PyUnresolvedReferences
import parser_pb2 # works even though IDE shows error (suppressing the warning)
# noinspection PyUnresolvedReferences
import parser_pb2_grpc # works even though IDE shows error (suppressing the warning)

class ParserServicer(parser_pb2_grpc.ParserServicer):
    def ParseCall(self, request, context):
        # Get data from the request
        raw_data = request.raw_data
        format_type = request.format

        # Log the request (for debugging)
        print(f"Received parse request with format: {format_type}")
        print(f"Data size: {len(raw_data)} bytes")
        print(f"Raw data: {raw_data}")

        if request.HasField('filter'):
            print(f"Filter format: {request.filter.format}")
            print(f"Filter size: {len(request.filter.filter)} bytes")

        # For this example, we'll just return success
        response = parser_pb2.ParseResponse(success=True, err_msg=f"Parsing {raw_data} in Python!")

        # If there was an error, it can be set like:
        # response = parser_pb2.ParseResponse(success=False, err_msg="Failed to parse data")

        return response


def serve():
    # Create a gRPC server
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))

    # Add the servicer to the server
    parser_pb2_grpc.add_ParserServicer_to_server(
        ParserServicer(), server
    )

    # Listen on port 50051
    server.add_insecure_port('[::]:50051')
    server.start()

    print("Server started, listening on port 50051")

    try:
        # Keep the server running until Ctrl+C
        while True:
            time.sleep(86400)  # One day in seconds
    except KeyboardInterrupt:
        server.stop(0)
        print("Server stopped")


if __name__ == '__main__':
    serve() 
