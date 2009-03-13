using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

namespace PLR.AST
{

    public class ProcessSystem : Node
    {

        public new ProcessDefinition this[int index]
        {
            get { return (ProcessDefinition)_children[index]; }
        }

        public void Add(ProcessDefinition pd)
        {
            _children.Add(pd);
        }
        public override void Accept(AbstractVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Compile(string exeName, string nameSpace) {
            AssemblyName name = new AssemblyName(exeName);
            AssemblyBuilder assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder module = assembly.DefineDynamicModule(exeName);
            foreach (ProcessDefinition procdef in this) {
                procdef.Compile(module, nameSpace);
            }
            //assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication);
            assembly.Save(exeName);
        }
    }
}
