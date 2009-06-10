/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using CCS.Parsing;
using PLR.AST;
using PLR.AST.Processes;
using PLR.AST.Actions;
using PLR.AST.Expressions;
using PLR.AST.Formatters;

namespace CCS.Tests
{
    [TestFixture]
    public class FormatterTest
    {
        private ProcessSystem Parse(string source)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter w = new StreamWriter(ms);
            w.Write(source);
            w.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            Parser p = new Parser(new Scanner(ms));
            p.Parse();
            return p.System;
        }

        [Test]
        public void TestSourceFormatter()
        {
            string src = @"
System_{n,k} = a . b . c . d . 0 + f . 0 | John
John = 0";
            ProcessSystem sys = Parse(src);
            BaseFormatter f = new BaseFormatter();

            f.Start(sys);
            string result = f.GetFormatted();
            Assert.AreEqual(src, result);
        }

    }
}
