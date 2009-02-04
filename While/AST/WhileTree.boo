namespace While.AST

import While
import While.AST.Statements
import System
import System.Reflection
import System.Reflection.Emit
import System.Diagnostics.SymbolStore
import System.Threading
import System.Collections.Generic

class WhileTree(Node):
	
	[getter(Statements)]
	_stmts as StatementSequence
	
	
	[getter(Procedures)]
	_procs as Dictionary[of string, Procedure]
	
	[getter(CompiledProcedures)]
	_compiledProcs = Dictionary[of string, MethodBuilder]()
	
	[property(Instance)]
	static _tree as WhileTree

	def constructor(stmts as StatementSequence, procs as Dictionary[of string, Procedure]):
		_stmts = stmts
		_procs = procs
		
	def ToString():
		return join(_procs.Values, ";\n") + "\n\n" + _stmts.ToString()

	def Compile(il as ILGenerator):
		pass
		
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

		if _seqPoints.Count > 0: #Make possible to start on "begin" statement
			EmitDebugInfo(il, 0, true)
			
		_stmts.Compile(il)
		if _seqPoints.Count > 0: #Make possible to end on "end" statement
			EmitDebugInfo(il, 1, true)
		il.Emit(OpCodes.Ret)
		

		module.CreateGlobalFunctions()
		assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication)
		if CompileOptions.Debug:
	       module.SetUserEntryPoint(mainMethod)
		
		assembly.Save(filename)
	
