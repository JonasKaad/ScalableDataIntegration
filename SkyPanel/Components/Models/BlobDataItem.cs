namespace SkyPanel.Components.Models;

public class BlobDataItem
{
    public string Parser { get; set; }
    public DateTime Date { get; set; }
    
    public string? RawPath { get; set; }
    public string? ParsedPath { get; set; }
    
    public BlobDataItem(String parser, DateTime date, string? rawPath = null, string? parsedPath = null)
    {
        Parser = parser;
        Date = date;
        RawPath = rawPath;
        ParsedPath = parsedPath;
    }

    public override string ToString()
    {
        return $"{Parser} - {Date} - {RawPath} - {ParsedPath}";
    }
}