namespace While.AST

import System.Collections.Generic
import While

static class VariableStack:

	private _stack = List[of Dictionary[of string, int]]()
	
	def PushScope():
		_stack.Add(Dictionary[of string, int]())
		
	def PopScope():
		_stack.RemoveAt(_stack.Count-1)

	def DefineVariable(name as string):
		if _stack.Count == 0:
			raise WhileException("Stack is empty, cannot define variable ${name}")
		if _stack[_stack.Count-1].ContainsKey(name):
			raise WhileException("Variable ${name} is already defined in this scope!")
		_stack[_stack.Count-1].Add(name, 0)

	def AssignValue(name as string, val as int):
		scope = FindScopeForVariable(name)
		if not scope:
			raise WhileException("Variable ${name} is not in scope!")
		scope[name] = val
	
	def GetValue(name as string) as int:
		scope = FindScopeForVariable(name)
		if not scope:
			raise WhileException("Variable ${name} is not in scope!")
		return scope[name]

	def FindScopeForVariable(name as string) as Dictionary[of string, int]:
		for i in range(_stack.Count-1, -1, -1):
			if _stack[i].ContainsKey(name):
				return _stack[i]
		return null
	

