package src.main.java.sdi.common;

import com.azure.identity.DefaultAzureCredential;
import com.azure.identity.DefaultAzureCredentialBuilder;
import com.azure.storage.blob.BlobClient;
import com.azure.storage.blob.BlobContainerClient;
import com.azure.storage.blob.BlobServiceClient;
import com.azure.storage.blob.BlobServiceClientBuilder;
import com.google.protobuf.ByteString;
import io.grpc.ServerBuilder;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.sql.Time;
import java.time.ZoneOffset;
import java.time.ZonedDateTime;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutionException;
import java.util.stream.Collectors;

public class Server {
    private static final Logger logger = LoggerFactory.getLogger(Server.class);

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
        logger.info("Parser Server started on port {}", port);

        // Keep the server running until terminated
        server.awaitTermination();
    }

    public boolean registerService(RegisterType registerType) {
        String parserName = System.getenv("PARSER_NAME");
        String parserUrl = System.getenv("PARSER_URL");
        String baseUrl = System.getenv("BASE_URL");

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
                    logger.info("Successfully registered {} of type: {}", parserName, response);
                    return true;
                } else {
                    logger.warn("Failed to register service: {}. {}", parserName, response.body());
                    return false;
                }
            } catch (InterruptedException | ExecutionException e) {
                logger.info("Failed to register service: {}. {}", parserName, e.getMessage());
                return false;
            }
        }
    }


    public void initialize(RegisterType registerType) {
        while(true){
            boolean registered = registerService(registerType);
            if (registered) {
                logger.info("Registering successful. Starting heartbeat service...");
                new Thread(() -> {
                    try {
                       heartbeatService(registerType);
                    } catch (Exception e) {
                        logger.warn("Error starting heartbeat service: {}",  e.getMessage());
                    }
                }).start();
                break;
            } else {
                logger.info("Failed to register service in initialization. Retrying in {} seconds...", (retryInterval / 1000));
                try {
                    Thread.sleep(retryInterval);
                } catch (InterruptedException e) {
                    logger.warn("Thread interrupted: {}", e.getMessage());
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
                    logger.info("Failed to register service from heartbeatService. Retrying in {} seconds...", (retryInterval / 1000));
                }
            }
            try {
                Thread.sleep(retryInterval);
            } catch (InterruptedException e) {
                logger.info("Thread interrupted: {}", e.getMessage());
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

    public static void saveToBlobStorage(byte[] rawFile, byte[] parsedFile) {
        BlobServiceClient blobServiceClient = checkAzureCredentials();
        if (blobServiceClient == null) {
            logger.warn("BlobServiceClient is null. Cannot save to Blob Storage.");
            return;
        }

        String containerName = System.getenv("PARSER_NAME");

        try {
            // Create a container if it doesn't exist
            blobServiceClient.getBlobContainerClient(containerName).createIfNotExists();
            BlobContainerClient blobContainerClient = blobServiceClient.getBlobContainerClient(containerName);

            ZonedDateTime time = ZonedDateTime.now(ZoneOffset.UTC);
            String formattedTime = String.format("%tY/%tm/%td/%tH%tM", time, time, time, time, time);
            String rawFileName = formattedTime + "-raw.txt";
            String parsedFileName = formattedTime + "-parsed.txt";
            logger.info("Raw file name: {}", rawFileName);
            logger.info("Parsed file name: {}", parsedFileName);
            try {
                // Check if the blob already exists
                BlobClient rawBlobClient = blobContainerClient.getBlobClient(rawFileName);
                BlobClient parsedClient = blobContainerClient.getBlobClient(parsedFileName);
                if (rawBlobClient.exists() || parsedClient.exists()) {
                    logger.info("Blob files {}, {} already exists. Skipping upload.", rawFileName, parsedFile);
                    return;
                }
                rawBlobClient.upload(new ByteArrayInputStream(rawFile), (long) rawFile.length);
                parsedClient.upload(new ByteArrayInputStream(parsedFile), (long) parsedFile.length);
                logger.info("Data uploaded to Blob Storage successfully.");
            } catch (Exception e) {
                logger.warn("Error checking blob existence: {}", e.getMessage());
            }
        } catch (Exception e) {
            logger.warn("Error uploading data to Blob Storage: {}", e.getMessage());
        }
    }

    private static BlobServiceClient checkAzureCredentials(){
        String connectStr = System.getenv("BLOB_CONNECTION_STRING");

        if (connectStr != null && !connectStr.isEmpty()) {
            try {
                return new BlobServiceClientBuilder()
                    .connectionString(connectStr)
                    .buildClient();
            } catch (Exception e) {
                logger.warn("Error creating BlobServiceClient with connection string: {}", e.getMessage());
                return null;
            }
        } else {
            DefaultAzureCredential defaultCredential = new DefaultAzureCredentialBuilder().build();
            try {

                return new BlobServiceClientBuilder()
                        .endpoint("https://parserstorage.blob.core.windows.net/")
                        .credential(defaultCredential)
                        .buildClient();
            } catch (Exception e) {
                logger.warn("Error creating BlobServiceClient azure credentials: {}", e.getMessage());
                return null;
            }
        }
    }
}
