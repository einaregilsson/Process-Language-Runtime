/**
 * $Id$ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PLR.Compilation {
    public class CompileOptions {

        #region Static initalization and members
        private static List<Option> _options = new List<Option>();

        static CompileOptions() {
            AddOption("d", "debug");
            AddOption("e", "embedPLR");
            AddOption("op", "optimize");
            AddOption("h", "help");
            AddOptionWithArgument("r", "reference", "");
            AddOptionWithArgument("o", "out", "");
        }

        public static void AddOption(string shortForm, string longForm) {
            _options.Add(new Option(shortForm, longForm));
        }

        public static void AddOptionWithArgument(string shortForm, string longForm, string defaultArgument) {
            _options.Add(new Option(shortForm, longForm, defaultArgument));
        }
        #endregion

        private Dictionary<string, string> _definedOpts = new Dictionary<string, string>();
        private List<string> _arguments = new List<string>();
        public List<string> Arguments { get { return _arguments; } }


        #region Statically typed getters
        public bool Debug { 
            get { return this.Contains("debug"); }
            set { this["debug"] = ""; }
        }

        public bool Help {
            get { return this.Contains("help"); }
            set { this["help"] = ""; }
        }

        public bool Optimize {
            get { return this.Contains("optimize"); }
            set { this["optimize"] = ""; }
        }

        public bool EmbedPLR { 
            get { return this.Contains("embedPLR"); }
            set { this["embedPLR"] = ""; }
        }
        public string OutputFile { 
            get { return this["out"]; }
            set { this["out"] = value; } 
        }

        public string References {
            get { return this["reference"]; }
            set { this["reference"] = value; }
        }
        
        #endregion


        private string Normalize(string key) {
            if (key.Length > 1) {
                return key.ToLower(); ;
            }
            foreach (Option opt in _options) {
                if (key.ToLower() == opt.ShortForm) {
                    return opt.LongForm;
                }
            }
            return null;
        }

        public bool Contains(string key) {
            return this[key] != null;
        }
        public string this[string key] {
            get {
                if (!_definedOpts.ContainsKey(Normalize(key))) {
                    return null;
                } else {
                    return _definedOpts[Normalize(key)];
                }
                
            }
            set {
                key = Normalize(key);
                if (this[key] != null) {
                    _definedOpts[key] = value;
                } else {
                    _definedOpts.Add(key, value);
                }
            }
        }

        private CompileOptions() { }

        public static CompileOptions Parse(List<string> args) {
            CompileOptions options = new CompileOptions();
            foreach (Option opt in _options) {
                if (opt.TakesArgument) { //Set default values
                    options._definedOpts.Add(opt.LongForm.ToLower(), opt.Argument);
                }
            }

            for (int i = args.Count - 1; i >= 0; i--) {
                if (!args[i].StartsWith("/") && !args[i].StartsWith("-")) {
                    options._arguments.Insert(0, args[i]);
                } else {
                    bool processed = false;
                    foreach (Option opt in _options) {
                        //Shortcut
                        if (Regex.IsMatch(args[i], @"(/|--?)\?")) {
                            options.Help = true;
                            processed = true;
                            continue;
                        }

                        if (!opt.TakesArgument && Regex.IsMatch(args[i].ToLower(), string.Format("^(-{0}|/{0}|--{1}|/{1})$", opt.ShortForm, opt.LongForm), RegexOptions.IgnoreCase)) {
                            options._definedOpts.Add(opt.LongForm, "");
                            processed = true;
                        } else if (opt.TakesArgument && Regex.IsMatch(args[i].ToLower(), string.Format("^(-{0}=?|/{0}:?|--{1}=?|/{1}:?)$", opt.ShortForm, opt.LongForm), RegexOptions.IgnoreCase)) {
                            Console.Error.WriteLine("Missing argument for parameter " + args[i]);
                            System.Environment.Exit(1);
                        } else if (!opt.TakesArgument && Regex.IsMatch(args[i].ToLower(), string.Format("^(-{0}=|/{0}:|--{1}=|/{1}:)$", opt.ShortForm, opt.LongForm), RegexOptions.IgnoreCase)) {
                            Console.Error.WriteLine("Parameter " + args[i] + " does not take an argument!");
                            System.Environment.Exit(1);
                        }

                        Match m = Regex.Match(args[i].ToLower(), string.Format("^(-{0}=|/{0}:|--{1}=|/{1}:)(.+)$", opt.ShortForm, opt.LongForm), RegexOptions.IgnoreCase);
                        if (opt.TakesArgument && m.Success) {
                            options._definedOpts[opt.LongForm] = m.Groups[2].Value;
                            processed = true;
                        }

                    }
                    if (!processed) {
                        Console.Error.WriteLine("ERROR: Unknown parameter '{0}'", args[i]);
                        System.Environment.Exit(1);
                    }
                }
            }
            return options;
        }

        public class Option {
            private string _shortForm;
            private string _longForm;
            private string _argument = "";
            private bool _takesArg = false;

            public Option(string shortForm, string longForm) {
                _shortForm = shortForm.ToLower();
                _longForm = longForm.ToLower();
            }

            public Option(string shortForm, string longForm, string argument) {
                _shortForm = shortForm;
                _longForm = longForm;
                _argument = argument;
                _takesArg = true;
            }

            public bool TakesArgument { get { return _takesArg; } }
            public string ShortForm { get { return _shortForm; } }
            public string LongForm { get { return _longForm; } }
            public string Argument { get { return _argument; } }

            public override int GetHashCode()
            {
                return (_shortForm + "__" + _longForm).GetHashCode();
            }
        }
    }
}

