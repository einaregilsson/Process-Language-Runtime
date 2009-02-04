namespace While.Test

import While
import While.AST
import NUnit.Framework
import System.IO
import System.Diagnostics

[TestFixture]
class CompileTest:
"""Test the abstract syntax tree"""

	[Test]
	def WriteStatement():
		Compile('write 4 <= 3', "False")
		

	[Test]
	def Stack2():
		Compile("""
begin
	proc t1(val b, res v) is
		
		call t2(b);
		write b;
		call t2(v)
		
	end;
	proc t2(res v) is
		v := 23
	end;
	
	call t1(x, x);
	write x
end
""",
"""23
23
""")

	private def Compile(src as string, expected as string):
		writer = StreamWriter(MemoryStream())
		writer.Write(src)
		writer.Flush()
		writer.BaseStream.Seek(0, SeekOrigin.Begin)
		
		p = Parser(Scanner(writer.BaseStream))
		result = StringWriter()
		p.errors.errorStream = result
		p.Parse()
		WhileTree.Instance.Compile("test.exe")
		pr = Process()
		pr.StartInfo.UseShellExecute = false
		pr.StartInfo.RedirectStandardOutput = true
		pr.StartInfo.FileName = "test.exe"
		pr.Start()
		output = pr.StandardOutput.ReadToEnd()
		pr.WaitForExit()
		File.Delete("test.exe")
		Assert.AreEqual(expected.Trim(), output.Trim())
		
