import asyncio
import grpc
import sys
import os

import easyocr
import cv2
import numpy as np

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
from common import init_filter, get_next_url, read_params, send_to_next_url, dd_warning, dd_info, dd_error

class FilterServicer(filter_pb2_grpc.FilterServicer):
    async def FilterCall(self, request, context):
        dd_info("FilterCall", "Received FilterCall request")
        
        # Get data from the request
        raw_data = request.raw_data
        dd_info("Data Received", f"Received raw data of size: {len(raw_data)} bytes")

        dd_info("Image Processing", "Converting raw data to image")
        nparr = np.frombuffer(raw_data, np.uint8)
        image_data = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
        dd_info("Image Info", f"Image shape: {image_data.shape}")

        (json_parameters, parameters) = read_params(request.parameters.split(";"), "startX")
        dd_info("Parameters", f"Received parameters: {json_parameters}")

        start_y = int(float(json_parameters["startY"]))
        start_x = int(float(json_parameters["startX"]))
        end_y = int(float(json_parameters["endY"]))
        end_x = int(float(json_parameters["endX"]))
        dd_info("Image Cropping", f"Cropping image with coordinates: ({start_x}, {start_y}) to ({end_x}, {end_y})")
        
        cropped = image_data[start_y:end_y, start_x:end_x]
        dd_info("Cropped Image", f"Cropped image shape: {cropped.shape}")
        
        img_bytes = cv2.imencode('.png', cropped)[1].tobytes()
        dd_info("OCR Processing", "Running OCR on cropped image")
        result = reader.readtext(img_bytes, detail=0)
        dd_info("OCR Results", f"OCR results: {result}")

        raw_data += b'magic'
        raw_data += bytes(';'.join(result), encoding='utf-8')
        dd_info("Data Processing", f"Final data size: {len(raw_data)} bytes")

        (next_url, urls) = get_next_url(request.next_urls.split(";"))
        dd_info("Routing", f"Next URL: {next_url}")

        dd_info("Data Transfer", "Sending data to next filter")
        (success, msg) = await send_to_next_url(next_url, raw_data, parameters, urls)
        dd_info("Transfer Result", f"Send result: success={success}, message={msg}")

        return filter_pb2.FilterReply(success=success, err_msg=msg)


async def serve():
    dd_info("Server Startup", "Initializing gRPC server")
    server = grpc.aio.server()
    global reader

    dd_info("Server Config", "Adding FilterServicer to server")
    filter_pb2_grpc.add_FilterServicer_to_server(
        FilterServicer(), server
    )
    
    dd_info("OCR Setup", "Initializing EasyOCR reader")
    reader = easyocr.Reader(['en'], gpu=False)

    port = 50051
    server.add_insecure_port(f'[::]:{port}')
    await server.start()
    dd_info("Server Status", f"Server started on port {port}")
    await server.wait_for_termination()

if __name__ == '__main__':
    dd_info("Service Startup", "Starting OCRFilter service")
    parameters = {
        "startX": "0",
        "startY": "0",
        "endX": "100",
        "endY": "100"
    }
    dd_info("Parameters", f"Initial parameters: {parameters}")
    
    loop = asyncio.new_event_loop()
    loop.create_task(init_filter(loop, parameters))
    asyncio.set_event_loop(loop)
    dd_info("Event Loop", "Running server event loop")
    loop.run_until_complete(serve())