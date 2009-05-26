/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
 ﻿using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System;
using PLR.Compilation;
using System.Diagnostics.SymbolStore;

namespace PLR.AST {

    public delegate void CompileEventHandler(CompileContext context);

    public class ProcessSystem : Node {

        public event CompileEventHandler BeforeCompile;
        public event CompileEventHandler AfterCompile;
        public event CompileEventHandler MainMethodStart;
        public event CompileEventHandler MainMethodEnd;

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

            if (BeforeCompile != null) {
                BeforeCompile(context);
            }

            foreach (ProcessDefinition procdef in this) {
                procdef.CompileSignature(module, context);
            }

            if (!context.ImportedClasses.Contains(typeof(PLR.Runtime.BuiltIns).FullName)) {
                context.ImportedClasses.Add(typeof(PLR.Runtime.BuiltIns).FullName);
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
                context.CurrentMasterType = null;
                procdef.Compile(context);
                if (context.CurrentMasterType.Variables != null) {
                    context.CurrentMasterType.Variables.CreateType();
                }
                context.CurrentMasterType = null;
            }
            List<LocalBuilder> initial = new List<LocalBuilder>();
            context.PushIL(mainMethod.GetILGenerator());
            if (MainMethodStart != null) {
                MainMethodStart(context);
            }
            foreach (ProcessDefinition procdef in this) {
                if (procdef.EntryProc) {
                    TypeInfo startProc = context.GetType(procdef.FullName);
                    LocalBuilder loc = context.ILGenerator.DeclareLocal(startProc.Builder);
                    context.ILGenerator.Emit(OpCodes.Newobj, startProc.Constructor);
                    context.ILGenerator.Emit(OpCodes.Stloc, loc);
                }
            }

            //Run Scheduler, who now knows all the new Processes
            CallScheduler("Run", true, context);

            if (MainMethodEnd!= null) {
                MainMethodEnd(context);
            }

            //return 0;
            context.ILGenerator.Emit(OpCodes.Ldc_I4_0);
            context.ILGenerator.Emit(OpCodes.Ret);

            if (AfterCompile != null) {
                AfterCompile(context);
            }
            module.CreateGlobalFunctions();
            assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication);
            assembly.Save(filename);
        }

        /// <summary>
        /// Call this method after constructing the tree to attach parent nodes to all nodes in the tree.
        /// This is easier than having to do this during parsing in each and every implementation.
        /// </summary>
        public void MeetTheParents() {
            MeetTheParents(this);
        }

        private void MeetTheParents(Node node) {
            foreach (Node child in node.ChildNodes) {
                if (child != null) {
                    child.Parent = node;
                    MeetTheParents(child);
                }
            }
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

