using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAssembly.Protection
{
    class ControlFlowObfuscation
    {
        public static void CtrlFlow(ModuleDefMD module)
        {
            // REDACTED
        }
        private static void ExecuteMethod(MethodDef method)
        {
            // REDACTED
        }
        private static readonly Random Rnd = new Random();

        private static List<Protection.ControlFlow.Block> Randomize(List<Protection.ControlFlow.Block> input)
        {
            var ret = new List<Protection.ControlFlow.Block>();
            // REDACTED
            return ret;
        }

        private static List<Instruction> Calc(int value)
        {
            var instructions = new List<Instruction> { Instruction.Create(OpCodes.Ldc_I4, value) };
            return instructions;
        }

        public void AddJump(IList<Instruction> instrs, Instruction target)
        {
            instrs.Add(Instruction.Create(OpCodes.Br, target));
        }
    }
}
