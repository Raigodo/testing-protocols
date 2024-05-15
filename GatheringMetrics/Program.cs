using GatheringMetrics.Util;
using GatheringMetrics.Util.Callers;
using GatheringMetrics.Util.Gather;
using System.Net;
using System.Net.Http.Json;

Console.Write("Input anything to proceed: ");
Console.ReadLine();

await Payload.PreparePayload("1kb");

var iterations = 10;

while (true)
{
    using var oneshotGatherer = new MetricsGatherer(new OneshotCall());

    Console.WriteLine("\nGathering metrics for ONESHOT...");

    await oneshotGatherer.GatherMemoryUsageAsync(iterations);
    await oneshotGatherer.GatherWaitTimeAsync(iterations);


    using var controlledGatherer = new MetricsGatherer(await ControlledCall.Create());

    Console.WriteLine("\nGathering metrics for CONTROLLED...");

    await controlledGatherer.GatherMemoryUsageAsync(iterations);
    await controlledGatherer.GatherWaitTimeAsync(iterations);


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

Console.Write("Input anything to Exit: ");
Console.ReadLine();

async Task<string> PerformHttpCallAsync(HttpRequestMessage request, HttpClient client)
{
    using HttpResponseMessage response = await client.SendAsync(request);
    return await response.Content.ReadAsStringAsync();
}
async Task<string> _PerformHttpCallAsync(HttpRequestMessage request)
{
    using var client = new HttpClient();
    return await PerformHttpCallAsync(request, client);
}