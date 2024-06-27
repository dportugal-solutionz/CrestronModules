using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using System.Diagnostics;
using System.Globalization;

namespace BarcoTransFormNCMSFor3Series
{
    public static class Tracer
    {
        public static bool State
        {
            get
            {
                return debug;
            }
        }

        static Tracer()
        {
            debug = true;
            module = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CrestronConsole.PrintLine("["+module+" "+version+"]");

        }
        
        private static readonly string module;
        private static readonly string version;
        //private static string version;
        private static bool debug;
        public static void DebugOn()
        {
            debug = true;
        }
        public static void DebugOff()
        {
            debug = false;
        }
        public static void LogError(string msg)
        {
            ErrorLog.Error(string.Format("[{0}] {1}", module,msg));
        }

        public static void PrintLine(string msg)
        {
            if (debug)
                CrestronConsole.PrintLine(string.Format("[{0} {1}] {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff", CultureInfo.InvariantCulture),module,msg));
        }        
        public static void PrintLine(string format, params object[] args)
        {
            if (debug)
                CrestronConsole.PrintLine(format, args);
        }
    }
}