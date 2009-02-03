namespace While.AST

import While
import While.AST.Statements
import System
import System.Reflection
import System.Reflection.Emit
import System.Diagnostics.SymbolStore
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

	def Compile(filename):
		name = AssemblyName(Name:filename)
		assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Save)
		module = assembly.DefineDynamicModule(filename, CompileOptions.Debug)

		method = module.DefineGlobalMethod("WhileMain",MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), array(Type,0))
		il = method.GetILGenerator()		
		if CompileOptions.Debug:
			Node.DebugWriter = module.DefineDocument(CompileOptions.InputFilename, Guid.Empty, Guid.Empty, SymDocumentType.Text)
		_stmts.Compile(il)
		il.Emit(OpCodes.Ret)
		module.CreateGlobalFunctions()
		assembly.SetEntryPoint(method, PEFileKinds.ConsoleApplication)
		if CompileOptions.Debug:
	       module.SetUserEntryPoint(method)
		
		assembly.Save(filename)
	
