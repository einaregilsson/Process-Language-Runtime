using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System;

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
            AssemblyBuilder assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Save);
            ModuleBuilder module = assembly.DefineDynamicModule(exeName, "test.exe");
            module.DefineManifestResource("PLR", new FileStream("PLR.dll.bak", FileMode.Open), ResourceAttributes.Public);
            foreach (ProcessDefinition procdef in this) {
                procdef.Compile(module, nameSpace);
            }
            TypeBuilder programType = module.DefineType("Program");
            ConstructorBuilder staticCons = programType.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, new Type[] { });
            MethodBuilder resolveAssemblyMethod = programType.DefineMethod("ResolveAssembly", MethodAttributes.Public | MethodAttributes.Static, typeof(Assembly), new Type[] { typeof(object), typeof(System.ResolveEventArgs)});
            ILGenerator ilResolve = resolveAssemblyMethod.GetILGenerator();

            LocalBuilder localBuf = ilResolve.DeclareLocal(typeof(byte[]));
            LocalBuilder localStream = ilResolve.DeclareLocal(typeof(Stream));
            ilResolve.EmitWriteLine("RESOLVING");
            ilResolve.Emit(OpCodes.Call, typeof(Assembly).GetMethod("GetExecutingAssembly"));
            ilResolve.Emit(OpCodes.Ldstr, "PLR");
            ilResolve.Emit(OpCodes.Callvirt, typeof(Assembly).GetMethod("GetManifestResourceStream", new Type[] {typeof(string)}));
            ilResolve.Emit(OpCodes.Stloc, localStream);
            ilResolve.Emit(OpCodes.Ldloc, localStream);
            ilResolve.Emit(OpCodes.Callvirt, typeof(Stream).GetMethod("get_Length"));
            ilResolve.Emit(OpCodes.Conv_Ovf_I);
            ilResolve.Emit(OpCodes.Newarr, typeof(byte[]));
            ilResolve.Emit(OpCodes.Stloc, localBuf);

            ilResolve.Emit(OpCodes.Ldloc, localStream);
            ilResolve.Emit(OpCodes.Ldloc, localBuf);
            ilResolve.Emit(OpCodes.Ldc_I4_0);
            ilResolve.Emit(OpCodes.Ldloc, localBuf);
            ilResolve.Emit(OpCodes.Ldlen);
            ilResolve.Emit(OpCodes.Conv_I4);
            ilResolve.Emit(OpCodes.Callvirt, typeof(Stream).GetMethod("Read", new Type[] {typeof(byte[]), typeof(int), typeof(int)}));
            ilResolve.Emit(OpCodes.Pop);
            ilResolve.Emit(OpCodes.Ldloc, localBuf);
            ilResolve.Emit(OpCodes.Call, typeof(Assembly).GetMethod("Load", new Type[] {typeof(byte[])}));
            ilResolve.Emit(OpCodes.Ret);

            MethodBuilder mainMethod = programType.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(int), new Type[] { });
            ILGenerator ilMain = mainMethod.GetILGenerator();

            //Put in the hook to load the PLR
            ILGenerator ilStartup = staticCons.GetILGenerator();
            ilStartup.Emit(OpCodes.Call, typeof(System.AppDomain).GetMethod("get_CurrentDomain"));
            ilStartup.Emit(OpCodes.Ldnull);
            ilStartup.Emit(OpCodes.Ldftn, resolveAssemblyMethod);
            ilStartup.Emit(OpCodes.Newobj, MethodResolver.GetConstructor(typeof(System.ResolveEventHandler)));
            ilStartup.Emit(OpCodes.Callvirt, MethodResolver.GetMethod(typeof(System.AppDomain), "add_AssemblyResolve"));
            ilStartup.EmitWriteLine("Added ref");
            ilStartup.Emit(OpCodes.Ret);

            foreach (ProcessDefinition procdef in this) {
                if (procdef.EntryProc || true) {
                    Type startProc = module.GetType(procdef.ProcessConstant.Name);
                    ilMain.Emit(OpCodes.Newobj, MethodResolver.GetConstructor(startProc));
                    ilMain.Emit(OpCodes.Callvirt, MethodResolver.GetMethod(startProc, "Run"));
                }
            }
            
            ilMain.Emit(OpCodes.Call, typeof(Scheduler).GetMethod("get_Instance"));
            ilMain.Emit(OpCodes.Callvirt, typeof(Scheduler).GetMethod("Run"));

            //return 0;
            ilMain.Emit(OpCodes.Ldc_I4_0);
            ilMain.Emit(OpCodes.Ret);
            programType.CreateType();
            assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication);
            assembly.Save(exeName);
        }

    }
}



