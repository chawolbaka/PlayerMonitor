using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayerMonitor.Modes
{
    public class Watchcat
    {
        private CancellationTokenSource token = new CancellationTokenSource();
        public async void Start(int waitTime, byte maxOverflow, double cpuUsage)
        {
            if (cpuUsage > 100)
                throw new ArgumentOutOfRangeException(nameof(cpuUsage), "CPU Usage cannot over 100%");

            int OverflowCount = 0;
            while (!token.IsCancellationRequested)
            {
                double CpuUsage = await GetCpuUsageForProcess();
                if (CpuUsage >= cpuUsage)
                {
                    if (++OverflowCount >= maxOverflow)
                        Program.Exit("killed by Watchcat.", true, -1);

                    //发现过高的CPU使用率后以每秒1次的速度去检查CPU使用率，如果一直那么高就自杀。
                    await Task.Delay(500);
                }
                else
                {
                    OverflowCount = 0;
                    await Task.Delay(waitTime);
                }
            }
        }
        public void Stop()
        {
            token.Cancel();
        }

        private async Task<double> GetCpuUsageForProcess()
        {
            //By: https://medium.com/@jackwild/getting-cpu-usage-in-net-core-7ef825831b8b
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }
    }
}
