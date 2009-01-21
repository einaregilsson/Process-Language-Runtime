
import System
import System.Collections.Generic
import While.AST
import While.AST.Statements
import While.AST.Expressions




public class Parser:

	public static final _EOF as int = 0
	public static final _ident as int = 1
	public static final _number as int = 2
	public static final maxT as int= 41

	private static final T = true

	private static final x = false

	private static final minErrDist = 2

	
	public scanner as Scanner

	public errors as Errors

	
	public t as Token

	// last recognized token
	public la as Token

	// lookahead token
	private errDist as int = minErrDist

	

	
	public def constructor(scanner as Scanner):
		self.scanner = scanner
		errors = Errors()
		self.InitBitset()

	
	private def SynErr(n as int):
		if errDist >= minErrDist:
			errors.SynErr(la.line, la.col, n)
		errDist = 0

	
	public def SemErr(msg as string):
		if errDist >= minErrDist:
			errors.SemErr(t.line, t.col, msg)
		errDist = 0

	
	private def Get():
		while true:
			t = la
			la = scanner.Scan()
			if la.kind <= maxT:
				errDist += 1
				break 

			la = t

	
	private def Expect(n as int):
		if la.kind == n:
			Get()
		else:
			SynErr(n)

	
	private def StartOf(s as int) as bool:
		return bitset[s][la.kind]

	
	private def ExpectWeak(n as int, follow as int):
		if la.kind == n:
			Get()
		else:
			SynErr(n)
			while not StartOf(follow):
				Get()

	
	
	private def WeakSeparator(n as int, syFol as int, repFol as int) as bool:
		kind as int = la.kind
		if kind == n:
			Get()
			return true
		elif StartOf(repFol):
			return false
		else:
			SynErr(n)
			while not ((bitset[syFol][kind] or bitset[repFol][kind]) or bitset[0][kind]):
				Get()
				kind = la.kind
			return StartOf(syFol)

	
	
	def Program():
		statements as StatementSequence
		StmtSeq(statements)

	def StmtSeq(ref statements as StatementSequence):
		stmt as Statement
		Stmt(stmt)
		while la.kind == 3:
			Get()
			Stmt(stmt)

	def Stmt(ref stmt as Statement):
		exp as Expression 
		if la.kind == 1:
			Assign(stmt)
		elif la.kind == 4:
			Get()
		elif la.kind == 8:
			Block(stmt)
		elif la.kind == 11:
			If(stmt)
		elif la.kind == 15:
			While(stmt)
		elif la.kind == 5:
			Get()
			Expect(1)
		elif la.kind == 6:
			Get()
			Expr(exp)
		else: SynErr(42)

	def Assign(ref assign as Statement):
		exp as Expression 
		Expect(1)
		Expect(10)
		Expr(exp)

	def Block(ref block as Statement):
		Expect(8)
		vars as (VariableDeclaration)
		if la.kind == 7:
			VarDec(vars)
		statements as StatementSequence 
		StmtSeq(statements)
		Expect(9)

	def If(ref ifStmt as Statement):
		ifBranch as StatementSequence; elseBranch as StatementSequence
		exp as Expression 
		foo as int
		Expect(11)
		Expr(exp)
		Expect(12)
		StmtSeq(ifBranch)
		if la.kind == 13:
			Get()
			StmtSeq(elseBranch)
		Expect(14)

	def While(ref whileStmt as Statement):
		exp as Expression; whileBranch as StatementSequence 
		Expect(15)
		Expr(exp)
		Expect(16)
		StmtSeq(whileBranch)
		Expect(17)

	def Expr(ref exp as Expression):
		LogicOr(exp)

	def VarDec(ref vars as (VariableDeclaration)):
		Expect(7)
		Expect(1)
		while la.kind == 3:
			Get()
			Expect(7)
			Expect(1)
		Expect(3)

	def LogicOr(ref exp as Expression):
		second as Expression 
		LogicAnd(exp)
		while la.kind == 18:
			Get()
			LogicAnd(second)

	def LogicAnd(ref exp as Expression):
		second as Expression 
		EqualComp(exp)
		while la.kind == 19:
			Get()
			EqualComp(second)

	def EqualComp(ref exp as Expression):
		second as Expression 
		GreatOrEqual(exp)
		if la.kind == 20 or la.kind == 21:
			if la.kind == 20:
				Get()
			else:
				Get()
			GreatOrEqual(second)

	def GreatOrEqual(ref exp as Expression):
		second as Expression 
		BitOr(exp)
		if StartOf(1):
			if la.kind == 22:
				Get()
			elif la.kind == 23:
				Get()
			elif la.kind == 24:
				Get()
			else:
				Get()
			BitOr(second)

	def BitOr(ref exp as Expression):
		second as Expression 
		BitXor(exp)
		while la.kind == 26:
			Get()
			BitXor(second)

	def BitXor(ref exp as Expression):
		second as Expression 
		BitAnd(exp)
		while la.kind == 27:
			Get()
			BitAnd(second)

	def BitAnd(ref exp as Expression):
		second as Expression 
		BitShift(exp)
		while la.kind == 28:
			Get()
			BitShift(exp)

	def BitShift(ref exp as Expression):
		second as Expression 
		PlusMinus(exp)
		while la.kind == 29 or la.kind == 30:
			if la.kind == 29:
				Get()
			else:
				Get()
			PlusMinus(second)

	def PlusMinus(ref exp as Expression):
		second as Expression 
		MulDivMod(exp)
		while la.kind == 31 or la.kind == 32:
			if la.kind == 31:
				Get()
			else:
				Get()
			MulDivMod(second)

	def MulDivMod(ref exp as Expression):
		second as Expression 
		UnaryOperator(exp)
		while la.kind == 33 or la.kind == 34 or la.kind == 35:
			if la.kind == 33:
				Get()
			elif la.kind == 34:
				Get()
			else:
				Get()
			UnaryOperator(second)

	def UnaryOperator(ref exp as Expression):
		if la.kind == 32 or la.kind == 36:
			if la.kind == 32:
				Get()
			else:
				Get()
		Terminal(exp)

	def Terminal(ref exp as Expression):
		if la.kind == 1:
			Get()
		elif la.kind == 2:
			Get()
		elif la.kind == 37:
			Get()
		elif la.kind == 38:
			Get()
		elif la.kind == 39:
			Get()
			Expr(exp)
			Expect(40)
		else: SynErr(43)


	
	public def Parse():
		la = Token()
		la.val = ''
		Get()
		Program()

		Expect(0)

	
	private bitset as List[of (bool)]

	private def InitBitset():
		bitset.Add((true ,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false))
		bitset.Add((false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,true ,true , true ,true ,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false))


