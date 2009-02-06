using System;
using System.Collections.Generic;

using System.Text;
using System.Reflection;
using System.Threading;
using System.Reflection.Emit;
using CCS.Nodes;

namespace CCS.Compiler {
    public class CCSCompiler {

        public void Compile(ArithmeticExpression exp) {
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
            ILGenerator ilGenerator = methodbuilder.GetILGenerator();
            
            CompileExpression(exp, ilGenerator);
            Type[] types = new Type[] {typeof(int)};
            MethodInfo m = typeof(System.Console).GetMethod("WriteLine", new Type[] {typeof(int)});
            ilGenerator.EmitCall(OpCodes.Call, m,null);

            ilGenerator.Emit(OpCodes.Ret);

            // bake it
            Type helloWorldType = typeBuilder.CreateType();

            // set the entry point for the application and save it
            assemblyBuilder.SetEntryPoint(methodbuilder, PEFileKinds.ConsoleApplication);
            assemblyBuilder.Save("HelloWorld.exe");
        }

        private void CompileExpression(ArithmeticExpression exp, ILGenerator il) {
            if (exp is Constant) {
                il.Emit(OpCodes.Ldc_I4, exp.Value);
            } else if (exp is UnaryMinus) {
                UnaryMinus us = (UnaryMinus) exp;
                CompileExpression(us.Term, il);
                il.Emit(OpCodes.Neg);
            } else if (exp is ArithmeticBinOpExpression) {
                ArithmeticBinOpExpression binop = (ArithmeticBinOpExpression)exp;
                CompileExpression(binop.Left, il);
                CompileExpression(binop.Right, il);
                Dictionary<ArithmeticBinOp, OpCode> codes = new Dictionary<ArithmeticBinOp, OpCode>() { 
                    {ArithmeticBinOp.Plus, OpCodes.Add},
                    {ArithmeticBinOp.Minus, OpCodes.Sub},
                    {ArithmeticBinOp.Divide, OpCodes.Div},
                    {ArithmeticBinOp.Multiply, OpCodes.Mul}
                };
                il.Emit(codes[binop.Op]);
            }
        }
    }
}
