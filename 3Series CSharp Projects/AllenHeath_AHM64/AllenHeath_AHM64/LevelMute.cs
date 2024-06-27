using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace AllenHeath_AHM64
{
    public class BlockType
    {
        public const int Input = 0;
        public const int Zone = 1;
        public const int Group = 2;

        public int Value {get;set;}

        public BlockType()
        {
            this.Value = 0;
        }
        public BlockType(int type)
        {
            this.Value = type;
        }

        //from int to BlockType
        public static implicit operator BlockType(int value)
        {
            BlockType b = new BlockType(value);            
            return b;
        }
        //from BlockType to int
        public static implicit operator int(BlockType b)
        {
            return b.Value;
        }
        public static implicit operator BlockType(byte value)
        {
            BlockType b = new BlockType((int)value);
            return b;
        }
        public static implicit operator byte(BlockType b)
        {
            return (byte)b.Value;
        }


        
    }


    public class LevelEventArgs : EventArgs 
    {
        public ushort Channel { get; set; }
        public ushort Pct { get; set; }
        public ushort Db { get; set; }
        public LevelEventArgs()
        {

        }
        public LevelEventArgs(int ch, int pct, int db)
        {
            Channel = (ushort)ch;
            Pct = (ushort)pct;
            Db = (ushort)db;
        }
        public LevelEventArgs(LevelMute obj)
        {
            Channel = (ushort)obj.Channel;
            Pct = (ushort)obj.Percent;
            Db = (ushort)obj.Level;
        }
    }

    public class MuteEventArgs : EventArgs
    {
        public ushort Channel { get; set; }
        public ushort Mute { get; set; }
        public MuteEventArgs()
        {
            Channel = 0;
            Mute = 0;
        }
        public MuteEventArgs(LevelMute obj)
        {
            Channel = (ushort)obj.Channel;
            Mute = (ushort)obj.Mute;
        }
        public MuteEventArgs(int channel, int mute)
        {
            Channel = (ushort)channel;
            Mute = (ushort)mute;
        }

    }

    public class LevelMute
    {
        public int Percent { get; set; }
        private int _level;
        public int Level {
            get { return _level; }
            set
            {
                _level = value;
                if (OnLevelChanged != null)
                    OnLevelChanged.Invoke(this, new LevelEventArgs(this));
            }
        }
        private int _mute;
        public int Mute {
            get { return _mute; }
            set
            {
                _mute = value;
                if (OnMuteChanged != null)
                    OnMuteChanged.Invoke(this, new MuteEventArgs(this));
            }
        }
        public BlockType Type { get; set; } 
        public int Channel { get; set; }
        public LevelMute()
        {
            this.MinDb = -48;
            this.MaxDb = 10;
        }
        public int MinDb { get; set; }
        public int MaxDb { get; set; }
        public int Span 
        { 
            get 
            { 
                return (this.MaxDb - this.MinDb); 
            }
        }

        public string MuteOn_Cmd()
        {            
            var b = new byte[6];
            b[0] = (byte)(0x90 + this.Type);
            b[1] = (byte) this.Channel;
            b[2] = 0x7F;
            b[3] = b[0];
            b[4] = b[1];
            b[5] = 0;
            return System.Text.Encoding.UTF8.GetString(b, 0, b.Length);         
        }
        public string MuteOff_Cmd()
        {            
            var b = new byte[6];
            b[0] = (byte)(0x90 + this.Type);
            b[1] = (byte)this.Channel;
            b[2] = 0x3F;
            b[3] = b[0];
            b[4] = b[1];
            b[5] = 0;
            return System.Text.Encoding.UTF8.GetString(b, 0, b.Length);         
        }
        public string GetMute_Cmd()
        {            
            var b = new byte[5];
            b[0] = this.Type;
            b[1] = 0x01;
            b[2] = 0x09;
            b[3] = (byte)this.Channel;
            b[4] = 0xF7;
            var c = Device.SysExString() + System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
            return c;
        }
        public string IncLevel_Cmd()
        {
            var b = new byte[9];
            b[0] = (byte)(0xB0 + this.Type);
            b[1] = 0x63;
            b[2] = (byte)this.Channel;
            b[3] = b[0];
            b[4] = 0x62;
            b[5] = 0x20;
            b[6] = b[0];
            b[7] = 0x06;
            b[8] = 0x7F;
            return System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
        }
        public string DecLevel_Cmd()
        {
            var b = new byte[9];
            b[0] = (byte)(0xB0 + this.Type);
            b[1] = 0x63;
            b[2] = (byte)this.Channel;
            b[3] = b[0];
            b[4] = 0x62;
            b[5] = 0x20;
            b[6] = b[0];
            b[7] = 0x06;
            b[8] = 0x3F;
            return System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
        }
        public string SetLevel_Cmd(int db)
        {
            byte value = 0;
            if (db < -48)
                value = 0;
            else if (db > 10)
                value = 127;
            else
                value = (byte)(((db + 48) / 58) * 127);
            
            var b = new byte[9];

            b[0] = (byte)(0xB0 + this.Type);
            b[1] = 0x63;
            b[2] = (byte)(this.Channel);
            b[3] = b[0];
            b[4] = 0x62;
            b[5] = 0x17;
            b[6] = b[0];
            b[7] = 0x06;
            b[8] = value;

            return System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
        }
        public string SetPct_Cmd(int pct) //pct = 0 to 65535
        {
            int db = this.MinDb + (pct / 65535) * this.Span;
            return SetLevel_Cmd(db);
        }
        public string GetLevel_Cmd()
        {
            var b = new byte[6];
            b[0] = this.Type;
            b[1] = 0x01;
            b[2] = 0x0B;
            b[3] = 0x17;
            b[4] = (byte)this.Channel;
            b[5] = 0xF7;
            var c = System.Text.Encoding.UTF8.GetString(b,0,b.Length);
            return Device.SysExString() + c;
        }

        public event EventHandler<LevelEventArgs> OnLevelChanged;
        public event EventHandler<MuteEventArgs> OnMuteChanged;
    }
}