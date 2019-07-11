using LockNess.Common.EventManager;
using System;
using Xunit;

namespace Common.Test
{
    public class OWnerEvets
    {
        private readonly EventKey _eventKey = new EventKey();
        private EventSet _eventSet= new EventSet();
        public EventSet EventSet { get { return _eventSet; } }

        public event EventHandler<OwnerEventArg> OwnerEvents
        {
            add { _eventSet.Add(_eventKey, value); }
            remove { _eventSet.Remove(_eventKey, value); }
        }

        protected void FireEvent(OwnerEventArg e)
        {
            _eventSet.Raise(_eventKey, this, e);
        }

        public void ExecEvent()
        {
            FireEvent(new OwnerEventArg());
        }
    }

    public class OwnerEventArg : EventArgs
    { }

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            OWnerEvets _oWnerEvets = new OWnerEvets();
            _oWnerEvets.OwnerEvents += HanlEvent;
            Assert.Equal(1, _oWnerEvets.EventSet.GetEvnetCount());
        }
         void HanlEvent(object sender,OwnerEventArg e)
        {
            
        }
    }
}
