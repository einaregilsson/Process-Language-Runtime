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
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using PLR;
using PLR.AST;
using PLR.Runtime;

namespace ProcessViewer {
    
    public partial class ProcessViewer : Form {

        enum RunStatus {
            Stopped,
            Running,
            Stepping
        }

        #region Fields
        Assembly _assembly;
        string _sourceFile;
        CandidateAction _chosenAction = null;
        Random _rng = new Random();
        RunStatus _status = RunStatus.Stopped;
        Dictionary<string, IParser> _parsers = new Dictionary<string, IParser>();
        ProcessSystem _loadedSystem;
        ProcessStateVisualization _visual;
        IParser _parser;
        List<ProcessBase> _processes = new List<ProcessBase>();
        bool _firstStep = true;
        #endregion

        #region Delegates
        delegate void ProcessChanged(ProcessBase p);
        delegate void TraceChanged(string item);
        delegate void NewCandidateActionsDelegate(List<CandidateAction> candidates);
        #endregion

        #region Constructor
        public ProcessViewer() {
            InitializeComponent();
            Scheduler.Instance.TraceItemAdded += new TraceEventHandler(TraceItemAdded);
            Scheduler.Instance.ProcessRegistered += new ProcessChangeEventHandler(ProcessRegistered);
            Scheduler.Instance.ProcessKilled += new ProcessChangeEventHandler(ProcessKilled);
            Scheduler.Instance.SelectAction = new SelectActionToExecute(this.ChooseActionInterActive);
            processThreadWorker.DoWork += new DoWorkEventHandler(RunProcessApp);
            processThreadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExecutionFinished);
            LoadParsers();
        }
        #endregion

        #region Private methods

