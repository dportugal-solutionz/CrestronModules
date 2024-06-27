using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace JsonFileReadWrite
{
    public class Debugger
    {
        public static bool Debug { get; set; }

        private static string Header()
        {
            return string.Format("{0} [JsonFileReadWrite] ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }

        public Debugger()
        {
            Debug = false;
        }

        public static void Print(string msg)
        {
            try
            {
                string m = Header() + msg;
                CrestronConsole.PrintLine(m);
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("Exception in debugger print.{0}",ex.ToString());
            }
        }

        public static void Print(string msg, params object [] args)
        {
            try
            {                
                string m = Header() + string.Format(msg, args);
                CrestronConsole.PrintLine(m);
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("Exception in debugger print.{0}", ex.ToString());
            }
        }
    }
}