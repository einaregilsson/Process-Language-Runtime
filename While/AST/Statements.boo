namespace While.AST.Statements
"""
This module contains all AST nodes that are
statements.
"""
import While
import While.AST
import While.AST.Expressions
import System.Reflection.Emit
import System.Collections.Generic

abstract class Statement(Node):
"""Base for all statements"""
	protected def Indent(str):
		return "\t" + str.ToString().Replace("\n", "\n\t")
	

class StatementSequence(Statement):
"""List of statements"""
	_statements as Statement*
	def constructor(statements as Statement*):
		_statements = statements
	
	def ToString():
		return join(_statements, ";\n")
	
	def Compile(il as ILGenerator):
		if _seqPoints.Count > 1: #If there's just one then we assume it is after the sequence (for "fi" and "od")
			EmitDebugInfo(il, 0, true)
		for s in _statements:
			s.Compile(il)
		if _seqPoints.Count > 0:
			EmitDebugInfo(il, _seqPoints.Count-1, true)
		
			

class VariableDeclarationSequence(Node):
"""List of variable declarations"""
	_vars as VariableDeclaration*

	def constructor(vars as VariableDeclaration*):
		_vars = vars

	def ToString():
		return join(_vars, ";\n") + ";\n"
	
	def Compile(il as ILGenerator):
		for v in _vars:
			v.Compile(il)
		
class Assign(Statement):
"""Assign an integer expression to a variable"""
	[Getter(Variable)]
	_var as Variable
	[Getter(Expression)]
	_exp as IntExpression
	
	def constructor(var as Variable, exp as IntExpression):
		_var = var
		_exp = exp
	
	def ToString():
		return "${_var} := ${_exp}"
	
	def Compile(il as ILGenerator):
		EmitDebugInfo(il,0, false)

		#Declare at first use
		if CompileOptions.BookVersion and not VariableStack.IsInScope(_var.Name):
			VariableStack.DefineVariable(_var.Name)
			lb = il.DeclareLocal(typeof(int))
			if CompileOptions.Debug:
				lb.SetLocalSymInfo(_var.Name)

		if VariableStack.IsResultArgument(_var.Name):
			il.Emit(OpCodes.Ldarg, VariableStack.GetValue(_var.Name))
			_exp.Compile(il)
			il.Emit(OpCodes.Stind_I4)
		elif VariableStack.IsArgument(_var.Name):
			_exp.Compile(il)
			il.Emit(OpCodes.Starg, VariableStack.GetValue(_var.Name))
		else:
			_exp.Compile(il)
			il.Emit(OpCodes.Stloc, VariableStack.GetValue(_var.Name))
			
class Skip(Statement):
"""No effect"""
	def ToString():
		return "skip"

	def Compile(il as ILGenerator):
		EmitDebugInfo(il,0,true)
		#Nop only emitted in debug build, otherwise nothing is emitted
		
class Call(Statement):
"""Procedure call"""
	[getter(Expressions)]
	_expr as List[of Expression]
	[getter(ProcedureName)]
	_name as string

	[getter(CallToken)]
	_callToken as Token
	[getter(LastArgumentToken)]
	_lastArgToken as Token
	
	def constructor(name as string, expressions as List[of Expression], callToken, lastArgToken):
		_expr = expressions
		_name = name
		_callToken = callToken
		_lastArgToken = lastArgToken
		
	def ToString():
		return "call ${_name}(${join(_expr, ', ')})"

	def SanityCheck():
		l,c = _callToken.line, _callToken.col
		
		if not WhileTree.Instance.Procedures.ContainsKey(ProcedureName):
			System.Console.Error.WriteLine("(${l},${c}) ERROR: Procedure '${_name}' is not defined")
			System.Environment.Exit(1)
			
		proc = WhileTree.Instance.Procedures[ProcedureName]
		if _expr.Count != proc.ArgumentCount:
			System.Console.Error.WriteLine("(${l},${c}) ERROR: Procedure '${_name}' does not take ${_expr.Count} arguments")
			System.Environment.Exit(1)

		if proc.ResultArg and not _expr[_expr.Count-1] isa Variable:
			System.Console.Error.WriteLine("(${_lastArgToken.line},${_lastArgToken.col}) ERROR: Only variables are allowed for result arguments")
			System.Environment.Exit(1)

	def Compile(il as ILGenerator):
		EmitDebugInfo(il,0,false)
		SanityCheck()		
		if _expr.Count > 0:
			for i in range(0, _expr.Count-1):
				_expr[i].Compile(il)
			proc = WhileTree.Instance.Procedures[ProcedureName]
			if proc.ResultArg:
				v as Variable = cast(Variable,_expr[_expr.Count-1])
				#Create at first use
				if CompileOptions.BookVersion and not VariableStack.IsInScope(v.Name):
					VariableStack.DefineVariable(v.Name)
					lb = il.DeclareLocal(typeof(int))
					if CompileOptions.Debug:
						lb.SetLocalSymInfo(v.Name)
	
				if VariableStack.IsResultArgument(v.Name):
					il.Emit(OpCodes.Ldarg, VariableStack.GetValue(v.Name))
				elif VariableStack.IsArgument(v.Name):
					il.Emit(OpCodes.Ldarga, VariableStack.GetValue(v.Name))
				else:
					il.Emit(OpCodes.Ldloca, VariableStack.GetValue(v.Name))
					
			else:
				_expr[_expr.Count-1].Compile(il)
			
		il.Emit(OpCodes.Call, WhileTree.Instance.CompiledProcedures[_name])
		

