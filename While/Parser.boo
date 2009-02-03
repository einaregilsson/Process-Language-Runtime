

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
	public static final maxT as int= 48

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
	
	def ExpectBool(exp as Expression, t as Token, isRightHandSide as bool):
		if not exp isa BoolExpression:
			if isRightHandSide:
				errors.SemErr(t.line, t.col, "'${t.val}' expects a boolean expression on its right side")
			else:
				errors.SemErr(t.line, t.col, "'${t.val}' expects a boolean expression on its left side")
			return false
		return true
	
	def ExpectInt(exp as Expression, t as Token, isRightHandSide as bool):
		if not exp isa IntExpression:
			if isRightHandSide:
				errors.SemErr(t.line, t.col, "'${t.val}' expects a integer expression on its right side")
			else:
				errors.SemErr(t.line, t.col, "'${t.val}' expects a integer expression on its left side")
			return false
		return true
	
	def IsStartOfResultArg() as bool:
		t as Token = scanner.Peek()
		scanner.ResetPeek()
		return t.val == "res"
	
	

	
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
		procs = Dictionary[of string, Procedure]() 
		while la.kind == 3:
			Proc(procs)
		StmtSeq(statements)
		_ast = WhileTree(statements, procs) 

	def Proc(procs as Dictionary[of string, Procedure]):
		statements as StatementSequence
		name as string 
		valArgs = List[of Variable]()
		resultArg as Variable 
		Expect(3)
		ptok = t 
		Expect(1)
		name = t.val 
		Expect(4)
		if la.kind == 5 or la.kind == 6:
			if la.kind == 5:
				Get()
				Expect(1)
				valArgs.Add(Variable(t.val))
				VariableStack.DefineArgument(t.val) 
				if la.kind == 11:
					Args(valArgs, resultArg)
			else:
				Get()
				Expect(1)
				resultArg = Variable(t.val)
				VariableStack.DefineArgument(t.val)
		Expect(7)
		Expect(8)
		StmtSeq(statements)
		Expect(9)
		Expect(10)
		if procs.ContainsKey(name):
			errors.SemErr(ptok.line, ptok.col, "Procedure '${name}' is already declared")
		else:
			procs.Add(name, Procedure(name, valArgs, resultArg, statements))
		VariableStack.Clear()
		

	def StmtSeq(ref statements as StatementSequence):
		stmt as Statement
		slist = List[of Statement]()
		Stmt(stmt)
		slist.Add(stmt) 
		while la.kind == 10:
			Get()
			Stmt(stmt)
			slist.Add(stmt) 
		statements = StatementSequence(slist) 

	def Args(valArgs as List[of Variable], ref resultArg as Variable):
		if IsStartOfResultArg():
			Expect(11)
			Expect(6)
			Expect(1)
			resultArg = Variable(t.val)
			if VariableStack.IsDeclaredInCurrentScope(t.val):
				errors.SemErr(t.line, t.col, "Argument '${t.val}' is already declared in this scope")
			else:
				VariableStack.DefineArgument(t.val)
			
		elif la.kind == 11:
			Get()
			Expect(1)
			valArgs.Add(Variable(t.val))
			if VariableStack.IsDeclaredInCurrentScope(t.val):
				errors.SemErr(t.line, t.col, "Argument '${t.val}' is already declared in this scope")
			else:
				VariableStack.DefineArgument(t.val)
			
			if la.kind == 11:
				Args(valArgs, resultArg)
		else: SynErr(49)

	def Stmt(ref stmt as Statement):
		exp as Expression
		sl,sc = la.line, la.col 
		if la.kind == 1:
			AssignStmt(stmt)
			stmt.AddSequencePoint(sl,sc, t.line,t.col+t.val.Length) 
		elif la.kind == 12:
			Get()
			stmt = Skip();stmt.AddSequencePoint(t.line,t.col, t.line,t.col+t.val.Length) 
		elif la.kind == 16:
			BlockStmt(stmt)
		elif la.kind == 18:
			IfStmt(stmt)
		elif la.kind == 22:
			WhileStmt(stmt)
		elif la.kind == 13:
			Get()
			Expect(1)
			stmt = Read(Variable(t.val)); stmt.AddSequencePoint(sl,sc, t.line,t.col+t.val.Length)
		elif la.kind == 14:
			Get()
			Expr(exp)
			stmt = Write(exp); stmt.AddSequencePoint(sl,sc, t.line,t.col+t.val.Length) 
		else: SynErr(50)

	def AssignStmt(ref assign as Statement):
		exp as Expression
		var as Variable 
		Expect(1)
		var = Variable(t.val)
		if not VariableStack.IsInScope(t.val):
			errors.SemErr(t.line, t.col, "Assignment to undeclared variable '${t.val}'") 
		Expect(17)
		tok = t 
		Expr(exp)
		return unless ExpectInt(exp, tok, true) 
		assign = Assign(var, exp) 

	def BlockStmt(ref block as Statement):
		Expect(16)
		vars as VariableDeclarationSequence
		VariableStack.PushScope()
		sl,sc,el,ec = t.line,t.col,t.line,t.col+t.val.Length 
		if la.kind == 15:
			VarDecStmt(vars)
		statements as StatementSequence 
		StmtSeq(statements)
		Expect(9)
		block = Block(vars, statements)
		block.AddSequencePoint(sl,sc,el,ec)
		block.AddSequencePoint(t.line,t.col, t.line, t.col+t.val.Length)
		VariableStack.PopScope() 

	def IfStmt(ref ifStmt as Statement):
		ifBranch as StatementSequence;
		elseBranch as StatementSequence
		exp as Expression 
		Expect(18)
		sl,sc,tok = t.line, t.col, t 
		Expr(exp)
		return unless ExpectBool(exp, tok, true) 
		Expect(19)
		el,ec = t.line, t.col+t.val.Length 
		StmtSeq(ifBranch)
		if la.kind == 20:
			Get()
			StmtSeq(elseBranch)
		Expect(21)
		ifStmt = If(exp, ifBranch, elseBranch)
		ifStmt.AddSequencePoint(sl,sc,el,ec)
		ifStmt.AddSequencePoint(t.line,t.col, t.line, t.col+t.val.Length)

	def WhileStmt(ref whileStmt as Statement):
		exp as Expression
		whileBranch as StatementSequence 
		Expect(22)
		sl,sc,tok = t.line, t.col,t 
		Expr(exp)
		return unless ExpectBool(exp, tok, true) 
		Expect(23)
		el,ec = t.line, t.col+t.val.Length 
		StmtSeq(whileBranch)
		Expect(24)
		whileStmt = While(exp, whileBranch)
		whileStmt.AddSequencePoint(sl,sc,el,ec)
		whileStmt.AddSequencePoint(t.line,t.col, t.line, t.col+t.val.Length)

	def Expr(ref exp as Expression):
		LogicOr(exp)

	def VarDecStmt(ref vars as VariableDeclarationSequence):
		list = List[of VariableDeclaration]() 
		VarDec(list)
		while la.kind == 15:
			VarDec(list)
		vars = VariableDeclarationSequence(list) 

	def VarDec(list as List[of VariableDeclaration]):
		Expect(15)
		sl,sc,el,ec = t.line,t.col,la.line,la.col+la.val.Length 
		Expect(1)
		if VariableStack.IsDeclaredInCurrentScope(t.val):
			errors.SemErr(t.line, t.col, "Variable '${t.val}' is already declared in this scope") 
		elif VariableStack.IsInScope(t.val):
			errors.Warning(t.line, t.col, "Variable '${t.val}' hides variable with same name in outer block")
			VariableStack.DefineVariable(t.val)
		else:
			VariableStack.DefineVariable(t.val) 
		vd = VariableDeclaration(Variable(t.val))
		vd.AddSequencePoint(sl,sc,el,ec)
		list.Add(vd) 
		Expect(10)

	def LogicOr(ref exp as Expression):
		second as Expression 
		LogicAnd(exp)
		while la.kind == 25:
			Get()
			tok = t 
			LogicAnd(second)
			return unless ExpectBool(exp, tok, false)
			return unless ExpectBool(second, tok, true) 
			exp = LogicBinaryOp(exp, second, LogicBinaryOp.Or) 

	def LogicAnd(ref exp as Expression):
		second as Expression 
		LogicXor(exp)
		while la.kind == 26:
			Get()
			tok = t 
			LogicXor(second)
			return unless ExpectBool(exp, tok, false)
			return unless ExpectBool(second, tok, true) 
			exp = LogicBinaryOp(exp, second, LogicBinaryOp.And) 

	def LogicXor(ref exp as Expression):
		second as Expression 
		Comparison(exp)
		while la.kind == 27:
			Get()
			tok = t 
			Comparison(second)
			return unless ExpectBool(exp, tok, false)
			return unless ExpectBool(second, tok, true) 
			exp = LogicBinaryOp(exp, second, LogicBinaryOp.Xor) 

	def Comparison(ref exp as Expression):
		second as Expression
		op as string 
		BitOr(exp)
		if StartOf(1):
			if la.kind == 28:
				Get()
				op = ComparisonBinaryOp.LessThan 
			elif la.kind == 29:
				Get()
				op = ComparisonBinaryOp.GreaterThan 
			elif la.kind == 30:
				Get()
				op = ComparisonBinaryOp.LessThanOrEqual 
			elif la.kind == 31:
				Get()
				op = ComparisonBinaryOp.GreaterThanOrEqual 
			elif la.kind == 32:
				Get()
				op = ComparisonBinaryOp.Equal
			else:
				Get()
				op = ComparisonBinaryOp.NotEqual 
			tok = t 
			Comparison(second)
			return unless ExpectInt(exp, tok, false)
			return unless ExpectInt(second, tok, true) 
			exp = ComparisonBinaryOp(exp, second, op) 

	def BitOr(ref exp as Expression):
		second as Expression 
		BitXor(exp)
		while la.kind == 34:
			Get()
			tok = t 
			BitXor(second)
			return unless ExpectInt(exp, tok, false)
			return unless ExpectInt(second, tok, true) 
			exp = BitBinaryOp(exp, second, BitBinaryOp.Or) 

	def BitXor(ref exp as Expression):
		second as Expression 
		BitAnd(exp)
		while la.kind == 35:
			Get()
			tok = t 
			BitAnd(second)
			return unless ExpectInt(exp, tok, false)
			return unless ExpectInt(second, tok, true) 
			exp = BitBinaryOp(exp, second, BitBinaryOp.Xor) 

	def BitAnd(ref exp as Expression):
		second as Expression 
		BitShift(exp)
		while la.kind == 36:
			Get()
			tok = t 
			BitShift(second)
			return unless ExpectInt(exp, tok, false)
			return unless ExpectInt(second, tok, true) 
			exp = BitBinaryOp(exp, second, BitBinaryOp.And) 

	def BitShift(ref exp as Expression):
		second as Expression
		op as string 
		PlusMinus(exp)
		while la.kind == 37 or la.kind == 38:
			if la.kind == 37:
				Get()
				op = BitBinaryOp.ShiftLeft 
			else:
				Get()
				op = BitBinaryOp.ShiftRight 
			tok = t 
			PlusMinus(second)
			return unless ExpectInt(exp, tok, false)
			return unless ExpectInt(second, tok, true) 
			exp = BitBinaryOp(exp, second, op)

	def PlusMinus(ref exp as Expression):
		second as Expression
		op as string
		MulDivMod(exp)
		while la.kind == 39 or la.kind == 40:
			if la.kind == 39:
				Get()
				op = ArithmeticBinaryOp.Plus 
			else:
				Get()
				op = ArithmeticBinaryOp.Minus 
			tok = t 
			MulDivMod(second)
			return unless ExpectInt(exp, tok, false)
			return unless ExpectInt(second, tok, true) 
			exp = ArithmeticBinaryOp(exp, second, op) 

	def MulDivMod(ref exp as Expression):
		second as Expression 
		UnaryOperator(exp)
		while la.kind == 41 or la.kind == 42 or la.kind == 43:
			if la.kind == 41:
				Get()
				op = ArithmeticBinaryOp.Multiplication 
			elif la.kind == 42:
				Get()
				op = ArithmeticBinaryOp.Division 
			else:
				Get()
				op = ArithmeticBinaryOp.Modulo 
			tok = t 
			UnaryOperator(second)
			return unless ExpectInt(exp, tok, false)
			return unless ExpectInt(second, tok, true) 
			exp = ArithmeticBinaryOp(exp, second, op) 

	def UnaryOperator(ref exp as Expression):
		op as string = null 
		if la.kind == 40 or la.kind == 44 or la.kind == 45:
			if la.kind == 40:
				Get()
				op = t.val 
			elif la.kind == 44:
				Get()
				op = t.val 
			else:
				Get()
				op = t.val 
				tok = t 
		Terminal(exp)
		if op in ('-','~'):
			return unless ExpectInt(exp, tok, true)
			exp = IntUnaryOp(exp, op)
		elif op == 'not':
			return unless ExpectBool(exp, tok, true)
			exp = NotUnaryOp(exp) 

	def Terminal(ref exp as Expression):
		if la.kind == 1:
			Get()
			exp = Variable(t.val)
			if not VariableStack.IsInScope(t.val):
				errors.SemErr(t.line, t.col, "Undeclared variable '${t.val}'") 
		elif la.kind == 2:
			Get()
			exp = Number(int.Parse(t.val)) 
		elif la.kind == 46:
			Get()
			exp = Bool(true) 
		elif la.kind == 47:
			Get()
			exp = Bool(false) 
		elif la.kind == 4:
			Get()
			Expr(exp)
			Expect(7)
		else: SynErr(51)


	
	public def Parse():
		la = Token()
		la.val = ''
		Get()
		Program()

		Expect(0)

	
	private bitset = List[of (bool)]()

	private def InitBitset():
		bitset.Add((true ,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false))
		bitset.Add((false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false,false,false, true ,true ,true ,true , true ,true ,false,false, false,false,false,false, false,false,false,false, false,false,false,false, false,false))


