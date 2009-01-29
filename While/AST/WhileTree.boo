namespace While.AST

import While.AST.Statements
import System
import System.Reflection
import System.Reflection.Emit
import System.Threading

class WhileTree:
	
	[getter(Statements)]
	_stmts as StatementSequence
	
	def constructor(stmts as StatementSequence):
		_stmts = stmts
		
	def ToString():
		return _stmts.ToString()

	def Execute():
		_stmts.Execute()

	def Compile():
		name = AssemblyName(Name:"WhileProgram")
		assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Save)
		module = assembly.DefineDynamicModule("WhileProgram.exe", true)

		#Create the type that holds our main method
		type = module.DefineType("WhileType", TypeAttributes.Public | TypeAttributes.Class)
		method = type.DefineMethod("Main", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), array(Type,0))
		il = method.GetILGenerator()		
		_stmts.Compile(il)
		il.Emit(OpCodes.Ret)
		type.CreateType()

		assembly.SetEntryPoint(method, PEFileKinds.ConsoleApplication)
		assembly.Save("WhileProgram.exe")
	
