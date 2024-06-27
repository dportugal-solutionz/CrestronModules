using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using System.Globalization;

namespace CrosspointEx
{

    /// <summary>
    /// a class that prefixes the message or error log with a context
    /// </summary>
    public class TracerContext
    {
        public string Context { get; set; }

        public TracerContext(string context)
        {
            Context = context;
        }
        private string Prefix(string msg)
        {
            string m = string.Format("({0}){1}", Context, msg);
            return m;
        }
        
        private string Prefix(string msg, params object[] args)
        {
            string m = "(" + Context + ")" + msg;
            return string.Format(m, args);
        }

        public void Print(string msg)
        {
            Tracer.Print(Prefix(msg));
        }
        
        public void Print(string msg, params object[] args)
        {
            Tracer.Print(Prefix(msg, args));
        }

        public void ErrorLog(string msg)
        {
            Tracer.ErrorLog(Prefix(msg));
        }
        public void ErrorLog(string msg, params object[] args)
        {
            Tracer.ErrorLog(Prefix(msg, args));
        }
    }

    public static class Tracer
    {        
        public static string Module {get; private set;}
        public static string Version { get; private set; }
        public static bool Enabled {get;set;}
        static Tracer()
        {        
            Enabled = true;
            Module = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();            
            CrestronConsole.PrintLine(string.Format("[{0} {1}]", Module, Version));
        }

        public static void SetDebug(ushort value)
        {
            Enabled = value > 0;
        }
        public static string Header
        {
            get
            {
                return string.Format("[{0}]", Module);
            }
        }
        public static string PrintHeader
        {
            get
            {
                /*
                var now = DateTime.Now;
                string ts = now.ToString("");
                string ms = now.Millisecond.ToString("yyyy-MM-dd hh:MM:ss");
                return string.Format("{0}.{1:000} {2} ",ts, ms, Header);
                */
                return string.Format("{0} {1}", 
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture),
                    Header);
            }
        }
        public static void Print(string msg)
        {
            if (Enabled)
            {
                string m = PrintHeader + msg;
                CrestronConsole.PrintLine(m);
            }
        }
        public static void Print(string msg, params object [] args)
        {
            if (Enabled)
            {
                string m = PrintHeader + msg;
                CrestronConsole.PrintLine(m, args);
            }
        }

        public static void ErrorLog(string msg, params object[] args)
        {
            Crestron.SimplSharp.ErrorLog.Error((Header + " " + msg), args);
        }
    }
}