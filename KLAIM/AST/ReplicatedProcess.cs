using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PLR.AST;
using PLR.AST.Processes;
using PLR.Compilation;
using System.Reflection.Emit;
using System.Threading;
using KLAIM.Runtime;
using System.Reflection;
using PLR;
using PLR.Runtime;

/*
 *  Ideas for how replicated processes should work:
 * 
 *  1. Endlessly spawn new instances. Impractical for obvious reasons.
 *  
 *  2. Spawn new processes when others block. What to do about out actions then?
 *  
 *  3. Assign guids to each action in a replicated process. Follow when they 
 *     occur and do something meaningful from that...
 *  
 *  Best option right now sounds like option 3...
 * 
 * 
 */
namespace KLAIM.AST {

    public class ReplicatedProcess : Process{

        public ReplicatedProcess(Process p) {
            _children.Add(p);
        }

        public Process Process {
            get { return (Process)  _children[2]; }
            set { _children[0] = value;}
        }
        public override void Accept(AbstractVisitor visitor) {
            visitor.Visit(this); 
        }

        public override void Compile(CompileContext context) {
            string innerTypeName = "Repl_" + context.Type.Name;
            ILGenerator il = context.ILGenerator;
            TypeInfo newType = null;
            newType = Process.CompileNewProcessStart(context, innerTypeName);
            Process.Compile(context);
            Process.CompileNewProcessEnd(context);
            Label startLoop = il.DefineLabel();
            il.MarkLabel(startLoop);
            LocalBuilder replCount = il.DeclareLocal(typeof(int));

            //Give the replicated process the variables as they are at this point...
            foreach (string paramName in newType.ConstructorParameters) {
                il.Emit(OpCodes.Ldloc, context.Type.GetLocal(paramName));
            }

            LocalBuilder loc = il.DeclareLocal(newType.Builder);
            il.Emit(OpCodes.Newobj, newType.Constructor);
            il.Emit(OpCodes.Stloc, loc);
            il.Emit(OpCodes.Ldloc, loc);

            //Set this process as the parent of the new proc, that allows it to activate this thread
            //again once it is past its first input action.
            il.Emit(OpCodes.Ldarg_0); //load the "this" pointer
            il.Emit(OpCodes.Call, MethodResolver.GetMethod(typeof(ProcessBase), "set_Parent"));

            //start the new instance of the replicated process
            il.Emit(OpCodes.Ldloc, loc);
            il.Emit(OpCodes.Call, MethodResolver.GetMethod(typeof(ProcessBase), "Run"));

            //Count how many we've emitted
            il.Emit(OpCodes.Ldloc, replCount);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc, replCount);

            //Print that information.
            il.Emit(OpCodes.Ldstr, "Number of " + innerTypeName + " started: ");
            il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Write", new Type[] {typeof(string)}));
            il.Emit(OpCodes.Ldloc, replCount);
            il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("WriteLine", new Type[] {typeof(int)}));

            //Suspend ourselves, we will be woken up by the replicated process once it gets
            //past its first action...
            il.Emit(OpCodes.Call, typeof(Thread).GetMethod("get_CurrentThread"));
            il.Emit(OpCodes.Call, typeof(Thread).GetMethod("Suspend"));
            il.Emit(OpCodes.Br, startLoop);
        }
    }
}