        private void ErrorMsg(string title, string msg) {
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void LoadParsers() {
            AppSettingsReader reader = new AppSettingsReader();
            string assemblies = (string) reader.GetValue("ParserAssemblies", typeof(string));
            string currentPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            foreach (string assemblyPath in assemblies.Split(';')) {

                string fullPath = Path.Combine(currentPath, assemblyPath);
                try {
                    Assembly ass = Assembly.LoadFile(fullPath);
                    bool foundParser = false;
                    foreach (Type t in ass.GetTypes()) {
                        if (typeof(IParser).IsAssignableFrom(t)) {
                            foundParser = true;
                            try {
                                IParser parser = (IParser)Activator.CreateInstance(t);
                                foreach (string ext in parser.FileExtensions.Split(';')) {
                                    if (_parsers.ContainsKey(ext.ToLower())) {
                                        string msg = "Cannot register " + ext + " as the file extension for " + parser.Language + ", extension is already registered!";
                                        ErrorMsg("Failed to register parser", msg);
                                    } else {
                                        _parsers.Add(ext.ToLower(), parser);
                                    }
                                }
                            } catch (Exception ex) {
                                ErrorMsg("Failed to create parser", "Failed to create instance of " + t.FullName + " and cast it to IParser. Error: " + ex.Message);
                            }
                        }
                    }
                    if (!foundParser) {
                        ErrorMsg("Found no parser", "No implementation of IParser was found in assembly " + assemblyPath);
                    }
                } catch (Exception ex) {
                    ErrorMsg("Failed to load parser assembly", "Failed to load parser assembly " + assemblyPath + ", error: " + ex.Message);
                }
            }
        }

        private CandidateAction ChooseActionInterActive(List<CandidateAction> candidates) {
            var procs = new List<ProcessBase>();
            foreach (ProcessBase p in lstProcesses.Items) {
                procs.Add(p);
            }

            if (_visual != null && _chosenAction != null) {
                _visual.NewState(procs, _chosenAction);
                txtProcessState.Invoke(new SetProcessStateDelegate(SetProcessState), _visual.ToString());
            }
            
            _chosenAction = null;

            lstCandidateActions.Invoke(new NewCandidateActionsDelegate(NewCandidateActions), candidates);



            if (_status == RunStatus.Stepping) {
                if (_firstStep) {
                    if (_visual != null) {
                        _visual.NewState(procs, null);
                        _firstStep = false;
                        txtProcessState.Invoke(new SetProcessStateDelegate(SetProcessState), _visual.ToString());
                    }
                }

                while (_chosenAction == null) {
                    Thread.Sleep(500); //Wait until user has selected something...
                }
                return _chosenAction;
            } else {
                return candidates[_rng.Next(candidates.Count)];
            }
        }

        private void NewCandidateActions(List<CandidateAction> candidates) {
            lstCandidateActions.Items.Clear();
            foreach (CandidateAction action in candidates) {
                lstCandidateActions.Items.Add(action);
            }
            if (candidates.Count > 0) {
                lstCandidateActions.SelectedIndex = 0;
                btnExecuteRandom.Enabled = true;
                btnChoose.Enabled = true;
            }
        }

        private void AddProcess(ProcessBase p) {
            _processes.Add(p);
            UpdateProcesses();
        }

        private void RemoveProcess(ProcessBase p) {
            if (_processes.Contains(p)) {
                _processes.Remove(p);
                UpdateProcesses();
            }
        }

        private void UpdateProcesses() {
            lstProcesses.DataSource = null;
            lstProcesses.DataSource = _processes;
        }

        private void AddToTrace(string item) {
            lstTrace.Items.Add(item);
        }

        private void EnableControls() {
            btnPause.Enabled = (_status == RunStatus.Running);
            btnStop.Enabled = (_status == RunStatus.Running || _status == RunStatus.Stepping);
            btnStart.Enabled = (_status == RunStatus.Stopped || _status == RunStatus.Stepping);
            btnStartStep.Enabled = (_status == RunStatus.Stopped);
            btnExecuteRandom.Enabled = (_status == RunStatus.Stepping);
            btnChoose.Enabled = (_status == RunStatus.Stepping);
        }
        #endregion

        #region Event handlers
        private void ExecutionFinished(object sender, RunWorkerCompletedEventArgs e) {
            _status = RunStatus.Stopped;
            EnableControls();
            Scheduler.Instance.Reset(); //Ready to start a new execution
        }

        private void ProcessKilled(object sender, ProcessEventArgs e) {
            lstProcesses.Invoke(new ProcessChanged(RemoveProcess), e.Process);
        }

        private void ProcessRegistered(object sender, ProcessEventArgs e) {
            lstProcesses.Invoke(new ProcessChanged(AddProcess), e.Process);
        }

        private void TraceItemAdded(object sender, TraceEventArgs e) {
            lstTrace.Invoke(new TraceChanged(AddToTrace), e.ExecutedAction.ToString());
            if (e.ExecutedAction.IsDeadlocked) {
                if (_visual != null && _chosenAction != null) {
                    var procs = new List<ProcessBase>();
                    foreach (ProcessBase p in lstProcesses.Items) {
                        procs.Add(p);
                    }
                    _visual.NewState(procs, _chosenAction);
                    txtProcessState.Invoke(new SetProcessStateDelegate(SetProcessState), _visual.ToString());
                }
            }

        }

        private void StartExecution(object sender, EventArgs e) {
            if (_status == RunStatus.Stopped) {
                _firstStep = true;
                if (_loadedSystem != null) {
                    _visual = new ProcessStateVisualization(_loadedSystem, _parser.Formatter);
                }
                _status = RunStatus.Running;
                ClearAll();
                processThreadWorker.RunWorkerAsync();
            } else if (_status == RunStatus.Stepping) {
                _status = RunStatus.Running;
                if (lstCandidateActions.Items.Count > 0) {
                    _chosenAction = (CandidateAction)lstCandidateActions.Items[_rng.Next(lstCandidateActions.Items.Count)];
                }
            }
            EnableControls();
        }

        private void StartExecutionInStepMode(object sender, EventArgs e) {
            if (_status == RunStatus.Stopped) {
                _firstStep = true;
                if (_loadedSystem != null) {
                    _visual = new ProcessStateVisualization(_loadedSystem, _parser.Formatter);
                }
            }
            _status = RunStatus.Stepping;
            ClearAll();
            processThreadWorker.RunWorkerAsync();
            EnableControls();
        }

        private void RunProcessApp(object sender, DoWorkEventArgs e) {
            MethodInfo mainMethod = this._assembly.GetModules(false)[0].GetMethod("Main");

            try {
                mainMethod.Invoke(null, new object[] { });
            } catch (TargetInvocationException ex) {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException);
            }
        }

        private void ClearAll() {
            lstProcesses.DataSource = null;
            lstTrace.Items.Clear();
            lstCandidateActions.Items.Clear();
        }

