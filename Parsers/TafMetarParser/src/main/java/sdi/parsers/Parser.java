package sdi.parsers;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.google.protobuf.ByteString;
import io.github.mivek.exception.ParseException;
import io.github.mivek.model.Metar;
import io.github.mivek.model.TAF;
import io.github.mivek.parser.MetarParser;
import io.github.mivek.parser.TAFParser;
import io.github.mivek.service.TAFService;
import io.grpc.stub.StreamObserver;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import sdi.parser.ParserGrpc;
import sdi.parser.ParserOuterClass;
import src.main.java.sdi.common.ConversionResult;
import src.main.java.sdi.common.Server;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

public class Parser {
    private static final Logger log = LoggerFactory.getLogger(Parser.class);
    private final MetarParser metarParser;
    private final TAFService tafService;

    public Parser() {
        this.metarParser = MetarParser.getInstance();
        this.tafService = TAFService.getInstance();
    }

    public WeatherReport parseWeatherData(String data) {
        try {
            // Try parsing as TAF first
            TAF taf = tafService.decode("TAF " + data);
            return new WeatherReport(taf);
        } catch (ParseException | NumberFormatException e)  {
            try {
                // If TAF parsing fails, try METAR
                Metar metar = metarParser.parse(data);
                return new WeatherReport(metar);
            } catch (ParseException ex) {
                log.warn("Failed to parse data: {}", data);
                return null;
            }
        }
    }

    static class TafMetarParserService extends ParserGrpc.ParserImplBase {
        @Override
        public void parseCall(ParserOuterClass.ParseRequest request, StreamObserver<ParserOuterClass.ParseResponse> responseObserver) {
            try {
                ByteString rawData = request.getRawData();
                String format = request.getFormat();

                ConversionResult conversionResult = Server.convertData(rawData, format);
                List<WeatherReport> reports = getWeatherReports(conversionResult);

                // Convert reports to byte array
                byte[] compiledReport = compileReports(reports);

                // Save original data and compiled report
                Server.saveToBlobStorage(rawData.toByteArray(), compiledReport);

                ParserOuterClass.ParseResponse response = ParserOuterClass.ParseResponse.newBuilder()
                        .setSuccess(true)
                        .build();

                responseObserver.onNext(response);
                responseObserver.onCompleted();

            } catch (Exception e) {
                log.warn("Error processing request: {}", e.getMessage());
                ParserOuterClass.ParseResponse errorResponse = ParserOuterClass.ParseResponse.newBuilder()
                        .setSuccess(false)
                        .setErrMsg("Not able to parse given data")
                        .build();

                responseObserver.onNext(errorResponse);
                responseObserver.onCompleted();
            }
        }
    }

    private static List<WeatherReport> getWeatherReports(ConversionResult conversionResult) {
        String[] relevantData = conversionResult.getRelevantData();
        Parser parser = new Parser();
        List<WeatherReport> reports = new ArrayList<>();

        for (String data : relevantData) {
            String[] lines = data.split("\n");
            for (String line : lines) {
                if (!line.trim().isEmpty() && !line.trim().equals("magic")) {
                    WeatherReport report = parser.parseWeatherData(line);
                    if (report != null) {
                        reports.add(report);
                    }
                }
            }
        }
        return reports;
    }

    private static byte[] compileReports(List<WeatherReport> reports) {
        ObjectMapper mapper = new ObjectMapper();
        mapper.setSerializationInclusion(JsonInclude.Include.NON_NULL);
        try {
            List<Map<String, Object>> reportMaps = reports.stream()
                    .map(WeatherReport::getReport)
                    .collect(Collectors.toList());
            return mapper.writeValueAsBytes(reportMaps);
        } catch (Exception e) {
            log.error("Error compiling reports to JSON", e);
            return new byte[0];
        }
    }
}