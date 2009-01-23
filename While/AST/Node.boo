namespace While.AST

import System
import System.Reflection.Emit

public abstract class Node:
	abstract def Compile(il as ILGenerator):
		pass
