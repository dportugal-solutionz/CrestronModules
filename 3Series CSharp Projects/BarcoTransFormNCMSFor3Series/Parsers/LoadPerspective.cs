using System;
using System.Text.RegularExpressions;

namespace BarcoTransFormNCMSFor3Series
{
    //Reply Example:
    //<I:BARCO@KARCLT0409||K:CMS||O:RELoadPerspective||A1:OK||>
    public class LoadPerspective : IRxParser
    {
        const string pattern = @"<I:BARCO.*\|\|K:CMS\|\|O:RELoadPerspective\|\|A1:(.*)\|\|>";
        private BarcoCMS CMS { get; set; }

        public LoadPerspective(BarcoCMS cms)
        {
            this.CMS = cms;
        }
        public void Action(string rx)
        {
            Tracer.PrintLine("LoadPerspective Action");
            if (CMS == null)
            {
                Tracer.PrintLine("LoadPerspective Action cms is null");
                return;
            }
            if (CMS.LastCommandSent == null)
            {
                Tracer.PrintLine("LoadPerspective Action cms.LastCommandSent is null");
                return;
            }
            int winIndex = CMS.LastCommandSent.WindowIndex;
            
            //int srcIndex = cms.LastCommandSent.SourceIndex;
            if (winIndex < CMS.Config.Windows.Count)
            {
                Tracer.PrintLine(string.Format("LastCommand Window Index {0} {1}", winIndex, CMS.Config.Windows[winIndex].Perspective));
                if (Regex.Match(rx, pattern).Groups[1].ToString().ToUpper().Trim() == "OK")
                {
                    Tracer.PrintLine(string.Format("activating window {0}",CMS.Config.Windows[winIndex].Perspective));
                    CMS.Config.Windows[winIndex].IsActive = true;
                }
                else
                {
                    //CMS.Config.Windows[winIndex].IsActive = false;
                    Tracer.PrintLine(string.Format("did not find OK. Token is ({0})",Regex.Match(rx, pattern).Groups[1].ToString().ToUpper().Trim()));
                }
            }
            else
            {
                Tracer.PrintLine("LoadPerspective Action cms.LastCommandSent invalid window index");
            }
        }

        public bool IsMatch(string rx)
        {
            return Regex.IsMatch(rx, pattern);
        }
    }
}
