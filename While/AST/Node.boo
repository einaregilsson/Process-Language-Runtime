namespace While.AST

import While
import System
import System.Diagnostics.SymbolStore
import System.Reflection.Emit

public abstract class Node:
"""
The base for all nodes in the abstract syntax tree.
Just contains some utility methods.
"""
	abstract def Compile(il as ILGenerator):
		pass
	
	[property(DebugWriter)]
	protected static _debugWriter as ISymbolDocumentWriter
	
	_seqPoints = [] #List of 4-tuples with startline, startcol, endline, endcol for debug
		
	def AddSequencePoint(t as Token):
		_seqPoints.Add((t.line, t.col, t.line, t.col+t.val.Length))
	
	def AddSequencePoint(arr as (int)):
		_seqPoints.Add(arr)

	def EmitDebugInfo(il as ILGenerator, index as int, addNOP as bool):
		if CompileOptions.Debug:
			sl,sc,el,ec = _seqPoints[index]
			il.MarkSequencePoint(DebugWriter, sl,sc,el,ec)
			if addNOP:
				il.Emit(OpCodes.Nop)
	
