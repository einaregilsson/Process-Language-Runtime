namespace While

import System

static class CompileOptions:
	[Property(Debug)]
	_debug = false

	[Property(OutputFilename)]
	_outputFile = ""

	[Property(InputFilename)]
	_inputFile = ""
	
	[Property(Empty)]
	_empty = true

	[Property(Help)]
	_help = false

	def Init(args as (string)):
		if args.Length == 0:
			_empty = true
			return
		
		_inputFile = args[-1]
		_outputFile = /(?i)\.w(hile)?$/.Replace(_inputFile,"")
		for arg as string in args:
			larg = arg.ToLower()
			_help = larg in ('/?', '/help')
			_debug = true if larg == '/debug'
			if larg.StartsWith("/out:"):
				_outputFile = larg.Substring(5)
			
		if not _outputFile.ToLower().EndsWith(".exe"):
			_outputFile += ".exe"

	def Print():
		System.Console.Error.WriteLine("""
            Compiler Options:
            	
/? or /help            Print this help message
/out:<filename>        Specify the name of the compiled executable
/debug                 Include debug information in compiled file
""")
		
