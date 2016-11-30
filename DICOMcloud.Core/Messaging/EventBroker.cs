using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


namespace DICOMcloud.Core.Messaging
{
    public class EventBroker
    {
        public EventBroker ( ) {}
        static EventBroker ()
        {
            _default  = new EventBroker ( ) ;
            _instance = _default ;
        }

        public static void RegisterInstance ( EventBroker instance )
        {
            _instance = instance ;
        }

        public static void RegisterDefault ( ) 
        {
            _instance = _default ;
        }

        public static EventBroker Instance { get { return _instance ; } }

        public void Publish<T>(T message) where T : EventArgs
        {
            List<Delegate> handlersPlain;
            
            if (_hanlders.TryGetValue(typeof(T), out handlersPlain))
            {
                OnPublishing ( message ) ;

                foreach ( var handler in handlersPlain.OfType<Action<T>> ( ) )
                {
                    handler(message);
                }
            }
        } 

        public void Subscribe<T>(Action<T> handler ) where T : EventArgs
        {
            List<Delegate> existingHandlersPlain ;
            // We don't actually care about the return value here...
            
            if ( !_hanlders.TryGetValue(typeof(T), out existingHandlersPlain))
            {
                existingHandlersPlain = new List<Delegate> ( ) ;
                
                _hanlders[typeof(T)] = existingHandlersPlain;   
            }

            existingHandlersPlain.Add ( handler);
        }

        public void Unsubscribe<T> (Action<T> handler)
        {
            List<Delegate> existingHandlersPlain ;

            
            if ( !_hanlders.TryGetValue(typeof(T), out existingHandlersPlain))
            {
                return ;   
            }

            existingHandlersPlain.Remove ( handler);        
        }

        protected virtual void OnPublishing<T> ( T message ) where T : EventArgs
        {
            
        }

        private ConcurrentDictionary<Type,List<Delegate>> _hanlders = new ConcurrentDictionary<Type, List<Delegate>> ( ) ;
        private static EventBroker _instance;
        private static EventBroker _default;
    }
}