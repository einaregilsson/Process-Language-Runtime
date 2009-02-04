namespace While.Test

import While
import NUnit.Framework
import System.IO

[TestFixture]
class ParseTest:
"""Test the parser"""

	[Test]
	def AssignStatement():
		Parse('x := 3', '')
		
	[Test]
	def SkipStatement():
		Parse('skip', '')

	[Test]
	def ReadStatement():
		Parse('read x', '')
		
	[Test]
	def WriteStatement():
		Parse('write x', '')

	[Test]
	def WriteStatementWithExpression():
		Parse('write 25+(32-1*3)%2', '')

	[Test]
	def WhileStatement():
		Parse('while true do skip ', '')
		
	[Test]
	def WhileStatementWithExpression():
		Parse('while 1+2 < 4+2 do ( skip )', '')

	[Test]
	def IfStatement():
		Parse('if true then skip ', '')

	[Test]
	def IfStatementWithExpression():
		Parse('if 1+3*4%2 <= 3 then (skip) ', '')

	[Test]
	def IfElseStatement():
		Parse('if true then skip else skip ', '')

	[Test]
	def BlockStatement():
		Parse('begin var x; var y; skip; skip end', '')

	[Test]
	def StatementSequence():
		Parse('skip; skip; read x; begin var y; skip end; while true do skip od', '')

	private def Parse(src as string, expectedOutput as string):
		writer = StreamWriter(MemoryStream())
		writer.Write(src)
		writer.Flush()
		writer.BaseStream.Seek(0, SeekOrigin.Begin)
		
		p = Parser(Scanner(writer.BaseStream))
		result = StringWriter()
		p.errors.errorStream = result
		p.Parse()
		Assert.AreEqual(expectedOutput, result.ToString())
