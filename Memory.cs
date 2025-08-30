using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyPooler
{
    internal class Memory
    {
        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQueryInformationProcess(IntPtr hProcess, int processInformationClass, ref uint processInformation, int processInformationLength, ref uint returnLength);
        private static bool isBackgroundCheckRunning = false;

        public static async void AntiTamper()
        {
            try
            {
                // 1. Basic Debugger Detection
                if (IsDebuggerPresent())
                {
                    LogAndExit("Access Denied. Debugger Detected!");
                    return;
                }
                // 2. Remote Debugger Detection
                bool remoteDebugger = false;
                CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remoteDebugger);
                if (remoteDebugger)
                {
                    LogAndExit("Access Denied. Remote Debugger Detected!");
                    return;
                }
                // 2. Debugger on Port Detection
                uint debugPort = 0;
                uint returnLength = 0;
                if (NtQueryInformationProcess(Process.GetCurrentProcess().Handle, 7, ref debugPort, sizeof(uint), ref returnLength) == 0 && debugPort != 0)
                {
                    LogAndExit("Access Denied. Debugger on Port Detected!");
                    return;
                }
                // 3. Process Monitoring
                string[] suspiciousProcesses = { "ollydbg", "x64dbg", "cheatengine", "ida", "wireshark", "fiddler", "processhacker", "dnSpy", "ILSpy", "Ghidra", "ModHog", "de4dot", "deobfuscator", "decrypter" };
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (suspiciousProcesses.Any(sp => process.ProcessName.ToLower().Contains(sp)))
                        {
                            LogAndExit($"Access Denied. Suspicious Process Detected!");
                            return;
                        }
                    }
                    catch { }
                }
                // 6. Virtual Machine/Sandbox Detection
                if (IsRunningInVirtualMachine())
                {
                    LogAndExit("Access Denied. Virtual Machine or Sandbox Detected!");
                    return;
                }
                // 7. Periodic Background Checks
                if (isBackgroundCheckRunning == false)
                {
                    Task.Run(async () =>
                    {
                        isBackgroundCheckRunning = true;
                        Random r = new Random();
                        while (true)
                        {
                            isBackgroundCheckRunning = true;
                            await Task.Delay(r.Next(3000, 5000));
                            // 1. Basic Debugger Detection
                            if (IsDebuggerPresent())
                            {
                                LogAndExit("Access Denied. Debugger Detected!");
                            }
                            // 2. Remote Debugger Detection
                            bool remoteDebugger2 = false;
                            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remoteDebugger2);
                            if (remoteDebugger2)
                            {
                                LogAndExit("Access Denied. Remote Debugger Detected!");
                            }
                            // 2. Debugger on Port Detection
                            uint debugPort2 = 0;
                            uint returnLength2 = 0;
                            if (NtQueryInformationProcess(Process.GetCurrentProcess().Handle, 7, ref debugPort2, sizeof(uint), ref returnLength2) == 0 && debugPort2 != 0)
                            {
                                LogAndExit("Access Denied. Debugger on Port Detected!");
                                return;
                            }
                            // 3. Process Monitoring
                            string[] suspiciousProcesses2 = { "ollydbg", "x64dbg", "cheatengine", "ida", "wireshark", "fiddler", "processhacker", "dnSpy", "ILSpy", "Ghidra", "ModHog", "de4dot", "deobfuscator", "decrypter" };
                            foreach (var process2 in Process.GetProcesses())
                            {
                                try
                                {
                                    if (suspiciousProcesses2.Any(sp => process2.ProcessName.ToLower().Contains(sp)))
                                    {
                                        LogAndExit($"Access Denied. Suspicious Process Detected!");
                                        return;
                                    }
                                }
                                catch { }
                            }
                            // 6. Virtual Machine/Sandbox Detection
                            if (IsRunningInVirtualMachine())
                            {
                                LogAndExit("Access Denied. Virtual Machine or Sandbox Detected!");
                                return;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogAndExit($"Access Denied. Anti-Tamper Error!");
            }
        }

        private static void LogAndExit(string message)
        {
            lock (Program.consoleLock)
            {
                Colorful.Console.Title = "Safeguarding your virtual vessel is the key to a secure voyage.";
                Colorful.Console.Clear();
                Program.Logo();
                Colorful.Console.WriteLine($"         {message}", Color.White);
            }
            Task.Delay(3000).Wait();
            Environment.Exit(0);
        }

        private static bool IsRunningInVirtualMachine()
        {
            try
            {
                // Check for common VM indicators
                string[] vmIndicators = { "virtualbox", "vmware", "qemu", "hyper-v", "parallels" };
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString().ToLower();
                        string model = obj["Model"]?.ToString().ToLower();
                        if (vmIndicators.Any(vm => manufacturer.Contains(vm) || model.Contains(vm)))
                        {
                            return true;
                        }
                    }
                }

                // Check for virtual disk signatures
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string model = obj["Model"]?.ToString().ToLower();
                        if (vmIndicators.Any(vm => model.Contains(vm)))
                        {
                            return true;
                        }
                    }
                }

                // Check CPUID for hypervisor presence
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string cpuName = obj["Name"]?.ToString().ToLower();
                        if (cpuName.Contains("virtual") || cpuName.Contains("kvm"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public static async Task<string> GetIP()
        {
            return null; // REDACTED
        }

        public static async Task SendLog(string content)
        {
            // REDACTED
        }

        public static string ComputeHmac(string data, string secret)
        {
            return null; // REDACTED
        }

        public static string GetHWID()
        {
            return null; // REDACTED
        }

        private static string GetWMI(string wmiClass, string wmiProperty)
        {
            return null; // REDACTED
        }

        private static string GetMAC()
        {
            return null; // REDACTED
        }

        public static string[] links = new string[]
        {
            "" // REDACTED
        };
    }
}