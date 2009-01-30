namespace While.Test

import While
import NUnit.Framework
import System.IO

[TestFixture]
class CompileTest:
"""Test the abstract syntax tree"""

	[Test]
	def AssignStatement():
		Compile('write 4 <= 3')
		
	private def Compile(src as string):
		writer = StreamWriter(MemoryStream())
		writer.Write(src)
		writer.Flush()
		writer.BaseStream.Seek(0, SeekOrigin.Begin)
		
		p = Parser(Scanner(writer.BaseStream))
		result = StringWriter()
		p.errors.errorStream = result
		p.Parse()
		p.AbstractSyntaxTree.Compile("test.exe")
