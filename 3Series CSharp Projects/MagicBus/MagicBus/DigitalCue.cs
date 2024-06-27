using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using CrosspointEx;

namespace CrosspointEx
{
    public class DigitalCue : Cue<ushort>
    {
        public DigitalCue(string address)
            :base(address)
        {

        }
    }
}