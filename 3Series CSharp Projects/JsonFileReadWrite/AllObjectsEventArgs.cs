using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace JsonFileReadWrite
{
    public class AllObjectsEventArgs : EventArgs
    {
        public string Filename;
        public string Path;
        public string Value;
        public System.Type ValueType;
        public ushort ValueAsDigital()
        {
            if (ValueType == typeof(bool))
            {
                bool b = bool.Parse(Value);
                ushort u = (b ? (ushort)1 : (ushort)0);
                return u;
            }
            return (ushort)0;
        }
        public ushort ValueAsAnalog()
        {
            if (ValueType == typeof(int))
            {
                int i = int.Parse(Value);
                ushort u = (ushort)i;
                return u;
            }
            return (ushort)0;
        }
        public AllObjectsEventArgs()
        {
            Filename = "";
            Path = "";
            Value = "";
            ValueType = null;
        }
        public AllObjectsEventArgs(string filename, string path, bool value)
        {
            Filename = filename;
            Path = path;
            Value = value.ToString();
            ValueType = typeof(bool);
        }
        public AllObjectsEventArgs(string filename, string path, ushort value)
        {
            Filename = filename;
            Path = path;
            Value = value.ToString();
            ValueType = typeof(int);
        }
        public AllObjectsEventArgs(string filename, string path, int value)
        {
            Filename = filename;
            Path = path;
            Value = value.ToString();
            ValueType = typeof(int);
        }
        public AllObjectsEventArgs(string filename, string path, string value)
        {
            Filename = filename;
            Path = path;
            Value = value;
            ValueType = typeof(string);
        }
    }
}