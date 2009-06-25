/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System.IO;
using PLR;
using PLR.AST;

namespace CCS.Parsing {
    
    public class ParserService : IParser{

        public string Language { get { return "CCS"; } }
        public string FileExtensions { get { return "ccs"; } }

        public PLR.AST.ProcessSystem Parse(string inputFile) {
            Parser p = new Parser(new Scanner(inputFile));
            StringWriter errorWriter = new StringWriter();
            p.errors.errorStream = errorWriter;
            p.Parse();
            if (p.errors.count > 0) {
                throw new ParseException(errorWriter.ToString());
            }
            return p.System;
        }

        public PLR.AST.ProcessSystem Parse(System.IO.Stream inputStream) {
            Parser p = new Parser(new Scanner(inputStream));
            p.Parse();
            return p.System;
        }
    }
}
