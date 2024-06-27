using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace AllenHeath_AHM64
{
    public class StringEventArgs : EventArgs
    {
        public string Data;
        public StringEventArgs()
        {

        }
        public StringEventArgs(string data)
        {
            Data = data;
        }
    }
}