﻿using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using PLR.Runtime;

namespace ProcessViewer {
    public partial class ProcessViewer : Form {

        #region Members
        Assembly assembly;
        String sourceFile;
        #endregion

        delegate void ProcessChanged(ProcessBase p);
        delegate void TraceChanged(string item);

        public ProcessViewer() {
            InitializeComponent();
            Scheduler.Instance.TraceItemAdded += new TraceEventHandler(TraceItemAdded);
            Scheduler.Instance.ProcessRegistered += new ProcessChangeEventHandler(ProcessRegistered);
            Scheduler.Instance.ProcessKilled += new ProcessChangeEventHandler(ProcessKilled);
            Scheduler.Instance.StepMode = true;
            processThreadWorker.DoWork += new DoWorkEventHandler(RunProcessApp);
            processThreadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExecutionFinished);
        }

        void ExecutionFinished(object sender, RunWorkerCompletedEventArgs e) {
            EnableControls(false);
            Scheduler.Reset(); //Ready to start a new execution
        }


        void ProcessKilled(object sender, ProcessEventArgs e) {
            lstProcesses.Invoke(new ProcessChanged(RemoveProcess), e.Process);
        }

        void ProcessRegistered(object sender, ProcessEventArgs e) {
            lstProcesses.Invoke(new ProcessChanged(AddProcess), e.Process);
        }

        void AddProcess(ProcessBase p) {
            lstProcesses.Items.Add(p);
        }

        void RemoveProcess(ProcessBase p) {
            if (lstProcesses.Items.Contains(p)) {
                lstProcesses.Items.Remove(p);
            }
        }

        void TraceItemAdded(object sender, TraceEventArgs e) {
            lstTrace.Invoke(new TraceChanged(AddToTrace), e.ItemAsString);
        }

        void AddToTrace(string item) {
            lstTrace.Items.Add(item);
        }

        private void EnableControls(bool isRunning) {
            btnPause.Enabled = isRunning;
            btnStop.Enabled = isRunning;
            btnStart.Enabled = !isRunning;
        }

        private void btnStart_Click(object sender, EventArgs e) {
            processThreadWorker.RunWorkerAsync();
            EnableControls(true);
        }

        void RunProcessApp(object sender, DoWorkEventArgs e) {
            MethodInfo mainMethod = this.assembly.GetModules(false)[0].GetMethod("Main");
            try {
                mainMethod.Invoke(null, new object[] { });
            } catch (TargetInvocationException ex) {
                MessageBox.Show(ex.Message + "\n\n" + ex.InnerException);
            }
        }


        private void btnOpen_Click(object sender, EventArgs e) {
            DialogResult result = openProcessExeDialog.ShowDialog();
            if (result == DialogResult.OK) {
                this.assembly = Assembly.Load(File.ReadAllBytes(openProcessExeDialog.FileName));
                btnStart.Enabled = true;
                this.Text = "Process Viewer - " + openProcessExeDialog.FileName;

                result = openProcessSourceDialog.ShowDialog();
                if (result == DialogResult.Cancel) {
                    MessageBox.Show("No source file selected. Visualization of the current state of the system will not be available during execution", "No source file chosen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                } else if (result == DialogResult.OK) {
                    this.sourceFile = openProcessSourceDialog.FileName;
                    txtProcessState.Text = File.ReadAllText(this.sourceFile);
                }

            }
        }

        private void btnStep_Click(object sender, EventArgs e) {
            Scheduler.Instance.MayDoNextStep = true;
        }

    }
}
