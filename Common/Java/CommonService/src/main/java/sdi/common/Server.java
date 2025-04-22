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
    public void initialize() {
        while(true){
            boolean registered = registerService();
            if (registered) {
                System.out.println("Service registered successfully.");
                new Thread(() -> {
                    try {
                        // Start the heartbeat service
                        new HeartbeatService().run();
                    } catch (Exception e) {
                        System.err.println("Error starting server: " + e.getMessage());
                    }
                });
                break;
            } else {
                System.out.println("Failed to register service. Retrying in " + retryInterval/1000 + " seconds...");
                try {
                    Thread.sleep(retryInterval);
                } catch (InterruptedException e) {
                    System.err.println("Thread interrupted: " + e.getMessage());
                }
            }
        }
    }
}
