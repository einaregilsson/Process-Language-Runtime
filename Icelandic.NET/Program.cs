using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Icelandic {
    class Program {
        static void Main(string[] args) {

            string ice = @"s verður b.
";
            MemoryStream ms = new MemoryStream();
            StreamWriter s = new StreamWriter(ms);
            s.Write(ice);
            s.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            Parser p = new Parser(new Scanner(ms));
            p.Parse();
            Console.ReadLine();
        }
    }
}
