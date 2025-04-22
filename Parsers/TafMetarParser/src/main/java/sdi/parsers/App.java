package sdi.parsers;

import com.google.protobuf.ByteString;
import io.grpc.Server;
import io.grpc.ServerBuilder;
import io.grpc.stub.StreamObserver;
import sdi.parser.ParserGrpc;
import sdi.parser.ParserOuterClass;

import java.io.IOException;

public class App {
    public static void main(String[] args) throws IOException, InterruptedException {
        // Create a gRPC server on port 50051
        Server server = ServerBuilder.forPort(50051)
                .addService(new TafMetarParserService())
                .build();

        // Start the server
        server.start();
        System.out.println("TafMetar Parser Server started on port 50051");

        // Keep the server running until terminated
        server.awaitTermination();
    }

    static class TafMetarParserService extends ParserGrpc.ParserImplBase {
        @Override
        public void parseCall(ParserOuterClass.ParseRequest request, StreamObserver<ParserOuterClass.ParseResponse> responseObserver) {
            try {
                // Extract data from request
                ByteString rawData = request.getRawData();
                String format = request.getFormat();

                System.out.println("Received parsing request with format: " + format);
                System.out.println("Raw data size: " + rawData.size() + " bytes");

                boolean success = true;
                String errorMsg = null;

                ParserOuterClass.ParseResponse.Builder responseBuilder = ParserOuterClass.ParseResponse.newBuilder()
                        .setSuccess(success);

                if (errorMsg != null) {
                    responseBuilder.setErrMsg(errorMsg);
                }

                // Send the response
                responseObserver.onNext(responseBuilder.build());
                responseObserver.onCompleted();

            } catch (Exception e) {
                System.err.println("Error processing request: " + e.getMessage());
                e.printStackTrace();

                ParserOuterClass.ParseResponse errorResponse = ParserOuterClass.ParseResponse.newBuilder()
                        .setSuccess(false)
                        .setErrMsg("Internal server error: " + e.getMessage())
                        .build();

                responseObserver.onNext(errorResponse);
                responseObserver.onCompleted();
            }
        }
    }
}