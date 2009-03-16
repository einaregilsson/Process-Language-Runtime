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

            GenerateAssemblyLookup(module);

            MethodBuilder mainMethod = module.DefineGlobalMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(int), new Type[] { });
            ILGenerator ilMain = mainMethod.GetILGenerator();

            foreach (ProcessDefinition procdef in this) {
                procdef.Compile(module, "");
            }
            foreach (ProcessDefinition procdef in this) {
                if (procdef.EntryProc || true) {
                    Type startProc = module.GetType(procdef.ProcessConstant.Name);
                    Call(New(startProc), "Run",true).Compile(ilMain);
                }
            }
            Call(Call(typeof(Scheduler), "get_Instance",false), "Run", true);

            //return 0;
            ilMain.Emit(OpCodes.Ldc_I4_0);
            ilMain.Emit(OpCodes.Ret);
            module.CreateGlobalFunctions();
            assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication);
            assembly.Save(exeName);
        }


        private void GenerateAssemblyLookup(ModuleBuilder module) {


            File.Copy("plr.dll", "plr.dll.embed", true);
            module.DefineManifestResource("PLR", new FileStream(@"plr.dll.embed", FileMode.Open), ResourceAttributes.Public);

            MethodBuilder resolveAssemblyMethod = module.DefineGlobalMethod("ResolveAssembly", MethodAttributes.Public | MethodAttributes.Static, typeof(Assembly), new Type[] { typeof(object), typeof(System.ResolveEventArgs) });
            ILGenerator ilResolve = resolveAssemblyMethod.GetILGenerator();

            LocalBuilder localStream = ilResolve.DeclareLocal(typeof(Stream));
            LocalBuilder localBuf = ilResolve.DeclareLocal(typeof(byte[]));
            ilResolve.EmitWriteLine("PLR Not found, loading embedded PLR");
            Assign(localStream, Call(Call(typeof(Assembly), "GetExecutingAssembly", false), "GetManifestResourceStream", false, "PLR"),ilResolve);

            Call(localStream, "get_Length", false).Compile(ilResolve);
            ilResolve.Emit(OpCodes.Conv_Ovf_I);
            ilResolve.Emit(OpCodes.Newarr, typeof(System.Byte));
            ilResolve.Emit(OpCodes.Stloc, localBuf);

            ilResolve.Emit(OpCodes.Ldloc, localStream);
            ilResolve.Emit(OpCodes.Ldloc, localBuf);
            ilResolve.Emit(OpCodes.Ldc_I4_0);
            ilResolve.Emit(OpCodes.Ldloc, localBuf);
            ilResolve.Emit(OpCodes.Ldlen);
            ilResolve.Emit(OpCodes.Conv_I4);
            ilResolve.Emit(OpCodes.Callvirt, typeof(Stream).GetMethod("Read", new Type[] { typeof(byte[]), typeof(int), typeof(int) }));
            ilResolve.Emit(OpCodes.Pop);

            Call(typeof(Assembly), "Load", false, localBuf).Compile(ilResolve);
            ilResolve.Emit(OpCodes.Ret);
            MethodBuilder moduleInitializer = module.DefineGlobalMethod(".cctor", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.RTSpecialName, null, new Type[] { });
            ILGenerator ilStartup = moduleInitializer.GetILGenerator();
            ilStartup.Emit(OpCodes.Call, typeof(System.AppDomain).GetMethod("get_CurrentDomain"));
            ilStartup.Emit(OpCodes.Ldnull);
            ilStartup.Emit(OpCodes.Ldftn, resolveAssemblyMethod);
            ilStartup.Emit(OpCodes.Newobj, MethodResolver.GetConstructor(typeof(System.ResolveEventHandler)));
            ilStartup.Emit(OpCodes.Callvirt, MethodResolver.GetMethod(typeof(System.AppDomain), "add_AssemblyResolve"));
            ilStartup.Emit(OpCodes.Ret);
        }
    }
}



