using Crestron.SimplSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp.CrestronIO;

namespace BarcoTransFormNCMSFor3Series
{
    /// <summary>
    /// Handles string to send and parse to Barco CMS
    /// Reference Document R591408_23_Installation.pdf (see page 182)
    /// </summary>
    public partial class BarcoCMS
    {
        /// <summary>
        /// file directory that contains the config json file
        /// </summary>
        public string FileDirectory = "/user/";

        /// <summary>
        /// file name pattern that contains the config json file
        /// </summary>
        public string FilePattern = @"*config.json";

        /// <summary>
        /// file name found using the filedirectory and filepattern
        /// </summary>
        private string fname;
        public string FileName
        {
            get
            {
                return fname;
            }
            private set
            {
                fname = value;
                if (OnFileNameChanged != null)
                    OnFileNameChanged(this, new StringEventArgs(fname));
            }
        }
        public event EventHandler<StringEventArgs> OnFileNameChanged;

        private bool isloading = false;
        public bool IsLoading
        {
            get
            {
                return isloading;
            }
            set
            {
                isloading = value;
                if(IsLoadingChanged!=null)
                    IsLoadingChanged(this, new BoolEventArgs(isloading));
            }
        }
        public event EventHandler<BoolEventArgs> IsLoadingChanged;

        private bool isfound = false;
        public bool IsFound
        {
            get
            {
                return isfound;
            }
            set
            {
                isfound = value;
                if(IsFoundChanged!=null)
                    IsFoundChanged(this, new BoolEventArgs(isfound));
            }
        }
        public event EventHandler<BoolEventArgs> IsFoundChanged;

        private bool readsuccessful = false;
        public bool ReadSuccessful
        {
            get
            {
                return readsuccessful;
            }
            set
            {
                readsuccessful = value;
                if(OnReadSuccessfulChanged!=null)
                    OnReadSuccessfulChanged(this, new BoolEventArgs(readsuccessful));
            }
        }
        public event EventHandler<BoolEventArgs> OnReadSuccessfulChanged;

        private bool readfailed = false;
        public bool ReadFailed
        {
            get
            {
                return readfailed;
            }
            set
            {
                readfailed = value;
                if(OnReadFailedChanged!=null)
                    OnReadFailedChanged(this, new BoolEventArgs(readfailed));
            }
        }
        public event EventHandler<BoolEventArgs> OnReadFailedChanged;

        /// <summary>
        /// String to send to CMS
        /// </summary>
        public event EventHandler<StringEventArgs> SendToCMS;

        /// <summary>
        /// json config data
        /// </summary>
        public BarcoCMSConfig Config;


        private readonly TransmitQueue<Command> txQ = new TransmitQueue<Command>(300);

