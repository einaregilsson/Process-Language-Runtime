using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bank {
    public class Functions {
        public static bool ValidPin(int accountNr, int pinNr) {
            //Here we could have something interfacing with the banks
            //actual software...
            return true;
        }

        public static bool WithDrawFromAccount(int accountNr, int amount) {
            //Withdraw from the account, return true if successful.
            return true;
        }
    }
}
