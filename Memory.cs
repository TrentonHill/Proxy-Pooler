using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyPooler
{
    internal class Memory
    {
        // -----------------------
        // User32 / kernel32 / ntdll imports
        // -----------------------
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLengthA(IntPtr HWND);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextA(IntPtr HWND, StringBuilder WindowText, int nMaxCount);
        [DllImport("kernel32.dll")]
        private static extern bool IsDebuggerPresent();
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
        [DllImport("ntdll.dll", CharSet = CharSet.Auto)]
        private static extern int NtQueryInformationProcess(IntPtr hProcess, int processInformationClass, int[] processInformation, int processInformationLength, ref int returnLength);
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, uint SystemInformationLength, ref uint ReturnLength);
        [DllImport("ntdll.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern void RtlInitUnicodeString(out UNICODE_STRING DestinationString, string SourceString);
        [DllImport("ntdll.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void RtlUnicodeStringToAnsiString(out ANSI_STRING DestinationString, UNICODE_STRING UnicodeString, bool AllocateDestinationString);
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint LdrGetDllHandle([MarshalAs(UnmanagedType.LPWStr)] string DllPath, [MarshalAs(UnmanagedType.LPWStr)] string DllCharacteristics, UNICODE_STRING LibraryName, ref IntPtr DllHandle);
        [DllImport("ntdll.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern uint LdrGetProcedureAddress(IntPtr Module, ANSI_STRING ProcedureName, ushort ProcedureNumber, out IntPtr FunctionHandle);
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, [MarshalAs(UnmanagedType.U4)] MemoryProtection flNewProtect, [MarshalAs(UnmanagedType.U4)] out MemoryProtection lpflOldProtect);
        internal enum MemoryProtection { ExecuteReadWrite = 0x40 }
        private static async Task<IntPtr> LowLevelGetModuleHandle(string Library)
        {
            IntPtr DllHandle = IntPtr.Zero;
            UNICODE_STRING DestinationString = default(UNICODE_STRING);
            RtlInitUnicodeString(out DestinationString, Library);
            LdrGetDllHandle(null, null, DestinationString, ref DllHandle);
            return DllHandle;
        }
        private static async Task<IntPtr> LowLevelGetProcAddress(IntPtr hModule, string Function)
        {
            IntPtr FunctionHandle = IntPtr.Zero;
            UNICODE_STRING DestinationString = default(UNICODE_STRING);
            ANSI_STRING DestinationString2 = default(ANSI_STRING);
            RtlInitUnicodeString(out DestinationString, Function);
            RtlUnicodeStringToAnsiString(out DestinationString2, DestinationString, true);
            LdrGetProcedureAddress(hModule, DestinationString2, 0, out FunctionHandle);
            return FunctionHandle;
        }
        private unsafe static void CopyBlock(void* destination, void* source, uint byteCount) { }
        private unsafe static void InitBlock(void* startAddress, byte value, uint byteCount) { }
        // -----------------------
        // Strong Anti-Tamper & Anti-Debug
        // -----------------------
        public static async void AntiTamper() // Not working yet ;)
        {
            try
            {
                // -------- Profiler / CLR checks --------
                if (Environment.GetEnvironmentVariable("complus_profapi_profilercompatibilitysetting") != null)
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                if (string.Compare(Environment.GetEnvironmentVariable("COR_ENABLE_PROFILING"), "1", StringComparison.Ordinal) == 0)
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                // -------- Native debugger --------
                IntPtr handle = Process.GetCurrentProcess().Handle;
                int[] arr = new int[6];
                int returnLength = 0;
                if (NtQueryInformationProcess(handle, 31, arr, 4, ref returnLength) == 0 && arr[0] != 1)
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                if (NtQueryInformationProcess(handle, 30, arr, 4, ref returnLength) == 0 && arr[0] != 0)
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                if (IsDebuggerPresent())
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                bool remote = false;
                CheckRemoteDebuggerPresent(handle, ref remote);
                if (remote)
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                // -------- Kernel debugger --------
                SYSTEM_KERNEL_DEBUGGER_INFORMATION kdi = new SYSTEM_KERNEL_DEBUGGER_INFORMATION();
                uint len = 0;
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(kdi));
                NtQuerySystemInformation(35, ptr, (uint)Marshal.SizeOf(kdi), ref len); // 35 = SystemKernelDebuggerInformation
                kdi = Marshal.PtrToStructure<SYSTEM_KERNEL_DEBUGGER_INFORMATION>(ptr);
                Marshal.FreeHGlobal(ptr);
                if (kdi.KernelDebuggerEnabled || !kdi.KernelDebuggerNotPresent)
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                // -------- Window scan --------
                string[] suspiciousWindows = new string[]
                {
                    "x32dbg", "x64dbg", "windbg", "ollydbg", "dnspy", "immunity debugger", "hyperdbg", "debug", "debugger", "ghidra", "ida"
                };
                IntPtr fg = GetForegroundWindow();
                int lenText = GetWindowTextLengthA(fg);
                if (lenText > 0)
                {
                    StringBuilder sb = new StringBuilder(lenText + 1);
                    GetWindowTextA(fg, sb, lenText + 1);
                    string title = sb.ToString().ToLowerInvariant();
                    foreach (string s in suspiciousWindows)
                        if (title.Contains(s.ToLower()))
                        {
                            Colorful.Console.WriteLine("         Access Denied!", Color.White);
                            await Task.Delay(3000);
                            Environment.Exit(0);
                        }
                }
                // -------- Process scan --------
                var procNames = Process.GetProcesses().Select(p => {
                    try { return p.ProcessName.ToLowerInvariant(); } catch { return ""; }
                }).ToArray();
                string[] suspiciousProcs = new string[] { "ida64", "ida", "ollydbg", "x64dbg", "x32dbg", "dnspy", "windbg" };
                if (procNames.Intersect(suspiciousProcs).Any())
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                // -------- Timing / anti-step --------
                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < 2000000; i++) { var x = i * i; }
                sw.Stop();
                if (sw.ElapsedMilliseconds > 2000)
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                // -------- Memory / assembly integrity --------
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                byte[] hash = SHA256File(exePath);
                byte[] expectedHash = LoadExpectedHash();
                if (!hash.SequenceEqual(expectedHash))
                {
                    Colorful.Console.WriteLine("         Access Denied!", Color.White);
                    await Task.Delay(3000);
                    Environment.Exit(0);
                }
                // -------- Random micro-checks --------
                Task.Run(() =>
                {
                    Random r = new Random();
                    while (true)
                    {
                        Thread.Sleep(r.Next(3000, 7000));
                        AntiTamperMicroCheck();
                    }
                });
            }
            catch
            {
                Colorful.Console.WriteLine("         Access Denied!", Color.White);
                await Task.Delay(3000);
                Environment.Exit(0);
            }
        }
        private static void AntiTamperMicroCheck()
        {
            // Quick runtime check for hot methods / memory anomalies
            if (IsDebuggerPresent())
            {
                Colorful.Console.WriteLine("         Access Denied!", Color.White);
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
            bool remote = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref remote);
            if (remote)
            {
                Colorful.Console.WriteLine("         Access Denied!", Color.White);
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
        }
        private static byte[] SHA256File(string path)
        {
            using (var sha = SHA256.Create())
            using (var fs = File.OpenRead(path))
                return sha.ComputeHash(fs);
        }
        private static byte[] LoadExpectedHash()
        {
            // Placeholder: replace with encrypted resource or CI-provided hash
            return new byte[] { /* build-time SHA256 */ };
        }
        // -----------------------
        // Structs
        // -----------------------
        private struct CONTEXT { private uint P1Home, P2Home, P3Home, P4Home, P5Home, P6Home; private long ContextFlags; private uint Dr0, Dr1, Dr2, Dr3, Dr4, Dr5, Dr6, Dr7; }
        private struct PROCESS_MITIGATION_BINARY_SIGNATURE_POLICY { private uint MicrosoftSignedOnly; }
        [StructLayout(LayoutKind.Explicit)]
        private struct SYSTEM_CODEINTEGRITY_INFORMATION { [FieldOffset(0)] private ulong Length; [FieldOffset(4)] private uint CodeIntegrityOptions; }
        private struct PROCESS_BASIC_INFORMATION { internal IntPtr Reserved1, PebBaseAddress, Reserved2_0, Reserved2_1, UniqueProcessId, InheritedFromUniqueProcessId; }
        private struct SYSTEM_KERNEL_DEBUGGER_INFORMATION { [MarshalAs(UnmanagedType.U1)] internal bool KernelDebuggerEnabled; [MarshalAs(UnmanagedType.U1)] internal bool KernelDebuggerNotPresent; }
        public struct UNICODE_STRING { private ushort Length, MaximumLength; private IntPtr Buffer; }
        public struct ANSI_STRING { private short Length, MaximumLength; private string Buffer; }
        // -----------------------
        // Other
        // -----------------------
        public static async Task db(string content, string type)
        {
            // REDACTED
        }
        public static async Task<long> GetServerTime()
        {
            // REDACTED
        }
        public static string ComputeHmac(string data, string secret)
        {
            // REDACTED
        }
        public static string GetHWID()
        {
            // REDACTED
        }
        private static string GetWMI(string wmiClass, string wmiProperty)
        {
            // REDACTED
        }
        private static string GetMAC()
        {
            // REDACTED
        }
    }
}