        /// <summary>
        /// Read the JSON config file
        /// </summary>
        public void ReadFile()
        {
            Tracer.PrintLine("Reading File");
            try
            {
                if (IsLoading)
                {
                    Tracer.PrintLine("Currently being read...");
                }
                else
                {
                    IsLoading = true;
                    IsFound = false;
                    ReadSuccessful = false;
                    ReadFailed = false;

                    Tracer.PrintLine("Searching for file.");
                    string[] files = Directory.GetFiles(FileDirectory, FilePattern);
                    if (files.Length == 0)
                    {
                        Tracer.LogError("Config file not found with matching pattern " + FilePattern);
                        return;
                    }

                    FileName = files[0];
                    if (FileName.Length == 0)
                    {
                        Tracer.LogError("Config file name length is 0.");
                        return;
                    }
                    Tracer.PrintLine("File Found:" + FileName);
                    IsFound = true;

                    StreamReader sr = new StreamReader(FileName);
                    string filecontents = sr.ReadToEnd();
                    sr.Close();
                    Tracer.PrintLine("File Contents Length:" + filecontents.Length);
                    try
                    {
                        Tracer.PrintLine("Deserializing File Contents");
                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Error,
                            NullValueHandling = NullValueHandling.Include,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                            Error = DeserializeErrorHandler
                        };
                        this.Config = JsonConvert.DeserializeObject<BarcoCMSConfig>(filecontents);
                        Tracer.PrintLine("Config File Report:");
                        PrintDataProperties();

                    }
                    catch (Exception e)
                    {                       
                        Tracer.PrintLine("Config error deserializing file.\r\n" + e.ToString());
                        Tracer.LogError("Config error deserializing file.\r\n" + e.ToString());
                        ReadFailed = true;
                    }

                    //sort the windows by Id
                    //if (Config != null)
                    //    Config.Windows.Sort((x, y) => x.Id.CompareTo(y.Id));

                    //subscribe to the window's on source change
                    foreach (var window in Config.Windows)
                    {
                        window.OnCurrentSourceChanged += WindowCurrentSourceChangedHandler;
                        window.OnActiveChanged += WindowActiveChangedHandler;
                    }

                    QueryDisplayTimers.Clear();
                    foreach (var display in Config.Displays)
                    {
                        QueryDisplayTimers.Add(null);
                    }
                    IsLoading = false;                    
                    ReadSuccessful = true;
                    ErrorLog.Notice("BarcoCMS Config File " + FileName + " Loaded");
                    Tracer.PrintLine("Read Completed");
                }
            }
            catch (Exception e)
            {
                ReadFailed = true;
                IsLoading = false;
                
                Tracer.PrintLine("Config Error reading file.\r\n" + e.ToString());            
                Tracer.LogError("Config error reading file.\r\n" + e.ToString());
            }
        }

        public event EventHandler<IntArrayEventArgs> OnWindowActiveChanged;

        private void WindowActiveChangedHandler(object sender, BoolEventArgs e)
        {
            ConfigWindow window = (ConfigWindow)sender;
            if(OnWindowActiveChanged!=null)
                OnWindowActiveChanged(this, new IntArrayEventArgs(Config.Windows.IndexOf(window), e.Value));

        }

