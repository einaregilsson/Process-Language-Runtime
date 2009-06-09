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

        public List<int> ActionNumbers { get; set; }
        public ReplicatedProcess(Process p) {
            this.ActionNumbers = new List<int>();
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
            
            context.Type.Builder.AddInterfaceImplementation(typeof(IActionSubscriber));
            context.Type.MustLiveOn = true; //We cannot die because we need to spawn more processes...
            TypeInfo newType = null;
            newType = Process.CompileNewProcessStart(context, innerTypeName);
            Process.Compile(context);
            Process.CompileNewProcessEnd(context);

            EmitRunProcess(context, newType, false, Process.LexicalInfo, true);

            DefineNotifyMethod(context, newType);
        }

        private void DefineNotifyMethod(CompileContext context, TypeInfo replProc) {
            MethodBuilder notify = context.Type.Builder.DefineMethod("NotifyAction", MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, typeof(void), new Type[] { typeof(int) });
            context.Type.Builder.DefineMethodOverride(notify, typeof(IActionSubscriber).GetMethod("NotifyAction"));
            ILGenerator il = notify.GetILGenerator();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", BindingFlags.Static | BindingFlags.Public, null, new Type[]{typeof(int)}, null));
            il.EmitWriteLine("Starting new instance of " + replProc.Name);
            context.PushIL(il);
            EmitRunProcess(context, replProc, false, Process.LexicalInfo, true);
            context.PopIL();

            //Notify this procs parent if there is one
            Label afterNotify = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0); //load the "this" pointer
            il.Emit(OpCodes.Call, MethodResolver.GetMethod(typeof(ProcessBase), "get_Parent"));
            il.Emit(OpCodes.Brfalse, afterNotify);
            il.Emit(OpCodes.Ldarg_0); //load the "this" pointer
            il.Emit(OpCodes.Call, MethodResolver.GetMethod(typeof(ProcessBase), "get_Parent"));
            il.Emit(OpCodes.Castclass, typeof(IActionSubscriber));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, MethodResolver.GetMethod(typeof(IActionSubscriber), "NotifyAction"));
            il.MarkLabel(afterNotify);

            il.Emit(OpCodes.Ret);
        }
    }
}
