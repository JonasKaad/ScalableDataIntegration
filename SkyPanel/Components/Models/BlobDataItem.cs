namespace SkyPanel.Components.Models;

public class BlobDataItem
{
    private String Parser { get; set; }
    private DateTime Date { get; set; }
    
    public BlobDataItem(String parser, DateTime date)
    {
        Parser = parser;
        Date = date;
    }
    
    
}