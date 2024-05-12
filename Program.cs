using System.Diagnostics;

namespace ProcessMonitor
{
    class Program
    {
        private static bool _stopMonitoring = false;

        static async Task Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: ProcessMonitor.exe <process_name> <max_lifetime_in_minutes> <monitoring_frequency_in_minutes>");
                return;
            }

            string processName = args[0];
            if (!int.TryParse(args[1], out int maxLifetimeMinutes) || !int.TryParse(args[2], out int monitoringFrequencyMinutes))
            {
                Console.WriteLine("Invalid arguments. Please provide valid numbers for max lifetime and monitoring frequency.");
                return;
            }

            string logFile = "process_monitor.log";
            Console.WriteLine($"Monitoring process '{processName}' every {monitoringFrequencyMinutes} minutes. Max lifetime is {maxLifetimeMinutes} minutes.");
            Console.WriteLine($"Press 'q' to quit.");

            Task quitListener = Task.Run(() =>
            {
                while (Console.ReadKey(true).Key != ConsoleKey.Q) ;
                _stopMonitoring = true;
            });

            while (!_stopMonitoring)
            {
                MonitorProcesses(processName, maxLifetimeMinutes, logFile);
                await Task.Delay(TimeSpan.FromMinutes(monitoringFrequencyMinutes));
            }

            Console.WriteLine("Monitoring stopped.");
        }

        private static void MonitorProcesses(string processName, int maxLifetimeMinutes, string logFile)
        {
            var processes = Process.GetProcessesByName(processName);
            var now = DateTime.Now;

            foreach (var process in processes)
            {
                TimeSpan processAge = now - process.StartTime;
                if (processAge.TotalMinutes > maxLifetimeMinutes)
                {
                    try
                    {
                        process.Kill();
                        LogProcessTermination(process, logFile, processAge);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to kill process '{processName}' with PID {process.Id}: {ex.Message}");
                    }
                }
            }
        }

        private static void LogProcessTermination(Process process, string logFile, TimeSpan processAge)
        {
            string logEntry = $"{DateTime.Now}: Terminated process '{process.ProcessName}' (PID: {process.Id}) after {processAge.TotalMinutes:F2} minutes.";
            File.AppendAllLines(logFile, new[] { logEntry });
            Console.WriteLine(logEntry);
        }
    }
}