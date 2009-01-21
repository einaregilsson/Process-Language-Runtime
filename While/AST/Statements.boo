namespace While.AST.Statements

import System.Collections.Generic
import While.AST
import While.AST.Expressions

abstract class Statement(Node):
	pass

class StatementSequence(Node):
	def constructor(statements as IEnumerable[of Statement]):
		pass

class VariableDeclarationSequence(Node):
	def constructor(vars as IEnumerable[of VariableDeclaration]):
		pass


class Assign(Statement):
	[Getter(Variable)]
	_var as Variable
	[Getter(Expression)]
	_exp as IntExpression
	
	def constructor(var as Variable, exp as IntExpression):
		_var = var
		_exp = exp
		
class Skip(Statement):
	pass
	
class VariableDeclaration(Statement):
	[Getter(Variable)]
	_var as Variable

	def constructor(var as Variable):
		_var = var
		
class Write(Statement):
	[Getter(Expression)]
	_exp as Expression
	
	def constructor(exp):
		_exp = exp
			
class Read(Statement):
	[Getter(Variable)]
	_var as Variable

	def constructor(var as Variable):
		_var = var

class Block(Statement):

	[Getter(Variables)]
	_vars as VariableDeclarationSequence
	[Getter(Statements)]
	_stmts as StatementSequence
	
	def constructor(vars as VariableDeclarationSequence, stmts as StatementSequence):
		_vars = vars
		_stmts = stmts

class If(Statement):

	[Getter(Expression)]
	_exp as BoolExpression
	[Getter(IfBranch)]
	_ifBranch as StatementSequence
	[Getter(ElseBranch)]
	_elseBranch as StatementSequence
	
	def constructor(exp as BoolExpression, ifBranch as StatementSequence, elseBranch as StatementSequence):
		_exp = exp
		_ifBranch = ifBranch
		_elseBranch = elseBranch

class While(Statement):

	[Getter(Expression)]
	_exp as BoolExpression
	[Getter(Statements)]
	_statements as StatementSequence
	
	def constructor(exp as BoolExpression, statements as StatementSequence):
		_exp = exp
		_statements = statements