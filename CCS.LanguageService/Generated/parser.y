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

%token INACTION PROC KWUSE METHOD FULLCLASS STRING
%token IDENTIFIER LCASEIDENT
%token KWIF KWELSE KWWHILE KWFOR KWCONTINUE KWBREAK KWRETURN
%token KWEXTERN KWSTATIC KWAUTO KWINT KWVOID 
%token NUMBER

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
    : Usings ProcDefs
    ;

Usings
    : Using Usings
    | Using error { CallHdlr("Expected use statement", @2); }
    | /* empty */
    ;

Using   
	: KWUSE FULLCLASS
	;
    
ProcDefs
    : ProcDef /* Must be at least one */
    | ProcDefs ProcDef
    | ProcDefs error { CallHdlr("Expected process definition", @2); }
    ;

ProcDef
    : PROC VariableParams '=' Process
    ;

Process
    : NonDeterministicChoice Relabel Restrict
    | '(' NonDeterministicChoice ')' Relabel Restrict { Match(@1, @3); }
    ;
    
Relabel
    : /* Empty */
    | '[' RelabelList ']' { Match(@1, @3); }
    | '[' METHOD ']' { Match(@1, @3); }
    ;    

RelabelList
    : RelabelOne
    | RelabelList ',' RelabelOne
    ;
    
RelabelOne
    : LCASEIDENT '/' LCASEIDENT
    ;
    
Restrict
    : /* Empty */
    | '\\' LCASEIDENT
    | '\\' METHOD
    | '\\' '{' IdentList '}'  { Match(@2, @4); }
    ;   

IdentList
    : LCASEIDENT
    | error    { CallHdlr("Expected channel name", @1); }
    | IdentList ',' LCASEIDENT
    | IdentList ',' error { CallHdlr("Expected channel name", @3); }
    ;
     
VariableParams
    : /* empty */
    | '(' VariableParamList ')' { Match(@1, @3); }
    ;

VariableParamList
    : LCASEIDENT
    | error    { CallHdlr("Expected variable name", @1); }
    | VariableParamList ',' LCASEIDENT
    | VariableParamList ',' error { CallHdlr("Expected variable name", @3); }
    
    ;

ExprParams
    : /* empty */
    | '(' ExprParamList ')'  { Match(@1, @3); }
    ;

ExprParamList
    : ExprParamList ',' Expr
    | ExprParamList ',' error { CallHdlr("Expected expression", @3); }
    | Expr
    | error    { CallHdlr("Expected expression", @1); }
    
    ;
MethodExprParamList
    : MethodExprParamList ',' MethodExpr
    | MethodExprParamList ',' error { CallHdlr("Expected expression", @3); }
    | MethodExpr
    | error    { CallHdlr("Expected expression", @1); }
    ;

MethodExpr
    : STRING
    | Expr
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
    : LCASEIDENT ExprParams '.' ActionPrefix
    | INACTION VariableParams '.' ActionPrefix
    | METHOD '(' MethodExprParamList ')' '.' ActionPrefix { Match(@2, @4); }
    | PROC
    | NUMBER { if (((Babel.Lexer.Scanner)this.scanner).yytext != "0") CallHdlr( "Expected 0 or a process" , @1); }
    | '(' NonDeterministicChoice ')' Relabel Restrict {  Match(@1, @3); }
    ; 


Expr
    : Factor
    | Expr '+' Factor
    | Expr '-' Factor
    | Expr '*' Factor
    | Expr '/' Factor
    | Expr '%' Factor
    ;
    
Factor
    : NUMBER
    | '(' Expr ')'  { Match(@1, @3); }
    | LCASEIDENT
    ;
    
%%



