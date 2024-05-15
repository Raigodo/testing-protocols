using System.Diagnostics;

namespace GatheringMetrics.Util.Gather;

public static class CallAnalyzer
{


    public static async Task<long> GetMemoryAllocated(Func<Task> call, int iterations = 1)
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
        }
        return (long)Math.Ceiling(sum / (double)iterations);
    }



    public static async Task<long> GetReponseTime(Func<Task> call, int iterations = 1)
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
}
