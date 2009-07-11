/**
 * $Id: IParser.cs 206 2009-07-07 21:14:46Z eboeg $ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.IO;
using PLR.AST;
using PLR.AST.Formatters;

namespace PLR.Runtime {

    public interface IParser {
        string Language { get; }
        string FileExtensions { get; }
        BaseFormatter Formatter { get; }
        ProcessSystem Parse(string inputFile);
        ProcessSystem Parse(Stream inputStream);
    }


    public class ParseException : Exception {
        public ParseException(string msg)
            : base(msg) {
        }
    }
}
