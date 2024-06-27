using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using CrosspointEx;

namespace CrosspointEx
{
    /// <summary>
    /// a method to set an single cue's value outside of this Cues object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sender"></param>
    /// <param name="element"></param>
    /// <param name="Value"></param>
    public delegate void CuesCueSetter<T> (Cues<T> sender, ICue<T> element, T Value);


    /// <summary>
    /// a collection of Cues
    /// </summary>
    /// <typeparam name="T">set to ushort for digital, ushort for analog, string for serial</typeparam>
    public class Cues<T>
    {
        public readonly string Address;
        public readonly Dictionary<string, ICue<T>> Data;
        public readonly Dictionary<string, int> GetSpIndex;
        public readonly Dictionary<int, string> GetAddress;

        public CuesCueSetter<T> SetCue { get; set; }

        /// <summary>
        /// creates a collection of ICues
        /// </summary>
        /// <param name="address">the address or unique id of this collection</param>
        public Cues(string address)
        {
            Address = address;
            Data = new Dictionary<string, ICue<T>>();
            GetSpIndex = new Dictionary<string, int>();
            GetAddress = new Dictionary<int, string>();
        }

        /// <summary>
        /// adds an ICue to the collection
        /// </summary>
        /// <param name="address">the string address</param>
        /// <param name="spindex">the simpl plus array index</param>
        public void Add(string address, int spindex, ICue<T> cue)
        {
            if (Data.ContainsKey(address))
                throw new ArgumentException(string.Format("Address %s already exists", address));

            Data.Add(address, cue);
            GetSpIndex.Add(address, spindex);
            GetAddress.Add(spindex, address);
            cue.SetCue = this.CueSetter;
        }

        /// <summary>
        /// returns the ICue object
        /// </summary>
        /// <param name="spIndex">the simplplus index</param>
        /// <returns></returns>
        public ICue<T> this[int spIndex]
        {
            get
            {
                if (!GetAddress.ContainsKey(spIndex))
                    throw new IndexOutOfRangeException(string.Format("Index {0} not found", spIndex));
                string address = GetAddress[spIndex];
                return Data[address];
            }            
        }


        /// <summary>
        /// returns the ICue object
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public ICue<T> this[string address]
        {
            get
            {
                if (!Data.ContainsKey(address))
                    throw new IndexOutOfRangeException(string.Format("Address {0} not found",address));
                return Data[address];
            }
        }
    
        /// <summary>
        /// the delegate implementation for the cue, this is assigned to the Cue at Add method.
        /// </summary>
        /// <param name="?"></param>
        private void CueSetter(ICue<T> member, T value)
        {
            if (this.SetCue == null)
            {
                Tracer.Print("Crosspoint CueSetter has no delegate assigned");
                return;
            }
            string address = member.Address;
            int spIndex = GetSpIndex[address];
            SetCue(this, member, value);
        }

        public bool ContainsKey(string address)
        {
            return Data.ContainsKey(address);
        }
        public bool ContainsKey(int index)
        {
            return GetAddress.ContainsKey(index);
        }

        //returns a comma delimited string of all cue's addresses
        public string GetAllAddresses()
        {
            List<string> ret = new List<string>();
            foreach (var kv in Data)
            {
                int index = this.GetSpIndex[kv.Key];
                string entry = string.Format("[{0}] {1}", index, kv.Key);
                ret.Add(entry);
            }
            return string.Join("\r\n", ret.ToArray());
        }
    }
}