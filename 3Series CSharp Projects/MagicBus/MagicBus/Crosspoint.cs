using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using CrosspointEx;

namespace CrosspointEx
{
    //SimplPlus Callbacks for Crosspoint
    public delegate void dgChangeDigital(SimplSharpString address, ushort index, ushort value);
    public delegate void dgChangeAnalog(SimplSharpString address, ushort index, ushort value);
    public delegate void dgChangeSerial(SimplSharpString address, ushort index, SimplSharpString value);    

    public enum CrosspointType 
    {
        Control = 0,
        Equipment = 1
    }


    /// <summary>
    /// a collection of Digital, Analog, and Serial cues
    /// this class should not be used by the Simpl+ module.
    /// </summary>
    public class Crosspoint 
    {
        private TracerContext tracer {get;set;}

        /// <summary>
        /// The type of crosspoint
        /// </summary>
        internal CrosspointType Type { get; set; }

        /// <summary>
        /// SimplPlus callbacks to change their output cues
        /// </summary>
        public dgChangeDigital ChangeDigital { get; set; }

        /// <summary>
        /// SimplPlus callbacks to change their output cues
        /// </summary>
        public dgChangeAnalog ChangeAnalog { get; set; }

        /// <summary>
        /// SimplPlus callbacks to change their output cues
        /// </summary>
        public dgChangeSerial ChangeSerial { get; set; }

        /// <summary>
        /// The address of this crosspoint
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// collection of digital cues
        /// </summary>
        public Cues<ushort> Digitals { get; private set; }

        /// <summary>
        /// collection of analog cues
        /// </summary>
        public Cues<ushort> Analogs { get; private set; }

        /// <summary>
        /// collection of serial cues
        /// </summary>
        public Cues<string> Serials { get; private set; }

        /// <summary>
        /// this event is invoked whenever a new connection to another crosspoint is made.
        /// </summary>
        public event EventHandler<OnConnectedEventArgs> OnConnected;

        /// <summary>
        /// this even is invoked when an existing connection is removed.
        /// </summary>
        public event EventHandler<OnConnectedEventArgs> OnDisconnected;

        /// <summary>
        /// a list of strings that is received before SetAddress is called.
        /// </summary>
        private List<string> PreInitConnect;

        public Crosspoint(CrosspointType type)
        {
            
            this.Type = type;
            this.Address = "";
            if (Type == CrosspointType.Control)
            {
                tracer = new TracerContext("CXP");
                CrosspointRouter.OnAddedEquipmentCrosspoint += this.CrosspointAddedHandler;
            }
            else
            {
                tracer = new TracerContext("EXP");
                CrosspointRouter.OnAddedControlCrosspoint += this.CrosspointAddedHandler;
            }

            PreInitConnect = new List<string>();
     
        }
        
        /// <summary>
        /// sets the address of this crosspoint
        /// </summary>
        /// <param name="address"></param>
        public void SetAddress(string address)
        {
            if (address.Length == 0)
            {
                tracer.Print("Invalid Crosspoint Address. Cannot set crosspoint address to empty string");
                return;
            }                       
            this.Address = address;
            tracer.Context += "-" + Address;
            CrosspointRouter.AddCrosspoint(this);            
            Digitals = new Cues<ushort>(Address);
            Analogs = new Cues<ushort>(Address);
            Serials = new Cues<string>(Address);

            if (PreInitConnect.Count > 0)
            {
                foreach (string dest in PreInitConnect)
                {
                    if (Connect(dest) > 0)
                        PreInitConnect.Remove(dest);
                }
            }
        }
        
        /// <summary>
        /// register a digital on this crosspoint
        /// </summary>
        /// <param name="address"></param>
        /// <param name="index"></param>
        public void RegisterDigitalCue(string address, ushort index)
        {           
            if (Digitals.ContainsKey(address))
                throw new Exception(string.Format("{0} Crosspoint has duplicate Digital Address {1}", this.Address, address));
            DigitalCue cue = new DigitalCue(address);
            Digitals.Add(address, index, cue);
        }

        /// <summary>
        /// register an analog on this crosspoint
        /// </summary>
        /// <param name="address"></param>
        /// <param name="index"></param>
        public void RegisterAnalogCue(string address, ushort index)
        {
            if (Analogs.ContainsKey(address))
                throw new Exception(string.Format("{0} Crosspoint has duplicate Analog Address {1}", this.Address, address));
            AnalogCue cue = new AnalogCue(address);
            Analogs.Add(address, index, cue);

        }

