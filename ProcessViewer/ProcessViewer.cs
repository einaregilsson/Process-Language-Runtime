using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProcessViewer {
    public partial class ProcessViewer : Form {
        public ProcessViewer() {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void openProcessApplicationToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog.ShowDialog();
        }
    }
}
