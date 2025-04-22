package src.main.java.sdi.common;

import com.google.protobuf.ByteString;
import io.grpc.ServerBuilder;
import io.grpc.stub.StreamObserver;
import sdi.parser.ParserGrpc;
import sdi.parser.ParserOuterClass;

import java.io.IOException;

public class Server {

    io.grpc.Server server;
    private int port;

    public Server(int port) {
        this.port = port; // Default port
    }

    public void registerService(io.grpc.BindableService service) {
        // Create a gRPC server on port 50051
        server = ServerBuilder.forPort(port)
                .addService(service)
                .build();
    }

    public void startServer() throws IOException, InterruptedException {
        // Start the server
        server.start();
        System.out.println("Parser Server started on port " + port);

        // Keep the server running until terminated
        server.awaitTermination();
    }
}