        /// <summary>
        /// register a serial on this crosspoint
        /// </summary>
        /// <param name="address"></param>
        /// <param name="index"></param>
        public void RegisterSerialCue(string address, ushort index)
        {
            if (Serials.ContainsKey(address))
                throw new Exception(string.Format("{0} Crosspoint has duplicate Serial Address {1}", this.Address, address));
            SerialCue cue = new SerialCue(address);
            Serials.Add(address, index, cue);
        }


        /// <summary>
        /// Sets the value of the cue, thus invoking connected crosspoint delegate
        /// this method should be called by the SimplPlus event
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetDigital(ushort index, ushort value)
        {
            try
            {
                if (!Digitals.ContainsKey(index))
                {
                    tracer.Print("Cannot set digital, index {0} is not registered", index);
                    return;
                } 
                Digitals[index].Set(value);
            }
            catch (Exception ex)
            {
                tracer.Print(ex.ToString());
            }
        }

        /// <summary>
        /// Sets the value of the cue, thus invoking connected crosspoint delegate
        /// this method should be called by the SimplPlus event
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetAnalog(ushort index, ushort value)
        {
            try
            {
                if (!Analogs.ContainsKey(index))
                {
                    tracer.Print("Cannot set analog, index {0} is not registered", index);
                    return;
                }
                Analogs[index].Set(value);
            }
            catch (Exception ex)
            {
                tracer.Print(ex.ToString());
            }
        }

        /// <summary>
        /// Sets the value of the cue, thus invoking connected crosspoint delegate
        /// this method should be called by the SimplPlus event
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetSerial(ushort index, string value)
        {
            try
            {
                if (!Serials.ContainsKey(index))
                {
                    tracer.Print("Cannot set serial, index {0} is not registered", index);
                    return;
                }
                Serials[index].Set(value);
            }
            catch (Exception ex)
            {
                tracer.Print(ex.ToString());
            }
        }