class VariableDeclaration(Statement):
"""Declare a variable (only with /CourseSyntax switch)"""
	[Getter(Variable)]
	_var as Variable

	def constructor(var as Variable):
		_var = var

	def ToString():
		return "var ${_var}"

	def Compile(il as ILGenerator):
		EmitDebugInfo(il,0, true)
		VariableStack.DefineVariable(_var.Name);
		lb = il.DeclareLocal(typeof(int))
		if CompileOptions.Debug:
			lb.SetLocalSymInfo(_var.Name)
		
		
class Write(Statement):
"""Write an expression to the screen"""
	[Getter(Expression)]
	_exp as Expression
	
	[property(TextWriter)]
	private static _writer = System.Console.Out
	
	def constructor(exp):
		_exp = exp

	def ToString():
		return "write ${_exp}"

	def Compile(il as ILGenerator):
		EmitDebugInfo(il,0,false)
		_exp.Compile(il)
		if _exp isa BoolExpression:
			mi = typeof(System.Console).GetMethod("WriteLine", (typeof(bool),))
		elif _exp isa IntExpression:
			mi = typeof(System.Console).GetMethod("WriteLine", (typeof(int),))
		il.Emit(OpCodes.Call, mi)
			
class Read(Statement):
"""Read an integer from the user"""
	[Getter(Variable)]
	_var as Variable

	def constructor(var as Variable):
		_var = var
	
	def ToString():
		return "read ${_var}"

	def Compile(il as ILGenerator):
		EmitDebugInfo(il,0,false)
		startLabel = il.DefineLabel()
		il.MarkLabel(startLabel)
		il.Emit(OpCodes.Ldstr, "${_var.Name}: ");
		il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Write", (typeof(string),)))
		il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadLine"))

		if CompileOptions.BookVersion and not VariableStack.IsInScope(_var.Name):
			VariableStack.DefineVariable(_var.Name)
			lb = il.DeclareLocal(typeof(int))
			if CompileOptions.Debug:
				lb.SetLocalSymInfo(_var.Name)

		if VariableStack.IsResultArgument(_var.Name):
			il.Emit(OpCodes.Ldarg, VariableStack.GetValue(_var.Name))
		elif VariableStack.IsArgument(_var.Name):
			il.Emit(OpCodes.Ldarga_S, VariableStack.GetValue(_var.Name))
		else:
			il.Emit(OpCodes.Ldloca_S, VariableStack.GetValue(_var.Name))
			
		il.Emit(OpCodes.Call, typeof(int).GetMethod("TryParse", (typeof(string),typeof(int).MakeByRefType())))
		il.Emit(OpCodes.Brfalse, startLabel)

class Block(Statement):
"""Block with variable declarations and statements"""
	[Getter(Variables)]
	_vars as VariableDeclarationSequence
	[Getter(Statements)]
	_stmts as StatementSequence
	
	def constructor(vars as VariableDeclarationSequence, stmts as StatementSequence):
		_vars = vars
		_stmts = stmts
	
	def ToString():
		return "begin\n${Indent(_vars)}\n${Indent(_stmts)}\nend"
		
	def Compile(il as ILGenerator):
		VariableStack.PushScope()
		il.BeginScope()
		EmitDebugInfo(il,0,true)
		if CompileOptions.Debug:
			il.Emit(OpCodes.Nop) #To step correctly
		_vars.Compile(il) if _vars
		_stmts.Compile(il)
		il.EndScope()
		VariableStack.PopScope()
		EmitDebugInfo(il, 1, true)


class If(Statement):
"""If-Else branching"""
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
	
	def Compile(il as ILGenerator):
		EmitDebugInfo(il,0,false)
		_exp.Compile(il)
		ifFalseLabel = il.DefineLabel()
		endLabel = il.DefineLabel()
		il.Emit(OpCodes.Brfalse, ifFalseLabel)
		_ifBranch.Compile(il)
		il.Emit(OpCodes.Br, endLabel)		
		il.MarkLabel(ifFalseLabel)
		_elseBranch.Compile(il) if _elseBranch
		il.MarkLabel(endLabel)

class While(Statement):
"""While loop"""
	[Getter(Expression)]
	_exp as BoolExpression
	[Getter(Statements)]
	_statements as StatementSequence
	
	def constructor(exp as BoolExpression, statements as StatementSequence):
		_exp = exp
		_statements = statements

	def ToString():
		return "while ${_exp} do\n${Indent(_statements)}\nod"
		
	def Compile(il as ILGenerator):
		EmitDebugInfo(il,0,false)
	
		evalConditionLabel = il.DefineLabel()
		afterTheLoopLabel = il.DefineLabel()
		il.MarkLabel(evalConditionLabel)
		_exp.Compile(il)
		il.Emit(OpCodes.Brfalse, afterTheLoopLabel)
		_statements.Compile(il)
		il.Emit(OpCodes.Br, evalConditionLabel)		
		il.MarkLabel(afterTheLoopLabel)
