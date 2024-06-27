using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace BarcoTransFormNCMSFor3Series
{
    /// For use in SIMPL+
    public class IntEventArgs : EventArgs
    {        
        public ushort Value;

        public IntEventArgs(int value)
        {            
            Value = (ushort)value;
        }
        public IntEventArgs()
        {
            Value = 65535;
        }
    }
}