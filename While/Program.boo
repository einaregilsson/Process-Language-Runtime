""" 
	While.NET compiler
	
	Compiler for the programming language While, found in the
	book Principles of Program Analysis by Nielson, Nielson and
	Hankin. Licensed under the GPL.
	
	Program author: Einar Egilsson (einar@einaregilsson.com)
"""

namespace While

import System
import System.IO
import While.AST

[STAThread]
static def Main(args as (string)):
	
	print """While.NET Compiler v0.9
Copyright (C) Einar Egilsson 2009. All rights reserved.
"""

	CompileOptions.Init(args)
	if CompileOptions.Empty:
		System.Console.Error.WriteLine("ERROR: No inputs specified")
		return 1
	elif CompileOptions.Help:
		System.Console.Error.WriteLine("Usage: wc.exe [options] filename")
		CompileOptions.Print()
		return 2
	elif not CompileOptions.ReadStdIn and not File.Exists(CompileOptions.InputFilename):
		System.Console.Error.WriteLine("ERROR: File '${CompileOptions.InputFilename}' does not exist");
		return 3

	p as Parser
	if CompileOptions.ReadStdIn:
		p = Parser(Scanner(System.Console.OpenStandardInput()))
	else:
		p = Parser(Scanner(FileStream(CompileOptions.InputFilename, FileMode.Open)))

	p.Parse()
	return if p.errors.count > 0
	VariableStack.Clear()
	WhileTree.Instance.Compile(CompileOptions.OutputFilename)
	return 0
