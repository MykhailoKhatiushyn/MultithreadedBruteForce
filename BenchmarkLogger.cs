using System;
using System.Text;

namespace MultithreadedBruteForce
{
    public class BenchmarkResult
    {
        public string TargetPassword { get; set; } = string.Empty;
        public string TargetHash { get; set; } = string.Empty;
        public TimeSpan SingleThreadTime { get; set; }
        public TimeSpan MultiThreadTime { get; set; }
        public long TotalAttempts { get; set; }

        public double SpeedupRatio => MultiThreadTime.TotalMilliseconds > 0
            ? SingleThreadTime.TotalMilliseconds / MultiThreadTime.TotalMilliseconds
            : 0;

        public double SingleThreadHashRate => SingleThreadTime.TotalSeconds > 0
            ? TotalAttempts / SingleThreadTime.TotalSeconds
            : 0;

        public double MultiThreadHashRate => MultiThreadTime.TotalSeconds > 0
            ? TotalAttempts / MultiThreadTime.TotalSeconds
            : 0;
    }

    public static class BenchmarkLogger
    {
        /// <summary>
        /// Formats benchmark metrics into a structured string report for display and logging.
        /// </summary>
        public static string FormatReport(BenchmarkResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("==================================================");
            sb.AppendLine("         BRUTE FORCE BENCHMARK REPORT             ");
            sb.AppendLine("==================================================");
            sb.AppendLine($"Target Password    : {result.TargetPassword}");
            sb.AppendLine($"Target Hash (SHA256): {result.TargetHash}");
            sb.AppendLine($"Total Combinations : {result.TotalAttempts:N0}");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"Single-Thread Time : {result.SingleThreadTime.TotalMilliseconds:N2} ms ({result.SingleThreadTime.TotalSeconds:F2} s)");
            sb.AppendLine($"Single-Thread Speed: {result.SingleThreadHashRate:N0} hashes/sec");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"Multi-Thread Time  : {result.MultiThreadTime.TotalMilliseconds:N2} ms ({result.MultiThreadTime.TotalSeconds:F2} s)");
            sb.AppendLine($"Multi-Thread Speed : {result.MultiThreadHashRate:N0} hashes/sec");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"Speedup Factor     : {result.SpeedupRatio:F2}x Faster");
            sb.AppendLine("==================================================");

            return sb.ToString();
        }
    }
}
