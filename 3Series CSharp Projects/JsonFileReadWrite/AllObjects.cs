using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace JsonFileReadWrite
{

    /// <summary>
    /// An object that holds the static member Dictionary JObjects and all functions to manipulate the JObject.
    /// any change in the value of any json key value pair should be done using this class so as to invoke the event OnValueSet notifying other modules.
    /// </summary>
    public class AllObjects
    {
        public static Dictionary<string, JObject>  Data
        {
            get
            {
                return JObjects;
            }
        }

        protected static Dictionary<string, JObject> JObjects = new Dictionary<string, JObject>();

        /// <summary>
        /// this is invoked a jsonObject key's value was changed using one of the Set methods.
        /// </summary>
        public static event EventHandler<AllObjectsEventArgs> OnValueSet;

        /// <summary>
        /// this is invoked when a jsonObject is added using the Add method.
        /// </summary>
        public static event EventHandler OnObjectAdded;

        //static constructor
        static AllObjects()
        {
            JObjects = new Dictionary<string,JObject>();
        }

        public static int Count
        {
            get
            {
                return JObjects.Count;
            }
        }        
        
        public static void Set(string filename, JObject jo)
        {
            Debugger.Print("JsonObject for file {0} replaced", filename);
            JObjects[filename] = jo;
        }
        public static void Add(string filename, JObject jo)
        {
            if (JObjects.ContainsKey(filename))
                Set(filename, jo);
            else
            {                
                JObjects.Add(filename, jo);
                Debugger.Print("JsonObject added with filename {0}", filename);
                if (OnObjectAdded != null)
                    OnObjectAdded.Invoke(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// returns null if filename is not found
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static JObject GetJObject(string filename)
        {
            if (JObjects.ContainsKey(filename))
                return JObjects[filename];
            return null;
        }

        public static bool ContainsKey(string filename)
        {
            return JObjects.ContainsKey(filename);
        }

        private static JToken GetJToken(string filename, string path)
        {
            JObject jo = GetJObject(filename);
            if (jo == null)
            {
                Debugger.Print("File {0} could not be found", filename);
                throw new PathNullReferenceException(filename, path);
            }
            JToken jt = jo.SelectToken(path);
            if (jt == null)
            {
                Debugger.Print("File {0}, cannot find JToken with path {1}", filename, path);
                //throw new NullReferenceException(string.Format("File {0}, cannot find JToken with path {1}", filename, path));
                throw new PathNullReferenceException(filename, path);
            }
            return jt;
        }

        #region JsonPath Read
        /// <summary>
        /// returns the value at the json path
        /// throws an exception if something goes wrong.
        /// </summary>
        /// <param name="filename">the filename of associated</param>
        /// <param name="path">the json path</param>
        /// <returns></returns>
        public static string GetStringValue(string filename, string path)
        {            
            var jt = GetJToken(filename, path);            
            return jt.ToString();
        }

        public static ushort GetDigitalValue(string filename, string path)
        {
            
            string value = GetStringValue(filename, path);
            try
            {
                bool b = bool.Parse(value);
                return (b ? (ushort)1 : (ushort) 0);
            }
            catch
            {
                Debugger.Print("Error trying to parse to bool value {0} at path {1}", value, path);
                return ((ushort)0);
            }
        }

        public static ushort GetAnalogValue(string filename, string path)
        {
            string value = GetStringValue(filename, path);
            try
            {
                int i = int.Parse(value);
                ushort u = (ushort)i;
                return u;
            }
            catch
            {
                Debugger.Print("Error trying to parse to int value {0} at path {1}", value, path);
                return ((ushort)0);
            }
        }
        #endregion

        #region JsonPath Write
        /// <summary>
        /// sets the value of the json path of a specific file
        /// </summary>
        /// <param name="filename">file name to change</param>
        /// <param name="path">the json path to be changed</param>
        /// <param name="value">the value to be set</param>
        public static void SetDigitalValue(string filename, string path, ushort value)
        {
            
            JToken jt = GetJToken(filename, path);
            if (jt == null)
                return;
            try
            {
                bool b = (value > 0);
                jt.Replace(b);
                if (OnValueSet != null)
                {
                    OnValueSet.Invoke(null, new AllObjectsEventArgs(filename, path, value));
                }
            }
            catch
            {
                Debugger.Print(string.Format("Error trying to set value of file {0} at path {1} with value {2}", filename, path, value));
            }
        }
        
        public static void SetAnalogValue(string filename, string path, ushort value)
        {
            JToken jt = GetJToken(filename, path);
            if (jt == null)
                return;
            try
            {
                int i = (int)value;
                jt.Replace(i);
                if (OnValueSet != null)
                {
                    OnValueSet.Invoke(null, new AllObjectsEventArgs(filename, path, value));
                }
            }
            catch
            {
                Debugger.Print(string.Format("Error trying to set value of file {0} at path {1} with value {2}", filename, path, value));
            }
        }
        
        public static void SetStringValue(string filename, string path, string value)
        {
            JToken jt = GetJToken(filename, path);
            if (jt == null)
                return;
            try
            {                
                jt.Replace(value);
                if (OnValueSet != null)
                {
                    OnValueSet.Invoke(null, new AllObjectsEventArgs(filename, path, value));
                }
            }
            catch
            {
                Debugger.Print(string.Format("Error trying to set value of file {0} at path {1} with value {2}", filename, path, value));
            }
        } 
        #endregion
    }    
}