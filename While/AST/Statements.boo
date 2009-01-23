namespace While.AST.Statements

import While.AST
import While.AST.Expressions

abstract class Statement(Node):
	
	protected def Indent(str):
		return "\t" + str.ToString().Replace("\n", "\n\t")
	
	abstract def Execute():
		pass
	

class StatementSequence(Node):
	_statements as Statement*
	def constructor(statements as Statement*):
		_statements = statements
	
	def ToString():
		return join(_statements, ";\n")
	
	def Execute():
		for s in _statements:
			s.Execute()
	
class VariableDeclarationSequence(Node):
	_vars as VariableDeclaration*

	def constructor(vars as VariableDeclaration*):
		_vars = vars

	def ToString():
		return join(_vars, ";\n") + ";\n"
	
	def Execute():
		for vd in _vars:
			vd.Execute()

class Assign(Statement):
	[Getter(Variable)]
	_var as Variable
	[Getter(Expression)]
	_exp as IntExpression
	
	def constructor(var as Variable, exp as IntExpression):
		_var = var
		_exp = exp
	
	def ToString():
		return "${_var} := ${_exp}"
	
	def Execute():
		VariableStack.AssignValue(_var.Name, _exp.IntValue)
		
class Skip(Statement):
	def ToString():
		return "skip"

	def Execute():
		pass
		
class VariableDeclaration(Statement):
	[Getter(Variable)]
	_var as Variable

	def constructor(var as Variable):
		_var = var

	def ToString():
		return "var ${_var}"

	def Execute():
		VariableStack.DefineVariable(_var.Name)
		
class Write(Statement):
	[Getter(Expression)]
	_exp as Expression
	
	[property(TextWriter)]
	private static _writer = System.Console.Out
	
	def constructor(exp):
		_exp = exp

	def ToString():
		return "write ${_exp}"

	def Execute():
		_writer.WriteLine(_exp.Value)
			
class Read(Statement):
	[Getter(Variable)]
	_var as Variable

	def constructor(var as Variable):
		_var = var
	
	def ToString():
		return "read ${_var}"

	def Execute():
		System.Console.Write(_var.Name + ": ")
		VariableStack.AssignValue(_var.Name, int.Parse(System.Console.ReadLine()))

class Block(Statement):

	[Getter(Variables)]
	_vars as VariableDeclarationSequence
	[Getter(Statements)]
	_stmts as StatementSequence
	
	def constructor(vars as VariableDeclarationSequence, stmts as StatementSequence):
		_vars = vars
		_stmts = stmts
	
	def ToString():
		return "begin\n${Indent(_vars)}\n${Indent(_stmts)}\nend"
		
	def Execute():
		VariableStack.PushScope()
		_vars.Execute()
		_stmts.Execute()
		VariableStack.PopScope()

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
		
	def ToString():
		if _elseBranch:
			return "if ${_exp} then\n${Indent(_ifBranch)}\nelse\n${Indent(_elseBranch)}\nfi"
		else:
			return "if ${_exp} then\n${Indent(_ifBranch)}\nfi"
	
	def Execute():
		if _exp.BoolValue:
			_ifBranch.Execute()
		elif _elseBranch:
			_elseBranch.Execute()

class While(Statement):

	[Getter(Expression)]
	_exp as BoolExpression
	[Getter(Statements)]
	_statements as StatementSequence
	
	def constructor(exp as BoolExpression, statements as StatementSequence):
		_exp = exp
		_statements = statements

	def ToString():
		return "while ${_exp} do\n${Indent(_statements)}\nod"
		
	def Execute():
		while _exp.BoolValue:
			_statements.Execute()
