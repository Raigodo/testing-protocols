using GatheringMetrics;
using GatheringMetrics.Util;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Middleman.Util;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Runtime.Intrinsics.X86;
using System.Text;

Console.Write("Input anything to proceed: ");
Console.ReadLine();

await Payload.PreparePayload("1mb");
await SetupHelper.StartWsConnection();
using var client = new HttpClient();

while (true)
{
    Console.WriteLine("gathering metrics...");

    Console.WriteLine($"http/2 memory allocated: \t\t{await GetMemoryAllocatedByCall(MakeCallOverHttp20)}");
    //Console.Write($"http/3 memory allocated: \t\t{await GetMemoryAllocatedByCall(MakeCallOverHttp30)}\n");
    Console.WriteLine($"ws memory allocated: \t\t\t{await GetMemoryAllocatedByCall(MakeCallOverWs)}");
    Console.WriteLine($"grpc memory allocated: \t\t\t{await GetMemoryAllocatedByCall(MakeCallOverGrpc)}");

    Console.WriteLine($"http/2 wait time: \t\t\t{await GetReponseTimeOfCall(MakeCallOverHttp20)}");
    //Console.Write($"http/3 wait time: \t\t\t{await GetReponseTimeOfCall(MakeCallOverHttp30)}\n");
    Console.WriteLine($"ws wait time: \t\t\t\t{await GetReponseTimeOfCall(MakeCallOverWs)}");
    Console.WriteLine($"grpc wait time: \t\t\t{await GetReponseTimeOfCall(MakeCallOverGrpc)}");

    Console.Write("\nInput 'X' symbol to end\n"
        + "or any one from folloving to use as payload: 1kb, 10kb, 50kb, 100kb, 500kb, 1mb\n"
        + "or any other value to repeat\n->");
    string input = Console.ReadLine() ?? string.Empty;
    if (Payload.SupportedFileNames.Contains(input))
        await Payload.PreparePayload(input);
    else if (input.ToUpper() == "X")
        break;

    Console.WriteLine(string.Empty);
}


await SetupHelper.CloseWsConnection();

Console.Write("Input anything to Exit: ");
Console.ReadLine();


async Task MakeCallOverGrpc()
{
    var client = new Greeter.GreeterClient(SetupHelper.Grpc);
    var reply = await client.FooAsync(new HelloRequest { Name = Payload.CurrentPayload });
}

async Task MakeCallOverWs()
{
    var message = Payload.CurrentPayload;
    var bytes = Encoding.UTF8.GetBytes(message);
    var arraySegment = new ArraySegment<byte>(bytes);
    await SetupHelper.Ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

    var buffer = new byte[1024];
    await SetupHelper.Ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
}

async Task MakeCallOverHttp20()
{
    await MakeHttpCall(Payload.CurrentPayload, HttpVersion.Version20, SetupHelper.Http);
}

async Task MakeCallOverHttp30()
{
    await MakeHttpCall(Payload.CurrentPayload, HttpVersion.Version30, SetupHelper.Http);
}

async Task<string> MakeHttpCall(string payload, Version version, HttpClient client)
{
    using var request = new HttpRequestMessage
    {
        RequestUri = new Uri("https://localhost:5002/http"),
        Version = version,
        Method = HttpMethod.Post,
        VersionPolicy = HttpVersionPolicy.RequestVersionExact,
        Content = JsonContent.Create(payload),
    };

    using HttpResponseMessage response = await client.SendAsync(request);
    return await response.Content.ReadAsStringAsync();
}

async Task<long> GetMemoryAllocatedByCall(Func<Task> call)
{
    long sum = 0;
    int iterationCount = 5;
    for (int i = 0; i < iterationCount; i++)
    {
        GC.Collect();
        GC.TryStartNoGCRegion(1000000000L);
        try
        {
            var memoryAllocated = GC.GetTotalAllocatedBytes();

            await Task.Run(call);

            memoryAllocated = GC.GetTotalAllocatedBytes() - memoryAllocated;
            sum += memoryAllocated;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"exception: {ex.Message}");
        }
        GC.EndNoGCRegion();
        GC.Collect();
    }
    return sum / iterationCount;
}

async Task<TimeSpan> GetReponseTimeOfCall(Func<Task> call)
{
    long sum = 0;
    int iterationCount = 5;
    for (int i = 0; i < iterationCount; i++)
    {
        try
        {
            var watch = Stopwatch.StartNew();

            await Task.Run(call);

            watch.Stop();
            sum += watch.Elapsed.Ticks;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"exception: {ex.Message}");
        }
    }
    return new TimeSpan(sum / iterationCount);
}