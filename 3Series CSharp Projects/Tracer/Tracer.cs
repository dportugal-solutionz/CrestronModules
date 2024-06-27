using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Tracing
{    
    public class Tracer
    {        
        /// <summary>
        /// the name of the Tracer
        /// </summary>
        public string Name { get; set; }
        public LogLevel Level { get; set; }
        public string Timestamp
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
        }
        public string Header
        {
            get
            {
                return string.Format("{0} [{1}] ",Timestamp, Name);
            }
        }

        public Tracer() 
        {
            Level = LogLevel.Debug;
        }

        /*
        public void Print(string msg)
        {
            CrestronConsole.PrintLine(Header + msg);
        }
         */
        public void Print(string msg)
        {
            CrestronConsole.PrintLine(Header + msg);
        }
        public void Print1(string msg, string arg)
        {            
            CrestronConsole.PrintLine(Header + msg, arg);
        }
        public void Print2(string msg, string arg1, string arg2)
        {
            CrestronConsole.PrintLine(Header + msg, arg1, arg2);
        }
        public void Print3(string msg, string arg1, string arg2, string arg3)
        {
            CrestronConsole.PrintLine(Header + msg, arg1, arg2, arg3);
        }
        private void PrintLevel(LogLevel level, string msg, params object[] args)
        {
            string m = string.Format("{0} {1,9} ", Header,level);
            m += msg;
            CrestronConsole.PrintLine(string.Format(m, args));
        }
        /*
        /// <summary>
        /// publish a Debug message
        /// </summary>
        /// <param name="msg"></param>
        public void Debug(string msg)
        {
            if (Level >= LogLevel.Debug)
                Print(msg);
        }
        */
        public void Debug(string msg)
        {
            if (Level >= LogLevel.Debug)
                PrintLevel(LogLevel.Debug, msg,null);
        }
        public void Debug1(string msg, string args)
        {
            if (Level >= LogLevel.Debug)
                PrintLevel(LogLevel.Debug, msg, args);
        }
        public void Debug2(string msg, string arg1, string arg2)
        {
            if (Level >= LogLevel.Debug)
                PrintLevel(LogLevel.Debug, msg, arg1, arg2);
        }
        /*
        /// <summary>
        /// publish an Info message
        /// </summary>
        /// <param name="msg"></param>
        public void Info(string msg)
        {
            if (Level >= LogLevel.Info)
                Print(msg);
        }
        */
        public void Info(string msg)
        {
            if (Level >= LogLevel.Info)
                PrintLevel(LogLevel.Info, msg);
        }
        public void Info1(string msg, string args)
        {
            if (Level >= LogLevel.Info)
                PrintLevel(LogLevel.Info, msg, args);
        }
        public void Info2(string msg, string arg1, string arg2)
        {
            if (Level >= LogLevel.Info)
                PrintLevel(LogLevel.Info, msg, arg1, arg2);
        }
        /*
        /// <summary>
        /// publish a Warning message
        /// </summary>
        /// <param name="msg"></param>
        public void Warning(string msg)
        {
            if (Level >= LogLevel.Warning)
                Print(msg);
        }
        */
        public void Warning(string msg)
        {
            if (Level >= LogLevel.Warning)
                PrintLevel(LogLevel.Warning, msg);
        }
        public void Warning1(string msg, string args)
        {
            if (Level >= LogLevel.Warning)
                PrintLevel(LogLevel.Warning, msg, args);
        }
        public void Warning2(string msg, string arg1, string arg2)
        {
            if (Level >= LogLevel.Warning)
                PrintLevel(LogLevel.Warning, msg, arg1, arg2);
        }
        /*
        /// <summary>
        /// publish an Error Message
        /// </summary>
        /// <param name="msg"></param>
        public void Error(string msg)
        {
            if (Level >= LogLevel.Error)
                Print(msg);
        }
         * */
        public void Error(string msg)
        {
            if (Level >= LogLevel.Error)
                PrintLevel(LogLevel.Error, msg);
        }
        public void Error1(string msg, string args)
        {
            if (Level >= LogLevel.Error)
                PrintLevel(LogLevel.Error, msg, args);
        }
        public void Error2(string msg, string arg1, string arg2)
        {
            if (Level >= LogLevel.Error)
                PrintLevel(LogLevel.Error, msg, arg1, arg2);
        }
        /*
        /// <summary>
        /// publish a Critical Message
        /// </summary>
        /// <param name="msg"></param>
        public void Critical(string msg)
        {
            if (Level >= LogLevel.Critical)
                Print(msg);
        } 
         * */
        public void Critical(string msg)
        {
            if (Level >= LogLevel.Critical)
                PrintLevel(LogLevel.Critical, msg);
        }
        public void Critical1(string msg, string args)
        {
            if (Level >= LogLevel.Critical)
                PrintLevel(LogLevel.Critical, msg, args);
        }
        public void Critical2(string msg, string arg1, string arg2)
        {
            if (Level >= LogLevel.Critical)
                PrintLevel(LogLevel.Critical, msg, arg1, arg2);
        }
    }
}