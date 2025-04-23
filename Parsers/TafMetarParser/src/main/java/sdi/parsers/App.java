package sdi.parsers;

import src.main.java.sdi.common.RegisterType;
import src.main.java.sdi.common.Server;

public class App {
    public static void main(String[] args) {
        Server server = new Server(50051);

        server.registerServer(new Parser.TafMetarParserService());
        server.initialize(RegisterType.Parser);
        try {
            server.start();
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }
}