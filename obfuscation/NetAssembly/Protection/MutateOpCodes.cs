using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmokeyObfuscator.Protection
{
    internal class MutateOpCodes
    {
        private static ProtoRandom.ProtoRandom _random = new ProtoRandom.ProtoRandom(5);

        private static void CtorCallProtection(MethodDef method)
        {
            // REDACTED
        }

        private static void LdfldProtection(MethodDef method)
        {
            // REDACTED
        }

        private static void CallvirtProtection(MethodDef method)
        {
            // REDACTED
        }

        public static void Process(ModuleDefMD module)
        {
            // REDACTED
        }
    }
}
