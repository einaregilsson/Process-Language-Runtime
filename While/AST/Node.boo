namespace While.AST

import While
import System
import System.Diagnostics.SymbolStore
import System.Reflection.Emit

public abstract class Node:
	abstract def Compile(il as ILGenerator):
		pass
	
	[property(DebugWriter)]
	protected static _debugWriter as ISymbolDocumentWriter
	
	_seqPoints = [] #List of 4-tuples with startline, startcol, endline, endcol for debug
		
	def AddSequencePoint(startline as int, startcol as int, endline as int, endcol as int):
		_seqPoints.Add((startline,startcol, endline, endcol))
	def AddSequencePoint(arr as (int)):
		_seqPoints.Add(arr)

	def EmitDebugInfo(il as ILGenerator, index as int, addNOP as bool):
		if CompileOptions.Debug:
			sl,sc,el,ec = _seqPoints[index]
			il.MarkSequencePoint(DebugWriter, sl,sc,el,ec)
			if addNOP:
				il.Emit(OpCodes.Nop)
	
