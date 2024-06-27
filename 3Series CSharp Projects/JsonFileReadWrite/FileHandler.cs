using System;
using System.Text;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;


namespace JsonFileReadWrite
{
    public delegate void DigitalValue(ushort index, ushort value);
    public delegate void AnalogValue(ushort index, ushort value);
    public delegate void StringValue(ushort index, SimplSharpString value);
   
    public class FileHandler
    {        
        /// <summary>
        /// turns on/off CrestronConsole prints
        /// </summary>
        public bool Debug
        {
            get
            {
                return Debugger.Debug;
            }
            set
            {
                Debugger.Debug = value;
            }
        }

        /// <summary>
        /// the directory where the json file is located
        /// </summary>
        public string Directory {get;set;}        

        /// <summary>
        /// the file pattern such as *.json to used to locate the json file.
        /// </summary>
        public string FilePattern{get;set;}

        private string filename;
        /// <summary>
        /// the file name found using the directory and file pattern when ReadFile is invoked.
        /// </summary>
        public string Filename
        {
            private set
            {
                filename = value;
            }
            get
            {
                return filename;
            }
        }

        /// <summary>
        /// a collection JsonPaths for digital cues using the S+ index (getlastmodifiedarrayindex) as the key
        /// </summary>
        private Dictionary<int, string> DigitalPaths;

        /// <summary>
        /// JsonPaths for analog cues  using the S+ index (getlastmodifiedarrayindex) as the key
        /// </summary>
        private Dictionary<int, string> AnalogPaths;

        /// <summary>
        /// JsonPaths for serial cues  using the S+ index (getlastmodifiedarrayindex) as the key
        /// </summary>
        private Dictionary<int, string> SerialPaths;

        /// <summary>
        /// event is raised with EventArgs.Empty when the file is read successfuly
        /// </summary>
        public event EventHandler<EventArgs> OnReadSuccessful;

        /// <summary>
        /// event is raised with EventArgs.Empty when reading the file encounters any exception
        /// </summary>
        public event EventHandler<EventArgs> OnReadFailed;

        /// <summary>
        /// event is raised with EventArgs.Empty when writing the json object to a file.
        /// </summary>
        public event EventHandler<EventArgs> OnWriteSuccessful;

        /// <summary>
        /// event is raised with EventArgs.Empty when writing the json objet to a file encounters any exception
        /// </summary>
        public event EventHandler<EventArgs> OnWriteFailed;

        /// <summary>
        /// delegate callback to S+ to update the digital cue associated with a path
        /// </summary>
        public DigitalValue DigitalValueFb { get; set; }

        /// <summary>
        /// delegate callback to S+ to update the analog cue associated with a path
        /// </summary>
        public AnalogValue AnalogValueFb { get; set; }

        /// <summary>
        /// delegate callback to S+ to update the serial cue associated with a path
        /// </summary>
        public StringValue StringValueFb { get; set; }
        
        /// <summary>
        /// the jsonString parsed as a JOBject, that will be used to query the digital, analog, and serial JsonPaths.
        /// </summary>
        private JObject JsonObject
        {
            get
            {
                if (string.IsNullOrEmpty(this.Filename))
                {
                    Debugger.Print("Cannot get JsonObject. Filename is empty");
                    return null;
                }
                if (AllObjects.ContainsKey(this.Filename))
                    return AllObjects.GetJObject(this.Filename);
                Debugger.Print("Cannot get JsonObject. AllObjects does not contain key with {0}", this.Filename);
                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(this.Filename))
                    throw new NullReferenceException("Cannot assign JObject, file name is null or empty");

                if (AllObjects.ContainsKey(this.Filename))
                    AllObjects.Set(this.Filename,value);
                else
                    AllObjects.Add(this.Filename, value);
            }
        }

        /// <summary>
        /// set to 1 to write the jsonObject to a file when a JsonPath's value is changed.
        /// </summary>
        public int WriteOnChange { get; set; }

        /// <summary>
        /// a timer to reset when write gets called, in cases where multiple paths are changed and must be written
        /// </summary>
        private CTimer WriteTimer;        

        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        public FileHandler()
        {
            DigitalPaths = new Dictionary<int, string>();
            AnalogPaths = new Dictionary<int, string>();
            SerialPaths = new Dictionary<int, string>();

            Directory = "";
            FilePattern = "";
            Filename = "";
            WriteOnChange = 0;
            this.Debug = false;

            WriteTimer = new CTimer(WriteTimerCallback, 1000);
            WriteTimer.Stop();            
        }        
        
