using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;

namespace CrosspointEx
{
    public delegate void UcmdOutput(string output);


    /// <summary>
    /// an accessor class for Simpl+ module to the CrosspointRouter class
    /// valid ucmds:
    ///     cxpex print all connections     - prints to console all control to equipment connections.
    ///     cxpex print all cxp             - prints to console all control crosspoints
    ///     cxpex print all exp             - prints to console all equipment crosspoints
    ///     cxpex print (exp|cxp) 'address' - prints to console all cues of a crosspoint
    /// </summary>
    public class CrosspointRouterUcmd
    {
        /// <summary>
        /// a delegate so as to see the output sent to the console other than console.
        /// </summary>
        public UcmdOutput CmdOutput{get;set;}

        //parser delegate signature
        //the method should return a string once it completes the parsing.
        private delegate string UcmdParser(string data);
        
        //the key of the dictionary should be a regex pattern to match against the incoming UCMD.
        private readonly Dictionary<string, UcmdParser> Parsers; //key is the regex pattern

        private readonly TracerContext tracer;

        public CrosspointRouterUcmd()
        {
            tracer = new TracerContext(typeof(CrosspointRouterUcmd).Name);
            Parsers = new Dictionary<string, UcmdParser>();
            Parsers.Add("cxpex print all connections", ShowAllConnections);
            Parsers.Add("cxpex print all cxp", ShowAllCxp);
            Parsers.Add("cxpex print all exp", ShowAllExp);
            Parsers.Add("cxpex print (exp|cxp) '(.*)'", PrintCues);
            Parsers.Add("cxpex debug (.*)",SetDebug);
        }

        /// <summary>
        /// receiver of the UCMD from Simpl/Simpl+
        /// </summary>
        /// <param name="ucmd"></param>
        public void UserProgCommand(string ucmd)
        {
            tracer.Print("ucmd received:{0}", ucmd);
            string result = "";
            bool found = false;
            foreach (var parser in Parsers)
            {
                if (Regex.IsMatch(ucmd, parser.Key))
                {
                    found = true;
                    result = parser.Value(ucmd);
                    break;
                }
            }
            if (!found)
                result = "Unknown command\r\n";
            tracer.Print(result);
            if (CmdOutput != null)
                CmdOutput.Invoke(result);
        }

        private string ShowAllConnections(string ucmd)
        {            
            string result = "";
            Dictionary<string, List<string>> connections = CrosspointRouter.GetAllConnections();
            foreach (var kv in connections)
            {
                string cxp = kv.Key;

                if (kv.Value.Count == 0)
                    result += string.Format("\r\nC:{0} -> none", cxp);
                else
                    foreach(var exp in kv.Value)                
                        result += string.Format("\r\nC:{0} -> E:{1}", cxp, exp);
                
            }
            tracer.Print("ShowAllConnections={0}",result);
            return result;
        }

        private string ShowAllCxp(string ucmd)
        {
            var cxp = CrosspointRouter.GetAllControls();
            string result = "";
            foreach (var kv in cxp)
            {
                result += string.Format("\r\nC:{0}",kv.Value.Address);
            }
            tracer.Print("ShowAllCxp={0}", result);
            return result;
        }

        private string ShowAllExp(string ucmd)
        {
            var cxp = CrosspointRouter.GetAllEquipments();
            string result = "";
            foreach (var kv in cxp)
            {
                result += string.Format("\r\nE:{0}", kv.Value.Address);
            }
            tracer.Print("ShowAllExp={0}", result);
            return result;
        }

        private string PrintCues(string ucmd)
        {
            MatchCollection matches = Regex.Matches(ucmd, "cxpex print (exp|cxp) '(.*)'");
            foreach (Match m in matches)
            {
                if (m.Groups.Count == 3)
                {
                    bool exp = m.Groups[1].ToString() == "exp";
                    string address = m.Groups[2].ToString();

                    Dictionary<string, Crosspoint> list = CrosspointRouter.GetAllControls();

                    if (exp)
                        list = CrosspointRouter.GetAllEquipments();

                    if (!list.ContainsKey(address))
                        return string.Format("Crosspoint {0} not found", address);

                    string result = list[address].GetAllAddresses();                    
                    tracer.Print("ShowAllExp={0}", result);
                    return result;
                }
                else
                {
                    return "PrintCues, Invalid number of match groups found";                    
                }                
            }
            return "PrintCues, nothing to show";
        }
    
        private string SetDebug(string ucmd)
        {
            MatchCollection matches = Regex.Matches(ucmd, "cxpex debug (.*)");
            try
            {
                int i = Convert.ToInt16(matches[0].Groups[1].ToString());
                Tracer.Enabled = i > 0;
                return string.Format("debug is {}",Tracer.Enabled);
            }
            catch
            {
                //do nothing
                return string.Format("debug is {}", Tracer.Enabled);
            }
        }
    }
}