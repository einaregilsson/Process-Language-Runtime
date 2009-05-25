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
%token IDENTIFIER NUMBER  LCASEIDENT
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
    : ProcDefs ProcDef
    | ProcDef /* Must be at least one */
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
    ;    

RelabelList
    : RelabelList LCASEIDENT '/' LCASEIDENT
    | LCASEIDENT '/' LCASEIDENT
    ;
    
Restrict
    : /* Empty */
    | '\\' LCASEIDENT
    | '\\' '{' IdentList '}'  { Match(@2, @4); }
    ;   

IdentList
    : IdentList ',' LCASEIDENT
    | IdentList ',' error { CallHdlr("Expected channel name", @3); }
    | LCASEIDENT
    | error    { CallHdlr("Expected channel name", @1); }
    ;
     
VariableParams
    : /* empty */
    | '(' VariableParamList ')' { Match(@1, @3); }
    ;

VariableParamList
    : VariableParamList ',' LCASEIDENT
    | VariableParamList ',' error { CallHdlr("Expected variable name", @3); }
    | LCASEIDENT
    | error    { CallHdlr("Expected variable name", @1); }
    
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
    | PROC
    | '0'
    | '(' NonDeterministicChoice ')' Relabel Restrict {  Match(@1, @3); }
    ; 


Expr
    : Expr '+' Factor
    | Expr '-' Factor
    | Expr '*' Factor
    | Expr '/' Factor
    | Factor
    ;
    
Factor
    : NUMBER
    | '(' Expr ')'  { Match(@1, @3); }
    | LCASEIDENT
    ;
    
%%



