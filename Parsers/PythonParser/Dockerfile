# Use the official Python image from the Docker Hub
FROM python:3.9-slim

# Set the working directory in the container
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY Parsers/PythonParser/ /app/Parsers/PythonParser

# Copy the GeneratedClients directory
COPY GeneratedClients/python/ /app/GeneratedClients/python
COPY Common/Python/ /app/Common/Python

# Install any needed packages specified in requirements.txt
RUN pip install --no-cache-dir -r /app/Parsers/PythonParser/requirements.txt
RUN pip install --no-cache-dir -r /app/Common/Python/requirements.txt

# Expose the port that the gRPC server will be listening on
EXPOSE 50051

# Run parser.py when the container launches
CMD ["python", "/app/Parsers/PythonParser/parser.py"]