// end Parser

public class Errors:

	public count = 0

	// number of errors detected
	public errorStream as System.IO.TextWriter = Console.Out

	// error messages go to this stream
	public errMsgFormat = '({0},{1}) {2}: {3}'

	// 0=line, 1=column, 2=text
	public def SynErr(line as int, col as int, n as int):
		s as string
		if n == 0: s = 'EOF expected'
		elif n == 1: s = 'ident expected'
		elif n == 2: s = 'number expected'
		elif n == 3: s = '"proc" expected'
		elif n == 4: s = '"(" expected'
		elif n == 5: s = '"val" expected'
		elif n == 6: s = '"res" expected'
		elif n == 7: s = '")" expected'
		elif n == 8: s = '"is" expected'
		elif n == 9: s = '"end" expected'
		elif n == 10: s = '";" expected'
		elif n == 11: s = '"," expected'
		elif n == 12: s = '"skip" expected'
		elif n == 13: s = '"read" expected'
		elif n == 14: s = '"write" expected'
		elif n == 15: s = '"var" expected'
		elif n == 16: s = '"begin" expected'
		elif n == 17: s = '":=" expected'
		elif n == 18: s = '"if" expected'
		elif n == 19: s = '"then" expected'
		elif n == 20: s = '"else" expected'
		elif n == 21: s = '"fi" expected'
		elif n == 22: s = '"while" expected'
		elif n == 23: s = '"do" expected'
		elif n == 24: s = '"od" expected'
		elif n == 25: s = '"or" expected'
		elif n == 26: s = '"and" expected'
		elif n == 27: s = '"xor" expected'
		elif n == 28: s = '"<" expected'
		elif n == 29: s = '">" expected'
		elif n == 30: s = '"<=" expected'
		elif n == 31: s = '">=" expected'
		elif n == 32: s = '"==" expected'
		elif n == 33: s = '"!=" expected'
		elif n == 34: s = '"|" expected'
		elif n == 35: s = '"^" expected'
		elif n == 36: s = '"&" expected'
		elif n == 37: s = '"<<" expected'
		elif n == 38: s = '">>" expected'
		elif n == 39: s = '"+" expected'
		elif n == 40: s = '"-" expected'
		elif n == 41: s = '"*" expected'
		elif n == 42: s = '"/" expected'
		elif n == 43: s = '"%" expected'
		elif n == 44: s = '"~" expected'
		elif n == 45: s = '"not" expected'
		elif n == 46: s = '"true" expected'
		elif n == 47: s = '"false" expected'
		elif n == 48: s = '??? expected'
		elif n == 49: s = 'invalid Args'
		elif n == 50: s = 'invalid Stmt'
		elif n == 51: s = 'invalid Terminal'

		else: s = ('error ' + n)
		errorStream.WriteLine(errMsgFormat, line, col, "ERROR", s)
		count += 1

	
	public def SemErr(line as int, col as int, s as string):
		errorStream.WriteLine(errMsgFormat, line, col, "ERROR", s)
		count += 1

	
	public def Warning(line as int, col as int, s as string):
		errorStream.WriteLine(errMsgFormat, line, col, "WARNING", s)

// Errors

public class FatalError(System.Exception):

	public def constructor(m as string):
		super(m)



