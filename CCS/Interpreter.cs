using System;
using System.Collections.Generic;
using System.Text;
using PLR.AST;
using PLR.AST.Processes;
using PLR.AST.Expressions;
using PLR.AST.Actions;
using CCS.Formatters;
using System.IO;

namespace CCS
{
    public delegate int ChooseAction();
    public class Interpreter
    {
        private List<Process> _activeProcs = new List<Process>();
        private ProcessSystem _system;
        private bool _interactive;
        private Random _rng = new Random();
        private long _iteration;
        private List<string> _trace = new List<string>();

        public Interpreter(ProcessSystem system, bool interactive)
        {
            this._interactive = interactive;
            _system = system;
            _iteration = 0;
            foreach (ProcessDefinition procdef in system)
            {
                if (procdef.EntryProc)
                {
                    _activeProcs.Add(procdef.Process);
                }
            }

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

            public override int GetHashCode() {
                string[] temp = new string[] { P1.ToString(), P2.ToString(), A1.ToString(), A2.ToString() };
                List<string> list = new List<string>(temp);
                list.Sort();
                return string.Join(",", list.ToArray()).GetHashCode();
            }
        }

        private List<Match> _matches;

        public int RunConsole() {
            int? choice = null;
            StringWriter tempBuffer;
            while (true) {
                ConsoleColor original = Console.ForegroundColor;
                tempBuffer = new StringWriter();
                int options = Iterate(tempBuffer, choice);
                int pos = tempBuffer.ToString().IndexOf('\n', 4);
                string header = tempBuffer.ToString().Substring(0, pos);
                string body = tempBuffer.ToString().Substring(pos+1);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(header);
                Console.ForegroundColor = original;
                string[] parts = System.Text.RegularExpressions.Regex.Split(body, "</?sel>");

                for (int i = 0; i < parts.Length; i++) {
                    if (i % 2 == 0) {
                        Console.ForegroundColor = original;
                    } else {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    Console.Write(parts[i]);
                }
                Console.ForegroundColor = original;
                
                if (options == 0) {
                    Console.ReadKey();
                    return 3;
                }
                if (_interactive) {
                    int ch = -1;
                    while (!int.TryParse(Console.ReadLine(), out ch) || ch < 1 || ch > options) {
                        Console.Write("Please enter a number between 1 and {0}: ", options);
                    }
                    choice = ch;
                } else {
                    choice = _rng.Next(options)+1;
                    Console.WriteLine("Randomly chose option {0}", choice);
                    if (_iteration % 50 == 0) {
                        Console.WriteLine("\n{0} iterations are finished, press Ctrl+C to quit, or any other key to run the next 50 iterations...", _iteration);
                        Console.ReadKey();
                    }
                }
            }
        }

        public int Iterate(TextWriter writer, int? choice)
        {
            if (choice.HasValue) {
                Match chosen = _matches[choice.Value-1];
                _trace.Add(chosen.A1.Name);
                _activeProcs.Remove(chosen.P1);
                _activeProcs.Remove(chosen.P2);
                AddProcessToActiveSet(chosen.P1, chosen.A1);
                AddProcessToActiveSet(chosen.P2, chosen.A2);
            }

            Dictionary<Process, List<Action>> possibleSyncs = new Dictionary<Process, List<Action>>();
            SourceFormatter formatter = new SourceFormatter();
            _iteration++;

            possibleSyncs.Clear();
            SplitUpParallelProcesses();
            ResolveProcessConstants();
            _matches = new List<Match>();
            int i = 1;
            writer.WriteLine("\n\n****** Iteration {0} ******", _iteration);
            writer.WriteLine("\nTrace so far: <" + string.Join(", ", _trace.ToArray()) + ">");
            writer.WriteLine("\nActive Processes:\n");
            foreach (Process p in _activeProcs)
            {
                possibleSyncs.Add(p, GetAvailableActions(p));
                writer.WriteLine("  P{0}: {1}", i++, formatter.Format(p));
            }

            writer.WriteLine("\nPossible actions:\n");
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
                                        if (!_matches.Contains(m))
                                        {
                                            _matches.Add(m);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (_matches.Count == 0) {
                writer.WriteLine("\nSystem is deadlocked");
                return 0;
            } else {
                int b = 1;
                foreach (Match m in _matches) {
                    if (m.A1 is OutAction) {
                        WriteCandidateMatch(b++, m.P1, m.A1, m.P2, m.A2, writer);
                    } else {
                        WriteCandidateMatch(b++, m.P2, m.A2, m.P1, m.A1, writer);
                    }
                }
            }
            writer.Write("\nSelect the number of the action to take: ");
            return _matches.Count;
        }

        //private Process GetProcess(ProcessConstant pconst)
        //{
        //    foreach (ProcessDefinition def in _system)
        //    {
        //        if (def.ProcessConstant.Equals(pconst))
        //        {
        //            return def.Process;
        //        }
        //    }
        //    return null;
        //}
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


        private void WriteCandidateMatch(int number, Process P1, Action a1, Process P2, Action a2, TextWriter writer)
        {
            SourceFormatter formatter = new SourceFormatter();
            string result = String.Format("  {0}: {1} -> {2}", number, formatter.Format(P1, a1.ID), formatter.Format(P2, a2.ID));
            writer.WriteLine(result);
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
            //List<Process> add = new List<Process>();
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
