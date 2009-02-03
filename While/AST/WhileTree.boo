namespace While.AST

import While
import While.AST.Statements
import System
import System.Reflection
import System.Reflection.Emit
import System.Diagnostics.SymbolStore
import System.Threading
import System.Collections.Generic

static class WhileTree:
	
	[getter(Statements)]
	_stmts as StatementSequence
	
	[getter(Procedures)]
	_procs as Dictionary[of string, Procedure]
	
	[getter(TypeBuilder)]
	_typeBuilder as TypeBuilder

	def SetParseResults(stmts as StatementSequence, procs as Dictionary[of string, Procedure]):
		_stmts = stmts
		_procs = procs
		
	def ToString():
		return join(_procs.Values, ";\n") + "\n\n" + _stmts.ToString()

	def Compile(filename):
		name = AssemblyName(Name:filename)
		assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Save)
		module = assembly.DefineDynamicModule(filename, CompileOptions.Debug)

		_typeBuilder = module.DefineType("WhileProgram", TypeAttributes.Class | TypeAttributes.Public)
		method = _typeBuilder.DefineMethod("WhileMain", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), array(Type,0))
		il = method.GetILGenerator()		
		if CompileOptions.Debug:
			Node.DebugWriter = module.DefineDocument(CompileOptions.InputFilename, Guid.Empty, Guid.Empty, SymDocumentType.Text)
		
		for proc as Procedure in _procs.Values:
			proc.Compile(_typeBuilder)
			
		_stmts.Compile(il)
		il.Emit(OpCodes.Ret)
		_typeBuilder.CreateType()
		assembly.SetEntryPoint(method, PEFileKinds.ConsoleApplication)
		if CompileOptions.Debug:
	       module.SetUserEntryPoint(method)
		
		assembly.Save(filename)
	
