using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CCS.Parsing;
using PLR.AST;
using PLR.AST.Processes;
using CCS.Formatters;

namespace CCS {
    class Program {

        static int Main(string[] args) {
            Compiler compiler = new Compiler();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            return compiler.Compile(args[0]);
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            return null;
        }
    }
}