        /// <summary>
        /// connects this crosspoint to another crosspoint with the sepcified address
        /// if this crosspoint's address has not been set, then it will be added to a list for later connection.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>1 if successful, 0 if not.</returns>
        public ushort Connect(string address)
        {   
            if (this.Address.Length == 0) //address has not been set.
            {
                if (!PreInitConnect.Contains(address))
                {
                    tracer.Print("Connect deferred:{0}", address);
                    PreInitConnect.Add(address);
                    return 0;
                }
                return 0;
            }
            try
            {
                if (Type == CrosspointType.Control)
                {
                    if (CrosspointRouter.Connect(this.Address, address))
                        return 1;
                }
                else
                {
                    if (CrosspointRouter.Connect(address, this.Address))
                        return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                tracer.Print(ex.ToString());
                return 0;
            }
        }
        public void Disconnect(string address)
        {
            try
            {
                if (Type == CrosspointType.Control)
                    CrosspointRouter.Disconnect(this.Address, address);
                else
                    CrosspointRouter.Disconnect(address, this.Address);
            }
            catch (Exception ex)
            {
                tracer.Print(ex.ToString());
            }
        }
        
        public void DisconnectFromAll()
        {
            try
            {
                if (Type == CrosspointType.Control)
                    CrosspointRouter.Disconnect_ControlFromAllEquipment(Address);
                else
                    CrosspointRouter.Disconnect_EquipmentFromAllControl(Address);
            }
            catch (Exception ex)
            {
                tracer.Print(ex.ToString());
            }
        }
        
        /// <summary>
        /// this method should be invoked by CrosspointRouter.Connect
        /// </summary>
        internal void InvokeNewConnect(OnConnectedEventArgs args)
        {
            if (OnConnected == null)
                return;
            OnConnected.Invoke(this, args);
        }

        internal void InvokeNewDisconnect(OnConnectedEventArgs args)
        {
            if (this.OnDisconnected == null)
                return;
            OnDisconnected.Invoke(this, args);            
        }

        /// <summary>
        /// the delegate implementation for CuesCueSetter. This method should be assigned to the Cues.SetCue when by the CrosspointRouter.Connect()
        /// </summary>
        /// <param name="digitalcues">the Cues collection whose member is being set</param>
        /// <param name="member">the member whose value is being set</param>
        /// <param name="value">the new value of the cue</param>
        internal void DigitalCuesSetter(Cues<ushort> digitalcues, ICue<ushort> member, ushort value)
        {
            if (this.ChangeDigital == null)
            {
                tracer.Print("ChangeDigital has no delegate assigned");
                return;
            }
            if (!Digitals.ContainsKey(member.Address))
            {
                tracer.Print("This crosspoint has no digital cue with address {1}", Address, member.Address);
                return;
            }            
            tracer.Print("Received from '{0}' D[{1}]={2}", digitalcues.Address, member.Address, value);
            ushort index = (ushort)Digitals.GetSpIndex[member.Address];
            ChangeDigital.Invoke(member.Address, index, value);
        }

        /// <summary>
        /// the delegate implementation for CuesCueSetter. This method should be assigned to the Cues.SetCue when by the CrosspointRouter.Connect()
        /// </summary>
        /// <param name="serialcues">the Cues collection whose member is being set</param>
        /// <param name="member">the member whose value is being set</param>
        /// <param name="value">the new value of the cue</param>
        internal void AnalogCuesSetter(Cues<ushort> analogcues, ICue<ushort> member, ushort value)
        {
            if (this.ChangeAnalog == null)
            {
                tracer.Print("ChangeAnalog has no delegate assigned");
                return;
            }
            if (!Analogs.ContainsKey(member.Address))
            {
                tracer.Print("This crosspoint has no analog cue with address {0}", member.Address);
                return;
            }
            tracer.Print("Received from '{0}' A[{1}]={2}", analogcues.Address, member.Address, value);            
            ushort index = (ushort)Analogs.GetSpIndex[member.Address];
            ChangeAnalog.Invoke(member.Address, index, value);
        }


        /// <summary>
        /// the delegate implementation for CuesCueSetter. This method should be assigned to the Cues.SetCue when by the CrosspointRouter.Connect()
        /// </summary>
        /// <param name="serialcues">the Cues collection whose member is being set</param>
        /// <param name="member">the member whose value is being set</param>
        /// <param name="value">the new value of the cue</param>
        internal void SerialCuesSetter(Cues<string> serialcues, ICue<string> member, string value)
        {
            if (this.ChangeSerial == null)
            {
                tracer.Print("ChangeSerial has no delegate assigned");
                return;
            }
            if (!Serials.ContainsKey(member.Address))
            {
                tracer.Print("This crosspoint has no serial cue with address {0}", member.Address);
                return;
            }
            tracer.Print("Received from '{0}' S[{1}]={2}", serialcues.Address, member.Address, value);
            ushort index = (ushort)Serials.GetSpIndex[member.Address];
            ChangeSerial.Invoke(member.Address, index, value);
        }

        public void PrintAllAddresses()
        {
            string msg = GetAllAddresses();
            tracer.Print(msg);

        }
        public string GetAllAddresses()
        {
            string msg = string.Format("Digital Cues:{0}", Digitals.GetAllAddresses());
            msg += string.Format("\r\nAnalog Cues:{0}", Analogs.GetAllAddresses());
            msg += string.Format("\r\nSerial Cues:{0}", Serials.GetAllAddresses());
            return msg;
        }

        private void CrosspointAddedHandler(object sender, OnAddedCrosspointEventArgs args)
        {
            if (PreInitConnect.Count == 0)
                return;

            string address = args.Crosspoint.Address;

            if (PreInitConnect.Contains(address))
                if (Connect(address) > 0)
                    PreInitConnect.Remove(args.Crosspoint.Address);            
        }
    }


    /// <summary>
    /// a collection of Digital, Analog, and Serial Cues
    /// Simpl+ should use this object. Use the SetAddress() method to initialize this class.
    /// </summary>
    public class ControlCrosspoint : Crosspoint
    {
        public ControlCrosspoint()
            :base(CrosspointType.Control)
        {

        }
    }

    /// <summary>
    /// a collection of Digital, Analog, and Serial Cues
    /// Simpl+ should use this object. Use the SetAddress() method to initialize this class.
    /// </summary>
    public class EquipmentCrosspoint : Crosspoint        
    {
        public EquipmentCrosspoint()
            : base(CrosspointType.Equipment)
        {

        }
    }
}
