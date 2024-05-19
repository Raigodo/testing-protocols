using GatheringMetrics.Callers;
using GatheringMetrics.Gather;
using GatheringMetrics.Util;
using GatheringMetrics.Values.Enums;

await Payload.PreparePayload("1kb");

var iterations = 20;
var cmd = string.Empty;

while (cmd != "X")
{
    Console.Write("cmd: ");
    cmd = Console.ReadLine();
    Console.WriteLine();

    switch (cmd)
    {
        case "mem":
            await GatherMemoryMetrics();
            break;
        case "time":
            await GatherTimeMetrics();
            break;
        case "cpu":
            await GatherCpuLoadMetrics();
            break;
        case "tput":
            await GatherThroughput();
            break;
        case "p":
            Console.WriteLine("c - show current payload size [B]\n or any payload size: 0kb, 1kb, 10kb, 50kb, 100kb, 500kb, 1mb\n");
            Console.Write("p / cmd:");
            var input = $"{Console.ReadLine()}";
            if (Payload.SupportedFileNames.Contains(input))
                await Payload.PreparePayload(input);
            else if (input == "c")
                Console.WriteLine($"current payload: {Payload.CurrentPayload.Length} bytes");
            break;
    }

    Console.WriteLine();
}

Console.Write("Input anything to Exit: ");
Console.ReadLine();


async Task GatherMemoryMetrics()
{
    using var controlledGatherer = new MetricsGatherer(await ControlledCall.Create());
    DisplayResults("Gathering Memory Usage...", await controlledGatherer.GatherMemoryUsageAsync(iterations));
}

async Task GatherTimeMetrics()
{
    using var controlledGatherer = new MetricsGatherer(await ControlledCall.Create());
    DisplayResults("Gathering Request Time...", await controlledGatherer.GatherWaitTimeAsync(iterations));
}

async Task GatherCpuLoadMetrics()
{
    using var controlledGatherer = new MetricsGatherer(await ControlledCall.Create());
    DisplayResults("Gathering CPU Load...", await controlledGatherer.GatherCpuLoad(iterations));
}

async Task GatherThroughput()
{
    using var controlledGatherer = new MetricsGatherer(await ControlledCall.Create());
    DisplayResults("Gathering Throughput...", await controlledGatherer.GatherThroughput());
}

void DisplayResults(string title, Dictionary<Protocols, double> results)
{
    Console.WriteLine($"\n{title}");
    Console.WriteLine($"HTTP/2:        {results[Protocols.HTTP20]}");
    Console.WriteLine($"HTTP/3:        {results[Protocols.HTTP30]}");
    Console.WriteLine($"WS:            {results[Protocols.WS]}");
    Console.WriteLine($"gRPC:          {results[Protocols.GRPC]}");
}