using System;
using System.Text.RegularExpressions;

namespace BarcoTransFormNCMSFor3Series
{
    public class LoadSourceOnPerspectiveResponse : IRxParser
    {        
        //<I:BARCO@server01||K:CMS||O:RELoadSourceOnPerspective||A1:OK||>


        const string pattern = @"<I:BARCO.*\|\|K:CMS\|\|O:RELoadSourceOnPerspective\|\|A1:(.*)\|\|>";
        private BarcoCMS CMS { get; set; }

        public LoadSourceOnPerspectiveResponse(BarcoCMS cms)
        {
            this.CMS = cms;
        }

        public void Action(string rx)
        {
            Tracer.PrintLine("LoadSourceOnPerspectiveResponse Action");
            if (CMS == null)
            {
                Tracer.PrintLine("LoadSourceOnPerspectiveResponse Action cms is null");
                return;
            }
            if (CMS.LastCommandSent == null)
            {
                Tracer.PrintLine("LoadSourceOnPerspectiveResponse Action cms.LastCommandSent is null");
                return;
            }
            int winIndex = CMS.LastCommandSent.WindowIndex;
            int srcIndex = CMS.LastCommandSent.SourceIndex;
            
            if (winIndex >= CMS.Config.Windows.Count)
            {
                Tracer.PrintLine("LoadSourceOnPerspectiveResponse Action cms.LastCommandSent invalid window index");
                return;
            }
            
            if (srcIndex >= CMS.Config.Sources.Count)
            {
                Tracer.PrintLine("LoadSourceOnPerspectiveResponse Action cms.LastCommandSent invalid source index");
                return;
            }

            Tracer.PrintLine(string.Format("LastCommand Window Index {0} {1}",winIndex,CMS.Config.Windows[winIndex].Perspective));
            Tracer.PrintLine(string.Format("LastCommand Source Index {0} {1}",srcIndex,CMS.Config.Sources[srcIndex].Name));

            if (Regex.Match(rx, pattern).Groups[1].ToString().ToUpper().Trim() == "OK")
            {
                Tracer.PrintLine(string.Format("Window {0} source set to {1}",CMS.Config.Windows[winIndex].Perspective,CMS.Config.Sources[srcIndex].Name));
                CMS.Config.Windows[winIndex].CurrentSource = CMS.Config.Sources[srcIndex].Name;       
            }
            else
            {
                Tracer.PrintLine(string.Format("Window Source not set. Did not find OK. Token is ({0})",Regex.Match(rx, pattern).Groups[1].ToString().ToUpper().Trim()));
            }            
        }

        public bool IsMatch(string rx)
        {
            return Regex.IsMatch(rx, pattern);
        }
    }
}
