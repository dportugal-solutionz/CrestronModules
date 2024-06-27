using System;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AllenHeath_AHM64
{
    public class Device
    {
        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        public Device()
        {
            Encoder = System.Text.Encoding.UTF8;
            Inputs = new List<LevelMute>();
            Zones = new List<LevelMute>();
            Groups = new List<LevelMute>();
            for (int i = 0; i < 32; i++)
            {
                var npt = new LevelMute();
                npt.Channel = i;
                npt.Type = BlockType.Input;
                npt.OnLevelChanged += this.input_level_handler;
                npt.OnMuteChanged += this.input_mute_handler;
                Inputs.Add(npt);

                var zn = new LevelMute();
                zn.Channel = i;
                zn.Type = BlockType.Zone;
                zn.OnLevelChanged += this.zone_level_handler;
                zn.OnMuteChanged += this.zone_mute_handler;
                Zones.Add(zn);

                var gp = new LevelMute();
                gp.Channel = i;
                gp.Type = BlockType.Group;
                gp.OnLevelChanged += this.group_level_handler;
                gp.OnMuteChanged += this.group_mute_handler;
                Groups.Add(gp);
            }
            
            Parsers = new Dictionary<string,Parser>();

            //Mute, sample response \x90\x00\x3F\x90\x00\x00
            //\x90 = input
            //\x00 = channel 1
            //\x3F = mute off
            //\x90 = input
            //\x00 = channel 1
            //\x00 = fixed value.            
            var pattern = "([\x90-\x92])(.)(.)([\x90-\x92])(.)\x00";
            Parsers.Add(pattern, Parse_Mute);

            //Level, sample response \xB0\x63\x00\xB0\x62\x17\xB0\x06\x22
            //\xB0 = input
            //\x63 = fixed
            //\x00 = channel 1
            //\xB0 = input
            //\xB1            
            pattern = "([\xB0-\xB2])\x63(.)([\xB0-\xB2])\x62\x17([\xB0-\xB2])\x06(.)";
            Parsers.Add(pattern, Parse_Level);            
        }

        private readonly Encoding Encoder;
        private delegate void Parser(Match match);
        private readonly Dictionary<string, Parser> Parsers;

        private void input_level_handler(object sender, LevelEventArgs e)
        {
            if (this.InputLevelChanged != null)
                InputLevelChanged.Invoke(this, new UShortArrayEventArgs(e.Channel, e.Db));
            if (this.InputPctChanged != null)
                InputPctChanged.Invoke(this, new UShortArrayEventArgs(e.Channel, e.Pct));
        }
        private void input_mute_handler(object sender, MuteEventArgs e)
        {
            if (this.InputMuteChanged != null)
                InputMuteChanged.Invoke(this, new UShortArrayEventArgs(e.Channel, e.Mute));
        }
        private void zone_level_handler(object sender, LevelEventArgs e)
        {
            if (this.ZoneLevelChanged != null)
                ZoneLevelChanged.Invoke(this, new UShortArrayEventArgs(e.Channel, e.Db));
            if (this.ZonePctChanged != null)
                ZonePctChanged.Invoke(this, new UShortArrayEventArgs(e.Channel, e.Pct));
        }
        private void zone_mute_handler(object sender, MuteEventArgs e)
        {
            if (this.ZoneMuteChanged != null)
                this.ZoneMuteChanged.Invoke(this, new UShortArrayEventArgs(e.Channel, e.Mute));
        }
        private void group_level_handler(object sender, LevelEventArgs e)
        {
            if (this.GroupLevelChanged != null)
                ZoneLevelChanged.Invoke(this, new UShortArrayEventArgs(e.Channel, e.Db));
            if (this.GroupPctChanged != null)
                ZonePctChanged.Invoke(this, new UShortArrayEventArgs(e.Channel, e.Pct));
        }
        private void group_mute_handler(object sender, MuteEventArgs e)
        {
            if (this.GroupMuteChanged != null)
                this.GroupMuteChanged.Invoke(this, new UShortArrayEventArgs(e.Channel, e.Mute));
        }
        
        public static string SysExString()
        {
            var SysEx = new byte[] { 0xF0, 0x00, 0x00, 0x1A, 0x50, 0x12, 0x01, 0x00 };
            return System.Text.Encoding.UTF8.GetString(SysEx, 0, SysEx.Length);
        }

        public List<LevelMute> Inputs {get;set;}
        public List<LevelMute> Zones {get;set;}
        public List<LevelMute> Groups {get;set;}
                      
        public void Parse(string data)
        {
            foreach(var parser in Parsers)
            {
                var m = Regex.Match(data, parser.Key);
                if (m.Success)
                {
                    parser.Value(m);
                }
            }
        }

        #region Parsers
        private void Parse_Mute(Match m)
        {
            if (m.Success)
            {
                if (m.Groups.Count != 6)
                {
                    CrestronConsole.PrintLine("\r\n[AHM] Parse_Mute, incorrect number of groups");
                    return;
                }

                int N     = 0x90 - (int)Encoder.GetBytes(m.Groups[1].Value)[0];
                int ch           = (int)Encoder.GetBytes(m.Groups[2].Value)[0];
                int value        = (int)Encoder.GetBytes(m.Groups[3].Value)[0];   
                int muteValue = (value > 0x3F) ? 1 : 0;
                if (ch > 32)
                {
                    CrestronConsole.PrintLine("\r\n[AHM] Parse_Mute, ch out of range");
                    return;
                }
                if (N < 0 || N > 2)
                {
                    CrestronConsole.PrintLine("\r\n[AHM] Parse_Mute, N out of range");
                    return;
                }
                switch(N)
                {
                    case (BlockType.Input):
                        Inputs[ch].Mute = muteValue;
                        break;
                    case (BlockType.Group):
                        Groups[ch].Mute = muteValue;
                        break;
                    case(BlockType.Zone):
                        Zones[ch].Mute = muteValue;
                        break;
                }

            }
        }       
        private void Parse_Level(Match m)
        {            
            if (m.Success)
            {
                if (m.Groups.Count != 6)
                {
                    CrestronConsole.PrintLine("\r\n[AHM] Parse_Level, incorrect number of groups");
                    return;
                }

                int N = 0xB0 - (int)Encoder.GetBytes(m.Groups[1].Value)[0];
                int ch = (int)Encoder.GetBytes(m.Groups[2].Value)[0];
                int value = (int)Encoder.GetBytes(m.Groups[5].Value)[0];
                
                if (ch > 32)
                {
                    CrestronConsole.PrintLine("\r\n[AHM] Parse_Level, ch out of range");
                    return;
                }
                if (N < 0 || N > 2)
                {
                    CrestronConsole.PrintLine("\r\n[AHM] Parse_Level, N out of range");
                    return;
                }
                switch (N)
                {
                    case (BlockType.Input):
                        Inputs[ch].Level = value;
                        break;
                    case (BlockType.Group):
                        Groups[ch].Level = value;
                        break;
                    case (BlockType.Zone):
                        Zones[ch].Level = value;
                        break;
                }

            }
        }               
        #endregion

        public event EventHandler<UShortArrayEventArgs> InputLevelChanged;
        public event EventHandler<UShortArrayEventArgs> InputPctChanged;
        public event EventHandler<UShortArrayEventArgs> InputMuteChanged;
        public event EventHandler<UShortArrayEventArgs> ZoneLevelChanged;
        public event EventHandler<UShortArrayEventArgs> ZonePctChanged;
        public event EventHandler<UShortArrayEventArgs> ZoneMuteChanged;
        public event EventHandler<UShortArrayEventArgs> GroupLevelChanged;
        public event EventHandler<UShortArrayEventArgs> GroupPctChanged;
        public event EventHandler<UShortArrayEventArgs> GroupMuteChanged;
        public event EventHandler<StringEventArgs> Send;


        private void _Send(string data)
        {
            if (this.Send != null)
                Send.Invoke(this, new StringEventArgs(data));
        }

        public void SetInputLevel(ushort channel, ushort value)
        {
            if (channel <= 0 || channel > 32)
                throw new ArgumentOutOfRangeException("channel");
            Int16 v = (Int16)value;

            if (v < -48)
                v = -48;
            if (v > 10)
                v = 10;
            _Send(Inputs[channel - 1].SetLevel_Cmd(v));                
        }
        public void SetInputPct(ushort channel, ushort pct)
        {
            if (channel <= 0 || channel > 32)
                throw new ArgumentOutOfRangeException("channel");

            if (pct < 0)
                pct = 0;
            if (pct > 65535)
                pct = 65535;
            _Send(Inputs[channel - 1].SetPct_Cmd(pct));
        }
        public void SetInputMute(ushort channel, ushort value)
        {
            if (channel <= 0 || channel > 32)
                throw new ArgumentOutOfRangeException("channel");
            if (value > 0)
                _Send(Inputs[channel].MuteOn_Cmd());
            else
                _Send(Inputs[channel].MuteOff_Cmd());
        }
        public void SetZoneLevel(ushort channel, ushort value)
        {
            if (channel <= 0 || channel > 32)
                throw new ArgumentOutOfRangeException("channel");
            Int16 v = (Int16)value;
            if (v < -48)
                v = -48;
            if (v > 10)
                v = 10;
            _Send(Zones[channel - 1].SetLevel_Cmd(v));
        }
        public void SetZonePct(ushort channel, ushort pct)
        {
            if (channel <= 0 || channel > 32)
                throw new ArgumentOutOfRangeException("channel");

            if (pct < 0)
                pct = 0;
            if (pct > 65535)
                pct = 65535;
            _Send(Zones[channel - 1].SetPct_Cmd(pct));
        }
        public void SetZoneMute(ushort channel, ushort value)
        {
            if (channel <= 0 || channel > 32)
                throw new ArgumentOutOfRangeException("channel");            

            if (value > 0)
                _Send(Zones[channel].MuteOn_Cmd());
            else
                _Send(Zones[channel].MuteOff_Cmd());
        }
        public void SetGroupLevel(ushort channel, ushort value)
        {
            if (channel <= 0 || channel > 32)
                throw new ArgumentOutOfRangeException("channel");
            Int16 v = (Int16)value;
            if (v < -48)
                v = -48;
            if (v > 10)
                v = 10;
            _Send(Groups[channel - 1].SetLevel_Cmd(v));
        }
        public void SetGroupPct(ushort channel, ushort pct)
        {
            if (channel <= 0 || channel > 32)
                throw new ArgumentOutOfRangeException("channel");

            if (pct < 0)
                pct = 0;
            if (pct > 65535)
                pct = 65535;
            _Send(Groups[channel - 1].SetPct_Cmd(pct));
        }
        public void SetGroupMute(ushort channel, ushort value)
        {
            if (channel <= 0 || channel > 32)
                throw new ArgumentOutOfRangeException("channel");

            if (value > 0)
                _Send(Groups[channel].MuteOn_Cmd());
            else
                _Send(Groups[channel].MuteOff_Cmd());
        }        
    }
}
