namespace While.AST

import System
import System.Diagnostics.SymbolStore
import System.Reflection.Emit

public abstract class Node:
	abstract def Compile(il as ILGenerator):
		pass
	
	[property(DebugWriter)]
	protected static _debugWriter as ISymbolDocumentWriter