// end Parser

public class Errors:

	public count = 0

	// number of errors detected
	public errorStream as System.IO.TextWriter = Console.Out

	// error messages go to this stream
	public errMsgFormat = '-- line {0} col {1}: {2}'

	// 0=line, 1=column, 2=text
	public def SynErr(line as int, col as int, n as int):
		s as string
		if n == 0: s = 'EOF expected'
		elif n == 1: s = 'ident expected'
		elif n == 2: s = 'number expected'
		elif n == 3: s = '";" expected'
		elif n == 4: s = '"skip" expected'
		elif n == 5: s = '"read" expected'
		elif n == 6: s = '"write" expected'
		elif n == 7: s = '"var" expected'
		elif n == 8: s = '"begin" expected'
		elif n == 9: s = '"end" expected'
		elif n == 10: s = '":=" expected'
		elif n == 11: s = '"if" expected'
		elif n == 12: s = '"then" expected'
		elif n == 13: s = '"else" expected'
		elif n == 14: s = '"fi" expected'
		elif n == 15: s = '"while" expected'
		elif n == 16: s = '"do" expected'
		elif n == 17: s = '"od" expected'
		elif n == 18: s = '"||" expected'
		elif n == 19: s = '"&&" expected'
		elif n == 20: s = '"==" expected'
		elif n == 21: s = '"!=" expected'
		elif n == 22: s = '"<" expected'
		elif n == 23: s = '">" expected'
		elif n == 24: s = '"<=" expected'
		elif n == 25: s = '">=" expected'
		elif n == 26: s = '"|" expected'
		elif n == 27: s = '"^" expected'
		elif n == 28: s = '"&" expected'
		elif n == 29: s = '"<<" expected'
		elif n == 30: s = '">>" expected'
		elif n == 31: s = '"+" expected'
		elif n == 32: s = '"-" expected'
		elif n == 33: s = '"*" expected'
		elif n == 34: s = '"/" expected'
		elif n == 35: s = '"%" expected'
		elif n == 36: s = '"~" expected'
		elif n == 37: s = '"true" expected'
		elif n == 38: s = '"false" expected'
		elif n == 39: s = '"(" expected'
		elif n == 40: s = '")" expected'
		elif n == 41: s = '??? expected'
		elif n == 42: s = 'invalid Stmt'
		elif n == 43: s = 'invalid Terminal'

		s = ('error ' + n)
		errorStream.WriteLine(errMsgFormat, line, col, s)
		count += 1

	
	public def SemErr(line as int, col as int, s as string):
		errorStream.WriteLine(errMsgFormat, line, col, s)
		count += 1

	
	public def SemErr(s as string):
		errorStream.WriteLine(s)
		count += 1

	
	public def Warning(line as int, col as int, s as string):
		errorStream.WriteLine(errMsgFormat, line, col, s)

	
	public def Warning(s as string):
		errorStream.WriteLine(s)

// Errors

public class FatalError(System.Exception):

	public def constructor(m as string):
		super(m)


