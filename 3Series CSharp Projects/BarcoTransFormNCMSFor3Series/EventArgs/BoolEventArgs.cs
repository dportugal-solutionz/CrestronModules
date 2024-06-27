using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace BarcoTransFormNCMSFor3Series
{
    /// <summary>
    /// For use in SIMPL+
    /// </summary>
    public class BoolEventArgs : EventArgs
    {        
        public ushort Value;
        public BoolEventArgs(bool value)
        {
            Value = value ? (ushort)1: (ushort)0;
        }
        public BoolEventArgs()
        {
            Value = 0;
        }
    }
}