        /// <summary>
        /// handler for deserialization errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeserializeErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            Tracer.PrintLine(string.Format("Error Deserializing:{0}", e.ErrorContext.Error.Message));
            e.ErrorContext.Handled = true;
        }

        /// <summary>
        /// prints the config onto the console
        /// </summary>
        private void PrintDataProperties()
        {
            if (Config == null)
            {
                Tracer.PrintLine("Config Data is null");
            }
            else
            {
                Tracer.PrintLine(JsonConvert.SerializeObject(this.Config, Formatting.Indented));
            }
        }

        private string ReceiveBuffer;

        /// <summary>
        /// Parse the received strings. Invokes parsers that match the pattern with the given string.
        /// </summary>
        /// <param name="rx"></param>
        public void ParseRx(string rx)
        {
            Tracer.PrintLine(string.Format("Received: {0}",rx));
            ReceiveBuffer += rx;
            while (ReceiveBuffer.Contains("\r\n"))
            {
                try
                {
                    int end = ReceiveBuffer.IndexOf("\r\n");
                    string msg = ReceiveBuffer.Substring(0, end);
                    ReceiveBuffer = ReceiveBuffer.Remove(0, end + 2);

                    Tracer.PrintLine(string.Format("Found Len:{0} Msg:[{1}]", msg.Length, msg));

                    bool found = false;
                    foreach (IRxParser p in Parsers)
                    {
                        if (p.IsMatch(msg))
                        {
                            found = true;
                            try
                            {
                                p.Action(msg);
                            }
                            catch (Exception ex)
                            {
                                Tracer.PrintLine(string.Format("Exception Parsing.\r\n{0}\r\n"), ex);                                
                            }
                        }
                    }
                    if (!found) Tracer.PrintLine("No Parsers found");
                    Tracer.PrintLine(string.Format("Parsing Completed. Receive Buffer Length {0}", ReceiveBuffer.Length));
                }
                catch (Exception e)
                {
                    Tracer.PrintLine(string.Format("Exception Parsing.{0}\r\n", e.ToString()));
                }
            }
            if (ReceiveBuffer.Length > 500)
                ReceiveBuffer = "";
            txQ.SendOk = true;
        }

        public void LoadLayout(string layout, string display)
        {
            Tracer.PrintLine(string.Format("LoadLayout {0} {1}",layout,display));
            if (display.Length > 0)
                txQ.Add(new Command(string.Format("<I:BARCO||K:CMS||O:LoadLayout||A1:{0}||A2:{1}||>\r\n",layout,display)));
            else
                txQ.Add(new Command(string.Format("<I:BARCO||K:CMS||O:LoadLayout||A1:{0}||>\r\n",layout)));
        }

        /// <summary>
        /// returns true if any part of windowA would overlap with windowB
        /// </summary>
        /// <param name="windowA"></param>
        /// <param name="windowB"></param>
        /// <returns></returns>
        public bool PerspectivesOverlap(ConfigWindow windowA, ConfigWindow windowB)
        {
            Tracer.PrintLine(string.Format("PerspectiveOverlap A:{0} B:{1}",windowA.Perspective, windowB.Perspective));
            if (windowA.Display != windowB.Display)
            {
                Tracer.PrintLine(" not on the same Display. returning false");
                return false;
            }

            //see https://stackoverflow.com/questions/306316/determine-if-two-rectangles-overlap-each-other
            //We will find overlap by negating when it is not overlapping.
            //given left to right increases x
            //given top to bottom increases y
            //no overlap exists if one of the conditions persists:
            //1. A's left edge is to the right of B's right edge, then A is totally to the right of B
            //   A.X > B.EndX
            //2. A's right edge is to the left of B's left edge, then A is totally to the left of B
            //   A.EndX < B.X
            //3. A's top edge is below B's bottom edge, then A is totally below B
            //   A.Y > B.EndY
            //4. A's bottom edge is above B's top edge, then A is totally above B
            //   A.EndY < B.Y
            // 
            // "no overlap" = 1 or 2 or 3 or 4            
            // "overlap" = not "no overlap"

            // int StartX = windowB.EndX - windowA.X;
            // int EndX = windowA.EndX - windowB.X;
            // int StartY = windowB.EndY - windowA.Y;
            // int EndY = windowA.EndY - windowB.Y;
            // Tracer.PrintLine(string.Format(" StartX:{StartX}");
            // Tracer.PrintLine(string.Format(" EndX:  {EndX}");
            // Tracer.PrintLine(string.Format(" StartY:{StartY}");
            // Tracer.PrintLine(string.Format(" EndY:  {EndY}");

            bool AisRight = windowA.X >= windowB.EndX;
            bool AisLeft = windowA.EndX <= windowB.X;
            bool AisAbove = windowA.EndY <= windowB.Y;
            bool AisBelow = windowA.Y >= windowB.EndY;
            Tracer.PrintLine(string.Format(" A is to the right of B:{0}",AisRight));
            Tracer.PrintLine(string.Format(" A is to the left  of B:{0}",AisLeft));
            Tracer.PrintLine(string.Format(" A is above B:          {0}",AisAbove));
            Tracer.PrintLine(string.Format(" A is below B:          {0}",AisBelow));
            bool NoOverlap = AisRight || AisLeft || AisAbove || AisBelow;
            Tracer.PrintLine(string.Format(" Overlap:               {0}",!NoOverlap));
            return !NoOverlap;
        }

        public void SetPerspectiveSourceByIndex(int sourceindex, int windowindex)
        {
            Tracer.PrintLine(string.Format("SetPerspectiveSourceByIndex {0} {1}",sourceindex,windowindex));
            if (Config == null || Config.Sources == null || Config.Sources.Count == 0 || Config.Windows == null || Config.Windows.Count == 0)
            {
                Tracer.PrintLine("Config or Sources or Windows are empty");
                return;
            }
            if (sourceindex < -1 || sourceindex >= Config.Sources.Count)
            {
                Tracer.PrintLine("Invalid source index");
                return;
            }
            if (windowindex >= Config.Windows.Count)
            {
                Tracer.PrintLine("Invalid window index");
                return;
            }

            try
            {
                if (sourceindex == -1) //clear
                {
                    Tracer.PrintLine("Source -1, Unloading Perspective");
                    txQ.Add(new Command(
                        string.Format("<I:BARCO||K:CMS||O:UnloadPerspective||A1:{0}||A2:{1}||>\r\n",Config.Windows[windowindex].Perspective,Config.Windows[windowindex].Display)
                        , windowindex
                        , sourceindex
                        , Config.Displays.IndexOf(Config.Windows[windowindex].Display)));

                    return;
                }
            }
            catch(Exception ex)
            {
                Tracer.PrintLine(string.Format("Exception Unloading Perspective.{0}\r\n", ex.ToString()));
            }

            ConfigSource src = Config.Sources[sourceindex];
            ConfigWindow window = Config.Windows[windowindex];
            Tracer.PrintLine(string.Format("Source: {0}",src.Name));
            Tracer.PrintLine(string.Format("Window: {0} {1}",window.Perspective,window.Display));
            try
            {
                //remove any perspective that may overlap
                foreach(ConfigWindow winB in Config.Windows)
                {
                    if (window.Id != winB.Id) //don't check itself.
                    {
                        if (PerspectivesOverlap(window,winB))
                        {
                            //unload winB
                            txQ.Add(new Command(
                                string.Format("<I:BARCO||K:CMS||O:UnloadPerspective||A1:{0}||A2:{1}||>\r\n",winB.Perspective,winB.Display)
                                , Config.Windows.IndexOf(winB)
                                , sourceindex
                                , Config.Displays.IndexOf(winB.Display)));
                        }
                    }
                }
                //remove any perspective as directed from config file
                //foreach (var r in window.HideWhenActive)
                //{
                //    //<I:BARCO||K:CMS||O:UnloadPerspective||A1:yahoo||A2:Display||>
                //    if (r < Config.Windows.Count)
                //    {
                //        //if (Config.Windows[r].IsActive)
                //        //{
                //        txQ.Add(new Command(
                //            string.Format("<I:BARCO||K:CMS||O:UnloadPerspective||A1:{Config.Windows[r].Perspective}||A2:{Config.Windows[r].Display}||>\r\n"
                //            , window: r
                //            , source: sourceindex
                //            , display: Config.Displays.IndexOf(Config.Windows[r].Display)));
                //        //}
                //    }
                //    else
                //        Tracer.PrintLine(string.Format("Invalid Window Index {r} to remove");
                //}
            }
            catch (Exception ex)
            {
                Tracer.PrintLine(string.Format("\r\nException Removing Perspectives\r\n{0}", ex));
                
            }

            //Load the perspective if needed
            //if (!Config.Windows[windowindex].IsActive)
            //{
            txQ.Add(new Command(
                string.Format("<I:BARCO||K:CMS||O:LoadPerspective||A1:{0}||A2:{1}||A3:{2}||A4:{3}||A5:{4}||A6:{5}||>\r\n",window.Perspective,window.Display,window.X,window.Y,window.Width,window.Height)
                , windowindex
                , sourceindex
                , Config.Displays.IndexOf(window.Display)));
            //}

            //remove the current source if needed
            //if (Config.Windows[windowindex].CurrentSource != Config.Sources[sourceindex].Name)
            //{
            //    txQ.Add(new Command(
            //        string.Format("<I:BARCO||K:CMS||O:RemoveSourceFromPerspectiveTile||A1:{window.Perspective}||A2:1||>\r\n"
            //        , windowindex
            //        , sourceindex
            //        , display: Config.Displays.IndexOf(window.Display)));
            //}

            //Set the Perspective's Source
            txQ.Add(new Command(
                string.Format("<I:BARCO||K:CMS||O:LoadSourceOnPerspective||A1:{0}||A2:{1}||A3:1||>\r\n",window.Perspective,src.Name)
                , windowindex
                , sourceindex
                , Config.Displays.IndexOf(window.Display)));

            //requery the display
            RequeryDisplay(Config.Displays.IndexOf(window.Display));
            }

        private readonly List<Timer> QueryDisplayTimers = new List<Timer>();
        void RequeryDisplay(int displayIndex)
        {
            try
            {
                if (displayIndex < 0 || displayIndex > Config.Displays.Count)
                    throw (new ArgumentOutOfRangeException("displayIndex", "RequeryDisplay displayIndex out of range"));

                if (QueryDisplayTimers[displayIndex] == null)
                {
                    Timer t = new Timer(SendQueryDisplay, displayIndex.ToString(), 3000);
                    QueryDisplayTimers[displayIndex] = t;
                }
                QueryDisplayTimers[displayIndex].Stop();
                QueryDisplayTimers[displayIndex].Reset(3000);
            }
            catch (Exception ex)
            {
                Tracer.PrintLine(string.Format("\r\nRequeryDisplay Exception.\r\n{0}\r\n", ex));                
            }
        }
        void SendQueryDisplay(object obj)
        {
            try
            {
                int displayIndex = Int32.Parse((string)obj);
                if (displayIndex < 0 || displayIndex > Config.Displays.Count)
                    throw (new ArgumentOutOfRangeException("displayIndex", "SendQueryDisplay displayIndex out of range"));
                txQ.Add(new Command(
                    string.Format("<I:BARCO||K:CMS||O:GetWindowListVersion3||A1:{0}||>\r\n",Config.Displays[displayIndex])
                    , displayIndex
                    ));
            }
            catch(Exception ex)
            {
                Tracer.PrintLine(string.Format("\r\nSendQueryDisplay Exception.\r\n{0}\r\n", ex));
                
            }
        }
        

        private readonly List<IRxParser> Parsers;
        public BarcoCMS()
        {
            Parsers = new List<IRxParser>
            {
                new LoadPerspective(this),
                new UnloadPerspective(this),
                new GetWindowsResponse(this),
                new LoadSourceOnPerspectiveResponse(this)
            };
            txQ.OnOutput += TxQListener;
        }

        public Command LastCommandSent { get; private set; }
        private void TxQListener(object sender, GenericEventArgs<Command> args)
        {
            Tracer.PrintLine(string.Format("Sending {0}",args.Data));
            LastCommandSent = args.Data;
            if(SendToCMS!=null)
                SendToCMS(this, new StringEventArgs(args.Data.Text));
        }

        public event EventHandler<StringArrayEventArgs> OnWindowSourceChanged;
        private void WindowCurrentSourceChangedHandler(object sender, StringEventArgs args)
        {
            int ndx = Config.Windows.IndexOf((ConfigWindow)sender);
            if ( ndx >= 0)
                if(OnWindowSourceChanged!=null)
                    OnWindowSourceChanged(this, new StringArrayEventArgs(ndx, args.Value));
        }

        public ushort GetWindowCurrentSourceIndex(int windowindex)
        {
            if (windowindex >= 0 && windowindex < Config.Windows.Count)
            {
                string srcName = Config.Windows[windowindex].CurrentSource;
                for(int i = 0; i < Config.Sources.Count; i++)
                {
                    if (Config.Sources[i].Name == srcName)
                        return (ushort)i;
                }
                return 65535;
            }
            return 65535;
        }

        public ushort GetSourceCount()
        {
            if (Config.Sources != null)
                return (ushort)Config.Sources.Count;
            return 0;
        }

        public string GetSourceName(int index)
        {
            return Config.Sources[index].Name;
        }
        public void Init()
        {
            for (int x = 0; x < Config.Displays.Count; x++)
                txQ.Add(new Command(
                    string.Format("<I:BARCO||K:CMS||O:GetWindowListVersion3||A1:{0}||>\r\n", Config.Displays[x])
                    , x));
        }

        public void ApplyDecoratorToWindow(int windowindex, int decoratorIndex)
        {
            Tracer.PrintLine("ApplyDecoratorToWindow {0} {1}", windowindex,decoratorIndex);
            if (windowindex >= Config.Windows.Count)
            {
                Tracer.PrintLine("Cannot apply decorator. Window Index out of bounds.");
                return;
            }
            if (decoratorIndex >= Config.Decorators.Count)
            {
                Tracer.PrintLine("Cannot apply decorator. Decorator Index out of bounds.");
                return;
            }
            if (!Config.Windows[windowindex].IsActive)
            {
                Tracer.PrintLine("Cannot apply decorator. Window is not active.");
                return;
            }

            ConfigWindow window = Config.Windows[windowindex];
            Decorator dec = Config.Decorators[decoratorIndex];

            window.AddDecorator(dec);                

            txQ.Add(new Command(
                string.Format("<I:BARCO||K:CMS||O:ApplyDecoratorToASourcePerspective||A1:{0}||A2:{1}||A3:{2}>\r\n",window.Perspective,window.CurrentSource,2)
                , windowindex
                , GetWindowCurrentSourceIndex(windowindex)
                , Config.Displays.IndexOf(window.Display)
                ));           
        }

        public void RemoveDecoratorToWindow(int windowindex, int decoratorIndex)
        {
            Tracer.PrintLine("RemoveDecoratorToWindow {0} {1}", windowindex, decoratorIndex);
            if (windowindex >= Config.Windows.Count)
            {
                Tracer.PrintLine("Cannot remove decorator. Window Index out of bounds.");
                return;
            }
            if (decoratorIndex >= Config.Decorators.Count)
            {
                Tracer.PrintLine("Cannot remove decorator. Decorator Index out of bounds.");
                return;
            }
            if (!Config.Windows[windowindex].IsActive)
            {
                Tracer.PrintLine("Cannot remove decorator. Window is not active.");
                return;
            }

            ConfigWindow window = Config.Windows[windowindex];
            Decorator dec = Config.Decorators[decoratorIndex];

            window.RemoveDecorator(dec);

            txQ.Add(new Command(
                string.Format("<I:BARCO||K:CMS||O:RemoveDecoratorFromASourcePerspective||A1:{0}||A2:{1}||A3:{2}>\r\n",window.Perspective,window.CurrentSource,dec.Name)
                , windowindex
                , GetWindowCurrentSourceIndex(windowindex)
                , Config.Displays.IndexOf(window.Display)
                ));
        }
        public void RemoveAllDecoratorsFromWindow(int windowindex)
        {
            Tracer.PrintLine("RemoveAllDecoratorsFromWindow {0}",windowindex);
            if (windowindex >= Config.Windows.Count)
            {
                Tracer.PrintLine("Cannot remove decorator. Window Index out of bounds.");
                return;
            }
            ConfigWindow window = Config.Windows[windowindex];
            foreach(Decorator dec in window.Decorators)
            {
                txQ.Add(new Command(
                    string.Format("<I:BARCO||K:CMS||O:RemoveDecoratorFromASourcePerspective||A1:{0}||A2:{1}||A3:{2}>\r\n",window.Perspective,window.CurrentSource,dec.Name)                    
                    , windowindex
                    , GetWindowCurrentSourceIndex(windowindex)
                    , Config.Displays.IndexOf(window.Display)
                    ));
            }
        }
    }
}