%using Microsoft.VisualStudio.TextManager.Interop
%namespace Babel.Parser
%valuetype LexValue
%partial

/* %expect 5 */


%union {
    public string str;
}


%{
    ErrorHandler handler = null;
    public void SetHandler(ErrorHandler hdlr) { handler = hdlr; }
    internal void CallHdlr(string msg, LexLocation val)
    {
        handler.AddError(msg, val.sLin, val.sCol, val.eCol - val.sCol);
    }
    internal TextSpan MkTSpan(LexLocation s) { return TextSpan(s.sLin, s.sCol, s.eLin, s.eCol); }

    internal void Match(LexLocation lh, LexLocation rh) 
    {
        DefineMatch(MkTSpan(lh), MkTSpan(rh)); 
    }
%}

%token OUTACTION INACTION PROC KWUSE METHOD FULLCLASS STRING
%token IDENTIFIER NUMBER 
%token KWIF KWELSE KWWHILE KWFOR KWCONTINUE KWBREAK KWRETURN
%token KWEXTERN KWSTATIC KWAUTO KWINT KWVOID 

  // %token ',' ';' '(' ')' '{' '}' '=' 
  // %token '+' '-' '*' '/' '!' '&' '|' '^'

//%token '='
%token EQ NEQ GT GTE LT LTE AMPAMP BARBAR
%token DOT PIPE PLUS
%token maxParseToken 
%token LEX_WHITE LEX_COMMENT LEX_ERROR

%left '*' '/'
%left '+' '-'
%%

Program
    : Usings ProcDefs Declarations 
    ;

Usings
    : Using Usings
    | Using error { CallHdlr("Expected use statement", @2); }
    | /* empty */
    ;

Using   
	: KWUSE FULLCLASS
	;
    
Declarations
    : Declaration Declarations
    | Declaration error         { CallHdlr("Expected Declaration", @2); }
    | /* empty */
    ;
    
ProcDefs
    : ProcDef ProcDefs
    | ProcDef /* Must be at least one */
    ;

ProcDef
    : PROC '=' NonDeterministicChoice
    ;


NonDeterministicChoice
    : NonDeterministicChoice '+' ParallelComposition
    | ParallelComposition
    ; 
       
ParallelComposition
    : ParallelComposition '|' ActionPrefix
    | ActionPrefix
    ; 

ActionPrefix
    : OUTACTION '.' ActionPrefix
    | PROC
    | '0'
    | '(' NonDeterministicChoice ')'
    ; 


Declaration   /* might need an init action for symtab init here */
	: Declaration_
	;

Declaration_
    : Class1 Type IDENTIFIER ParenParams Block  
    | Class1 IDENTIFIER ParenParams Block                                                                                              
    | Type IDENTIFIER ParenParams Block                                                                       
    | IDENTIFIER ParenParams Block                                                                       
    | SimpleDeclaration    
    ;


SimpleDeclarations1
    : SimpleDeclaration SimpleDeclarations1
    | SimpleDeclaration 
    ;

SimpleDeclaration
    : SemiDeclaration ';'
    | SemiDeclaration error ';'     { CallHdlr("Bad declaration, expected ';'", @2); }
    ;


SemiDeclaration
    : SemiDeclaration ',' IDENTIFIER                                    
    | Class1 Type IDENTIFIER  
    | Type IDENTIFIER  
    ;


Params1
    : Params1 ',' Type IDENTIFIER 
    | Type IDENTIFIER            
    ;

ParenParams
    :  '(' ')'                   { Match(@1, @2); }
    |  '(' Params1 ')'           { Match(@1, @3); }
    |  '(' Params1 error         { CallHdlr("unmatched parentheses", @3); }
    |  '(' error ')'             { $$ = $3;
                                   CallHdlr("error in params", @2); }
    ;


Class1
    : KWSTATIC                  
    | KWAUTO
    | KWEXTERN
    ;


