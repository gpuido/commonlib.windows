namespace OasCommonLib.Logger
{
    using Config;
    using Helpers;
    using OasEventManager;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Timers;

    public enum LogItemType { Info, Warning, Error };

    public class LogItem
    {
        public DateTime EventDate { get; private set; }
        public string Module { get; private set; }
        public string Event { get; private set; }
        public LogItemType EventType { get; private set; }
        public string Version { get; private set; }
        public string StackTrace { get; private set; }

        public LogItem(string module, string version, string eventName, LogItemType eventType, string stackTrace)
        {
            EventDate = DateTime.Now;
            Module = module;
            Version = version;
            Event = eventName;
            EventType = eventType;
            StackTrace = stackTrace;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(StackTrace))
            {
                return String.Format("{0}--{1}--{2}--{3}--{4}", EventDate.ToString(), Module, Version, EventType.ToString(), Event);
            }
            else
            {
                return String.Format("{0}--{1}--{2}--{3}--{4}--+{5}//*{6}*//", EventDate.ToString(), Module, Version, EventType.ToString(), Event, Environment.NewLine, StackTrace);
            }
        }
    }

    public sealed class LogQueue : IDisposable
    {
        public static readonly string TAG = "LogQueue";

        private static readonly OasConfig _cfg = OasConfig.Instance;

        public readonly ConcurrentBag<LogItem> LogItemList = new ConcurrentBag<LogItem>();
        private readonly Timer timer;

        public string LogFileName { get; private set; }

        public string LastItem { get; private set; }

        public string Version { get; set; }

        private static readonly OasEventSource _oasEvent = GlobalEventManager.Instance.OasEventSource;

        private int _lastDay;
        private int _elapseTimerMin;

        private string _startMessage;


        #region static part
        private static volatile LogQueue _logQueue = null;
        private static object syncRoot = new Object();
        public static LogQueue Instance
        {
            get
            {
                if (null == _logQueue)
                {
                    lock (syncRoot)
                    {
                        if (_logQueue == null)
                        {
                            Debug.WriteLine("Static Log was created");
                            _logQueue = new LogQueue();
                        }
                    }
                }

                return _logQueue;
            }
        }
        #endregion

        public LogQueue()
        {
            _elapseTimerMin = 5;

            timer = new Timer(_elapseTimerMin * 60000); // once in a minute
            timer.Elapsed += timer_Elapsed;
            timer.Enabled = true;

            LogFileName = MakeLogFileName();

            Debug.WriteLine("Log was created");

            _lastDay = DateTime.Now.Day;

            _startMessage = StartMessage();
        }

        public static string MakeLogFileName()
        {
            return DateTime.Now.ToString(FormatHelper.DateFormat) + ".log";
        }

        public void Dispose()
        {
            timer.Elapsed -= timer_Elapsed;
            timer.Enabled = false;

            FlushLog();
            Debug.WriteLine("Log was disposed");
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            FlushLog();
            Debug.WriteLine("Log timer elapsed");
        }

        #region add/isnert methods

        public void Insert(int index, string _module, string _eventName)
        {
            Insert(index, _module, _eventName, LogItemType.Info, null);
        }

        public void Insert(int index, string _module, string _eventName, LogItemType _eventType, Exception ex = null)
        {
            if (!string.IsNullOrEmpty(_module) && !string.IsNullOrEmpty(_eventName))
            {
                var logEvent = new LogItem(_module, Version, _eventName, _eventType, null != ex ? ex.StackTrace : null);
                LastItem = logEvent.ToString();
                LogItemList.Add(logEvent);

                _oasEvent.RaiseEvent(OasEventType.NewLogItem, logEvent.ToString());

                Debug.WriteLine("Log event was added : " + logEvent.ToString());

                if (_eventType == LogItemType.Error)
                {
                    FlushLog();
                }
            }
        }

        public void Add(string _module, string _eventName)
        {
            Add(_module, _eventName, LogItemType.Info, null);
        }

        public void Add(string _module, string _eventName, LogItemType _eventType, Exception ex = null)
        {
            if (!string.IsNullOrEmpty(_module) && !string.IsNullOrEmpty(_eventName))
            {
                var logEvent = new LogItem(_module, Version, _eventName, _eventType, null != ex ? ex.StackTrace : null);
                LastItem = logEvent.ToString();
                LogItemList.Add(logEvent);

                _oasEvent.RaiseEvent(OasEventType.NewLogItem, logEvent.ToString());

                Debug.WriteLine("Log event was added : " + logEvent.ToString());

                if (_eventType == LogItemType.Error)
                {
                    FlushLog();
                }
            }
        }

        public void AddError(string _module, Exception ex, string message = null)
        {
            if (!string.IsNullOrEmpty(_module) && null != ex)
            {
                var logEvent = new LogItem(_module, Version, ex.Message + (string.IsNullOrEmpty(message) ? string.Empty : ", " + message), LogItemType.Error, ex.StackTrace);
                LastItem = logEvent.ToString();
                LogItemList.Add(logEvent);

                _oasEvent.RaiseEvent(OasEventType.NewLogItem, logEvent.ToString());

                Debug.WriteLine("Log event was added : " + logEvent.ToString());

                FlushLog();
            }
        }
        #endregion

        private DateTime _lastLogFlush = DateTime.Now;
        private int _updateInterval = 5; // 5 minutes
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void FlushLog()
        {
            if (null == _cfg)
            {
                return;
            }

            if (string.IsNullOrEmpty(_cfg.LogPath))
            {
                return;
            }

            if (null == LogItemList)
            {
                return;
            }

            if (DateTime.Now.AddMinutes(-_updateInterval) < _lastLogFlush)
            {
                return;
            }
            _lastLogFlush = DateTime.Now;

            string fname = MakeLogFileName();
            try
            {
                LogItem item;
                List<string> items = new List<string>();

                if (!string.IsNullOrEmpty(_startMessage))
                {
                    items.Add(_startMessage);
                    _startMessage = null;
                }

                while (!LogItemList.IsEmpty)
                {
                    if (LogItemList.TryTake(out item))
                    {
                        items.Add(item.ToString());
                    }
                }

                if (_lastDay != DateTime.Now.Day)
                {
                    _startMessage = StartMessage();
                    _lastDay = DateTime.Now.Day;
                }
                File.AppendAllLines(Path.Combine(_cfg.LogPath, fname), items);

                Debug.WriteLine("Log was flushed");
            }
            catch (Exception ex)
            {
                AddError(TAG,
                    ex,
                    string.Format("Failed to save log into file '{0}'", fname));
            }
        }

        private string StartMessage()
        {
            return string.Format(
                "version:{0} started @ {1}, on {2}\n server: {3}, local server: {4}",
                Version,
                DateTime.Now.ToString(FormatHelper.DateTimeFormat),
                Environment.MachineName,
                _cfg.DataServiceUrl,
                _cfg.StartWebServer ?
                    string.Format(
                        "url:{0}, port:{1}",
                        string.Join(",", _cfg.Ips.ToArray()),
                        _cfg.WebServerPort
                    ) : "no"
                );
        }
    }
}
