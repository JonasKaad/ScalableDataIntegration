package src.main.java.sdi.common;

import io.github.cdimascio.dotenv.Dotenv;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.Optional;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutionException;

public class Heartbeat {
    private static final Logger logger = LoggerFactory.getLogger(Heartbeat.class);

    public static boolean sendHeartbeat(RegisterType registerType) {
        // send post request to url
        Dotenv dotenv = Dotenv.load();
        String parserName = dotenv.get("PARSER_NAME");
        String baseUrl = dotenv.get("BASE_URL");

        String url = String.format("%s/%s/%s/heartbeat", baseUrl, registerType.toString(), parserName);

        //send post request to url
        try ( HttpClient client = HttpClient.newHttpClient() ) {
            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(url))
                    .POST(HttpRequest.BodyPublishers.noBody())
                    .build();
            try {
                CompletableFuture<HttpResponse<String>> futureResponse = client.sendAsync(request,
                        HttpResponse.BodyHandlers.ofString());
                HttpResponse<String> response = futureResponse.get();
                if (response.statusCode() == 200) {
                    logger.info("Heartbeat sent status: {}", response.statusCode());
                    return true;
                } else {
                    logger.warn("Heartbeat failed. Status: {}. {}", response.statusCode(), response.body());
                    return false;
                }
            } catch (InterruptedException | ExecutionException e) {
                logger.warn("Heartbeat failed: Reason: {}", e.getMessage());
                return false;
            }
        }
    }
}
