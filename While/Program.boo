namespace While


import System
import System.IO
import While.AST

[STAThread]
static def Main(args as (string)):
	p = Parser(Scanner(FileStream(args[0], FileMode.Open)))
	p.Parse()
	return if p.errors.count > 0
	VariableStack.Clear()
	p.AbstractSyntaxTree.Compile()
