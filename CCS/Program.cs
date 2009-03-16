using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CCS.Parsing;
using PLR.AST;
using PLR.AST.Processes;
using CCS.Formatters;
using System.Reflection;
using System.Security.Cryptography;

namespace CCS {
    class Program {

        static int Main(string[] args) {
            Compiler compiler = new Compiler();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            return compiler.Compile(args[0]);
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("CCS.PLR.dll");
            byte[] buf = new byte[s.Length];
            s.Read(buf, 0, buf.Length);
            Console.WriteLine(Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(buf)));
            Console.WriteLine("Loaded from self");
            return Assembly.Load(buf);
        }
    }
}
