using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Diagnostics;
using System.Reflection;

namespace ProcessMonitor.Tests
{
    [TestFixture]
    public class MonitorTests
    {
        private const string LogFile = "test_process_monitor.log";

        [SetUp]
        public void SetUp()
        {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }

        [Test]
        public void TestProcessTerminationLogging()
        {
            var process = StartTestProcess("notepad");

            var monitorMethod = typeof(Program).GetMethod("MonitorProcesses", BindingFlags.Static | BindingFlags.NonPublic);
            monitorMethod?.Invoke(null, new object[] { "notepad", 0, LogFile });

            Assert.That(process.HasExited, Is.True);

            var logEntries = File.ReadAllLines(LogFile);
            Assert.That(logEntries.Length, Is.EqualTo(1));
            StringAssert.Contains("Terminated process 'notepad'", logEntries[0]);
        }

        [Test]
        public void TestProcessNotTerminatedIfWithinLifetime()
        {
            var process = StartTestProcess("notepad");

            var monitorMethod = typeof(Program).GetMethod("MonitorProcesses", BindingFlags.Static | BindingFlags.NonPublic);
            monitorMethod?.Invoke(null, new object[] { "notepad", 10, LogFile });

            Assert.That(process.HasExited, Is.False);

            Assert.That(File.Exists(LogFile), Is.False);

        }

        private Process StartTestProcess(string processName)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processName,
                    UseShellExecute = true
                }
            };
            process.Start();
            Thread.Sleep(1000);
            return process;
        }
    }
}