using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace BarcoTransFormNCMSFor3Series
{
    /// For use in SIMPL+
    public class IntArrayEventArgs : EventArgs
    {

        public ushort Value;
        public ushort Index;
        public IntArrayEventArgs(int index, int value)
        {
            Index = (ushort)index;
            Value = (ushort)value;
        }
        public IntArrayEventArgs()
        {
            Value = 0;
            Index = 65535;
        }
    }
}