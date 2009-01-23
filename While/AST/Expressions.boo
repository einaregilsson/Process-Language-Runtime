namespace While.AST.Expressions

import System
import While
import While.AST

import System.Reflection.Emit

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
	
	def Compile(il as ILGenerator):
		if _boolValue:
			il.Emit(OpCodes.Ldc_I4_1)
		else:
			il.Emit(OpCodes.Ldc_I4_0)
	
class Number(IntExpression):
	[Getter(IntValue)]
	_nr as int

	def constructor(nr as int):
		_nr = nr
		
	def ToString():
		return _nr.ToString()

	def Compile(il as ILGenerator):
		il.Emit(OpCodes.Ldc_I4, _nr)

class Variable(IntExpression):
	[Getter(Name)]
	_name as string

	IntValue as int:
		get:
			return VariableStack.GetValue(_name)
		
	def constructor(name as string):
		_name = name

	def ToString():
		return _name.ToString()
		
	def Compile(il as ILGenerator):
		il.Emit(OpCodes.Ldloc, 0)
		

abstract class IntBinaryOp[of ChildType](IntExpression):

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

abstract class BoolBinaryOp[of ChildType](BoolExpression):

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
			if _op == Plus: return l + r
			elif _op == Minus: return l - r
			elif _op == Multiplication: return l * r
			elif _op == Division: return l / r
			elif _op == Modulo: return l % r
			raise WhileException("Unknown operator '${Op}'")

	def Compile(il as ILGenerator):
		_left.Compile(il)
		_right.Compile(il)
		if _op == Plus: il.Emit(OpCodes.Add_Ovf)
		elif _op == Minus: il.Emit(OpCodes.Sub_Ovf)
		elif _op == Multiplication: il.Emit(OpCodes.Mul_Ovf)
		elif _op == Division: il.Emit(OpCodes.Div)
		elif _op == Modulo: il.Emit(OpCodes.Rem)
		

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

	def Compile(il as ILGenerator):
		_left.Compile(il)
		_right.Compile(il)
		if Op == GreaterThan: il.Emit(OpCodes.Cgt)
		elif Op == LessThan: il.Emit(OpCodes.Clt)
		elif Op == GreaterThanOrEqual:
			il.Emit(OpCodes.Clt)
			il.Emit(OpCodes.Ldc_I4_0)
			il.Emit(OpCodes.Ceq)
		elif Op == LessThanOrEqual:
			il.Emit(OpCodes.Cgt)
			il.Emit(OpCodes.Ldc_I4_0)
			il.Emit(OpCodes.Ceq)
	
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

	def Compile(il as ILGenerator):
		_left.Compile(il)
		_right.Compile(il)
		if Op == Equal: il.Emit(OpCodes.Ceq)
		elif Op == NotEqual:
			il.Emit(OpCodes.Ceq)
			il.Emit(OpCodes.Ldc_I4_0)
			il.Emit(OpCodes.Ceq)

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

	def Compile(il as ILGenerator):
		_left.Compile(il)
		_right.Compile(il)
		if _op == ShiftLeft: il.Emit(OpCodes.Shl)
		elif _op == ShiftRight: il.Emit(OpCodes.Shr)
		elif _op == Or: il.Emit(OpCodes.Or)
		elif _op == And: il.Emit(OpCodes.And)
		elif _op == Xor: il.Emit(OpCodes.Xor)

class LogicBinaryOp(BoolBinaryOp[of BoolExpression]):
	public static final And = '&&'
	public static final Or = '||'
	public static final Xor = '^'

	def constructor(l as BoolExpression, r as BoolExpression, op as string):
		super(l,r,op)

	BoolValue:
		get:
			l, r = Left.BoolValue, Right.BoolValue
			if _op == And: return l and r
			elif _op == Or: return l or r
			elif _op == Xor: return l ^ r
			raise WhileException("Unknown operator '${Op}'")

	def Compile(il as ILGenerator):
		_left.Compile(il)
		_right.Compile(il)
		if _op == Or: il.Emit(OpCodes.Or)
		elif _op == And: il.Emit(OpCodes.And)
		elif _op == Xor: il.Emit(OpCodes.Xor)

class MinusUnaryOp(IntExpression):
	_exp as IntExpression
	
	IntValue:
		get: return -_exp.IntValue
	
	def constructor(exp as IntExpression):
		_exp = exp	
		
	def ToString():
		return "-${_exp}"

	def Compile(il as ILGenerator):
		pass
		
class NotUnaryOp(BoolExpression):
	_exp as BoolExpression

	BoolValue:
		get: return not _exp.BoolValue

	def constructor(exp as BoolExpression):
		_exp = exp			
		
	def ToString():
		return "!${_exp}"

	def Compile(il as ILGenerator):
		pass
		
