using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace BarcoTransFormNCMSFor3Series
{
    /// For use in SIMPL+
    public class StringEventArgs : EventArgs
    {        
        public string Value;

        public StringEventArgs()
        {
            Value = "";        
        }

        public StringEventArgs(string value)
        {            
            Value = value;
        }
    }
}