using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CrosspointEx
{
    public class SerialCue : Cue<string>
    {
        public SerialCue(string address)
            :base(address)
        {

        }
    }
}