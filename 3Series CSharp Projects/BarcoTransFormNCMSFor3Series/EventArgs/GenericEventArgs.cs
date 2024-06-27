using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarcoTransFormNCMSFor3Series
{
    public class GenericEventArgs<T> : EventArgs
    {
        public T Data { get; set; }
        public GenericEventArgs(T data)
        {
            Data = data;
        }
        public GenericEventArgs()
        {
            Data = default(T);
        }
    }
}
