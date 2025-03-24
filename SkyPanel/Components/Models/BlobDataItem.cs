namespace SkyPanel.Components.Models;

public class BlobDataItem
{
    public string? ParserName { get; set; }
    public DateTime Date { get; set; }
    
    public string? RawPath { get; set; }
    public string? ParsedPath { get; set; }
    
    public BlobDataItem(string? parserName, DateTime date, string? rawPath = null, string? parsedPath = null)
    {
        ParserName = parserName;
        Date = date;
        RawPath = rawPath;
        ParsedPath = parsedPath;
    }

    public override string ToString()
    {
        return $"{ParserName} - {Date} - {RawPath} - {ParsedPath}";
    }
}