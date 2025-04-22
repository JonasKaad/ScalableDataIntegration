FROM golang:1.23.3-alpine

WORKDIR /app

# Copy go mod files
COPY Parsers/MetarParser/src/scalabledataintegration/go.mod Parsers/MetarParser/src/scalabledataintegration/
COPY Parsers/MetarParser/src/scalabledataintegration/go.sum Parsers/MetarParser/src/scalabledataintegration/

# Copy shared dependencies
COPY GeneratedClients/go/ /app/GeneratedClients/go
COPY Common/go/ /app/Common/go

# Copy main source code
COPY Parsers/MetarParser/src/scalabledataintegration/ Parsers/MetarParser/src/scalabledataintegration/

# Set working directory for building
WORKDIR /app/Parsers/MetarParser/src/scalabledataintegration

# Download dependencies
RUN go mod download

# Build the application
RUN go build -o /app/metarparser main.go

# Expose the port
EXPOSE 50051

# Set the working directory for running the application
WORKDIR /app

# Run the application
CMD ["./metarparser"]