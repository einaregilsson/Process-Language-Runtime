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

%token OUTACTION PROC METHOD FULLCLASS STRING
%token IDENTIFIER LCASEIDENT
%token KWIF KWELSE KWTHEN KWUSE KWAND KWOR KWXOR KWTRUE KWFALSE
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
    : Usings Using 
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
    : NonDeterministicChoice 
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
    | IdentList ',' LCASEIDENT
    | IdentList ',' error { CallHdlr("Expected channel name", @3); }
    ;
     
VariableParams
    : /* empty */
    | '(' VariableParamList ')' { Match(@1, @3); }
    ;

VariableParamList
    : LCASEIDENT
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
    ;

MethodExprParamList
    : MethodExprParamList ',' MethodExpr
    | MethodExprParamList ',' error { CallHdlr("Expected expression", @3); }
    | MethodExpr
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
    : LCASEIDENT VariableParams '.' ActionPrefix
    | OUTACTION ExprParams '.' ActionPrefix
    | METHOD '(' MethodExprParamList ')' '.' ActionPrefix { Match(@2, @4); }
    | PROC Relabel Restrict
    | KWIF Expr KWTHEN Process KWELSE Process
    | NUMBER { if (((Babel.Lexer.Scanner)this.scanner).yytext != "0") CallHdlr( "Expected 0 or a process" , @1); }
      Relabel Restrict 
    | '(' NonDeterministicChoice ')' Relabel Restrict {  Match(@1, @3); }
    ; 


Expr
    : OrTerm
    | Expr KWOR OrTerm
    ;
 

OrTerm
    : AndTerm
    | OrTerm KWAND AndTerm
    ;


AndTerm
    : RelationalTerm
    | AndTerm KWXOR RelationalTerm
    ;

RelationalTerm
    : ArithmeticExpression
    | ArithmeticExpression EQ ArithmeticExpression
    | ArithmeticExpression NEQ ArithmeticExpression
    | ArithmeticExpression GT ArithmeticExpression
    | ArithmeticExpression GTE ArithmeticExpression
    | ArithmeticExpression LT ArithmeticExpression
    | ArithmeticExpression LTE ArithmeticExpression
    ;

ArithmeticExpression
    : PlusMinusTerm
    | ArithmeticExpression '+' PlusMinusTerm
    | ArithmeticExpression '-' PlusMinusTerm
    ;

PlusMinusTerm
    : UnaryMinusTerm
    | PlusMinusTerm '*' UnaryMinusTerm
    | PlusMinusTerm '/' UnaryMinusTerm
    | PlusMinusTerm '%' UnaryMinusTerm
    ;

UnaryMinusTerm
    : '-' UnaryMinusTerm
    | NUMBER
    | '(' Expr ')'  { Match(@1, @3); }
    | LCASEIDENT
    | KWTRUE
    | KWFALSE
    ;
    
%%



