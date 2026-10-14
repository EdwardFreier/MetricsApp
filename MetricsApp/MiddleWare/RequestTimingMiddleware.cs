using System.Collections.Concurrent;
using System.Diagnostics;
using MetricsApp.Metrics;

namespace MetricsApp.Middleware
{
    public class RequestTimingMiddleware
    {
        //Class variables
        private readonly RequestDelegate _next;
        private static readonly ConcurrentQueue<RequestMetric> _lastMetrics = new();
        private const int MaxItems = 20;



        //constructor
        public RequestTimingMiddleware(RequestDelegate next) => _next = next;


        //asynchronus task
        public async Task InvokeAsync(HttpContext context)
        {
            //make a stopwatch
            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var metric = new RequestMetric(
                    Path: context.Request.Path.ToString(),
                    Method: context.Request.Method,
                    StatusCode: context.Response?.StatusCode,
                    ElapsedMilliseconds: sw.ElapsedMilliseconds,
                    Utc: DateTime.UtcNow
                    );

                Console.WriteLine($"[METRIC] {metric.Method} {metric.Path} -> {metric.StatusCode} in {metric.ElapsedMilliseconds}");

                _lastMetrics.Enqueue(metric);
                while (_lastMetrics.Count > MaxItems && _lastMetrics.TryDequeue(out _)) { }

            }
        }


        //helper method
        public static IReadOnlyList<RequestMetric> Snapshot()
            => _lastMetrics.Reverse().ToList();
    }
}