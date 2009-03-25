using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System;

namespace PLR.AST {

    public class ProcessSystem : Node {

        public new ProcessDefinition this[int index] {
            get { return (ProcessDefinition)_children[index]; }
        }

        public void Add(ProcessDefinition pd) {
            _children.Add(pd);
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileInfo info) {
        }

        public void Compile(CompileOptions options) {
            string absolutePath = Path.Combine(Environment.CurrentDirectory, options.OutputFile);
            string filename = Path.GetFileName(absolutePath);
            string folder = Path.GetDirectoryName(absolutePath);
            AssemblyName name = new AssemblyName(filename);
            AssemblyBuilder assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Save, folder);
            ModuleBuilder module = assembly.DefineDynamicModule(options.OutputFile, filename);
            if (options.EmbedPLR) {
                GenerateAssemblyLookup(module);
            }

            CompileInfo mainInfo = new CompileInfo();
            mainInfo.Module = module;
            MethodBuilder mainMethod = module.DefineGlobalMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(int), new Type[] { });
            mainInfo.ILGenerator = mainMethod.GetILGenerator();

            foreach (ProcessDefinition procdef in this) {
                procdef.CompileSignature(module);
            }
            CompileInfo info = new CompileInfo();
            info.Module = module;
            foreach (ProcessDefinition procdef in this) {
                procdef.Compile(info);
            }
            List<LocalBuilder> initial = new List<LocalBuilder>();
            foreach (ProcessDefinition procdef in this) {
                if (procdef.EntryProc) {
                    Type startProc = module.GetType(procdef.ProcessConstant.Name);
                    LocalBuilder loc = mainInfo.ILGenerator.DeclareLocal(startProc);
                    Assign(loc, New(startProc), mainInfo);
                }
            }
            //Run Scheduler, who now knows all the new Processes
            mainInfo.ILGenerator.EmitWriteLine("Starting Scheduler");
            CallScheduler("Run", true, mainInfo);

            //return 0;
            mainInfo.ILGenerator.Emit(OpCodes.Ldc_I4_0);
            mainInfo.ILGenerator.Emit(OpCodes.Ret);

            module.CreateGlobalFunctions();
            assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication);
            assembly.Save(filename);
        }


        private void GenerateAssemblyLookup(ModuleBuilder module) {
            File.Copy("plr.dll", "plr.dll.embed", true);
            module.DefineManifestResource("PLR", new FileStream(@"plr.dll.embed", FileMode.Open), ResourceAttributes.Public);

            MethodBuilder resolveAssemblyMethod = module.DefineGlobalMethod("ResolveAssembly", MethodAttributes.Public | MethodAttributes.Static, typeof(Assembly), new Type[] { typeof(object), typeof(System.ResolveEventArgs) });
            ILGenerator ilResolve = resolveAssemblyMethod.GetILGenerator();
            CompileInfo resolveInfo = new CompileInfo();
            resolveInfo.ILGenerator = ilResolve;
            LocalBuilder localStream = ilResolve.DeclareLocal(typeof(Stream));
            LocalBuilder localBuf = ilResolve.DeclareLocal(typeof(byte[]));
            ilResolve.EmitWriteLine("PLR Not found, loading embedded PLR");
            Assign(localStream, Call(Call(typeof(Assembly), "GetExecutingAssembly", false), "GetManifestResourceStream", false, "PLR"), resolveInfo);

            Call(localStream, "get_Length", false).Compile(resolveInfo);
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

            Call(typeof(Assembly), "Load", false, localBuf).Compile(resolveInfo);
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



