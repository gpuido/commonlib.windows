namespace OasCommonLib.OasEventManager
{
    using System;
    using System.Diagnostics;
    using System.Windows;

    public class EventManager : WeakEventManager
    {
        private static EventManager CurrentManager
        {
            get
            {
                EventManager manager = (EventManager)GetCurrentManager(typeof(EventManager));

                if (manager == null)
                {
                    manager = new EventManager();
                    SetCurrentManager(typeof(EventManager), manager);
                }

                return manager;
            }
        }


        public static void AddListener(OasEventSource source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(OasEventSource source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        protected override void StartListening(object source)
        {
            ((OasEventSource)source).OasEvent += DeliverEvent;
        }

        protected override void StopListening(object source)
        {
            ((OasEventSource)source).OasEvent -= DeliverEvent;
        }
    }

    public class LegacyWeakEventListener : IWeakEventListener
    {
        private void OnEvent(object source, EventArgs args)
        {
            OasEventArgs oasEventArgs = (OasEventArgs)args;
            Debug.WriteLine("LegacyWeakEventListener received event : " + oasEventArgs.Type.ToString());
        }

        public LegacyWeakEventListener(OasEventSource source)
        {
            EventManager.AddListener(source, this);
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            OnEvent(sender, e);

            return true;
        }

        ~LegacyWeakEventListener()
        {
            Console.WriteLine("LegacyWeakEventListener finalized.");
        }
    }
}
