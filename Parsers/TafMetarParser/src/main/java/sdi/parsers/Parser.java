package sdi.parsers;

import io.github.mivek.exception.ParseException;
import io.github.mivek.model.Metar;
import io.github.mivek.service.MetarService;

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
}
