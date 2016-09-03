namespace OasCommonLib.OasEventManager
{
    using System;

    public class GlobalEventManager
    {
        private static volatile GlobalEventManager _instance = null;
        private static object syncRoot = new Object();
        public static GlobalEventManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new GlobalEventManager();
                        }
                    }
                    
                }

                return _instance;
            }
        }

        public readonly OasEventSource OasEventSource = new OasEventSource();
    }
}
