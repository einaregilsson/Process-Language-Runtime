namespace While


import System
import System.IO
import While.AST

[STAThread]
static def Main(args as (string)):
	
	print """While.NET Compiler v0.9
Copyright (C) Einar Egilsson 2009. All rights reserved.
"""
	if args.Length == 0:
		System.Console.Error.WriteLine("ERROR: No inputs specified")
		return 1
	elif args.Length > 0 and args[0].ToLower() in ('/?','/help'):
		System.Console.Error.WriteLine("Usage: wc.exe [options] filename")
		System.Console.Error.WriteLine("""
            Compiler Options:
            	
/? or /help            Print this help message
/out:<filename>        Specify the name of the compiled executable
/debug                 Include debug information in compiled file
""")
		return 2
	elif not File.Exists(args[-1]):
		System.Console.Error.WriteLine("ERROR: File '${args[-1]}' does not exist");
		return 3

	inputfile = args[-1]
	filename = /(?i)\.w(hile)?$/.Replace(inputfile,"")
	
	for arg as string in args[:-1]:
		if arg.ToLower().StartsWith("/out:"):
			filename = arg.Substring(5)
	
	if not filename.ToLower().EndsWith(".exe"):
		filename += ".exe"

	p = Parser(Scanner(FileStream(inputfile, FileMode.Open)))
	p.Parse()
	return if p.errors.count > 0
	VariableStack.Clear()
	p.AbstractSyntaxTree.Compile(filename)
	return 0
