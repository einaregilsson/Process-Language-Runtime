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
	_empty = false

	[Property(Help)]
	_help = false

	[Property(BookVersion)]
	_bookVersion = true

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
			_bookVersion = false if larg == '/coursesyntax'
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

/coursesyntax          Use the modified syntax as I learned it in the
                       Program Analysis course taught at DTU. This means
                       that () are not used for if and while blocks, 
                       instead if and while blocks end with fi and od
                       respectively. This switch also means that all 
                       variables must be declared before use inside of
                       begin-end blocks. 
                       
                       Example:
                       
                       begin
                           var x;
                           x := 3;
                           if x < 4 then
                               write 1
                           else
                               write 2
                           fi;
                           begin
                               var y;
                               y := x;
                               while x < 20 do
                                   x := x +1;
                                   skip
                               od
                           end;
                           skip
                       end
                           
""")
		
