﻿namespace ProcessViewer {
    partial class ProcessViewer {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProcessViewer));
            this.openProcessExeDialog = new System.Windows.Forms.OpenFileDialog();
            this.tlbMain = new System.Windows.Forms.ToolStrip();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnStart = new System.Windows.Forms.ToolStripButton();
            this.btnStartStep = new System.Windows.Forms.ToolStripButton();
            this.btnPause = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.grpTrace = new System.Windows.Forms.GroupBox();
            this.lstTrace = new System.Windows.Forms.ListBox();
            this.grpProcesses = new System.Windows.Forms.GroupBox();
            this.lstProcesses = new System.Windows.Forms.ListBox();
            this.grpCandidateActions = new System.Windows.Forms.GroupBox();
            this.txtCandidateDescription = new System.Windows.Forms.RichTextBox();
            this.btnExecuteRandom = new System.Windows.Forms.Button();
            this.btnChoose = new System.Windows.Forms.Button();
            this.lstCandidateActions = new System.Windows.Forms.ListBox();
            this.txtProcessState = new System.Windows.Forms.RichTextBox();
            this.openProcessSourceDialog = new System.Windows.Forms.OpenFileDialog();
            this.processThreadWorker = new System.ComponentModel.BackgroundWorker();
            this.tlbMain.SuspendLayout();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.grpTrace.SuspendLayout();
            this.grpProcesses.SuspendLayout();
            this.grpCandidateActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // openProcessExeDialog
            // 
            this.openProcessExeDialog.Filter = "Process applications|*.exe";
            this.openProcessExeDialog.RestoreDirectory = true;
            this.openProcessExeDialog.Title = "Open process application...";
            // 
            // tlbMain
            // 
            this.tlbMain.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tlbMain.BackgroundImage")));
            this.tlbMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpen,
            this.toolStripSeparator3,
            this.btnStart,
            this.btnStartStep,
            this.btnPause,
            this.btnStop});
            this.tlbMain.Location = new System.Drawing.Point(0, 0);
            this.tlbMain.Name = "tlbMain";
            this.tlbMain.Size = new System.Drawing.Size(834, 25);
            this.tlbMain.TabIndex = 1;
            this.tlbMain.Text = "toolStrip1";
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Red;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(23, 22);
            this.btnOpen.Text = "Open...";
            this.btnOpen.ToolTipText = "Open process application";
            this.btnOpen.Click += new System.EventHandler(this.OpenAssembly);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // btnStart
            // 
            this.btnStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStart.Enabled = false;
            this.btnStart.Image = ((System.Drawing.Image)(resources.GetObject("btnStart.Image")));
            this.btnStart.ImageTransparentColor = System.Drawing.Color.Red;
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(23, 22);
            this.btnStart.Text = "Start";
            this.btnStart.ToolTipText = "Start process application";
            this.btnStart.Click += new System.EventHandler(this.StartExecution);
            // 
            // btnStartStep
            // 
            this.btnStartStep.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStartStep.Enabled = false;
            this.btnStartStep.Image = ((System.Drawing.Image)(resources.GetObject("btnStartStep.Image")));
            this.btnStartStep.ImageTransparentColor = System.Drawing.Color.Red;
            this.btnStartStep.Name = "btnStartStep";
            this.btnStartStep.Size = new System.Drawing.Size(23, 22);
            this.btnStartStep.Text = "Start execution in step mode";
            this.btnStartStep.Click += new System.EventHandler(this.StartExecutionInStepMode);
            // 
            // btnPause
            // 
            this.btnPause.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPause.Enabled = false;
            this.btnPause.Image = ((System.Drawing.Image)(resources.GetObject("btnPause.Image")));
            this.btnPause.ImageTransparentColor = System.Drawing.Color.Red;
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(23, 22);
            this.btnPause.Text = "Pause";
            this.btnPause.ToolTipText = "Pause process application (enter step mode)";
            this.btnPause.Click += new System.EventHandler(this.PauseExecution);
            // 
            // btnStop
            // 
            this.btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStop.Enabled = false;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageTransparentColor = System.Drawing.Color.Red;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(23, 22);
            this.btnStop.Text = "Stop";
            this.btnStop.ToolTipText = "Stop process application";
            this.btnStop.Click += new System.EventHandler(this.StopExecution);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.Location = new System.Drawing.Point(0, 25);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.grpTrace);
            this.splitContainer.Panel1.Controls.Add(this.grpProcesses);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.grpCandidateActions);
            this.splitContainer.Panel2.Controls.Add(this.txtProcessState);
            this.splitContainer.Size = new System.Drawing.Size(834, 568);
            this.splitContainer.SplitterDistance = 258;
            this.splitContainer.TabIndex = 2;
            // 
            // grpTrace
            // 
            this.grpTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTrace.Controls.Add(this.lstTrace);
            this.grpTrace.Location = new System.Drawing.Point(12, 290);
            this.grpTrace.Name = "grpTrace";
            this.grpTrace.Size = new System.Drawing.Size(243, 268);
            this.grpTrace.TabIndex = 5;
            this.grpTrace.TabStop = false;
            this.grpTrace.Text = "Trace";
            // 
            // lstTrace
            // 
            this.lstTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstTrace.FormattingEnabled = true;
            this.lstTrace.IntegralHeight = false;
            this.lstTrace.Location = new System.Drawing.Point(6, 19);
            this.lstTrace.Name = "lstTrace";
            this.lstTrace.Size = new System.Drawing.Size(231, 243);
            this.lstTrace.TabIndex = 1;
            // 
            // grpProcesses
            // 
            this.grpProcesses.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpProcesses.Controls.Add(this.lstProcesses);
            this.grpProcesses.Location = new System.Drawing.Point(12, 16);
            this.grpProcesses.Name = "grpProcesses";
            this.grpProcesses.Size = new System.Drawing.Size(243, 268);
            this.grpProcesses.TabIndex = 4;
            this.grpProcesses.TabStop = false;
            this.grpProcesses.Text = "Active Processes";
            // 
            // lstProcesses
            // 
            this.lstProcesses.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstProcesses.FormattingEnabled = true;
            this.lstProcesses.Location = new System.Drawing.Point(6, 19);
            this.lstProcesses.Name = "lstProcesses";
            this.lstProcesses.Size = new System.Drawing.Size(231, 238);
            this.lstProcesses.TabIndex = 0;
            // 
            // grpCandidateActions
            // 
            this.grpCandidateActions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCandidateActions.Controls.Add(this.txtCandidateDescription);
            this.grpCandidateActions.Controls.Add(this.btnExecuteRandom);
            this.grpCandidateActions.Controls.Add(this.btnChoose);
            this.grpCandidateActions.Controls.Add(this.lstCandidateActions);
            this.grpCandidateActions.Location = new System.Drawing.Point(8, 370);
            this.grpCandidateActions.Name = "grpCandidateActions";
            this.grpCandidateActions.Size = new System.Drawing.Size(552, 188);
            this.grpCandidateActions.TabIndex = 1;
            this.grpCandidateActions.TabStop = false;
            this.grpCandidateActions.Text = "Next action";
            // 
            // txtCandidateDescription
            // 
            this.txtCandidateDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCandidateDescription.BackColor = System.Drawing.SystemColors.Control;
            this.txtCandidateDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCandidateDescription.Location = new System.Drawing.Point(326, 19);
            this.txtCandidateDescription.Name = "txtCandidateDescription";
            this.txtCandidateDescription.ReadOnly = true;
            this.txtCandidateDescription.Size = new System.Drawing.Size(220, 163);
            this.txtCandidateDescription.TabIndex = 3;
            this.txtCandidateDescription.Text = "";
            // 
            // btnExecuteRandom
            // 
            this.btnExecuteRandom.Enabled = false;
            this.btnExecuteRandom.Location = new System.Drawing.Point(213, 50);
            this.btnExecuteRandom.Name = "btnExecuteRandom";
            this.btnExecuteRandom.Size = new System.Drawing.Size(106, 23);
            this.btnExecuteRandom.TabIndex = 2;
            this.btnExecuteRandom.Text = "Execute randomly";
            this.btnExecuteRandom.UseVisualStyleBackColor = true;
            this.btnExecuteRandom.Click += new System.EventHandler(this.ExecuteRandomAction);
            // 
            // btnChoose
            // 
            this.btnChoose.Enabled = false;
            this.btnChoose.Location = new System.Drawing.Point(212, 20);
            this.btnChoose.Name = "btnChoose";
            this.btnChoose.Size = new System.Drawing.Size(107, 23);
            this.btnChoose.TabIndex = 1;
            this.btnChoose.Text = "Execute selected";
            this.btnChoose.UseVisualStyleBackColor = true;
            this.btnChoose.Click += new System.EventHandler(this.ExecuteSelectedAction);
            // 
            // lstCandidateActions
            // 
            this.lstCandidateActions.FormattingEnabled = true;
            this.lstCandidateActions.IntegralHeight = false;
            this.lstCandidateActions.Location = new System.Drawing.Point(6, 19);
            this.lstCandidateActions.Name = "lstCandidateActions";
            this.lstCandidateActions.Size = new System.Drawing.Size(199, 163);
            this.lstCandidateActions.TabIndex = 0;
            this.lstCandidateActions.SelectedIndexChanged += new System.EventHandler(this.CandidateActionHighlighted);
            // 
            // txtProcessState
            // 
            this.txtProcessState.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProcessState.Location = new System.Drawing.Point(8, 16);
            this.txtProcessState.Name = "txtProcessState";
            this.txtProcessState.Size = new System.Drawing.Size(552, 348);
            this.txtProcessState.TabIndex = 0;
            this.txtProcessState.Text = "";
            // 
            // openProcessSourceDialog
            // 
            this.openProcessSourceDialog.Filter = "Process source files|*.*";
            this.openProcessSourceDialog.Title = "Open process application source file...";
            // 
            // ProcessViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 593);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.tlbMain);
            this.MinimumSize = new System.Drawing.Size(850, 629);
            this.Name = "ProcessViewer";
            this.Text = "Process Viewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProcessViewer_FormClosed);
            this.tlbMain.ResumeLayout(false);
            this.tlbMain.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.grpTrace.ResumeLayout(false);
            this.grpProcesses.ResumeLayout(false);
            this.grpCandidateActions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openProcessExeDialog;
        private System.Windows.Forms.ToolStrip tlbMain;
        private System.Windows.Forms.ToolStripButton btnStart;
        private System.Windows.Forms.ToolStripButton btnPause;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListBox lstTrace;
        private System.Windows.Forms.ListBox lstProcesses;
        private System.Windows.Forms.RichTextBox txtProcessState;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.OpenFileDialog openProcessSourceDialog;
        private System.ComponentModel.BackgroundWorker processThreadWorker;
        private System.Windows.Forms.GroupBox grpCandidateActions;
        private System.Windows.Forms.ListBox lstCandidateActions;
        private System.Windows.Forms.Button btnExecuteRandom;
        private System.Windows.Forms.Button btnChoose;
        private System.Windows.Forms.GroupBox grpTrace;
        private System.Windows.Forms.GroupBox grpProcesses;
        private System.Windows.Forms.ToolStripButton btnStartStep;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.RichTextBox txtCandidateDescription;
    }
}

