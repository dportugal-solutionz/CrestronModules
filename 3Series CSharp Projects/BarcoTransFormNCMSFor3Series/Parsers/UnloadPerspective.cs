using System;
using System.Text.RegularExpressions;

namespace BarcoTransFormNCMSFor3Series
{
    //Reply (example)
    //<I:BARCO @noiclt22815||K:CMS||O:REUnloadPerspective||A1:OK||>
    //sent     <I:BARCO||K:CMS||O:UnloadPerspective||A1:A21||A2:Wall-1||>
    //received <I:BARCO @server02||K:CMS||O:REUnloadPerspective||A1:OK||>
    public class UnloadPerspective : IRxParser
    {
        const string pattern = @"<I:BARCO.*\|\|K:CMS\|\|O:REUnloadPerspective\|\|A1:(.*)\|\|>";
        private BarcoCMS CMS { get; set; }

        public UnloadPerspective(BarcoCMS cms)
        {
            this.CMS = cms;
        }
        public void Action(string rx)
        {
            Tracer.PrintLine("UnloadPerspective Action");
            if (CMS == null)
            {
                Tracer.PrintLine("UnloadPerspective Action cms is null");
                return;
            }
            if (CMS.LastCommandSent == null)
            {
                Tracer.PrintLine("UnloadPerspective Action cms.LastCommandSent is null");
                return;
            }
            int winIndex = CMS.LastCommandSent.WindowIndex;
            Tracer.PrintLine(string.Format("LastCommand Window Index {0} {1}",winIndex,CMS.Config.Windows[winIndex].Perspective));
            //int srcIndex = cms.LastCommandSent.SourceIndex;
            if (winIndex < CMS.Config.Windows.Count)
            {
                if (Regex.Match(rx, pattern).Groups[1].ToString().ToUpper().Trim() == "OK")
                {
                    Tracer.PrintLine(string.Format("deactivating window {0}",CMS.Config.Windows[winIndex].Perspective));
                    CMS.Config.Windows[winIndex].IsActive = false;
                }
                else
                {
                    //CMS.Config.Windows[winIndex].IsActive = true;
                    Tracer.PrintLine(string.Format("did not find OK. Token is ({0})",Regex.Match(rx, pattern).Groups[1].ToString().ToUpper().Trim()));
                }
            }
            else
            {
                Tracer.PrintLine("UnloadPerspective Action cms.LastCOmmandSent invalid window index");
            }
        }

        public bool IsMatch(string rx)
        {
            return Regex.IsMatch(rx, pattern);
        }
    }
}