        delegate void SetProcessStateDelegate(string state);
        private void SetProcessState(string state) {
            txtProcessState.Text = state;
        }

        private void OpenAssembly(object sender, EventArgs e) {
            DialogResult result = openProcessExeDialog.ShowDialog();
            if (result == DialogResult.OK) {
                try {
                    _assembly = Assembly.Load(File.ReadAllBytes(openProcessExeDialog.FileName));
                    MethodInfo m = _assembly.GetModules(false)[0].GetMethod("Main");
                    if (m == null) {
                        MessageBox.Show("Unable to open assembly " + openProcessExeDialog.FileName + ", it does not appear to have been compiled with the PLR!", "Failed to open assembly", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                } catch (Exception) {
                    MessageBox.Show("Unable to open assembly " + openProcessExeDialog.FileName + ", it does not appear to be a .NET assembly!", "Failed to open assembly", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                btnStart.Enabled = true;
                btnStartStep.Enabled = true;
                this.Text = "Process Viewer - " + openProcessExeDialog.FileName;
                ClearAll();
                result = openProcessSourceDialog.ShowDialog();
                _loadedSystem = null;
                _visual = null;
                txtProcessState.Text = "";
                if (result == DialogResult.Cancel) {
                    MessageBox.Show("No source file selected. Visualization of the current state of the system will not be available during execution", "No source file chosen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                } else if (result == DialogResult.OK) {
                    _sourceFile = openProcessSourceDialog.FileName;
                    txtProcessState.Text = File.ReadAllText(_sourceFile);
                    string ext = Path.GetExtension(_sourceFile).ToLower().Substring(1);
                    if (!_parsers.ContainsKey(ext)) {
                        ErrorMsg("No suitable parser found", "No parser is loaded that parses files of type \"" + ext + "\"");
                    } else {
                        _parser = _parsers[ext];
                        try {
                            _loadedSystem = _parser.Parse(_sourceFile);
                            FindUnsupportedNodes f = new FindUnsupportedNodes();
                            f.Start(_loadedSystem);
                            if (f.typeName != "") {
                                MessageBox.Show("Sorry, visualization are currently not supported for systems that use the class " + f.typeName + ", however, the application can be run without visualization!", "Cannot show visualization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                _loadedSystem = null;
                            }
                        } catch (ParseException pex) {
                            ErrorMsg("Failed to parse source file", "Failed to parse source file " + _sourceFile + ", error: \n" + pex.Message);
                        }
                    }
                }

            }
        }

        private class FindUnsupportedNodes : AbstractVisitor {
            public string typeName = "";
            public override void Visit(PLR.AST.Processes.BranchProcess node) {
                typeName = "BranchProcess";
            }
        }

        private void ExecuteRandomAction(object sender, EventArgs e) {
            if (lstCandidateActions.Items.Count > 0) {
                _chosenAction = (CandidateAction)lstCandidateActions.Items[_rng.Next(lstCandidateActions.Items.Count)];
            }
        }

        private void ExecuteSelectedAction(object sender, EventArgs e) {
            if (lstCandidateActions.SelectedItem != null) {
                _chosenAction = (CandidateAction)lstCandidateActions.SelectedItem;
            }
        }

        private void PauseExecution(object sender, EventArgs e) {
            _status = RunStatus.Stepping;
            EnableControls();
        }

        private void StopExecution(object sender, EventArgs e) {
            Scheduler.Instance.StopAllProcesses();
            ExecuteRandomAction(this, null);
        }

        private void CandidateActionHighlighted(object sender, EventArgs e) {
            txtCandidateDescription.Text = "";
            CandidateAction action = (CandidateAction)lstCandidateActions.SelectedItem;
            if (action == null) {
                return;
            }
            if (action.IsSync) {
                txtCandidateDescription.Text = "Synchronization between\n\n" + action.Process1 + "\n\nand\n\n" + action.Process2;
                if (action.IsTau) {
                    txtCandidateDescription.Text += "\n\non restricted channel " + action.ToString() + " (tau action)";
                } else {
                    txtCandidateDescription.Text += "\n\non channel " + action.ToString();
                }
            } else {
                txtCandidateDescription.Text = "Asynchronous action " + action + " by \n\n" + action.Process1;
            }
        }

        #endregion
    }
}