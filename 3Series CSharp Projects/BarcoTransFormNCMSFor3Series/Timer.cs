using Crestron.SimplSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarcoTransFormNCMSFor3Series
{
    /// <summary>
    /// a Wrapper CTimer with added event Expired and members IsAcitve and IsRepeating
    /// </summary>
    public class Timer
    {
        private CTimer timer;

        private long dueTime;
        private long repeatTime;
       
        public bool IsActive = false;
        public bool IsRepeating = false;

        private CTimerCallbackFunction callbackMethod;
        private object callbackObject;

        public void Reset()
        {
            IsActive = true;
            IsRepeating = true;
            if (timer != null)
                timer = new CTimer(CallbackHandler, dueTime);
            timer.Stop();
            timer.Reset();
        }
        public void Reset(long dueTime)
        {
            this.dueTime = dueTime;
            IsRepeating = true;
            IsActive = true;
            if (timer != null) ;
                timer = new CTimer(CallbackHandler, dueTime);
            timer.Stop();
            timer.Reset(dueTime);
        }
        public void Reset(long dueTime,long repeatTime)
        {
            IsActive = true;
            IsRepeating = true;
            this.repeatTime = repeatTime;
            if (timer != null)
                timer = new CTimer(CallbackHandler, dueTime);
            timer.Stop();
            timer.Reset(dueTime, this.repeatTime);            
        }
        public void Stop()
        {
            IsActive=false;
            if (timer!=null)
                timer.Stop();
        }

        private void CallbackHandler(object obj)
        {
            callbackMethod(callbackObject);
            if (!IsRepeating)
                this.IsActive = false;
            if(Expired!=null)
                Expired(this, new EventArgs());
        }

        public event EventHandler<EventArgs> Expired;

        public Timer(CTimerCallbackFunction callback, long duetime)
        {
            callbackMethod = callback;
            dueTime = duetime;
        }
        public Timer(CTimerCallbackFunction callback, object callbackobject,long duetime)
        {
            callbackMethod = callback;
            callbackObject = callbackobject;
            dueTime = duetime;
        }
    }
}
