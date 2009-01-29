namespace While.AST.Statements

import While.AST
import While.AST.Expressions
import System.Reflection.Emit

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
	
	def Compile(il as ILGenerator):
		for s in _statements:
			s.Compile(il)

class VariableDeclarationSequence(Node):
	_vars as VariableDeclaration*

	def constructor(vars as VariableDeclaration*):
		_vars = vars

	def ToString():
		return join(_vars, ";\n") + ";\n"
	
	def Execute():
		for vd in _vars:
			vd.Execute()

	def Compile(il as ILGenerator):
		for v in _vars:
			v.Compile(il)
		
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

	def Compile(il as ILGenerator):
		_exp.Compile(il)
		il.Emit(OpCodes.Stloc, VariableStack.GetValue(_var.Name))
		
class Skip(Statement):
	def ToString():
		return "skip"

	def Execute():
		pass

	def Compile(il as ILGenerator):
		il.Emit(OpCodes.Nop)
		
class VariableDeclaration(Statement):
	[Getter(Variable)]
	_var as Variable

	def constructor(var as Variable):
		_var = var

	def ToString():
		return "var ${_var}"

	def Execute():
		VariableStack.DefineVariable(_var.Name, false)

	def Compile(il as ILGenerator):
		VariableStack.DefineVariable(_var.Name, true);
		lb = il.DeclareLocal(typeof(int))
		lb.SetLocalSymInfo(_var.Name)
		
		
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

	def Compile(il as ILGenerator):
		_exp.Compile(il)
		if _exp isa BoolExpression:
			mi = typeof(System.Console).GetMethod("WriteLine", (typeof(bool),))
		elif _exp isa IntExpression:
			mi = typeof(System.Console).GetMethod("WriteLine", (typeof(int),))
		il.Emit(OpCodes.Call, mi)
			
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

	def Compile(il as ILGenerator):
		il.Emit(OpCodes.Ldstr, "${_var.Name}: ");
		il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Write", (typeof(string),)))
		il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadLine"))
		il.Emit(OpCodes.Call, typeof(int).GetMethod("Parse", (typeof(string),)))
		il.Emit(OpCodes.Stloc, VariableStack.GetValue(_var.Name))

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

	def Compile(il as ILGenerator):
		VariableStack.PushScope()
		il.BeginScope()
		_vars.Compile(il)
		_stmts.Compile(il)
		il.EndScope()
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

	def Compile(il as ILGenerator):
		pass

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

	def Compile(il as ILGenerator):
		pass
