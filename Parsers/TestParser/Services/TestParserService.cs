namespace TestParser.Services;
using Grpc.Core;
using Sdi.Parser;

public class TestParserService : Parser.ParserBase
{
    private readonly ILogger<TestParserService> _logger;

    public TestParserService(ILogger<TestParserService> logger)
    {
        _logger = logger;
    }
    
    public override Task<ParseResponse> ParseCall(ParseRequest request, ServerCallContext context)
    {
         return Task.FromResult(new ParseResponse
         {
             Success = GetRandomNumberTrueOrFalse()
         });
    }


    public bool GetRandomNumberTrueOrFalse()
    {
        Random random = new Random();
        int n = random.Next(0, 2);
        
        return n == 1;
    }
}