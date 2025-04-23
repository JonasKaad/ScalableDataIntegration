package src.main.java.sdi.common;

import com.google.protobuf.ByteString;
import io.github.cdimascio.dotenv.Dotenv;
import io.grpc.ServerBuilder;

import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutionException;
import java.util.stream.Collectors;

public class Server {

    private final int retryInterval = 60000; // Retry interval in milliseconds
    io.grpc.Server server;
    private final int port;

    public Server(int port) {
        this.port = port; // Default port
    }

    public void registerServer(io.grpc.BindableService service) {
        // Create a gRPC server on port 50051
        server = ServerBuilder.forPort(port)
                .addService(service)
                .build();
    }

    public void start() throws IOException, InterruptedException {
        // Start the server
        server.start();
        System.out.println("Parser Server started on port " + port);

        // Keep the server running until terminated
        server.awaitTermination();
    }

    public boolean registerService(RegisterType registerType) {
        Dotenv dotenv = Dotenv.load();
        String parserName = dotenv.get("PARSER_NAME");
        String parserUrl = dotenv.get("PARSER_URL");
        String baseUrl = dotenv.get("BASE_URL");

        String url = String.format("%s/%s/%s/register", baseUrl, registerType.toString(), parserName);

        //send post request to url
        try ( HttpClient client = HttpClient.newHttpClient() ) {
            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(url))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString("{\"url\": \"" + parserUrl + "\"}"))
                    .build();
            try {
                CompletableFuture<HttpResponse<String>> futureResponse = client.sendAsync(request,
                        HttpResponse.BodyHandlers.ofString());
                HttpResponse<String> response = futureResponse.get();
                if (response.statusCode() == 200) {
                    System.out.printf("Successfully registered %s of type: %s \n", parserName, response);
                    return true;
                } else {
                    System.out.printf("Failed to register service: %s. %s", parserName, response.body());
                    return false;
                }
            } catch (InterruptedException | ExecutionException e) {
                System.out.printf("Failed to register service: %s. %s", parserName, e.getMessage());
                return false;
            }
        }
    }


    public void initialize(RegisterType registerType) {
        while(true){
            boolean registered = registerService(registerType);
            if (registered) {
                System.out.println("Registering successful. Starting heartbeat service...");
                new Thread(() -> {
                    try {
                       heartbeatService(registerType);
                    } catch (Exception e) {
                        System.err.println("Error starting server: " + e.getMessage());
                    }
                }).start();
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

    public void heartbeatService(RegisterType registerType) {
        boolean alive = true;

        while (true) {
            if (alive) {
                alive = Heartbeat.sendHeartbeat(registerType);
            } else {
                alive = registerService(registerType);
                if(!alive) {
                    System.out.println("Failed to register service. Retrying in " + retryInterval / 1000 + " seconds...");
                }
            }
            try {
                Thread.sleep(retryInterval);
            } catch (InterruptedException e) {
                System.err.println("Thread interrupted: " + e.getMessage());
                break;
            }
        }
    }

    public static ConversionResult convertData(ByteString rawData, String dataType) {
        byte[] data = rawData.toByteArray();


        // Split on "magic" and create lists for results
        List<byte[]> sections = Arrays.stream(new String(data).split("magic"))
                .map(String::getBytes)
                .collect(Collectors.toList());

        List<String> relevant = new ArrayList<>();
        List<byte[]> raw = new ArrayList<>();

        for (byte[] section : sections) {
            if (section.length == 0) continue;

            if (dataType.equals("str")) {
                try {
                    String decodedStr = new String(section, StandardCharsets.UTF_8);
                    relevant.addAll(Arrays.asList(decodedStr.split(";")));
                } catch (Exception e) {
                    raw.add(section);
                }
            } else if (dataType.equals("img")) {
                relevant.add(new String(section));
            } else {
                raw.add(section);
            }
        }

        // Combine raw sections into single byte array
        byte[] combinedRaw = raw.stream()
                .reduce(new byte[0], (a, b) -> {
                    byte[] result = Arrays.copyOf(a, a.length + b.length);
                    System.arraycopy(b, 0, result, a.length, b.length);
                    return result;
                });

        return new ConversionResult(
                relevant.toArray(new String[0]),
                combinedRaw
        );
    }
}
