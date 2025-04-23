package sdi.parsers;

import com.google.protobuf.ByteString;
import io.github.mivek.exception.ParseException;
import io.github.mivek.model.Metar;
import io.github.mivek.parser.MetarParser;
import io.github.mivek.service.MetarService;
import io.grpc.stub.StreamObserver;
import sdi.parser.ParserGrpc;
import sdi.parser.ParserOuterClass;
import src.main.java.sdi.common.ConversionResult;
import src.main.java.sdi.common.Server;

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

                ConversionResult conversionResult = Server.convertData(rawData, format);
                String[] relevantData = conversionResult.getRelevantData();

                Parser parser = new Parser();
                for (String data : relevantData) {
                    String[] metarParts = data.split("\n");
                    for (String metarPart : metarParts) {
                        if (metarPart.trim().isEmpty() || metarPart.trim().equals("magic")) {
                            continue;
                        }
                        parser.ParseMetar(metarPart);
                    }
                }

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
