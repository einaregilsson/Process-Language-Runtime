using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System;
using PLR.Compilation;
using System.Diagnostics.SymbolStore;

namespace PLR.AST {

    public class ProcessSystem : Node {

        private List<String> _importedClasses = new List<string>();

        public new ProcessDefinition this[int index] {
            get { return (ProcessDefinition)_children[index]; }
        }

        public void AddImport(string import) {
            _importedClasses.Add(import);
        }

        public void Add(ProcessDefinition pd) {
            _children.Add(pd);
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this);
        }

        public override void Compile(CompileContext context) {
        }

        public void Compile(CompileOptions options) {
            string absolutePath = Path.Combine(Environment.CurrentDirectory, options.OutputFile);
            string filename = Path.GetFileName(absolutePath);
            string folder = Path.GetDirectoryName(absolutePath);
            AssemblyName name = new AssemblyName(filename);
            AssemblyBuilder assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Save, folder);
            ModuleBuilder module = assembly.DefineDynamicModule(options.OutputFile, filename, options.Debug);
            if (options.EmbedPLR) {
                GenerateAssemblyLookup(module);
            }

            CompileContext context = new CompileContext();
            context.Options = options;
            context.Module = module;
            MethodBuilder mainMethod = module.DefineGlobalMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(int), new Type[] { });
            if (options.Debug) {
                context.DebugWriter = module.DefineDocument(options.Arguments[0], Guid.Empty, Guid.Empty, SymDocumentType.Text);
                module.SetUserEntryPoint(mainMethod);
            }

            context.Module = module;
            context.ImportedClasses = _importedClasses;

            foreach (ProcessDefinition procdef in this) {
                context.StartNewProcessDefiniton(procdef.ProcessConstant.Name);
                procdef.CompileSignature(module, context);
            }
            if (!context.ImportedClasses.Contains("PLR.BuiltIns")) {
                context.ImportedClasses.Add("PLR.BuiltIns");
            }
            context.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly());
            if (options.References != "") {

                foreach (string assemblyName in options.References.Split(',')) {
                    string absoluteAssemblyPath = Path.Combine(Directory.GetCurrentDirectory(), assemblyName);
                    if (!File.Exists(absoluteAssemblyPath)) {
                        Console.Error.WriteLine("Assembly '{0}' does not exist!", absoluteAssemblyPath);
                        Environment.Exit(1);
                    }
                    context.ReferencedAssemblies.Add(Assembly.LoadFile(absoluteAssemblyPath));
                }
            }

            foreach (ProcessDefinition procdef in this) {
                context.ProcessName = procdef.ProcessConstant.Name;
                procdef.Compile(context);
            }

            List<LocalBuilder> initial = new List<LocalBuilder>();
            context.PushIL(mainMethod.GetILGenerator());
            foreach (ProcessDefinition procdef in this) {
                if (procdef.EntryProc) {
                    Type startProc = module.GetType(procdef.ProcessConstant.Name);
                    LocalBuilder loc = context.ILGenerator.DeclareLocal(startProc);
                    context.ILGenerator.Emit(OpCodes.Newobj, context.NamedProcessConstructors[procdef.ProcessConstant.Name]);
                    context.ILGenerator.Emit(OpCodes.Stloc, loc);
                }
            }
            //Run Scheduler, who now knows all the new Processes
            context.ILGenerator.EmitWriteLine("Starting Scheduler");
            CallScheduler("Run", true, context);

            //return 0;
            context.ILGenerator.Emit(OpCodes.Ldc_I4_0);
            context.ILGenerator.Emit(OpCodes.Ret);

            module.CreateGlobalFunctions();
            assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication);
            assembly.Save(filename);
        }


        private void GenerateAssemblyLookup(ModuleBuilder module) {
            File.Copy("plr.dll", "plr.dll.embed", true);
            module.DefineManifestResource("PLR", new FileStream(@"plr.dll.embed", FileMode.Open), ResourceAttributes.Public);

            MethodBuilder resolveAssemblyMethod = module.DefineGlobalMethod("ResolveAssembly", MethodAttributes.Public | MethodAttributes.Static, typeof(Assembly), new Type[] { typeof(object), typeof(System.ResolveEventArgs) });
            ILGenerator ilResolve = resolveAssemblyMethod.GetILGenerator();
            CompileContext resolvecontext = new CompileContext();
            resolvecontext.PushIL(ilResolve);
            LocalBuilder localStream = ilResolve.DeclareLocal(typeof(Stream));
            LocalBuilder localBuf = ilResolve.DeclareLocal(typeof(byte[]));
            ilResolve.EmitWriteLine("PLR Not found, loading embedded PLR");
            Assign(localStream, Call(Call(typeof(Assembly), "GetExecutingAssembly", false), "GetManifestResourceStream", false, "PLR"), resolvecontext);

            Call(localStream, "get_Length", false).Compile(resolvecontext);
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

            Call(typeof(Assembly), "Load", false, localBuf).Compile(resolvecontext);
            ilResolve.Emit(OpCodes.Ret);
            ilResolve.Emit(OpCodes.Pop);
            resolvecontext.PopIL();

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



