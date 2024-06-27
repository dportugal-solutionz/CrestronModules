using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace JsonFileReadWrite
{
    public class PathNullReferenceException : Exception
    {
        public string Filename { get; set; }
        public string Path { get; set; }
        public PathNullReferenceException()
        {
            Filename = "";
            Path = "";
        }
        public PathNullReferenceException(string filename, string path)
        {
            Filename = filename;
            Path = path;
        }
        public PathNullReferenceException(string path)
        {
            Path = path;
            Filename = "";
        }
        public override string ToString()
        {
            return string.Format("Path not found {0} in File {1}. {2}", Path, Filename, base.ToString());            
        }
    }
}