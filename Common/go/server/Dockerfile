FROM ubuntu:latest
LABEL authors="jonaskaad"

ENTRYPOINT ["top", "-b"]

# Set the working directory in the container
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY Parsers/MetarParser/ /app/Parsers/MetarParser

# Copy the GeneratedClients directory
COPY GeneratedClients/go/ /app/GeneratedClients/go
COPY Common/Go/ /app/Common/go


# Expose the port that the gRPC server will be listening on
EXPOSE 50051
