#
#
#namespace WC
#
#import While.AST
#import While.AST.Statements
#import While.AST.Expressions
#import System.Collections.Generic
#import System
#
#
#
#
#public class Parser:
#
#	private static final _EOF = 0
#
#	private static final _ident = 1
#
#	private static final _number = 2
#
#	private static final maxT = 41
#
#	
#	private static final T = true
#
#	private static final x = false
#
#	private static final minErrDist = 2
#
#	
#	public scanner as Scanner
#
#	public errors as Errors
#
#	
#	public t as Token
#
#	// last recognized token
#	public la as Token
#
#	// lookahead token
#	private errDist as int = minErrDist
#
#	
#	
#	
#	public def constructor(scanner as Scanner):
#		self.scanner = scanner
#		errors = Errors()
#
#	
#	private def SynErr(n as int):
#		if errDist >= minErrDist:
#			errors.SynErr(la.line, la.col, n)
#		errDist = 0
#
#	
#	public def SemErr(msg as string):
#		if errDist >= minErrDist:
#			errors.SemErr(t.line, t.col, msg)
#		errDist = 0
#
#	
#	private def Get():
#		goto converterGeneratedName1
#		while true:
#			:converterGeneratedName1
#			break  unless 
#			t = la
#			la = scanner.Scan()
#			if la.kind <= maxT:
#				errDist += 1
#				break 
#			
#			la = t
#
#	
#	private def Expect(n as int):
#		if la.kind == n:
#			Get()
#		else:
#			SynErr(n)
#
#	
#	private def StartOf(s as int) as bool:
#		return set[s, la.kind]
#
#	
#	private def ExpectWeak(n as int, follow as int):
#		if la.kind == n:
#			Get()
#		else:
#			SynErr(n)
#			while not StartOf(follow):
#				Get()
#
#	
#	
#	private def WeakSeparator(n as int, syFol as int, repFol as int) as bool:
#		kind as int = la.kind
#		if kind == n:
#			Get()
#			return true
#		elif StartOf(repFol):
#			return false
#		else:
#			SynErr(n)
#			while not ((set[syFol, kind] or set[repFol, kind]) or set[0, kind]):
#				Get()
#				kind = la.kind
#			return StartOf(syFol)
#
#	
#	
#	private def Program():
#		statements as StatementSequence = null
#		StmtSeq(statements)
#
#	
#	private def StmtSeq(ref statements as StatementSequence):
#		statementList as var = List[of Statement]()
#		stmt as Statement = null
#		Stmt(stmt)
#		statementList.Add(stmt)
#		while la.kind == 3:
#			Get()
#			Stmt(stmt)
#			statementList.Add(stmt)
#		statements = StatementSequence(statementList)
#
#	
#	private def Stmt(ref stmt as Statement):
#		exp as Expression = null
#		stmt = null
#		converterGeneratedName2 = la.kind
#		if converterGeneratedName2 == 1:
#			Assign(stmt)
#			goto converterGeneratedName2_end
#		elif converterGeneratedName2 == 4:
#			Get()
#			stmt = Skip()
#			goto converterGeneratedName2_end
#		elif converterGeneratedName2 == 8:
#			Block(stmt)
#			goto converterGeneratedName2_end
#		elif converterGeneratedName2 == 11:
#			If(stmt)
#			goto converterGeneratedName2_end
#		elif converterGeneratedName2 == 15:
#			While(stmt)
#			goto converterGeneratedName2_end
#		elif converterGeneratedName2 == 5:
#			Get()
#			Expect(1)
#			stmt = Read(Variable(t.val))
#			goto converterGeneratedName2_end
#		elif converterGeneratedName2 == 6:
#			Get()
#			Expr(exp)
#			stmt = Write(exp)
#			goto converterGeneratedName2_end
#		else:
#			SynErr(42)
#		:converterGeneratedName2_end
#
#	
#	private def Assign(ref assign as Statement):
#		exp as Expression = null
#		Expect(1)
#		v = Variable(t.val)
#		Expect(10)
#		Expr(exp)
#		assign = Assign(v, exp)
#
#	
#	private def Block(ref block as Statement):
#		statements as StatementSequence = null
#		vars as (VariableDeclaration) = null
#		Expect(8)
#		if la.kind == 7:
#			VarDec(vars)
#		StmtSeq(statements)
#		block = Block(vars, statements)
#		Expect(9)
#
#	
#	private def If(ref ifStmt as Statement):
#		exp as Expression = null
#		ifBranch as StatementSequence = null
#		elseBranch as StatementSequence = null
#		Expect(11)
#		Expr(exp)
#		Expect(12)
#		StmtSeq(ifBranch)
#		if la.kind == 13:
#			Get()
#			StmtSeq(elseBranch)
#		Expect(14)
#		ifStmt = If(exp, ifBranch, elseBranch)
#
#	
#	private def While(ref whileStmt as Statement):
#		exp as Expression = null
#		whileBranch as StatementSequence = null
#		Expect(15)
#		Expr(exp)
#		Expect(16)
#		StmtSeq(whileBranch)
#		Expect(17)
#		whileStmt = While.AST.Statements.While(exp, whileBranch)
#
#	
#	private def Expr(ref exp as Expression):
#		LogicOr(exp)
#
#	
#	private def VarDec(ref vars as (VariableDeclaration)):
#		varList as var = List[of VariableDeclaration]()
#		Expect(7)
#		Expect(1)
#		varList.Add(VariableDeclaration(t.val))
#		while la.kind == 3:
#			Get()
#			Expect(7)
#			Expect(1)
#			varList.Add(VariableDeclaration(t.val))
#		Expect(3)
#		vars = varList.ToArray()
#
#	
#	private def LogicOr(ref exp as Expression):
#		second as Expression = null
#		LogicAnd(exp)
#		while la.kind == 18:
#			Get()
#			LogicAnd(second)
#			exp = BinaryOp(BinaryOp.LogicOr, exp, second)
#
#	
#	private def LogicAnd(ref exp as Expression):
#		second as Expression = null
#		EqualComp(exp)
#		while la.kind == 19:
#			Get()
#			EqualComp(second)
#			exp = BinaryOp(BinaryOp.LogicAnd, exp, second)
#
#	
#	private def EqualComp(ref exp as Expression):
#		second as Expression = null
#		op as string
#		GreatOrEqual(exp)
#		if (la.kind == 20) or (la.kind == 21):
#			if la.kind == 20:
#				Get()
#				op = BinaryOp.Equal
#			else:
#				Get()
#				op = BinaryOp.NotEquals
#			GreatOrEqual(second)
#			exp = BinaryOp(op, exp, second)
#
#	
#	private def GreatOrEqual(ref exp as Expression):
#		second as Expression = null
#		op as string
#		BitOr(exp)
#		if StartOf(1):
#			if la.kind == 22:
#				Get()
#				op = BinaryOp.LessThan
#			elif la.kind == 23:
#				Get()
#				op = BinaryOp.GreaterThan
#			elif la.kind == 24:
#				Get()
#				op = BinaryOp.LessThanOrEqual
#			else:
#				Get()
#				op = BinaryOp.GreaterThanOrEqual
#			BitOr(second)
#			exp = BinaryOp(op, exp, second)
#
#	
#	private def BitOr(ref exp as Expression):
#		second as Expression = null
#		BitXor(exp)
#		while la.kind == 26:
#			Get()
#			BitXor(second)
#			exp = BinaryOp(BinaryOp.BitOr, exp, second)
#
#	
#	private def BitXor(ref exp as Expression):
#		second as Expression = null
#		BitAnd(exp)
#		while la.kind == 27:
#			Get()
#			BitAnd(second)
#			exp = BinaryOp(BinaryOp.BitXor, exp, second)
#
#	
#	private def BitAnd(ref exp as Expression):
#		second as Expression = null
#		BitShift(exp)
#		while la.kind == 28:
#			Get()
#			BitShift(exp)
#			exp = BinaryOp(BinaryOp.BitAnd, exp, second)
#
#	
#	private def BitShift(ref exp as Expression):
#		second as Expression = null
#		op as string
#		PlusMinus(exp)
#		while (la.kind == 29) or (la.kind == 30):
#			if la.kind == 29:
#				Get()
#				op = BinaryOp.BitShiftLeft
#			else:
#				Get()
#				op = BinaryOp.BitShiftRight
#			PlusMinus(second)
#			exp = BinaryOp(op, exp, second)
#
#	
#	private def PlusMinus(ref exp as Expression):
#		second as Expression = null
#		op as string
#		MulDivMod(exp)
#		while (la.kind == 31) or (la.kind == 32):
#			if la.kind == 31:
#				Get()
#				op = BinaryOp.Plus
#			else:
#				Get()
#				op = BinaryOp.Minus
#			MulDivMod(second)
#			exp = BinaryOp(op, exp, second)
#
#	
#	private def MulDivMod(ref exp as Expression):
#		second as Expression = null
#		op as string
#		UnaryOperator(exp)
#		while ((la.kind == 33) or (la.kind == 34)) or (la.kind == 35):
#			if la.kind == 33:
#				Get()
#				op = BinaryOp.Multiplication
#			elif la.kind == 34:
#				Get()
#				op = BinaryOp.Division
#			else:
#				Get()
#				op = BinaryOp.Modulo
#			UnaryOperator(second)
#			exp = BinaryOp(op, exp, second)
#
#	
#	private def UnaryOperator(ref exp as Expression):
#		op as string = null
#		isUnary = false
#		if (la.kind == 32) or (la.kind == 36):
#			if la.kind == 32:
#				isUnary = true
#				Get()
#				op = UnaryOp.Minus
#			else:
#				Get()
#				op = UnaryOp.BitNegate
#		Terminal(exp)
#		if isUnary:
#			exp = UnaryOp(op, exp)
#
#	
#	private def Terminal(ref exp as Expression):
#		exp = null
#		if la.kind == 1:
#			Get()
#			exp = Variable(t.val)
#		elif la.kind == 2:
#			Get()
#			exp = Number(int.Parse(t.val))
#		elif la.kind == 37:
#			Get()
#			exp = Bool(true)
#		elif la.kind == 38:
#			Get()
#			exp = Bool(false)
#		elif la.kind == 39:
#			Get()
#			Expr(exp)
#			Expect(40)
#		else:
#			SynErr(43)
#
#	
#	
#	
#	public def Parse():
#		la = Token()
#		la.val = ''
#		Get()
#		Program()
#		
#		Expect(0)
#
#	
#	private set as (bool, 2) = ((T, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x), (x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, T, T, T, T, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x, x))
#	
#
#// end Parser
#
#public class Errors:
#
#	public count = 0
#
#	// number of errors detected
#	public errorStream as System.IO.TextWriter = Console.Out
#
#	// error messages go to this stream
#	public errMsgFormat = '-- line {0} col {1}: {2}'
#
#	// 0=line, 1=column, 2=text
#	public def SynErr(line as int, col as int, n as int):
#		s as string
#		converterGeneratedName3 = n
#		if converterGeneratedName3 == 0:
#			s = 'EOF expected'
#		elif converterGeneratedName3 == 1:
#			s = 'ident expected'
#		elif converterGeneratedName3 == 2:
#			s = 'number expected'
#		elif converterGeneratedName3 == 3:
#			s = '";" expected'
#		elif converterGeneratedName3 == 4:
#			s = '"skip" expected'
#		elif converterGeneratedName3 == 5:
#			s = '"read" expected'
#		elif converterGeneratedName3 == 6:
#			s = '"write" expected'
#		elif converterGeneratedName3 == 7:
#			s = '"var" expected'
#		elif converterGeneratedName3 == 8:
#			s = '"begin" expected'
#		elif converterGeneratedName3 == 9:
#			s = '"end" expected'
#		elif converterGeneratedName3 == 10:
#			s = '":=" expected'
#		elif converterGeneratedName3 == 11:
#			s = '"if" expected'
#		elif converterGeneratedName3 == 12:
#			s = '"then" expected'
#		elif converterGeneratedName3 == 13:
#			s = '"else" expected'
#		elif converterGeneratedName3 == 14:
#			s = '"fi" expected'
#		elif converterGeneratedName3 == 15:
#			s = '"while" expected'
#		elif converterGeneratedName3 == 16:
#			s = '"do" expected'
#		elif converterGeneratedName3 == 17:
#			s = '"od" expected'
#		elif converterGeneratedName3 == 18:
#			s = '"||" expected'
#		elif converterGeneratedName3 == 19:
#			s = '"&&" expected'
#		elif converterGeneratedName3 == 20:
#			s = '"==" expected'
#		elif converterGeneratedName3 == 21:
#			s = '"!=" expected'
#		elif converterGeneratedName3 == 22:
#			s = '"<" expected'
#		elif converterGeneratedName3 == 23:
#			s = '">" expected'
#		elif converterGeneratedName3 == 24:
#			s = '"<=" expected'
#		elif converterGeneratedName3 == 25:
#			s = '">=" expected'
#		elif converterGeneratedName3 == 26:
#			s = '"|" expected'
#		elif converterGeneratedName3 == 27:
#			s = '"^" expected'
#		elif converterGeneratedName3 == 28:
#			s = '"&" expected'
#		elif converterGeneratedName3 == 29:
#			s = '"<<" expected'
#		elif converterGeneratedName3 == 30:
#			s = '">>" expected'
#		elif converterGeneratedName3 == 31:
#			s = '"+" expected'
#		elif converterGeneratedName3 == 32:
#			s = '"-" expected'
#		elif converterGeneratedName3 == 33:
#			s = '"*" expected'
#		elif converterGeneratedName3 == 34:
#			s = '"/" expected'
#		elif converterGeneratedName3 == 35:
#			s = '"%" expected'
#		elif converterGeneratedName3 == 36:
#			s = '"~" expected'
#		elif converterGeneratedName3 == 37:
#			s = '"true" expected'
#		elif converterGeneratedName3 == 38:
#			s = '"false" expected'
#		elif converterGeneratedName3 == 39:
#			s = '"(" expected'
#		elif converterGeneratedName3 == 40:
#			s = '")" expected'
#		elif converterGeneratedName3 == 41:
#			s = '??? expected'
#		elif converterGeneratedName3 == 42:
#			s = 'invalid Stmt'
#		elif converterGeneratedName3 == 43:
#			s = 'invalid Terminal'
#		else:
#			
#			s = ('error ' + n)
#		errorStream.WriteLine(errMsgFormat, line, col, s)
#		count += 1
#
#	
#	public def SemErr(line as int, col as int, s as string):
#		errorStream.WriteLine(errMsgFormat, line, col, s)
#		count += 1
#
#	
#	public def SemErr(s as string):
#		errorStream.WriteLine(s)
#		count += 1
#
#	
#	public def Warning(line as int, col as int, s as string):
#		errorStream.WriteLine(errMsgFormat, line, col, s)
#
#	
#	public def Warning(s as string):
#		errorStream.WriteLine(s)
#
#// Errors
#
#public class FatalError(Exception):
#
#	public def constructor(m as string):
#		super(m)
#
#
