namespace CommonDis.Models;

public class ParserModel
{
    public ParserModel()
    {
    }

    public ParserModel(string url)
    {
        Url = url;
    }

    public string Url { get; set; }
}