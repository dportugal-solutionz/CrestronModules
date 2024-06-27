using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace AllenHeath_AHM64
{
    public class UShortArrayEventArgs : EventArgs
    {
        public ushort Data { get; set; }
        public ushort Index { get; set; }
        public UShortArrayEventArgs()
        {
        }
        public UShortArrayEventArgs(int index, int data)
        {
            this.Data = (ushort)data;
            this.Index = (ushort)index;
        }
    }
}