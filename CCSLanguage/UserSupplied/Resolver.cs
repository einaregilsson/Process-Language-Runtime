/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Babel
{
	public class Resolver : Babel.IASTResolver
	{
		#region IASTResolver Members

		public IList<Babel.Declaration> FindCompletions(object result, int line, int col)
		{
			return new List<Babel.Declaration>();
		}

		public IList<Babel.Declaration> FindMembers(object result, int line, int col)
		{
			// ManagedMyC.Parser.AAST aast = result as ManagedMyC.Parser.AAST;
			List<Babel.Declaration> members = new List<Babel.Declaration>();

            //foreach (string state in aast.startStates.Keys)
            //    members.Add(new Declaration(state, state, 0, state));

			return members;
		}

		public string FindQuickInfo(object result, int line, int col)
		{
			return "unknown";
		}

		public IList<Babel.Method> FindMethods(object result, int line, int col, string name)
		{
            Babel.Method m = new Method();
            m.Name = "DUMMYNAME";

			return new List<Babel.Method>(){m};
		}

		#endregion
	}
}
