using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarcoTransFormNCMSFor3Series
{

    public class ConfigWindow
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("Perspective")]
        public string Perspective { get; set; }

        [JsonProperty("Display")]
        public string Display { get; set; }


        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonIgnore]
        public int EndX
        {
            get
            {
                return X + Width;
            }
        }

        [JsonIgnore]
        public int EndY
        {
            get
            {
                return Y + Height;
            }
        }


        [JsonProperty("Width")]
        public int Width { get; set; }

        [JsonProperty("Height")]
        public int Height { get; set; }

        [JsonProperty("HideWhenActive")]
        public List<int> HideWhenActive { get; set; }

        [JsonIgnore]
        private bool isActive;

        [JsonIgnore]
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
                if(OnActiveChanged!=null)
                    OnActiveChanged(this,new BoolEventArgs(isActive));
            }
        }

        public event EventHandler<BoolEventArgs> OnActiveChanged;

        [JsonIgnore]
        public string currentsource = "";

        [JsonIgnore]
        public string CurrentSource
        {
            get
            {
                return currentsource;
            }
            set
            {
                currentsource = value;
                if(OnCurrentSourceChanged!=null)
                    OnCurrentSourceChanged(this, new StringEventArgs(currentsource));
            }
        }

        public event EventHandler<StringEventArgs> OnCurrentSourceChanged;

        [JsonIgnore]
        private readonly Dictionary<Decorator, Decorator> AppliedDecorators;

        public ConfigWindow()
        {
            AppliedDecorators = new Dictionary<Decorator, Decorator>();
        }

        [JsonIgnore]
        public List<Decorator> Decorators
        {
            get
            {
                return AppliedDecorators.Values.ToList<Decorator>();
            }
        }
        
        public void AddDecorator(Decorator d)
        {
            if (d != null)
                if (!AppliedDecorators.ContainsKey(d))
                    AppliedDecorators.Add(d, d);
        }

        public bool DecoratedWith(Decorator d)
        {
            return AppliedDecorators.ContainsKey(d);                
        }

        public void RemoveDecorator(Decorator d)
        {
            if (d != null)
                if (AppliedDecorators.ContainsKey(d))
                    AppliedDecorators.Remove(d);
        }
    }

    public class ConfigSource
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }

    public class BarcoCMSConfig
    {
        [JsonProperty("Ip")]
        public string Ip { get; set; }

        [JsonProperty("IpPort")]
        public ushort IpPort { get; set; }

        [JsonProperty("Displays")]
        public List<string> Displays { get; set; }

        [JsonProperty("Windows")]
        public List<ConfigWindow> Windows { get; set; }

        [JsonProperty("Sources")]
        public List<ConfigSource> Sources {get; set;}

        [JsonProperty("Decorators")]
        public List<Decorator> Decorators { get; set; }
    }

    public class Decorator
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}