        /// <summary>
        /// read the file and parse it, calling back any S+ delegates for their updated values.
        /// </summary>
        public void ReadFile()
        {
            try
            {
                if (string.IsNullOrEmpty(Directory))
                    throw new NullReferenceException("Json file directory is empty");

                if (string.IsNullOrEmpty(FilePattern))
                    throw new NullReferenceException("Json file pattern is empty");

                
                string dir = Crestron.SimplSharp.CrestronIO.Directory.GetApplicationRootDirectory() + this.Directory;
                
                Debugger.Print("Looking for json file in dir:{0} pattern:{1}", dir, FilePattern);
                
                string[] files = Crestron.SimplSharp.CrestronIO.Directory.GetFiles(dir, this.FilePattern);
                
                if (files == null)
                {
                    ErrorLog.Error(string.Format("No json files found in directory {} with matching file pattern {}", Directory, FilePattern));
                    throw new Exception(string.Format("No json files found in directory {} with matching file pattern {}", Directory, FilePattern));
                }
                if (files.Length == 0)
                {
                    ErrorLog.Error(string.Format("No json files found in directory {} with matching file pattern {}", Directory, FilePattern));
                    throw new Exception(string.Format("No json files found in directory {} with matching file pattern {}", Directory, FilePattern));
                }
                
                this.filename = files[0];

                ErrorLog.Notice(string.Format("Reading json file {0}", filename));
                Debugger.Print(string.Format("Reading file {0}", filename));

                string fileContents = "";

                try
                {

                    using (StreamReader reader = new StreamReader(filename))
                    {
                        fileContents = reader.ReadToEnd();
                        Debugger.Print(string.Format("File Content Length: {0}", fileContents.Length));
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Print("Error reading file.\r\n{0}", ex.ToString());
                    ErrorLog.Exception(string.Format("Error reading json file {0}", filename), ex);
                    if (this.OnReadFailed != null)
                        this.OnReadFailed.Invoke(this, EventArgs.Empty);

                    throw ex;
                }

                if (fileContents.Length == 0)
                {
                    Debugger.Print(string.Format("File {0} is empty.", Filename));
                    return;
                }

                try
                {
                    lock (fileContents)
                    {
                        var JO = JObject.Parse(fileContents);

                        if (JO == null)
                        {
                            Debugger.Print(string.Format("Error parsing json file {0}", filename));
                            ErrorLog.Error(string.Format("Error parsing json file {0}", filename));

                            if (this.OnReadFailed != null)
                                this.OnReadFailed.Invoke(this, EventArgs.Empty);

                            return;
                        }

                        Debugger.Print(string.Format("File contents parsed into json object. {0} \r\n", JsonConvert.SerializeObject(JO, Formatting.Indented)));


                        if (AllObjects.ContainsKey(filename))
                        {
                            AllObjects.Set(filename, JO);
                        }
                        else
                        {
                            AllObjects.Add(filename, JO);
                            AllObjects.OnValueSet += this.ValueSetEventHandler;
                        }

                        if (this.OnReadSuccessful != null)
                            this.OnReadSuccessful.Invoke(this, EventArgs.Empty);

                        GetAllPathsValue();
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Print("Error encountered parsing json in file {0}\r\n{1}", filename, ex.ToString());
                    ErrorLog.Exception(string.Format("Error encountered parsing json in file {0}.", filename), ex);
                    if (this.OnReadFailed != null)
                        this.OnReadFailed.Invoke(this, EventArgs.Empty);

                    throw ex;
                }
            }
            catch (Exception ex)
            {
                Debugger.Print("Error Reading File: {}", ex.ToString());
                throw ex;
            }
        }

        private void ValueSetEventHandler(object sender, AllObjectsEventArgs e)
        {
            if (e.Filename != this.Filename)
                return;

            if (e.ValueType == typeof(bool) && DigitalPaths.ContainsValue(e.Path))
            {
                //could be one or more items.
                foreach (var kv in DigitalPaths)
                {
                    if (kv.Value == e.Path)
                    {
                        if (this.DigitalValueFb != null)
                        {                            
                            ushort index = (ushort)kv.Key;
                            this.DigitalValueFb.Invoke(index, e.ValueAsDigital());
                        }
                    }
                }
            }
            else if (e.ValueType == typeof(int) && AnalogPaths.ContainsValue(e.Path))
            {
                foreach (var kv in AnalogPaths)
                {
                    if (kv.Value == e.Path)
                    {
                        if (this.DigitalValueFb != null)
                        {
                            ushort index = (ushort)kv.Key;
                            this.AnalogValueFb.Invoke(index, e.ValueAsAnalog());
                        }
                    }
                }
            }
            else if (e.ValueType == typeof(string) && SerialPaths.ContainsValue(e.Path))
            {
                foreach (var kv in SerialPaths)
                {
                    if (kv.Value == e.Path)
                    {
                        if (this.DigitalValueFb != null)
                        {
                            ushort index = (ushort)kv.Key;
                            this.StringValueFb.Invoke(index, e.Value);
                        }
                    }
                }
            }
        }
        
        public void GetAllPathsValue()
        {
            Debugger.Print("GetAllPathsValue");

            List<ParseException> deferred = new List<ParseException>();

            foreach (var p in DigitalPaths)
            {
                string path = "";
                try
                {
                    path = p.Value;
                    ushort index = (ushort)p.Key;                    
                    ushort value = AllObjects.GetDigitalValue(filename, path);
                    Debugger.Print("Found JsonPath {0} = {1}", path, value);
                    if (this.DigitalValueFb != null)
                        this.DigitalValueFb.Invoke(index, value);
                }
                catch (PathNullReferenceException pex)
                {
                    Debugger.Print("Error retrieving data for path :{0}. PathNullRefEx: {1}", path,pex.ToString());
                    continue;
                }
                catch (NullReferenceException nex)
                {
                    Debugger.Print("Error retrieving data for path :{0}. NullRefEx: {1}", path,nex.ToString());
                    continue;
                }
                catch (Exception ex)
                {
                    Debugger.Print("Error retrieving data for path :{0}. Ex: {1}", path, ex.ToString());
                    var pe = new ParseException(path, ex);
                    deferred.Add(pe);
                    continue;
                }
            }

            foreach (var p in AnalogPaths)
            {
                string path = "";
                try
                {
                    path = p.Value;
                    ushort index = (ushort)p.Key;                    
                    ushort value = AllObjects.GetAnalogValue(filename, path);
                    Debugger.Print("Found JsonPath {0} = {1}", path, value);
                    if (this.AnalogValueFb != null)
                        this.AnalogValueFb.Invoke(index, value);
                }
                catch (PathNullReferenceException pex)
                {
                    Debugger.Print("Error retrieving data for path :{0}. PathNullRefEx: {1}", path, pex.ToString());
                    continue;
                }
                catch (NullReferenceException nex)
                {
                    Debugger.Print("Error retrieving data for path :{0}. NullRefEx: {1}", path, nex.ToString());
                    continue;
                }
                catch (Exception ex)
                {
                    Debugger.Print("Error retrieving data for path :{0}. Ex: {1}", path, ex.ToString());
                    var pe = new ParseException(path, ex);
                    deferred.Add(pe);
                    continue;
                }
            }

            foreach (var p in SerialPaths)
            {
                string path = "";
                try
                {
                    path = p.Value;
                    ushort index = (ushort)p.Key;
                    string value = AllObjects.GetStringValue(filename, path);
                    Debugger.Print("Found JsonPath {0} = {1}", path, value);
                    if (this.StringValueFb != null)
                        this.StringValueFb.Invoke(index, value);
                }
                catch (PathNullReferenceException pex)
                {
                    Debugger.Print("Error retrieving data for path :{0}. PathNullRefEx: {1}", path, pex.ToString());
                    continue;
                }
                catch (NullReferenceException nex)
                {
                    Debugger.Print("Error retrieving data for path :{0}. NullRefEx: {1}", path, nex.ToString());
                    continue;
                }
                catch (Exception ex)
                {
                    Debugger.Print("Error retrieving data for path :{0}. Ex: {1}", path, ex.ToString());
                    var pe = new ParseException(path, ex);
                    deferred.Add(pe);
                }
            }

            if (deferred.Count > 0)
            {
                Debugger.Print(string.Format("Exceptions found during Path resolution: {0}", deferred.Count));
                foreach (var ex in deferred)
                {
                    Debugger.Print(ex.ToString());                    
                }
                    
                //throw deferred[0];
            }
        }

        public void WriteTimerCallback(object userSpecific)
        {            
            try
            {
                lock (JsonObject)
                {
                    string jsonString = JsonConvert.SerializeObject(JsonObject, Formatting.Indented);
                    Debugger.Print("Writing jsonObject to file {0} length: {1}", Filename, jsonString.Length.ToString());
                    using (StreamWriter sw = new StreamWriter(Filename, false, Encoding.ASCII, jsonString.Length))
                    {
                        sw.Write(jsonString);
                    }
                    ErrorLog.Notice(string.Format("Updating json file {0}", Filename));

                    if (this.OnWriteSuccessful != null)
                        this.OnWriteSuccessful.Invoke(this, EventArgs.Empty);
                    Debugger.Print(string.Format("Writing Json File {0} Successful", Filename));
                    jsonString = ""; //release some memory
                }
            }
            catch (Exception ex)
            {
                Debugger.Print("Error writing file.\r\n{0}", ex.ToString());
                if (this.OnWriteFailed != null)
                    this.OnWriteFailed.Invoke(this, EventArgs.Empty);
            }
        }

        public void WriteFile()
        {
            //lets delay the writing of the file until we received all the changes..
            this.WriteTimer.Reset(1000);
        }
        
        public void AddDigitalPath(int index, string path)
        {
            DigitalPaths.Add(index, path);
            Debugger.Print("Path added to digital: {0}", path);
        }
        
        public void AddAnalogPath(int index, string path)
        {
            AnalogPaths.Add(index, path);
            Debugger.Print("Path added to analog: {0}", path);
        }
        
        public void AddSerialPath(int index, string path)
        {
            SerialPaths.Add(index, path);
            Debugger.Print("Path added to serial: {0}", path);
        }

        private JToken GetJToken(string path)
        {
            if (this.JsonObject == null)
            {
                Debugger.Print("Cannot find token. JsonObject is null. Try reading the json file first");
                return null;
            }

            JToken jt = JsonObject.SelectToken(path);
            
            if (jt == null)
                Debugger.Print("Cannot find token with path {0}", path);
            
            return jt;
        }

        public ushort GetDigitalValue(string path)
        {
            return AllObjects.GetDigitalValue(filename, path);
        }
        
        public void SetDigitalValue(string path, int value)
        {
            var jt = GetJToken(path);
            if (jt == null)
            {
                Debugger.Print("Cannot write value. Json path could not be found:{0}", path);
                return;
            }
            try
            {
                Debugger.Print("setting {0} to {1}",path, (value > 0).ToString());
                var prevValue = AllObjects.GetDigitalValue(filename, path);
                jt.Replace( (value > 0) );

                if (prevValue != value)
                    if (WriteOnChange > 0)
                        WriteFile();
            }
            catch (Exception ex)
            {
                Debugger.Print("Error replacing value for json path {0}.\r\n{1}", path, ex.ToString());
            }                            
        }

        public ushort GetAnalogValue(string path)
        {
            return AllObjects.GetAnalogValue(filename, path);
        }

        public void SetAnalogValue(string path, int value)
        {
            var jt = GetJToken(path);
            if (jt == null)
            {
                Debugger.Print("Cannot write value. Json path could not be found:{0}", path);
                return;
            }
            try
            {
                Debugger.Print("setting {0} to {1}", path, value.ToString());
                var prevValue = AllObjects.GetAnalogValue(filename, path);
                jt.Replace(value);
                if (prevValue != value)
                    if (WriteOnChange > 0)
                        WriteFile();
            }
            catch (Exception ex)
            {
                Debugger.Print("Error replacing value for json path {0}.\r\n{1}", path, ex.ToString());
            }
        }

        public string GetStringValue(string path)
        {
            return AllObjects.GetStringValue(filename, path);
        }

        public void SetStringValue(string path, string value)
        {
            var jt = GetJToken(path);
            if (jt == null)
            {
                Debugger.Print("Cannot write value. Json path could not be found:{0}", path);
                return;
            }
            try
            {
                var prevValue = AllObjects.GetStringValue(filename, path);
                jt.Replace(value);
                Debugger.Print("setting {0} to {1}", path, value);
                if (prevValue != value)
                    if (WriteOnChange > 0)
                        WriteFile();
            }
            catch (Exception ex)
            {
                Debugger.Print("Error replacing value for json path {0}.\r\n{1}", path, ex.ToString());
            }
        }
        
        public void DebugOn()
        {
            this.Debug = true;
        }
        
        public void DebugOff()
        {
            this.Debug = false;
        }
    }
}
