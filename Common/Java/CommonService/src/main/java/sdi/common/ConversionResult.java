package src.main.java.sdi.common;

public class ConversionResult
{
    private final String[] relevantData;
    private final byte[] rawData;

    public ConversionResult(String[] relevantData, byte[] rawData) {
        this.relevantData = relevantData;
        this.rawData = rawData;
    }

    public String[] getRelevantData() { return relevantData; }
    public byte[] getRawData() { return rawData; }
}
