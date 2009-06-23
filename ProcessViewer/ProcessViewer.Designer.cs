namespace ProcessViewer {
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
            this.btnStart = new System.Windows.Forms.ToolStripButton();
            this.btnPause = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.lblTrace = new System.Windows.Forms.Label();
            this.lblProcesses = new System.Windows.Forms.Label();
            this.lstTrace = new System.Windows.Forms.ListBox();
            this.lstProcesses = new System.Windows.Forms.ListBox();
            this.txtProcessState = new System.Windows.Forms.RichTextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnHelp = new System.Windows.Forms.ToolStripButton();
            this.openProcessSourceDialog = new System.Windows.Forms.OpenFileDialog();
            this.processThreadWorker = new System.ComponentModel.BackgroundWorker();
            this.btnStep = new System.Windows.Forms.ToolStripButton();
            this.tlbMain.SuspendLayout();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
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
            this.toolStripSeparator1,
            this.btnStart,
            this.btnPause,
            this.btnStop,
            this.toolStripSeparator2,
            this.btnHelp,
            this.btnStep});
            this.tlbMain.Location = new System.Drawing.Point(0, 0);
            this.tlbMain.Name = "tlbMain";
            this.tlbMain.Size = new System.Drawing.Size(934, 25);
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
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
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
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
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
            this.btnPause.ToolTipText = "Pause process application";
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
            this.splitContainer.Panel1.Controls.Add(this.lblTrace);
            this.splitContainer.Panel1.Controls.Add(this.lblProcesses);
            this.splitContainer.Panel1.Controls.Add(this.lstTrace);
            this.splitContainer.Panel1.Controls.Add(this.lstProcesses);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.txtProcessState);
            this.splitContainer.Size = new System.Drawing.Size(934, 612);
            this.splitContainer.SplitterDistance = 258;
            this.splitContainer.TabIndex = 2;
            // 
            // lblTrace
            // 
            this.lblTrace.AutoSize = true;
            this.lblTrace.Location = new System.Drawing.Point(13, 304);
            this.lblTrace.Name = "lblTrace";
            this.lblTrace.Size = new System.Drawing.Size(35, 13);
            this.lblTrace.TabIndex = 3;
            this.lblTrace.Text = "Trace";
            // 
            // lblProcesses
            // 
            this.lblProcesses.AutoSize = true;
            this.lblProcesses.Location = new System.Drawing.Point(13, 287);
            this.lblProcesses.Name = "lblProcesses";
            this.lblProcesses.Size = new System.Drawing.Size(89, 13);
            this.lblProcesses.TabIndex = 2;
            this.lblProcesses.Text = "Active Processes";
            // 
            // lstTrace
            // 
            this.lstTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstTrace.FormattingEnabled = true;
            this.lstTrace.IntegralHeight = false;
            this.lstTrace.Location = new System.Drawing.Point(12, 320);
            this.lstTrace.Name = "lstTrace";
            this.lstTrace.Size = new System.Drawing.Size(243, 277);
            this.lstTrace.TabIndex = 1;
            // 
            // lstProcesses
            // 
            this.lstProcesses.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstProcesses.FormattingEnabled = true;
            this.lstProcesses.Location = new System.Drawing.Point(12, 16);
            this.lstProcesses.Name = "lstProcesses";
            this.lstProcesses.Size = new System.Drawing.Size(243, 264);
            this.lstProcesses.TabIndex = 0;
            // 
            // txtProcessState
            // 
            this.txtProcessState.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProcessState.Location = new System.Drawing.Point(8, 16);
            this.txtProcessState.Name = "txtProcessState";
            this.txtProcessState.Size = new System.Drawing.Size(652, 581);
            this.txtProcessState.TabIndex = 0;
            this.txtProcessState.Text = "";
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
            // btnHelp
            // 
            this.btnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHelp.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp.Image")));
            this.btnHelp.ImageTransparentColor = System.Drawing.Color.Red;
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(23, 22);
            this.btnHelp.Text = "btnHelp";
            this.btnHelp.ToolTipText = "About Process Viewer...";
            // 
            // openProcessSourceDialog
            // 
            this.openProcessSourceDialog.Filter = "Process source files|*.*";
            this.openProcessSourceDialog.Title = "Open process application source file...";
            // 
            // btnStep
            // 
            this.btnStep.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnStep.Image = ((System.Drawing.Image)(resources.GetObject("btnStep.Image")));
            this.btnStep.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStep.Name = "btnStep";
            this.btnStep.Size = new System.Drawing.Size(34, 22);
            this.btnStep.Text = "Step";
            this.btnStep.Click += new System.EventHandler(this.btnStep_Click);
            // 
            // ProcessViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(934, 637);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.tlbMain);
            this.MinimumSize = new System.Drawing.Size(950, 673);
            this.Name = "ProcessViewer";
            this.Text = "Process Viewer";
            this.tlbMain.ResumeLayout(false);
            this.tlbMain.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openProcessExeDialog;
        private System.Windows.Forms.ToolStrip tlbMain;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnStart;
        private System.Windows.Forms.ToolStripButton btnPause;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label lblProcesses;
        private System.Windows.Forms.ListBox lstTrace;
        private System.Windows.Forms.ListBox lstProcesses;
        private System.Windows.Forms.RichTextBox txtProcessState;
        private System.Windows.Forms.Label lblTrace;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnHelp;
        private System.Windows.Forms.OpenFileDialog openProcessSourceDialog;
        private System.ComponentModel.BackgroundWorker processThreadWorker;
        private System.Windows.Forms.ToolStripButton btnStep;
    }
}

