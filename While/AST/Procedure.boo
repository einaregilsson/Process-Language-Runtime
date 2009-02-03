namespace While.AST

import System
import System.Collections.Generic
import System.Reflection
import System.Reflection.Emit
import While.AST.Expressions
import While.AST.Statements

class Procedure(Node):
	
	[getter(ValueArgs)]
	_valArgs as List[of Variable]
	[getter(Result)]
	_resultArg as Variable
	[getter(Statements)]
	_stmts as StatementSequence
	[getter(Name)]
	_name as string
	
	def constructor(name as string, valArgs as List[of Variable], resultArg as Variable, statements as StatementSequence):
		_valArgs = valArgs
		_resultArg = resultArg
		_stmts = statements
		_name = name
		
	def Compile(il as ILGenerator):
		pass

	def Compile(module as ModuleBuilder):
		argCount = _valArgs.Count
		if _resultArg: argCount += 1
		argtypes = array(Type, argCount)
		for i in range(0, argCount):
			argtypes[i] = typeof(int)
		if _resultArg:
			argtypes[-1] = typeof(int).MakeByRefType()
			
		method = module.DefineGlobalMethod(_name, MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), argtypes)
		il = method.GetILGenerator()
		_stmts.Compile(il)
		
		
		
	def ToString():
		s = "procedure ${_name}("
		if _valArgs.Count > 0:
			s += "val "
			s += _valArgs[0]
			for i in range(1, _valArgs.Count):
				s += ", " + _valArgs[i]
			if _resultArg:
				s += ", res " + _resultArg
		elif _resultArg:
			s += "res " + _resultArg
		s += ")\n"
		s += "\t" + _stmts.ToString().Replace("\n", "\n\t")
		s += "\nend;"
		return s
