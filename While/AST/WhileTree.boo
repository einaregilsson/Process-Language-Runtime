namespace While.AST

import While
import While.AST.Statements
import System
import System.Reflection
import System.Reflection.Emit
import System.Diagnostics.SymbolStore
import System.Threading
import System.Collections.Generic

class WhileTree:
	
	[getter(Statements)]
	_stmts as StatementSequence
	
	[getter(Procedures)]
	_procs as Dictionary[of string, Procedure]

	def constructor(stmts as StatementSequence, procs as Dictionary[of string, Procedure]):
		_stmts = stmts
		_procs = procs
		System.Console.Write(ToString())
		
	def ToString():
		return join(_procs.Values, ";\n") + "\n\n" + _stmts.ToString()

	def Compile(filename):
		name = AssemblyName(Name:filename)
		assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Save)
		module = assembly.DefineDynamicModule(filename, CompileOptions.Debug)

		method = module.DefineGlobalMethod("WhileMain", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), array(Type,0))
		il = method.GetILGenerator()		
		if CompileOptions.Debug:
			Node.DebugWriter = module.DefineDocument(CompileOptions.InputFilename, Guid.Empty, Guid.Empty, SymDocumentType.Text)
		
		for proc as Procedure in _procs.Values:
			proc.Compile(module)
			
		_stmts.Compile(il)
		il.Emit(OpCodes.Ret)
		module.CreateGlobalFunctions()
		assembly.SetEntryPoint(method, PEFileKinds.ConsoleApplication)
		if CompileOptions.Debug:
	       module.SetUserEntryPoint(method)
		
		assembly.Save(filename)
	
