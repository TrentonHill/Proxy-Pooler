using dnlib.DotNet;
using dnlib.DotNet.Writer;
using NetAssembly.Protection;
using NetAssembly.Protection.FlowControl;
using NetAssembly.Protection.Proxy;
using SmokeyObfuscator.Protection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetAssembly
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                MessageBox.Show("No file path provided!", "ProxyPool Obfuscator");
                return;
            }
            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"File not found at {filePath}!", "ProxyPool Obfuscator");
                return;
            }
            ModuleDefMD module = ModuleDefMD.Load(filePath);
            Constants.Process(module);
            AddIntPhase.Execute(module);
            ControlFlowObfuscation.CtrlFlow(module);
            JumpCFlow.Execute(module);
            MutateOpCodes.Process(module);
            NumberChanger.Process(module);
            Strings.Execute(module);
            Int.Execute(module);
            SaveAssembly(module, filePath);
        }
        public static void SaveAssembly(ModuleDefMD module, string path)
        {
            string AssemblyPath = Path.GetDirectoryName(module.Location);
            if (!AssemblyPath.EndsWith("\\")) AssemblyPath += "\\";
            string savePath = AssemblyPath + Path.GetFileNameWithoutExtension(module.Location) + " V2.0" + Path.GetExtension(module.Location);
            var opts = new ModuleWriterOptions(module);
            opts.Logger = DummyLogger.NoThrowInstance;
            module.Write(savePath, opts);
            MessageBox.Show($"{path} has been obfuscated!", "ProxyPool Obfuscator");
            Environment.Exit(0);
        }
    }
}
