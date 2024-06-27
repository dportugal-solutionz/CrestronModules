using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace BarcoTransFormNCMSFor3Series
{
    /// For use in SIMPL+
    public class StringArrayEventArgs : EventArgs
    {
        public string Value;
        public ushort Index;

        /// <summary>
        /// An event args with string Value and ushort Index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public StringArrayEventArgs(int index, string value)
        {
            Index = (ushort)index;
            Value = value;
        }
        public StringArrayEventArgs()
        {
            Index = 65535;
            Value = "";
        }
    }
}