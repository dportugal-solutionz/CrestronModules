using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarcoTransFormNCMSFor3Series
{
    /// <summary>
    /// Since the replies from Barco are not contextual we need to keep track of certain things when we send commands
    /// </summary>
    public class Command
    {
        public int WindowIndex = -1;
        public int SourceIndex = -1;
        public int DisplayIndex = -1;
        public string Text = "";
        public Command(string txt, int window, int source, int display)
        {
            Text = txt;
            WindowIndex = window;
            SourceIndex = source;
            DisplayIndex = display;
        }
        public Command (string txt)
        {
            Text = txt;
            WindowIndex = -1;
            SourceIndex = -1;
            DisplayIndex = -1;
        }

        public Command(string txt, int display)
        {
            Text = txt;
            DisplayIndex = display;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
