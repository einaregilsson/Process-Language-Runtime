namespace While.Test

import While
import While.AST
import NUnit.Framework
import System.IO

[TestFixture]
class ASTTest:
"""Test the abstract syntax tree"""

	[Test]
	def AssignStatement():
		Parse('x := 3', 'x := 3', true)
		
	[Test]
	def SkipStatement():
		Parse('skip', 'skip', true)

	[Test]
	def ReadStatement():
		Parse('read x', 'read x', true)
		
	[Test]
	def WriteStatement():
		Parse('write x', 'write x', true)

	[Test]
	def WriteStatementWithExpression():
		Parse('write 25+(32-1*3)%2', 'write (25 + ((32 - (1 * 3)) % 2))', true)

	[Test]
	def WhileStatement():
		Parse('while true do skip od', """
while true do
	skip
od
		""", false)
		
	[Test]
	def WhileStatementWithExpression():
		Parse('while 1+2 < 4+2 do skip od', """
while ((1 + 2) < (4 + 2)) do
	skip
od
		""",false)

	[Test]
	def IfStatement():
		Parse('if true then skip fi', """
if true then
	skip
fi
		""",false)

	[Test]
	def IfStatementWithExpression():
		Parse('if 1+3*4%2 <= 3 then skip fi', """
if ((1 + ((3 * 4) % 2)) <= 3) then
	skip
fi
		""",false)

	[Test]
	def IfElseStatement():
		Parse('if true then skip else skip fi', """
if true then
	skip
else
	skip
fi
		""",false)

	[Test]
	def BlockStatement():
		Parse('begin var x; var y; skip; skip end', """
begin
	var x;
	var y;
	
	skip;
	skip
end
		""",false)

	[Test]
	def StatementSequence():
		Parse('skip; skip; read x; begin var y; skip end; while true do skip od', """
skip;
skip;
read x;
begin
	var y;
	
	skip
end;
while true do
	skip
od
		""",false)
		
	private def Parse(src as string, expAst as string, bookVersion as bool):
		writer = StreamWriter(MemoryStream())
		writer.Write(src)
		writer.Flush()
		writer.BaseStream.Seek(0, SeekOrigin.Begin)
		CompileOptions.BookVersion = bookVersion
		p = Parser(Scanner(writer.BaseStream))
		result = StringWriter()
		p.errors.errorStream = result
		p.Parse()
		Assert.AreEqual("", result.ToString())
		Assert.AreEqual(expAst.Trim().Replace("\r",""), WhileTree.Instance.ToString().Trim())
