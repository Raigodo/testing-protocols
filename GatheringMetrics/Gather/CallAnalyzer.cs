using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GatheringMetrics.Gather;

public static class CallAnalyzer
{


    public static async Task<long> GetAvgMemoryAllocated(Func<Task> call, int iterations = 1)
    {
        long sum = 0;
        for (int i = 0; i < iterations; i++)
        {
            GC.Collect();
            GC.TryStartNoGCRegion(1000000000L);

            var memoryUsage = GC.GetTotalAllocatedBytes();
            await Task.Run(call);
            sum = GC.GetTotalAllocatedBytes() - memoryUsage;

            GC.EndNoGCRegion();
            GC.Collect();

            await Task.Delay(10);
        }
        return (long)Math.Ceiling(sum / (double)iterations);
    }



    public static async Task<long> GetAvgReponseTime(Func<Task> call, int iterations = 1)
    {
        long time = 0;
        for (int i = 0; i < iterations; i++)
        {
            var watch = Stopwatch.StartNew();
            await Task.Run(call);
            watch.Stop();
            time += watch.Elapsed.Ticks;
        }
        return (long)Math.Ceiling(time / (double)iterations);
    }



    public static async Task<double> GetAvgCpuUsage(Func<Task> call, int resultCount)
    {
        double result = 0;
        var i = 0;
        while (i < resultCount)
        {
            var got = await GatherCpuUsage(call, 10);
            if (got <= 0)
                continue;

            result += got;
            i++;
        }
        return Math.Round(result / resultCount, 1);
    }

    private static async Task<double> GatherCpuUsage(Func<Task> call, int callCount = 1)
    {
        //used to stop cpu logging process
        var cts = new CancellationTokenSource();
        var awaitReady = new TaskCompletionSource();
        double totalCpuUsage = 0;

        var job = Task.Run(() => LaunchCpuMonitoring(
            cancellationToken: cts.Token,
            onReady: () => awaitReady.SetResult(),
            onFinish: (usageSum) => totalCpuUsage = usageSum
        ));
        await awaitReady.Task;

        //make results more noticable with loop
        for (int i = 0; i < callCount; i++)
            await Task.Run(call);

        cts.Cancel();
        await job;

        return totalCpuUsage;
    }

    //Runnable only on windows
    private static async Task LaunchCpuMonitoring(CancellationToken cancellationToken, Action onReady, Action<double> onFinish)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Only Windows OS is supported to acquire CPU load");
            return;
        }
        using (Process process = Process.GetCurrentProcess())
        using (PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName))
        {
            cpuCounter.NextValue();

            onReady.Invoke();

            double totalCpuUsage = 0.0;
            float currentCpuUsage = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                currentCpuUsage = cpuCounter.NextValue() / Environment.ProcessorCount;
                totalCpuUsage += currentCpuUsage;

                await Task.Delay(0);
            }

            onFinish.Invoke(totalCpuUsage);
        }
    }

    public static async Task<int> CountCalls(Func<Task> call, int waitSeconds = 10, string targetProtocolName = "Not specified")
    {
        Console.WriteLine($"Starting counting calls for: {targetProtocolName}");
        var cts = new CancellationTokenSource();
        var count = 0;
        var job = Task.Run(async () => count = await GatherThroughput(call, cts.Token));

        for (var i = 0; i < waitSeconds; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.Write(".");
        }
        Console.Write("\n");

        cts.Cancel();

        Console.WriteLine($"Finishing counting calls for: {targetProtocolName}");

        await job;

        return count;
    }

    private static async Task<int> GatherThroughput(
        Func<Task> caller,
        CancellationToken cancellationToken,
        int agentCount = 1)
    {
        //simpler alternative, but i lke what i created
        //return await LaunchThroughputAgent(caller, cancellationToken);
        int i = 0;
        var agents = new Task<int>[agentCount];
        for (i = 0; i < agentCount; i++)
            agents[i] = Task.Run(() => LaunchThroughputAgent(caller, cancellationToken));

        var tcs = new TaskCompletionSource();
        cancellationToken.Register(tcs.SetResult);

        await tcs.Task;
        var callCounts = await Task.WhenAll(agents);

        return callCounts.Sum();
    }

    private static async Task<int> LaunchThroughputAgent(Func<Task> caller, CancellationToken cancellationToken)
    {
        int i = 0;
        while (true)
        {
            await caller();
            i++;
            if (cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Trying to stop while loop");
                break;
            }
        }
        return i;
    }


}
