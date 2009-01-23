

namespace While



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

	
#Node
	[Getter(AbstractSyntaxTree)]
	_ast as WhileTree


	
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

	
	def Program():
		statements as StatementSequence
		StmtSeq(statements)
		_ast = WhileTree(statements) 

	def StmtSeq(ref statements as StatementSequence):
		stmt as Statement 
		slist = List[of Statement]()
		Stmt(stmt)
		slist.Add(stmt) 
		while la.kind == 3:
			Get()
			Stmt(stmt)
			slist.Add(stmt) 
		statements = StatementSequence(slist) 

	def Stmt(ref stmt as Statement):
		exp as Expression 
		if la.kind == 1:
			AssignStmt(stmt)
		elif la.kind == 4:
			Get()
			stmt = Skip() 
		elif la.kind == 8:
			BlockStmt(stmt)
		elif la.kind == 11:
			IfStmt(stmt)
		elif la.kind == 15:
			WhileStmt(stmt)
		elif la.kind == 5:
			Get()
			Expect(1)
			stmt = Read(Variable(t.val)) 
		elif la.kind == 6:
			Get()
			Expr(exp)
			stmt = Write(exp) 
		else: SynErr(42)

	def AssignStmt(ref assign as Statement):
		exp as Expression 
		var as Variable 
		Expect(1)
		var = Variable(t.val) 
		Expect(10)
		Expr(exp)
		assign = Assign(var, exp) 

	def BlockStmt(ref block as Statement):
		Expect(8)
		vars as VariableDeclarationSequence
		if la.kind == 7:
			VarDecStmt(vars)
		statements as StatementSequence 
		StmtSeq(statements)
		Expect(9)
		block = Block(vars, statements) 

	def IfStmt(ref ifStmt as Statement):
		ifBranch as StatementSequence; 
		elseBranch as StatementSequence
		exp as Expression 
		Expect(11)
		Expr(exp)
		Expect(12)
		StmtSeq(ifBranch)
		if la.kind == 13:
			Get()
			StmtSeq(elseBranch)
		Expect(14)
		ifStmt = If(exp, ifBranch, elseBranch) 

	def WhileStmt(ref whileStmt as Statement):
		exp as Expression
		whileBranch as StatementSequence 
		Expect(15)
		Expr(exp)
		Expect(16)
		StmtSeq(whileBranch)
		Expect(17)
		whileStmt = While(exp, whileBranch) 

	def Expr(ref exp as Expression):
		LogicOr(exp)

	def VarDecStmt(ref vars as VariableDeclarationSequence):
		list = List[of VariableDeclaration]() 
		Expect(7)
		Expect(1)
		list.Add(VariableDeclaration(Variable(t.val))) 
		Expect(3)
		while la.kind == 7:
			Get()
			Expect(1)
			list.Add(VariableDeclaration(Variable(t.val))) 
			Expect(3)
		vars = VariableDeclarationSequence(list) 

	def LogicOr(ref exp as Expression):
		second as Expression 
		LogicAnd(exp)
		while la.kind == 18:
			Get()
			LogicAnd(second)
			exp = LogicBinaryOp(exp, second, LogicBinaryOp.Or) 

	def LogicAnd(ref exp as Expression):
		second as Expression 
		EqualComp(exp)
		while la.kind == 19:
			Get()
			EqualComp(second)
			exp = LogicBinaryOp(exp, second, LogicBinaryOp.And) 

	def EqualComp(ref exp as Expression):
		second as Expression 
		op as string 
		GreatOrEqual(exp)
		if la.kind == 20 or la.kind == 21:
			if la.kind == 20:
				Get()
				op = EqualityBinaryOp.Equal 
			else:
				Get()
				op = EqualityBinaryOp.NotEqual 
			GreatOrEqual(second)
			exp = EqualityBinaryOp(exp, second, op) 

	def GreatOrEqual(ref exp as Expression):
		second as Expression 
		op as string 
		BitOr(exp)
		if StartOf(1):
			if la.kind == 22:
				Get()
				op = ComparisonBinaryOp.LessThan 
			elif la.kind == 23:
				Get()
				op = ComparisonBinaryOp.GreaterThan 
			elif la.kind == 24:
				Get()
				op = ComparisonBinaryOp.LessThanOrEqual 
			else:
				Get()
				op = ComparisonBinaryOp.GreaterThanOrEqual 
			BitOr(second)
			exp = ComparisonBinaryOp(exp, second, op) 

	def BitOr(ref exp as Expression):
		second as Expression 
		BitXor(exp)
		while la.kind == 26:
			Get()
			BitXor(second)
			exp = BitBinaryOp(exp, second, BitBinaryOp.Or) 

	def BitXor(ref exp as Expression):
		second as Expression 
		BitAnd(exp)
		while la.kind == 27:
			Get()
			BitAnd(second)
			exp = BitBinaryOp(exp, second, BitBinaryOp.Xor) 

	def BitAnd(ref exp as Expression):
		second as Expression 
		BitShift(exp)
		while la.kind == 28:
			Get()
			BitShift(second)
			exp = BitBinaryOp(exp, second, BitBinaryOp.And) 

	def BitShift(ref exp as Expression):
		second as Expression
		op as string 
		PlusMinus(exp)
		while la.kind == 29 or la.kind == 30:
			if la.kind == 29:
				Get()
				op = BitBinaryOp.ShiftLeft 
			else:
				Get()
				op = BitBinaryOp.ShiftRight 
			PlusMinus(second)
			exp = BitBinaryOp(exp, second, op)

	def PlusMinus(ref exp as Expression):
		second as Expression 
		op as string
		MulDivMod(exp)
		while la.kind == 31 or la.kind == 32:
			if la.kind == 31:
				Get()
				op = ArithmeticBinaryOp.Plus 
			else:
				Get()
				op = ArithmeticBinaryOp.Minus 
			MulDivMod(second)
			exp = ArithmeticBinaryOp(exp, second, op) 

	def MulDivMod(ref exp as Expression):
		second as Expression 
		UnaryOperator(exp)
		while la.kind == 33 or la.kind == 34 or la.kind == 35:
			if la.kind == 33:
				Get()
				op = ArithmeticBinaryOp.Multiplication 
			elif la.kind == 34:
				Get()
				op = ArithmeticBinaryOp.Division 
			else:
				Get()
				op = ArithmeticBinaryOp.Modulo 
			UnaryOperator(second)
			exp = ArithmeticBinaryOp(exp, second, op) 

	def UnaryOperator(ref exp as Expression):
		op as string = null 
		if la.kind == 32 or la.kind == 36:
			if la.kind == 32:
				Get()
				op = t.val 
			else:
				Get()
				op = t.val 
		Terminal(exp)
		if op == '-': exp = MinusUnaryOp(exp) 
		elif op == '~': exp = NotUnaryOp(exp) 

	def Terminal(ref exp as Expression):
		if la.kind == 1:
			Get()
			exp = Variable(t.val) 
		elif la.kind == 2:
			Get()
			exp = Number(int.Parse(t.val)) 
		elif la.kind == 37:
			Get()
			exp = Bool(true) 
		elif la.kind == 38:
			Get()
			exp = Bool(false) 
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

	
	private bitset = List[of (bool)]()

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

		else: s = ('error ' + n)
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


