using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;

namespace JsonFileReadWrite
{
    /// <summary>
    /// A simpl+ interface to read/write to the same jobject in the FileHandler.
    /// </summary>
    public class SplusJObjectInterface
    {
        public DigitalValue OnInitialized { get; set; }
        public ushort GetDigitalValue(string path)
        {
            CheckInit();
            return AllObjects.GetDigitalValue(AllObjectsKey, path);
        }
        public ushort GetAnalogValue(string path)
        {
            CheckInit();
            return AllObjects.GetAnalogValue(AllObjectsKey, path);
        }
        public string GetStringValue(string path)
        {
            CheckInit();
            return AllObjects.GetStringValue(AllObjectsKey, path);
        }
        public void SetDigitalValue(string path, ushort value)
        {
            CheckInit();
            AllObjects.SetDigitalValue(AllObjectsKey, path, value);
        }
        public void SetAnalogValue(string path, ushort value)
        {
            CheckInit();
            AllObjects.SetAnalogValue(AllObjectsKey, path, value);
        }
        public void SetStringValue(string path, string value)
        {
            CheckInit();
            AllObjects.SetStringValue(AllObjectsKey, path, value);
        }

        private void CheckInit()
        {
            if (AllObjectsKey.Length == 0)
            {
                Debugger.Print(string.Format("[{0}] object is not ready. Check Directory and FilePattern", typeof(SplusJObjectInterface)));
                throw new NullReferenceException(string.Format("[{0}] object is not ready. Check Directory and FilePattern", typeof(SplusJObjectInterface)));
            }
            if (AllObjects.ContainsKey(AllObjectsKey))
            {
                Debugger.Print(string.Format("[{0}] AllObjectsKey {1}, not found in FileHandler.AllObjects",typeof(SplusJObjectInterface),AllObjectsKey));
                throw new NullReferenceException(string.Format("[{0}] AllObjectsKey {1}, not found in FileHandler.AllObjects", typeof(SplusJObjectInterface), AllObjectsKey));
            }
        }

        public string FilePattern { get; private set; }
        public string Directory {get; private set;}
        public string AllObjectsKey {get; private set;}        
        
        public void Init(string directory, string filepattern)
        {
            this.Directory = directory;
            this.FilePattern = filepattern;
            SetObjectKey();
        }

        public void SetDirectory(string value)
        {
            this.Directory = value;
            SetObjectKey();
        }

        public void SetFilePattern(string value)
        {
            this.FilePattern = value;
            SetObjectKey();
        }

        private void SetObjectKey()
        {
            if (AllObjects.Count == 0)
            {
                Debugger.Print("[{0}] could not set AllObjectsKey, file handler AllObjects is empty", typeof(SplusJObjectInterface));
                AllObjectsKey = "";
                if (OnInitialized != null)
                    OnInitialized.Invoke(0, 0);
                return;
            }
            if (AllObjects.Count == 1)
            {
                foreach (var kv in AllObjects.Data)
                {
                    AllObjectsKey = kv.Key;
                    Debugger.Print("[{0}] setting AllObjectsKey to {1}", typeof(SplusJObjectInterface), AllObjectsKey);
                    if (OnInitialized != null)
                        OnInitialized.Invoke(0, 1);
                    return;
                }
            }
            if (AllObjects.Count > 1)
            {
                foreach (var kv in AllObjects.Data)
                {
                    string filename = kv.Key;
                    if (Regex.Match(filename, this.FilePattern).Success)
                    {
                        AllObjectsKey = filename;
                        Debugger.Print("[{0}] setting AllObjectsKey to {1}", typeof(SplusJObjectInterface), AllObjectsKey);
                        if (OnInitialized != null)
                            OnInitialized.Invoke(0, 1);
                        return;
                    }
                }
                Debugger.Print("[{0}] could not find file with pattern {1} in FileHandler.AllObjects.", typeof(SplusJObjectInterface), this.FilePattern);
                if (OnInitialized != null)
                    OnInitialized.Invoke(0, 0);
            }
        }

        public ushort IsInitialized
        {
            get
            {
                return ( AllObjectsKey.Length > 0 ? (ushort)1 : (ushort)0);
            }
        }
        
        public SplusJObjectInterface()
        {
            AllObjectsKey = "";
            Directory = "";
            FilePattern = "";
            AllObjects.OnObjectAdded += AllObjects_OnObjectAddedHandler;
        }

        private void AllObjects_OnObjectAddedHandler(object sender, EventArgs e)
        {
            if (Directory.Length > 0 && FilePattern.Length > 0)
                SetObjectKey();
        }
    }
}