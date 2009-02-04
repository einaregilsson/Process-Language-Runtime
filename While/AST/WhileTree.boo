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
	
	[getter(CompiledProcedures)]
	_compiledProcs = Dictionary[of string, MethodBuilder]()


	def SetParseResults(stmts as StatementSequence, procs as Dictionary[of string, Procedure]):
		_stmts = stmts
		_procs = procs
		
	def ToString():
		return join(_procs.Values, ";\n") + "\n\n" + _stmts.ToString()

	def Compile(filename):
		name = AssemblyName(Name:filename)
		assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave)
		module = assembly.DefineDynamicModule(filename, CompileOptions.Debug)

		mainMethod = module.DefineGlobalMethod("Main", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), array(Type,0))
		
		if CompileOptions.Debug:
			Node.DebugWriter = module.DefineDocument(CompileOptions.InputFilename, Guid.Empty, Guid.Empty, SymDocumentType.Text)
		
		#First compile the signatures
		for proc as Procedure in _procs.Values:
			method = proc.CompileSignature(module)
			_compiledProcs.Add(proc.Name, method)
		
		for proc as Procedure in _procs.Values:
			method = _compiledProcs[proc.Name]
			proc.Compile(method.GetILGenerator())
			
		il = mainMethod.GetILGenerator()		
		if CompileOptions.BookVersion:
			VariableStack.PushScope()
		_stmts.Compile(il)
		il.Emit(OpCodes.Ret)
		module.CreateGlobalFunctions()
		assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication)
		if CompileOptions.Debug:
	       module.SetUserEntryPoint(mainMethod)
		
		assembly.Save(filename)
	
