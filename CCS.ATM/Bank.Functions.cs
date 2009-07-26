using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bank {
    public class Functions {
        public static bool ValidPin(int accountNr, int pinNr) {
            //Here we could have something interfacing with the banks
            //actual software...
            DialogResult result = MessageBox.Show("Is the pin " + pinNr + " valid for the account nr " + accountNr + "?", "Validate PIN", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        public static bool WithDrawFromAccount(int accountNr, int amount) {
            //Withdraw from the account, return true if successful.
            return true;
        }
    }
}
