using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CrosspointEx
{
    public class OnConnectedEventArgs : EventArgs
    {
        public string AddedOrRemovedCrosspointAddress;
        public string Connections;
        public OnConnectedEventArgs()
        {
            Connections = "";
        }
        public OnConnectedEventArgs(string added_or_removed, List<string> connections)
        {
            AddedOrRemovedCrosspointAddress = added_or_removed;
            Connections = string.Join(",", connections.ToArray());
        }
    }

    public class OnAddedCrosspointEventArgs : EventArgs
    {
        public Crosspoint Crosspoint {get;set;}
        public OnAddedCrosspointEventArgs (Crosspoint crosspoint)
	    {
            Crosspoint = crosspoint;
	    }
    }
    
    /// <summary>
    /// a static class the does the actual routing.
    /// </summary>
    internal static class CrosspointRouter
    {
        private static TracerContext tracer;
        private static Dictionary<string, Crosspoint> ControlCrosspoints;
        private static Dictionary<string, Crosspoint> EquipmentCrosspoints;
        private static Dictionary<string, List<string>> ControlConnections;
        private static Dictionary<string, List<string>> EquipmentConnections;
        
        public static event EventHandler<OnAddedCrosspointEventArgs> OnAddedControlCrosspoint;
        public static event EventHandler<OnAddedCrosspointEventArgs> OnAddedEquipmentCrosspoint;

        static CrosspointRouter()
        {
            tracer = new TracerContext(typeof(CrosspointRouter).Name);
            ControlCrosspoints = new Dictionary<string, Crosspoint>();
            EquipmentCrosspoints = new Dictionary<string, Crosspoint>();
            ControlConnections = new Dictionary<string, List<string>>();
            EquipmentConnections = new Dictionary<string, List<string>>();
        }

        public static void AddControlCrosspoint(Crosspoint crosspoint)
        {
            if (crosspoint == null)
                return;
            if (ControlCrosspoints.ContainsKey(crosspoint.Address))
            {
                tracer.Print("Error, Cannot add duplicate crosspoint address {0}", crosspoint.Address);
                tracer.ErrorLog("Error, Cannot add duplicate crosspoint address {0}", crosspoint.Address);
                return;
            }
            ControlCrosspoints.Add(crosspoint.Address, crosspoint);
            tracer.Print("Control {0} added.", crosspoint.Address);
            if (OnAddedControlCrosspoint != null)
                OnAddedControlCrosspoint.Invoke(typeof(CrosspointRouter), new OnAddedCrosspointEventArgs(crosspoint));
        }

        public static void AddEquipmentCrosspoint(Crosspoint crosspoint)
        {
            if (crosspoint == null)
                return;
            if (EquipmentCrosspoints.ContainsKey(crosspoint.Address))
            {
                tracer.Print("Error, Cannot add duplicate crosspoint address {0}", crosspoint.Address);
                tracer.ErrorLog("Error, Cannot add duplicate crosspoint address {0}", crosspoint.Address);
                return;
            }
            EquipmentCrosspoints.Add(crosspoint.Address, crosspoint);
            tracer.Print("Equipment {0} added.", crosspoint.Address);
            if (OnAddedEquipmentCrosspoint != null)
                OnAddedEquipmentCrosspoint.Invoke(typeof(CrosspointRouter), new OnAddedCrosspointEventArgs(crosspoint));
        }

        public static void AddCrosspoint(Crosspoint crosspoint)
        {
            if (crosspoint == null)
                return;
            if (crosspoint.Type == CrosspointType.Control)
            {
                AddControlCrosspoint(crosspoint);
            }
            else
            {
                AddEquipmentCrosspoint(crosspoint);
            }
        }



        /// <summary>
        /// connects a control crosspoint to an equipment crosspoint
        /// </summary>
        /// <param name="control_address"></param>
        /// <param name="equipment_address"></param>
        /// <returns>returns true if already connected or a successfully connected. </returns>
        public static bool Connect(string control_address, string equipment_address)
        {
            tracer.Print("Connect {0} to {1}", control_address, equipment_address);
            if (!ControlCrosspoints.ContainsKey(control_address))
            {
                tracer.Print("Cannot Connect. ControlCrosspoint {0} does not exist", control_address);
                tracer.ErrorLog("Cannot Connect. ControlCrosspoint {0} does not exist", control_address);
                return false;
            }
            Crosspoint cxpoint = ControlCrosspoints[control_address];
            tracer.Print("Control Crosspoint Found:{0}", cxpoint.Address);


            if (!EquipmentCrosspoints.ContainsKey(equipment_address))
            {
                tracer.Print("Cannot Connect. EquipmentCrosspoint {0} does not exist.", equipment_address);
                tracer.ErrorLog("Cannot Connect. EquipmentCrosspoint {0} does not exist.", equipment_address);
                return false;
            }

            Crosspoint expoint = EquipmentCrosspoints[equipment_address];
            tracer.Print("Equipment Crosspoint Found:{0}", expoint.Address);
            if (ControlConnections.ContainsKey(control_address))
                if (ControlConnections[control_address].Contains(equipment_address))
                {
                    tracer.Print("Control {0} already connected to {1}", control_address, equipment_address);
                    return true;
                }
            tracer.Print("No existing control {0} to equipment {1} connection found.", control_address, equipment_address);


            if (EquipmentConnections.ContainsKey(equipment_address))
                if (EquipmentConnections[equipment_address].Contains(control_address))
                {
                    tracer.Print("Equipment {0} already connected to {1}", equipment_address, control_address);
                    return true;
                }
            tracer.Print("No existing equipment {1} to control {0} connection found.", control_address, equipment_address);

            var added = AddConnection(control_address, equipment_address);

            if (added)
            {
                tracer.Print("Connections added successfully");                
                ConnectDelegates(cxpoint, expoint);
                tracer.Print("Invoking OnConnected events");
                ControlCrosspoints[control_address].InvokeNewConnect(new OnConnectedEventArgs(equipment_address, ControlConnections[control_address]));
                EquipmentCrosspoints[equipment_address].InvokeNewConnect(new OnConnectedEventArgs(control_address, EquipmentConnections[equipment_address]));
                return true;
            }
            tracer.Print("CrosspointEx AddConnection failed on {0} to {1}", control_address, equipment_address);
            tracer.ErrorLog("CrosspointEx AddConnection failed on {0} to {1}", control_address, equipment_address);
            return false;
        }

        private static bool AddConnection(string cxp_address, string exp)
        {
            tracer.Print("AddConnection c:{0}, e:{1}", cxp_address, exp);

            if (!ControlConnections.ContainsKey(cxp_address))
                ControlConnections.Add(cxp_address, new List<string>());
            

            if (!EquipmentConnections.ContainsKey(exp))
                EquipmentConnections.Add(exp, new List<string>());

            bool cxp_added = false;
            bool exp_added = false;

            if (!ControlConnections[cxp_address].Contains(exp))
            {
                tracer.Print("Adding Connection c:{0} e:{1}", cxp_address, exp);
                ControlConnections[cxp_address].Add(exp);
                cxp_added = true;
            }

            if (!EquipmentConnections[exp].Contains(cxp_address))
            {
                tracer.Print("Adding Connection e:{0} c:{1}", exp, cxp_address);
                EquipmentConnections[exp].Add(cxp_address);
                exp_added = true;
            }
            return cxp_added && exp_added;
        
        }

        private static void RemoveConnections(string cxp_address, string exp)
        {
            if (ControlConnections.ContainsKey(cxp_address))
            {
                if (ControlConnections[cxp_address].Contains(exp))
                {
                    Tracer.Print("Removing from ControlConnections Connection c:{0} e:{1}", cxp_address, exp);
                    ControlConnections[cxp_address].Remove(exp);
                }
                else
                    Tracer.Print("Did not find {0} in ControlConnections[{1}]", exp, cxp_address);
            }
            else
                Tracer.Print("Did not find {0} in ControlConnections", cxp_address);
           
            
            if (EquipmentConnections.ContainsKey(exp))
            {
                if (EquipmentConnections[exp].Contains(cxp_address))
                {
                    Tracer.Print("Removing from EquipmentConnections Connection e:{0} c:{1}", exp, cxp_address);
                    EquipmentConnections[exp].Remove(cxp_address);
                }
                else
                {
                    Tracer.Print("Did not find {0} in EquipmentConnections[{1}]", cxp_address, exp);
                }
            }
            else
            {
                Tracer.Print("Did not find {0} in EquipmentConnections", exp);
            }            
        }

        private static void ConnectDelegates(Crosspoint cxp, Crosspoint exp)
        {
            Tracer.Print("Connecting Delegates of {0} and {1}", cxp.Address, exp.Address);
            cxp.Digitals.SetCue += exp.DigitalCuesSetter;
            cxp.Analogs.SetCue += exp.AnalogCuesSetter;
            cxp.Serials.SetCue += exp.SerialCuesSetter;

            exp.Digitals.SetCue += cxp.DigitalCuesSetter;
            exp.Analogs.SetCue += cxp.AnalogCuesSetter;
            exp.Serials.SetCue += cxp.SerialCuesSetter;
        }
       
        /// <summary>
        /// Disconnects a control crosspoint to an equipment crosspoint
        /// </summary>
        /// <param name="control_address"></param>
        /// <param name="equipment_address"></param>
        public static void Disconnect(string control_address, string equipment_address)
        {
            Tracer.Print("Disconnect {0} from {1}", control_address, equipment_address);
            if (!ControlCrosspoints.ContainsKey(control_address))
            {
                Tracer.ErrorLog("Cannot Disconnect. ControlCrosspoint {0} does not exist", control_address);
                return;
            }
            if (!EquipmentCrosspoints.ContainsKey(equipment_address))
            {
                Tracer.ErrorLog("Cannot Disconnect. EquipmentCrosspoint {0} does not exist.", equipment_address);
                return;
            }
            RemoveConnections(control_address, equipment_address);
            DisconnectDelegates(ControlCrosspoints[control_address], EquipmentCrosspoints[equipment_address]);

            ControlCrosspoints[control_address].InvokeNewDisconnect(new OnConnectedEventArgs(equipment_address, ControlConnections[control_address]));
            EquipmentCrosspoints[equipment_address].InvokeNewDisconnect(new OnConnectedEventArgs(control_address, EquipmentConnections[equipment_address]));
        }

        /// <summary>
        /// Disconnects a control crosspoint to an equipment crosspoint
        /// </summary>
        /// <param name="control_address"></param>
        /// <param name="equipment_address"></param>
        public static void Disconnect_ControlFromAllEquipment(string control_address)
        {
            Tracer.Print("Disconnect_ControlFromAllEquipment {0}", control_address);
            if (!ControlConnections.ContainsKey(control_address))
            {
                Tracer.Print("Error, ControlConnections does not contain {1}", control_address);
                return;
            }
            var exps = ControlConnections[control_address].ToArray(); //copy it because we're about to clear it.
            
            foreach (var exp in exps)
            {
                RemoveConnections(control_address, exp);
                DisconnectDelegates(ControlCrosspoints[control_address], EquipmentCrosspoints[exp]);
            }
            
            //Check for any left over delegates stills subscribed.
            if (ControlCrosspoints[control_address].Digitals.SetCue != null)
            {
                Tracer.Print("Error, left over ControlCrosspoints[{0}].Digitals.SetCue still assigned. Setting it to null", control_address);
                ControlCrosspoints[control_address].Digitals.SetCue = null;
            }
            if (ControlCrosspoints[control_address].Analogs.SetCue != null)
            {
                Tracer.Print("Error, left over ControlCrosspoints[{0}].Analogs.SetCue still assigned. Setting it to null", control_address);
                ControlCrosspoints[control_address].Analogs.SetCue = null;
            }
            if (ControlCrosspoints[control_address].Serials.SetCue != null)
            {
                Tracer.Print("Error, left over ControlCrosspoints[{0}].Serials.SetCue still assigned. Setting it to null", control_address);
                ControlCrosspoints[control_address].Serials.SetCue = null;
            }
        }

        private static void DisconnectDelegates(Crosspoint cxp, Crosspoint exp)
        {
            Tracer.Print("Disconnecting Delegates of {0} and {1}", cxp.Address, exp.Address);
            cxp.Digitals.SetCue -= exp.DigitalCuesSetter;
            cxp.Analogs.SetCue -= exp.AnalogCuesSetter;
            cxp.Serials.SetCue -= exp.SerialCuesSetter;

            exp.Digitals.SetCue -= cxp.DigitalCuesSetter;
            exp.Analogs.SetCue -= cxp.AnalogCuesSetter;
            exp.Serials.SetCue -= cxp.SerialCuesSetter;
        }

        /// <summary>
        /// Disconnects a equipment crosspoint from all control crosspoints.
        /// </summary>
        /// <param name="control_address"></param>
        /// <param name="equipment_address"></param>
        public static void Disconnect_EquipmentFromAllControl(string equipment_address)
        {
            Tracer.Print("Disconnect_EquipmentFromAllControl {0} from {1}", equipment_address);
            if (!EquipmentConnections.ContainsKey(equipment_address))
            {
                Tracer.Print("Error, EquipmentConnections does not contain key {0}", equipment_address);
                return;
            }
            var cxps = EquipmentConnections[equipment_address].ToArray(); //copy it because RemoveConnections is about to modify the list
            
            foreach (var cxp in cxps)
            {
                RemoveConnections(cxp, equipment_address);
                DisconnectDelegates(ControlCrosspoints[cxp], EquipmentCrosspoints[equipment_address]);
            }


            //check for any left overs
            if (EquipmentCrosspoints[equipment_address].Digitals.SetCue != null)
            {
                Tracer.Print("Error, left over EquipmentCrosspoints[{0}].Digitals.SetCue still assigned. Setting it to null", equipment_address);
                EquipmentCrosspoints[equipment_address].Digitals.SetCue = null;
            }

            if (EquipmentCrosspoints[equipment_address].Analogs.SetCue != null)
            {
                Tracer.Print("Error, left over EquipmentCrosspoints[{0}].Analogs.SetCue still assigned. Setting it to null", equipment_address);
                EquipmentCrosspoints[equipment_address].Analogs.SetCue = null;
            }

            if (EquipmentCrosspoints[equipment_address].Serials.SetCue != null)
            {
                Tracer.Print("Error, left over EquipmentCrosspoints[{0}].Serials.SetCue still assigned. Setting it to null", equipment_address);
                EquipmentCrosspoints[equipment_address].Serials.SetCue = null;
            }
        }

        public static Dictionary<string, List<string>> GetAllConnections()
        {
            return ControlConnections;
        }

        public static Dictionary<string, Crosspoint> GetAllControls()
        {
            return ControlCrosspoints;
        }
        public static Dictionary<string, Crosspoint> GetAllEquipments()
        {
            return EquipmentCrosspoints;
        }
    }
}