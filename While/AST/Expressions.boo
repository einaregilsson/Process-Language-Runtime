namespace While.AST.Expressions

import System
import While
import While.AST

abstract class Expression(Node):
	abstract Value as object:
		get: pass
	
		
abstract class BoolExpression(Expression):
	Value:
		get: return BoolValue
	virtual BoolValue as bool:
		get: pass

abstract class IntExpression(Expression):
	Value:
		get: return IntValue
	virtual IntValue as int:
		get: pass

	
class Bool(BoolExpression):
	[Getter(BoolValue)]
	_boolValue as bool
	
	def constructor(val as bool):
		_boolValue = val
		
	def ToString():
		return _boolValue.ToString().ToLower()
	
class Number(IntExpression):
	[Getter(IntValue)]
	_nr as int

	def constructor(nr as int):
		_nr = nr
		
	def ToString():
		return _nr.ToString()

class Variable(IntExpression):
	[Getter(Name)]
	_name as string

	IntValue as int:
		get:return 0 #TODO: implement stack
		
	def constructor(name as string):
		_name = name

	def ToString():
		return _name.ToString()

class IntBinaryOp[of ChildType](IntExpression):

	[Getter(Left)]	
	_left as ChildType
	[Getter(Right)]	
	_right as ChildType
	[Getter(Op)]
	_op as string
	
	def constructor(l as ChildType, r as ChildType, op as string):
		_left,_right,_op = l,r,op
		
	def ToString():
		return "(${_left} ${_op} ${_right})"
		
	
class BoolBinaryOp[of ChildType](BoolExpression):

	[Getter(Left)]	
	_left as ChildType
	[Getter(Right)]	
	_right as ChildType
	[Getter(Op)]
	_op as string
	
	def constructor(l as ChildType, r as ChildType, op as string):
		_left = l
		_right = r
		_op = op

	def ToString():
		return "(${_left} ${_op} ${_right})"

class ArithmeticBinaryOp(IntBinaryOp[of IntExpression]):

	public static final Plus = '+'
	public static final Minus = '-'
	public static final Multiplication = '*'
	public static final Division = '/'
	public static final Modulo = '%'
	
	def constructor(l as IntExpression, r as IntExpression, op as string):
		super(l,r,op)
		
	IntValue:
		get:
			l,r = Left.IntValue, Right.IntValue
			if _op == Plus:
				return l + r
			elif _op == Minus:
				return l - r
			elif _op == Multiplication:
				return l * r
			elif _op == Division:
				return l / r
			elif _op == Modulo:
				return l % r
			raise WhileException("Unknown operator '${Op}'")

class ComparisonBinaryOp(BoolBinaryOp[of IntExpression]):
	public static final GreaterThan = '>'
	public static final LessThan = '<'
	public static final GreaterThanOrEqual = '>='
	public static final LessThanOrEqual = '<='

	def constructor(l as IntExpression, r as IntExpression, op as string):
		super(l,r,op)

	BoolValue:
		get:
			l, r = Left.IntValue, Right.IntValue
			if _op == GreaterThan:
				return l > r
			elif _op == LessThan:
				return l < r
			elif _op == GreaterThanOrEqual:
				return l >= r
			elif _op == LessThanOrEqual:
				return l <= r
			raise WhileException("Unknown operator '${Op}'")
	
class EqualityBinaryOp(BoolBinaryOp[of Expression]):
	public static final Equal = '=='
	public static final NotEqual = '!='

	def constructor(l as Expression, r as Expression, op as string):
		super(l,r,op)

	BoolValue:
		get:
			l, r = Left.Value, Right.Value
			if _op == Equal:
				return l == r
			elif _op == NotEqual:
				return l != r
			raise WhileException("Unknown operator '${Op}'")


class BitBinaryOp(IntBinaryOp[of IntExpression]):
	public static final ShiftLeft = '<<'
	public static final ShiftRight = '>>'
	public static final And = '&'
	public static final Or = '|'
	public static final Xor = '^'

	def constructor(l as IntExpression, r as IntExpression, op as string):
		super(l,r,op)

	IntValue:
		get:
			l, r = Left.IntValue, Right.IntValue
			if _op == ShiftLeft:
				return l << r
			elif _op == ShiftRight:
				return l >> r
			elif _op == And:
				return l & r
			elif _op == Or:
				return l | r
			elif _op == Xor:
				return l ^ r
			raise WhileException("Unknown operator '${Op}'")

class LogicBinaryOp(BoolBinaryOp[of BoolExpression]):
	public static final And = '&&'
	public static final Or = '||'
	public static final Xor = '^'

	def constructor(l as BoolExpression, r as BoolExpression, op as string):
		super(l,r,op)

	BoolValue:
		get:
			l, r = Left.BoolValue, Right.BoolValue
			if _op == And:
				return l and r
			elif _op == Or:
				return l or r
			elif _op == Xor:
				return l ^ r
			raise WhileException("Unknown operator '${Op}'")


class MinusUnaryOp(IntExpression):
	_exp as IntExpression
	
	IntValue:
		get: return -_exp.IntValue
	
	def constructor(exp as IntExpression):
		_exp = exp	
		
	def ToString():
		return "-${_exp}"
		
class NotUnaryOp(BoolExpression):
	_exp as BoolExpression

	BoolValue:
		get: return not _exp.BoolValue

	def constructor(exp as BoolExpression):
		_exp = exp			
		
	def ToString():
		return "!${_exp}"
		
