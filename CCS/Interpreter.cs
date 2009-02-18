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
                ResolveProcessConstants();
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
                    Console.ReadKey();
                    System.Environment.Exit(1);
                } else {
                    int b = 1;
                    foreach (Match m in matches) {
                        if (m.A1 is OutAction) {
                            WriteCandidateMatch(b++, m.P1, m.A1, m.P2, m.A2);
                        } else {
                            WriteCandidateMatch(b++, m.P2, m.A2, m.P1, m.A1);
                        }
                    }
                }
                Console.Write("Select the number of the action to take: ");
                choice = int.Parse(Console.ReadLine());
                Match chosen = matches[choice - 1];
                _activeProcs.Remove(chosen.P1);
                _activeProcs.Remove(chosen.P2);
                AddProcessToActiveSet(chosen.P1, chosen.A1);
                AddProcessToActiveSet(chosen.P2, chosen.A2);
            }
        }
        private Process GetProcess(ProcessConstant pconst)
        {
            foreach (ProcessDefinition def in _system)
            {
                if (def.ProcessConstant.Equals(pconst))
                {
                    return def.Process;
                }
            }
            return null;
        }
        private bool AddProcessToActiveSet(Process p, Action actionTaken)
        {
            if (p is ActionPrefix && actionTaken.ID == ((ActionPrefix)p).Action.ID)
            {
                _activeProcs.Add(((ActionPrefix)p).Process);
                return true;
            }
            else if (p is NonDeterministicChoice)
            {
                foreach (Process subProc in p)
                {
                    if (AddProcessToActiveSet(subProc, actionTaken))
                    {
                        return true;
                    }
                }
            }
            else if (p is ParallelComposition)
            {
            }
            return false;
        }


        private void WriteCandidateMatch(int number, Process P1, Action a1, Process P2, Action a2)
        {
            SourceFormatter formatter = new SourceFormatter();
            string result = String.Format("{0}: {1} -> {2}", number, formatter.Format(P1, a1.ID), formatter.Format(P2, a2.ID));
            string[] parts = System.Text.RegularExpressions.Regex.Split(result, "<sel>");
            ConsoleColor original = Console.ForegroundColor;
            
            for (int i = 0; i < parts.Length; i++)
            {
                if (i % 2 == 0)
                {
                    Console.ForegroundColor = original;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.Write(parts[i]);
            }
            Console.ForegroundColor = original;
            Console.WriteLine();
            

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

        private void ResolveProcessConstants()
        {
            List<Process> delete = new List<Process>();
            List<Process> add = new List<Process>();
            foreach (Process p in _activeProcs)
            {
                if (p is ProcessConstant)
                {
                    delete.Add(p);
                }
            }
            foreach (Process p in delete)
            {
                _activeProcs.Remove(p);
            }
            foreach (ProcessDefinition procdef in _system)
            {
                foreach (ProcessConstant pconst in delete) {
                    if (procdef.ProcessConstant.Name == pconst.Name)
                    {
                        _activeProcs.Add(procdef.Process);
                    }
                }
                
            }
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
