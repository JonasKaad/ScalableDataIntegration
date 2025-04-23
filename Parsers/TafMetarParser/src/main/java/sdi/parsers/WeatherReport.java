package sdi.parsers;

import com.fasterxml.jackson.annotation.JsonInclude;
import io.github.mivek.model.Cloud;
import io.github.mivek.model.Metar;
import io.github.mivek.model.TAF;
import io.github.mivek.model.trend.MetarTrend;

import java.time.LocalTime;
import java.time.format.DateTimeFormatter;
import java.util.LinkedHashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

@JsonInclude(JsonInclude.Include.NON_NULL)
public class WeatherReport {
    private static final DateTimeFormatter TIME_FORMATTER = DateTimeFormatter.ofPattern("HHmm");
    private final Map<String, Object> report;

    public WeatherReport(Metar metar) {
        this.report = new LinkedHashMap<>();
        addBasicInfo(metar.getStation(), "METAR", metar.getDay(), metar.getTime());
        addWindInfo(metar.getWind());
        addVisibilityInfo(metar.getVisibility());
        addTemperatureInfo(metar);
        addCloudsInfo(metar.getClouds());
        addMetarTrendsInfo(metar.getTrends());
    }

    public WeatherReport(TAF taf) {
        this.report = new LinkedHashMap<>();
        addBasicInfo(taf.getStation(), "TAF", taf.getDay(), taf.getTime());
        addWindInfo(taf.getWind());
        addVisibilityInfo(taf.getVisibility());
        addCloudsInfo(taf.getClouds());
        addTafTrendsInfo(taf);
    }

    private void addBasicInfo(String station, String type, int day, LocalTime time) {
        report.put("station", station);
        report.put("type", type);
        report.put("day", day);
        addTimeInfo(time);
    }

    private void addTimeInfo(LocalTime time) {
        if (time != null) {
            report.put("time", time.format(TIME_FORMATTER));
        }
    }

    private void addWindInfo(io.github.mivek.model.Wind wind) {
        if (wind == null) return;

        Map<String, Object> windInfo = new LinkedHashMap<>();
        windInfo.put("direction", wind.getDirectionDegrees());
        windInfo.put("speed", wind.getSpeed());
        windInfo.put("gust", wind.getGust());
        windInfo.put("unit", "KT");
        report.put("wind", windInfo);
    }

    private void addVisibilityInfo(io.github.mivek.model.Visibility visibility) {
        if (visibility == null) return;

        Map<String, Object> visibilityInfo = new LinkedHashMap<>();
        visibilityInfo.put("main", visibility.getMainVisibility());
        visibilityInfo.put("min", visibility.getMinVisibility());
        visibilityInfo.put("minDirection", visibility.getMinDirection());
        report.put("visibility", visibilityInfo);
    }

    private void addTemperatureInfo(Metar metar) {
        Map<String, Object> tempInfo = new LinkedHashMap<>();
        tempInfo.put("temperature", metar.getTemperature());
        tempInfo.put("dewPoint", metar.getDewPoint());
        report.put("temperature", tempInfo);
    }

    private void addCloudsInfo(List<Cloud> clouds) {
        if (clouds == null || clouds.isEmpty()) return;

        List<Map<String, Object>> cloudsList = clouds.stream()
                .map(cloud -> {
                    Map<String, Object> cloudInfo = new LinkedHashMap<>();
                    cloudInfo.put("quantity", cloud.getQuantity());
                    cloudInfo.put("height", cloud.getHeight() != null ? cloud.getHeight() * 100 : null);
                    cloudInfo.put("type", cloud.getType());
                    return cloudInfo;
                })
                .collect(Collectors.toList());

        report.put("clouds", cloudsList);
    }

    private void addMetarTrendsInfo(List<MetarTrend> trends) {
        if (trends == null || trends.isEmpty()) return;

        List<Map<String, Object>> trendsList = trends.stream()
                .map(trend -> {
                    Map<String, Object> trendInfo = new LinkedHashMap<>();
                    trendInfo.put("type", "TEMPO");
                    trendInfo.put("clouds", trend.getClouds());
                    trendInfo.put("weather", trend.getWeatherConditions());
                    return trendInfo;
                })
                .collect(Collectors.toList());

        report.put("trends", trendsList);
    }

    private void addTafTrendsInfo(TAF taf) {
        if (taf.getTempos() == null || taf.getTempos().isEmpty()) return;

        List<Map<String, Object>> trendsList = taf.getTempos().stream()
                .map(tempo -> {
                    Map<String, Object> trendInfo = new LinkedHashMap<>();
                    trendInfo.put("type", "TEMPO");
                    if (tempo.getValidity() != null) {
                        Map<String, Object> validity = new LinkedHashMap<>();
                        validity.put("startDay", tempo.getValidity().getStartDay());
                        validity.put("startHour", tempo.getValidity().getStartHour());
                        validity.put("endDay", tempo.getValidity().getEndDay());
                        validity.put("endHour", tempo.getValidity().getEndHour());
                        trendInfo.put("validity", validity);
                    }
                    trendInfo.put("clouds", tempo.getClouds());
                    return trendInfo;
                })
                .collect(Collectors.toList());

        report.put("trends", trendsList);
    }

    public Map<String, Object> getReport() {
        return report;
    }
}