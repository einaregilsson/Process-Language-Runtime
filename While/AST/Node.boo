namespace While.AST

import System
import System.Collections.Generic

public abstract class Node:
	public virtual def GetChildren() as List[of Node]:
		return List[of Node]()