Type
    : KWINT
    | KWVOID
    ;


Block
    : OpenBlock CloseBlock      { Match(@1, @2); }
    | OpenBlock BlockContent1 CloseBlock
                                { Match(@1, @3); }
    | OpenBlock BlockContent1 error 
                                { CallHdlr("missing '}'", @3); }
    | OpenBlock error CloseBlock
                                { Match(@1, @3); }
    ;

OpenBlock
    : '{'                       { /*  */ }
    ;

CloseBlock
    : '}'                       { /*  */ }
    ;

BlockContent1
    : SimpleDeclarations1 Statements1
    | SimpleDeclarations1
    | Statements1
    ;

Statements1
    : Statement Statements1
    | Statement
    ;

Statement
    : SemiStatement ';'
    | SemiStatement error ';'       { CallHdlr("expected ';'", @2); } 
  
    | KWWHILE ParenExprAlways Statement
    | KWFOR ForHeader Statement
    | KWIF ParenExprAlways Statement
    | KWIF ParenExprAlways Statement KWELSE Statement
                                { /*  */ }
    | Block
    ;

ParenExprAlways
    : ParenExpr
    | error ')'                 { CallHdlr("error in expr", @1); }
    | error                     { CallHdlr("error in expr", @1); }
    ;

ParenExpr
    : '(' Expr ')'              { Match(@1, @3); }
    | '(' Expr error            { CallHdlr("unmatched parentheses", @3); }
    ;

ForHeader
    : '(' ForBlock ')'          { Match(@1, @3); }
    | '(' ForBlock error        { CallHdlr("unmatched parentheses", @3); }
    | '(' error ')'             { Match(@1, @3); 
                                  CallHdlr("error in for", @2); }
    ;

ForBlock
    : AssignExpr ';' Expr ';' AssignExpr
    ;

SemiStatement
    : AssignExpr 
    | KWRETURN Expr 
    | KWBREAK 
    | KWCONTINUE     
    ;
    
Arguments1
    : Expr ',' Arguments1
    | Expr
    ;

ParenArguments
    : StartArg EndArg                { Match(@1, @2); } 
    | StartArg Arguments1 EndArg     { Match(@1, @3); }
    | StartArg Arguments1 error      { CallHdlr("unmatched parentheses", @3); }
    ;

StartArg
    : '('                       { /*  */ }
    ;

EndArg
    : ')'                       { /*  */ }
    ;    

AssignExpr
    : Identifier '=' Expr   
    | Expr
    ;

Expr
    : RelExpr BoolOp Expr
    | RelExpr
    | RelExpr RelExpr           { CallHdlr("error in relational expression", @2); }
    | error '}'                 { CallHdlr("unexpected symbol skipping to '}'", @1); }
    ;

BoolOp
    : AMPAMP | BARBAR 
    ;

RelExpr
    : BitExpr RelOp RelExpr
    | BitExpr
    ;

RelOp
    : GT | GTE | LT | LTE | EQ | NEQ
    ;
     
BitExpr
    : AddExpr BitOp BitExpr
    | AddExpr
    ;

BitOp
    : '|' | '&' | '^'
    ;


AddExpr
    : MulExpr AddOp AddExpr
    | MulExpr
    ;

AddOp
    : '+' | '-'
    ;


MulExpr
    : PreExpr MulOp MulExpr
    | PreExpr 
    ;

MulOp 
    : '*' | '/'
    ;

PreExpr
    : PrefixOp Factor
    | Factor
    ;

PrefixOp
    : '!' 
    ;


Factor
    : Identifier ParenArguments 
    | Identifier
    | NUMBER
    | ParenExpr
    ;     
    
Identifier
    : IDENTIFIER				  { /*  */ }
    | Identifier '.' IDENTIFIER	  { /*  */ }	
    | Identifier '.' error        { CallHdlr("expected identifier", @3); }
    ;
    
%%



