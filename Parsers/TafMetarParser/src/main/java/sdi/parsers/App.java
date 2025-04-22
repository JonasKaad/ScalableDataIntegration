package sdi.parsers;

import com.google.protobuf.ByteString;
import io.grpc.ServerBuilder;
import io.grpc.stub.StreamObserver;
import src.main.java.sdi.common.Server;
import sdi.parser.ParserGrpc;
import sdi.parser.ParserOuterClass;

import java.io.IOException;

public class App {
    public static void main(String[] args) {
        Server server = new Server(50051);

        server.registerService(new Parser.TafMetarParserService());
        try {
            server.startServer();
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }
}