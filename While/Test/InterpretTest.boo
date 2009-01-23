namespace While.Test

import While
import NUnit.Framework
import System.IO

[TestFixture]
class InterpretTest:
"""Test the abstract syntax tree interpretation"""
	
	[Test]
	def RowOfNumbers():
		Execute("""
begin
	var x;
	var y;
	x := 1;
	y := 20;
	while x < y do
		write x;
		x := x + 1-3+3
	od
end
		""", 
"""1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
"""
)
		
	[Test]
	def OuterInnerScope():
		Execute("""
				begin
					var x;
					var y;
					x := 5;
					write x;
					begin
						var x;
						write x						
					end;
					write x
				end
		""", 
"""5
0
5
""")

	[Test]
	def OutOfScope():
		try:
			Execute("x := 1","")
		except ex as WhileException:
			Assert.AreEqual("Variable x is not in scope!", ex.Message)

	[Test]
	def OutOfScope2():
		try:
			Execute("begin var x; x := y end","")
		except ex as WhileException:
			Assert.AreEqual("Variable y is not in scope!", ex.Message)

	private def Execute(src as string, exp as string):
		writer = StreamWriter(MemoryStream())
		writer.Write(src)
		writer.Flush()
		writer.BaseStream.Seek(0, SeekOrigin.Begin)
		
		p = Parser(Scanner(writer.BaseStream))
		result = StringWriter()
		While.AST.Statements.Write.TextWriter = result
		p.Parse()
		p.AbstractSyntaxTree.Execute()
		Assert.AreEqual(exp, result.ToString())
