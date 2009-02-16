using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST;
using PLR.AST.Processes;
using PLR.AST.Expressions;
using PLR.AST.Actions;
using CCS.Formatters;

namespace CCS
{
    public class Interpreter
    {
        private List<Process> _activeProcs = new List<Process>();
        private ProcessSystem _system;
        public void Interpret(ProcessSystem system)
        {
            _system = system;
            foreach (ProcessDefinition procdef in system)
            {
                if (procdef.EntryProc)
                {
                    _activeProcs.Add(procdef.Process);
                }
            }

            InterpretLoop();

        }

        private class Match
        {
            public Match(Process p1, Process p2, Action a1, Action a2)
            {
                P1 = p1;
                P2 = p2;
                A1 = a1;
                A2 = a2;
            }
            public Process P1, P2;
            public Action A1, A2;
            public override bool Equals(object obj)
            {
                if (!(obj is Match)){
                    return false;
                }
                Match other = (Match)obj;
                return this.P1 == other.P1 && this.P2 == other.P2 && this.A1 == other.A1 && this.A2 == other.A2
                || this.P1 == other.P2 && this.P2 == other.P1 && this.A1 == other.A2 && this.A2 == other.A1;
            }
        }
        private void InterpretLoop()
        {
            Dictionary<Process, List<Action>> possibleSyncs = new Dictionary<Process, List<Action>>();
            SourceFormatter formatter = new SourceFormatter();
            List<Match> matches;
            int choice;
            while (true)
            {
                possibleSyncs.Clear();
                SplitUpParallelProcesses();
                matches = new List<Match>();
                int i = 1;
                Console.WriteLine("\n\nActive Processes:\n");
                foreach (Process p in _activeProcs)
                {
                    possibleSyncs.Add(p, GetAvailableActions(p));
                    Console.WriteLine("{0}: {1}", i++, formatter.Format(p));
                }
                
                Console.WriteLine("\nPossible actions:\n");
                foreach (Process first in possibleSyncs.Keys)
                {
                    foreach (Action act1 in possibleSyncs[first])
                    {
                        foreach (Process second in possibleSyncs.Keys)
                        {
                            if (first != second)
                            {
                                foreach (Action act2 in possibleSyncs[second])
                                {
                                    if (!(act2 is TauAction) && !(act1 is TauAction))
                                    {
                                        if (act1.Name == act2.Name &&
                                            (act1 is InAction && act2 is OutAction || act1 is OutAction && act2 is InAction))
                                        {
                                            Match m = new Match(first, second, act1, act2);
                                            if (!matches.Contains(m))
                                            {
                                                matches.Add(m);
                                            }
                                            

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (matches.Count == 0) {
                    Console.WriteLine("System is deadlocked");
                } else {
                    int b = 1;
                    foreach (Match m in matches) {
                        if (m.A1 is OutAction) {
                            Console.WriteLine("{0}: {1} -{2}-> {3}", b++, formatter.Format(m.P1), m.A1.Name, formatter.Format(m.P2));
                        } else {
                            Console.WriteLine("{0}: {1} -{2}-> {3}", b++, formatter.Format(m.P2), m.A2.Name, formatter.Format(m.P1));
                        }
                    }
                }
                Console.Write("Select the number of the action to take: ");
                choice = int.Parse(Console.ReadLine());
            }
        }

        private List<Action> GetAvailableActions(Process p)
        {
            List<Action> available = new List<Action>();
            if (p is ActionPrefix)
            {
                available.Add(((ActionPrefix)p).Action);
            }
            else if (p is NonDeterministicChoice)
            {
                foreach (Process parallel in p)
                {
                    available.AddRange(GetAvailableActions(parallel));
                }
            }
            else if (p is ProcessConstant)
            {
                ProcessConstant pconst = (ProcessConstant) p;
                foreach (ProcessDefinition def in _system)
                {
                    if (def.ProcessConstant.Name == pconst.Name && def.ProcessConstant.Subscript.Count == pconst.Subscript.Count)
                    {
                        available.AddRange(GetAvailableActions(def.Process));
                    }
                }

            }
            return available;
        }

        private void SplitUpParallelProcesses() {
            List<Process> delete = new List<Process>();
            List<Process> add = new List<Process>();
            foreach (Process p in _activeProcs)
            {
                if (p is ParallelComposition)
                {
                    foreach (Process child in p.ChildNodes)
                    {
                        add.Add(child);
                    }
                    delete.Add(p);
                }
            }
            foreach (Process p in delete)
            {
                _activeProcs.Remove(p);
            }
            _activeProcs.AddRange(add);

        
        }
    }
}
