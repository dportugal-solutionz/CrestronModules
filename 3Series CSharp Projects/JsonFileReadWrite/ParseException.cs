using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace JsonFileReadWrite
{
    public class ParseException : Exception
    {
        public string JsonPath { get; set; }
        public Exception Ex {get;set;}
        public ParseException()
        {
            JsonPath = "";
            Ex = null;
        }
        public ParseException(string path, Exception ex)
        {
            this.JsonPath = path;
            this.Ex = ex;
        }
        public string Print()
        {
            return string.Format("Path = {0}.\r\n{1}",this.JsonPath, this.Ex);
        }
        public override string ToString()
        {
            return Print();
        }
        public override string Message
        {
            get
            {
                return Print();
            }
        }        
    }
}