%using Babel.ParserGenerator;
%using Babel;
%using Babel.Parser;

%namespace Babel.Lexer


%x COMMENT

%{
       
       internal void LoadYylval()
       {
           yylval.str = tokTxt;
           yylloc = new LexLocation(tokLin, tokCol, tokLin, tokCol + tokLen);
       }
       
       public override void yyerror(string s, params object[] a)
       {
           if (handler != null) handler.AddError(s, tokLin, tokCol, tokLin, tokCol + tokLen);
       }
%}


White0          [ \t\r\f\v]
White           {White0}|\n

CmntStart    \#
CmntEnd      \n
ABStar       [^\*\n]*

%%

use                       { return (int)Tokens.KWUSE; }
if                        { return (int)Tokens.KWIF; }
then                      { return (int)Tokens.KWTHEN; }
else                      { return (int)Tokens.KWELSE; }
and                      { return (int)Tokens.KWAND; }
or                      { return (int)Tokens.KWOR; }
xor                      { return (int)Tokens.KWXOR; }
true                      { return (int)Tokens.KWTRUE; }
false                      { return (int)Tokens.KWFALSE; }
[A-Z][a-zA-Z0-9]*\.[A-Z][a-zA-Z0-9]*(\.[A-Z][a-zA-Z0-9]*)* { return (int)Tokens.FULLCLASS; }
[a-z]+                    { return (int)Tokens.LCASEIDENT; }
_[a-z]+_                  { return (int)Tokens.OUTACTION; }
:[a-zA-Z_0-9]+            { return (int)Tokens.METHOD; }
[A-Z][a-zA-Z0-9]*         { return (int)Tokens.PROC; }
\"[^\"]*\"                { return (int)Tokens.STRING; }
[0-9]+                    { return (int)Tokens.NUMBER; }

+                         { return (int)'+';    }
\|                         { return (int)'|';    }
\.                         { return (int)'.';    }

;                         { return (int)';';    }
,                         { return (int)',';    }
\(                        { return (int)'(';    }
\)                        { return (int)')';    }
\[                        { return (int)'[';    }
\]                        { return (int)']';    }
\{                        { return (int)'{';    }
\}                        { return (int)'}';    }
=                         { return (int)'=';    }
\^                        { return (int)'^';    }
\+                        { return (int)'+';    }
\-                        { return (int)'-';    }
\*                        { return (int)'*';    }
\/                        { return (int)'/';    }
\!                        { return (int)'!';    }
==                        { return (int)Tokens.EQ;  }
\!=                       { return (int)Tokens.NEQ;   }
\>                        { return (int)Tokens.GT; }
\>=                       { return (int)Tokens.GTE;    }
\<                        { return (int)Tokens.LT;     }
\<=                       { return (int)Tokens.LTE;    }
\&                        { return (int)'&';    }
\&\&                      { return (int)Tokens.AMPAMP; }
\|                        { return (int)'|';    }
\|\|                      { return (int)Tokens.BARBAR; }
\.                        { return (int)'.';    }
\\                        { return (int) '\\'; }

{CmntStart}{ABStar}\**{CmntEnd} { return (int)Tokens.LEX_COMMENT; } 
{CmntStart}{ABStar}\**          { BEGIN(COMMENT); return (int)Tokens.LEX_COMMENT; }
<COMMENT>\n                     |                                
<COMMENT>{ABStar}\**            { return (int)Tokens.LEX_COMMENT; }                                
<COMMENT>{ABStar}\**{CmntEnd}   { BEGIN(INITIAL); return (int)Tokens.LEX_COMMENT; }

{White0}+                  { return (int)Tokens.LEX_WHITE; }
\n                         { return (int)Tokens.LEX_WHITE; }
.                          { yyerror("illegal char");
                             return (int)Tokens.LEX_ERROR; }

%{
                      LoadYylval();
%}

%%

/* .... */
