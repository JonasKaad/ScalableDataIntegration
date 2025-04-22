package sdi.parsers;

import com.google.protobuf.ByteString;
import io.github.mivek.exception.ParseException;
import io.github.mivek.model.Metar;
import io.github.mivek.service.MetarService;
import io.grpc.stub.StreamObserver;
import sdi.parser.ParserGrpc;
import sdi.parser.ParserOuterClass;

public class Parser {

    MetarService metarService;

    public Parser() {
        metarService = MetarService.getInstance();
    }

    public void ParseMetar(String metar) {
        try {
            Metar parsedMetar = metarService.decode(metar);
            System.out.println(parsedMetar.getAirport());
        } catch (ParseException e) {
            System.out.println("Error: " + e.getMessage());
        }
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
                // Convert ByteString to String
                rawData = ByteString.copyFrom(rawData.toByteArray());
                String rawDataString = rawData.toStringUtf8();
                Parser parser = new Parser();
                parser.ParseMetar(rawDataString);

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
