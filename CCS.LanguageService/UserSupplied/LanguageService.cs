/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace Babel
{
    /*
     * The Babel.LanguageService class is needed to register the VS language service.  
     * This class derives from the Babel.BabelLanguageService base class which provides all the necessary 
     * functionality for a babel-based language service.  This class can be used to override and extend that 
     * base class if necessary.
     * 
     * Note that the Babel.BabelLanguageService class derives from the Managed 
     * Package Framework's LanguageService class.
     *     
     * Of special interest is the GUID attribute that is used to uniquely identify this language service.  
     * If this code is copied for a different language service, then the GUID should be regenerated
     * so to not interfere with this sample language service's GUID.
     */

    [Guid("73DA124B-2CC5-4f79-A0DB-B11B6AAA2BE5")]
    class LanguageService : BabelLanguageService
    {
        public override string GetFormatFilterList()
        {
            return "CCS File (*.ccs)\n*.ccs";
        }

        public override Microsoft.VisualStudio.Package.AuthoringScope ParseSource(ParseRequest req) {
            Source source = (Source)this.GetSource(req.FileName);
            bool yyparseResult = false;

            // req.DirtySpan seems to be set even though no changes have occurred
            // source.IsDirty also behaves strangely
            // might be possible to use source.ChangeCount to sync instead

            if (req.DirtySpan.iStartIndex != req.DirtySpan.iEndIndex
                || req.DirtySpan.iStartLine != req.DirtySpan.iEndLine) {
                Babel.Parser.ErrorHandler handler = new Babel.Parser.ErrorHandler();
                Babel.Lexer.Scanner scanner = new Babel.Lexer.Scanner(); // string interface
                Parser.Parser parser = new Parser.Parser();  // use noarg constructor
                parser.scanner = scanner;
                scanner.Handler = handler;
                parser.SetHandler(handler);
                scanner.SetSource(req.Text, 0);

                parser.MBWInit(req);
                yyparseResult = parser.Parse();

                // store the parse results
                // source.ParseResult = aast;
                source.ParseResult = null;
                source.Braces = parser.Braces;

                // for the time being, just pull errors back from the error handler
                if (handler.ErrNum > 0) {
                    foreach (Babel.Parser.Error error in handler.SortedErrorList()) {
                        TextSpan span = new TextSpan();
                        span.iStartLine = span.iEndLine = error.line - 1;
                        span.iStartIndex = error.column;
                        span.iEndIndex = error.column + error.length;
                        req.Sink.AddError(req.FileName, error.message, span, Severity.Error);
                    }
                }
            }

            switch (req.Reason) {
                case ParseReason.Check:
                case ParseReason.HighlightBraces:
                case ParseReason.MatchBraces:
                case ParseReason.MemberSelectAndHighlightBraces:
                    // send matches to sink
                    // this should (probably?) be filtered on req.Line / col
                    if (source.Braces != null) {
                        foreach (TextSpan[] brace in source.Braces) {
                            if (brace.Length == 2)
                                req.Sink.MatchPair(brace[0], brace[1], 1);
                            else if (brace.Length >= 3)
                                req.Sink.MatchTriple(brace[0], brace[1], brace[2], 1);
                        }
                    }
                    break;
                default:
                    break;
            }

            return new CCS.LanguageService.AuthoringScope(req.Text);
        }

    }
}
