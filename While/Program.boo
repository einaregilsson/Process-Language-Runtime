namespace While


import System
import System.IO

[STAThread]
static def Main(args as (string)):
	p = Parser(Scanner(FileStream(args[0], FileMode.Open)))
	p.Parse()
	return if p.errors.count > 0
	
	p.AbstractSyntaxTree.Compile()