# Build stage
FROM maven:3.9.8-eclipse-temurin-21 AS build

# Set up workspace
WORKDIR /workspace

# Copy generated gRPC code
COPY /GeneratedClients/java/sdi/parser/* /workspace/GeneratedClients/java/sdi/parser/
COPY /GeneratedClients/java/sdi/filter/* /workspace/GeneratedClients/java/sdi/filter/

# Build CommonService
COPY /Common/Java/CommonService /workspace/CommonService
WORKDIR /workspace/CommonService
RUN mvn clean install -DskipTests

# Copy the project files
WORKDIR /workspace/app
COPY /Parsers/TafMetarParser/pom.xml .
COPY /Parsers/TafMetarParser/src ./src

# Set up generated clients in source path
RUN mkdir -p src/main/java/sdi/parser src/main/java/sdi/filter && \
    cp -r /workspace/GeneratedClients/java/sdi/parser/* src/main/java/sdi/parser/ && \
    cp -r /workspace/GeneratedClients/java/sdi/filter/* src/main/java/sdi/filter/


# Build the application
RUN mvn clean package -DskipTests

# Runtime stage
FROM openjdk:21-jdk-slim

WORKDIR /app

# Copy the built artifacts and environment file
COPY --from=build /workspace/app/target/TafMetarParser-1.0-SNAPSHOT.jar app.jar

# Expose the gRPC port
EXPOSE 50051

# Run the application
CMD ["java", "-jar", "app.jar"]