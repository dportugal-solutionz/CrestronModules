using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BarcoTransFormNCMSFor3Series
{
    //Reply(example)
    //<I:BARCO @NOICLT32051||K:CMS||O:REGetWindowListVersion3
    //  ||A1: PerspectiveWindow,P1,1,0.0,0.0,960.0,600.0,1
    //  ||A2: PerspectiveWindow,P3,4,960.0,600.0,960.0,600.0,2||>
    //<I:BARCO@server02||K:CMS||O:REGetWindowListVersion3||NOK:||>{0D}{0A}
    public class GetWindowsResponse : IRxParser
    {
        const string pattern = @"<I:BARCO.*\|\|K:CMS\|\|O:REGetWindowListVersion3\|\|.*\|\|>";
        private BarcoCMS CMS { get; set; }

        public GetWindowsResponse(BarcoCMS cms)
        {
            this.CMS = cms;
        }
        public void Action(string rx)
        {
            Tracer.PrintLine("GetWindowsResponse Action");
            if (CMS == null)
            {
                Tracer.PrintLine("GetWindowsResponse Action cms is null");
                return;
            }
            if (CMS.LastCommandSent == null)
            {
                Tracer.PrintLine("GetWindowsResponse Action cms.LastCommandSent is null");
                return;
            }

            Tracer.PrintLine(string.Format("LastCommand Display Index {0} {1}",CMS.LastCommandSent.DisplayIndex,CMS.Config.Displays[CMS.LastCommandSent.DisplayIndex]));
            //< I:BARCO @NOICLT32051|| K:CMS || O:REGetWindowListVersion3
            //  ||A1: PerspectiveWindow,P1,1,0.0,0.0,960.0,600.0,1
            //  ||A2: PerspectiveWindow,P3,4,960.0,600.0,960.0,600.0,2||>           
            //<I:BARCO@server02||K:CMS||O:REGetWindowListVersion3||NOK:||>{0D}{0A}
            //string[] tokens = rx.Split(new string[] { "||" }), StringSplitOptions.RemoveEmptyEntries);
            
            string[] tokens = Regex.Split(rx,@"\|\|");

            //remove empties
            tokens = tokens.Where( val => val.Length > 0).ToArray();


            if (rx.Contains("||NOK:||"))
            {
                Tracer.PrintLine("NOK received.");
                //then there are no perspectives on the wall.
                foreach (var window in CMS.Config.Windows)
                {
                    int lastDI = CMS.LastCommandSent.DisplayIndex;
                    string display = CMS.Config.Displays[lastDI];
                    if (window.Display == display)
                    {
                        Tracer.PrintLine(string.Format("deactivating window {0}",window.Perspective));
                        window.IsActive = false;
                    }
                }
            }

            List<ConfigWindow> Received = new List<ConfigWindow>();
            foreach (var token in tokens)
            {
                if (token.Contains("PerspectiveWindow"))
                {
                    string[] values = token.Split(',');
                    string perspective = values[1];
                    Tracer.PrintLine(string.Format("Perspective Received:{0}",perspective));
                    var configWindow = FindWindowByPerspective(perspective);
                    if (configWindow != null)
                    {
                        Tracer.PrintLine(string.Format("activating window {0}",configWindow.Perspective));
                        configWindow.IsActive = true;
                        Received.Add(configWindow);
                    }
                    else
                        Tracer.PrintLine(string.Format("did not find window with perspective {0}",perspective));
                }
            }

            //we need to set the other (not received) perspectives that are for this display/wall to false.
            var WindowsOfTheWall =from window in CMS.Config.Windows where window.Display == CMS.Config.Displays[CMS.LastCommandSent.DisplayIndex] select window;
            foreach(var window in WindowsOfTheWall)
            {
                if (!Received.Contains(window))
                {
                    window.IsActive = false;
                }
            }
        }

        public ConfigWindow FindWindowByPerspective(string perspective)
        {
            foreach (var window in CMS.Config.Windows)
                if (window.Perspective == perspective)
                    return window;
            return null;
        }

        public bool IsMatch(string rx)
        {
            return Regex.IsMatch(rx, pattern);
        }
    }
}
