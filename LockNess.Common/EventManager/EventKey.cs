using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

/* CLR VIA C# */
namespace LockNess.Common.EventManager
{
    public class EventKey
    {}

    public class EventSet {
        private readonly Dictionary<EventKey, Delegate> m_evnet= new Dictionary<EventKey, Delegate>();

        public void Add(EventKey eventKey, Delegate @delegate)
        {
            Monitor.Enter(m_evnet);
            Delegate d;
            m_evnet.TryGetValue(eventKey, out d);
            m_evnet[eventKey]=Delegate.Combine(d, @delegate);
            Monitor.Exit(m_evnet);
        }

        public void Remove(EventKey eventKey, Delegate @delegate)
        {
            Monitor.Enter(m_evnet);
            Delegate d;
            if (m_evnet.TryGetValue(eventKey, out d))
            {
                d = Delegate.Remove(d, @delegate);
                if (d != null)
                {
                    m_evnet[eventKey] = d;

                }
                else
                    m_evnet.Remove(eventKey);
            }
            Monitor.Exit(m_evnet);
        }

        public void Raise(EventKey key, Object sender, EventArgs e)
        {
            Delegate d;
            Monitor.Enter(m_evnet);
            m_evnet.TryGetValue(key, out d);
            Monitor.Exit(m_evnet);
            if (d != null)
            {
                d.DynamicInvoke(new object[] { sender, e });
            }
           
        }


        public int GetEvnetCount()
        {
            return m_evnet.Count;
        }
    }
}
