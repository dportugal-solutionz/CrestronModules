using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CrosspointEx
{
    /// <summary>
    /// a delegate to set a single cue's value
    /// </summary>
    /// <typeparam name="T">digital = ushort, analog = ushort, serial = string</typeparam>
    /// <param name="sender">the cue that is to be changed</param>
    /// <param name="newvalue">the new value for the cue, this should be sent to the output of the Simpl+ module</param>
    public delegate void CueSetter<T>(ICue<T> sender, T newvalue);

    public interface ICue<T>
    {
        string Address {get; set;}
        /// <summary>
        /// the delegate that will be subscribed to when crosspoints are connected.
        /// </summary>
        CueSetter<T> SetCue { get; set; }

        /// <summary>
        /// the method that should be called by SimplPlus
        /// </summary>
        /// <param name="value">the new value of the cue</param>
        void Set(T value);
    }

    /// <summary>
    /// a Cue, as in the SIMPL signal. this object does not hold the value but offloads it to the delegate SetCue.   
    /// </summary>
    /// <typeparam name="T">ushort for Digital, ushort for Analog, string for Serial</typeparam>
    public class Cue<T>: ICue<T>
    {
        public string Address { get; set; }
        public CueSetter<T> SetCue { get; set; }
        public Cue(string address)
        {
            Address = address;
        }
        public void Set(T value)
        {
            Tracer.Print("Cue {0} set to {1}", Address, value);
            if (SetCue == null)
            {
                Tracer.Print("Error: {0} SetCue is null", Address);
                return;
            }
            SetCue.Invoke(this, value);
        }
    }  
}