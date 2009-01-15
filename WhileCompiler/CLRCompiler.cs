using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace WC {
    public class CLRCompiler : IWhileCompiler{

        private ILGenerator il;

        public void Compile() {
            // create a dynamic assembly and module 
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "HelloWorld2";
            AssemblyBuilder assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
            ModuleBuilder module;
            module = assemblyBuilder.DefineDynamicModule("HelloWorld.exe");

            // create a new type to hold our Main method
            TypeBuilder typeBuilder = module.DefineType("HelloWorldType", TypeAttributes.Public | TypeAttributes.Class);

            // create the Main(string[] args) method
            MethodBuilder methodbuilder = typeBuilder.DefineMethod("Main", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), new Type[] { typeof(string[]) });

            // generate the IL for the Main method
            il = methodbuilder.GetILGenerator();
            
            //Type[] types = new Type[] {typeof(int)};
            //MethodInfo m = typeof(System.Console).GetMethod("WriteLine", new Type[] {typeof(int)});
            //ilGenerator.EmitCall(OpCodes.Call, m,null);
            ilGenerator.Emit(OpCodes.Ret);

            // bake it
            Type helloWorldType = typeBuilder.CreateType();

            // set the entry point for the application and save it
            assemblyBuilder.SetEntryPoint(methodbuilder, PEFileKinds.ConsoleApplication);
            assemblyBuilder.Save("HelloWorld.exe");
        }

        public void Ident(string ident) {
        }

        public void Number(int nr) {
            il.Emit(OpCodes.Ldc_I4, nr);
        }

        public void Boolean(bool b) {
        }

        public void Skip() {
            il.Emit(OpCodes.Nop);
        }

        public void VarDec(string name) {
        }

        public void BlockBegin() {
            il.BeginScope();
        }

        public void BlockEnd() {
            il.EndScope();
        }

        public void Read(string variable) {
        }

        public void WriteArithmetic() {
        }

        public void Assign(string variable) {
            il.Emit(OpCodes.br
        }

        public void UnaryOp(string op) {
        }

        public void BinaryOp(string op) {
        }

    }
}
