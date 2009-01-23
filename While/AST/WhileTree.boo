namespace While.AST

import While.AST.Statements
import System

class WhileTree(Node):
	
	[getter(Statements)]
	_stmts as StatementSequence
	
	def constructor(stmts as StatementSequence):
		_stmts = stmts
		
	def ToString():
		return _stmts.ToString()

	def Execute():
		_stmts.Execute()
