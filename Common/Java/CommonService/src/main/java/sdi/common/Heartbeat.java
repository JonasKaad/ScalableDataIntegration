package src.main.java.sdi.common;

import io.github.cdimascio.dotenv.Dotenv;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.Optional;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ExecutionException;

public class Heartbeat {

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
                    System.out.printf("Heartbeat sent status: %s\n", response.statusCode());
                    return true;
                } else {
                    System.out.printf("Heartbeat failed. Status: %s. %s\n", response.statusCode(), response.body());
                    return false;
                }
            } catch (InterruptedException | ExecutionException e) {
                System.out.printf("Heartbeat failed: Reason: %s\n", e.getMessage());
                return false;
            }
        }
    }
}
