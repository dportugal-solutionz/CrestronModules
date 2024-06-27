using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BarcoTransFormNCMSFor3Series
{
    /// <summary>
    /// A Queue that implements a Timer, when timer expires it automatically dequeues.
    /// Output of the Queue are sent via the event OnOutput
    /// </summary>
    public class TransmitQueue<T>
    {
        private readonly Queue<T> Q = new Queue<T>();
        public EventHandler<GenericEventArgs<T>> OnOutput;

        private bool sendOk;
        public bool SendOk
        {
            get 
            { 
                return sendOk; 
            }
            set
            {
                sendOk = value;
                if (sendOk)
                {
                    StartSending();
                    SendNext(null);
                }
            }
        }

        public void Add(T item)
        {
            Tracer.PrintLine(string.Format("TransmitQueue Add {0}",item));
            if (Q.Count > 1000)
            {
                Tracer.PrintLine("Something is wrong with the TransmitQueue. Current Count is over 1000");
                Tracer.PrintLine("Clearing Queue");
                ClearQueue();
            }
            Tracer.PrintLine("TransmitQueue Enqueing");
            Q.Enqueue(item);
            StartSending();
        }

        public int Count
        {
            get { return Q.Count; }
        }

        private readonly int Timeout = 300;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timeout"> in milliseconds</param>
        public TransmitQueue(int timeout)
        {
            Timeout = timeout;
            Timer = new CTimer(SendNext, null, Timeout, Timeout);
            TimerStarted = true;
        }

        private CTimer Timer { get; set; }

        bool TimerStarted = false;
        public void StartSending()
        {
            if (!TimerStarted)
            {
                Tracer.PrintLine("TransmitQueue StartSending");
                TimerStarted = true;
                Timer.Reset(Timeout, Timeout);
            }
        }

        public void StopSending()
        {
            Tracer.PrintLine("TransmitQueue StopSending");
            TimerStarted = false;
            Timer.Stop();            
        }

        public void ClearQueue()
        {
            Tracer.PrintLine("TransmitQueue ClearQueue");
            Q.Clear();            
            Tracer.PrintLine("TransmitQueue ClearQueue Completed");
        }
        private void SendNext(object userSpecific)
        {
            //Tracer.PrintLine("TransmitQueue SendNext");
            if (Q.Count > 0)
            {
                SendOk = false;
                T tx = Q.Dequeue();
                
                if (tx != null)
                {
                    string str = string.Format("{0}",tx);
                    //Tracer.PrintLine(string.Format("TransmitQueue Sending Len:{str.Length} {str}");
                    if(OnOutput!=null)
                        OnOutput(this, new GenericEventArgs<T>(tx));
                }
            }
        }
    }
}