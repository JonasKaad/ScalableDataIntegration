# Use the official Python image from the Docker Hub
FROM python:3.10-slim

# Set the working directory in the container
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY Filters/OCRFilter/ /app/Filters/OCRFilter

# Copy the GeneratedClients directory
COPY GeneratedClients/python/ /app/GeneratedClients/python
COPY Common/Python/ /app/Common/Python

# Install any needed packages specified in requirements.txt
RUN pip install --no-cache-dir -r /app/Filters/OCRFilter/requirements.txt
RUN pip install --no-cache-dir -r /app/Common/Python/requirements.txt

# Expose the port that the gRPC server will be listening on
EXPOSE 50051

# Run filter.py when the container launches
CMD ["python", "/app/Filters/OCRFilter/filter.py"]
