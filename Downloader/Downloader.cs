using System.Text;
using Google.Protobuf;
using Grpc.Net.Client;
using Sdi.Parser;


Dictionary<string, string> serviceAddresses = new()
{
    { "1", "http://localhost:5102" },
    { "2", "http://localhost:50051" },
};

Console.WriteLine("Select a parser:\n1 - TestParser (csharp)\n2 - PythonTestParser");
var selectedParser = Console.ReadLine();

if (!serviceAddresses.TryGetValue(selectedParser, out var address))
{
    Console.WriteLine("Invalid parser selected");
    throw new ArgumentException($"Unknown source type: {selectedParser}");
}
using var channel = GrpcChannel.ForAddress(address);
var client = new Parser.ParserClient(channel);

byte[] data = Encoding.ASCII.GetBytes("1234567890");

var reply = await client.ParseCallAsync(
    new ParseRequest { RawData = ByteString.CopyFrom(data) });

Console.WriteLine($"Success: {reply.Success}. Msg: {reply.ErrMsg}");