namespace OasCommonLib.EventManager
{
    using System;

    public enum EVCPEventType
    {
        ErrorMessage, Message,

        // case broker events
        CaseAdded, CaseUpdated, CaseDeleted, CaseAddInfoDeleted, CaseAssigned,

        // config events
        ImagePathCahnged, LogPathChanged,
        ServerPollTimeoutChanged, WebServerChanged, HowManyDaysToShowChanged,

        // log item was added
        NewLogItem,

        // refresh case list
        RefreshCaseList
    };

    public class EVCPEventArgs : EventArgs
    {
        public EVCPEventType Type { get; set; }
        public object Data { get; set; }
    }

    public class EVCPEventSource
    {
        public event EventHandler<EVCPEventArgs> OASEvent = delegate { };

        public void RaiseEvent(EVCPEventType type, object data = null)
        {
            OASEvent(this, new EVCPEventArgs() { Type = type, Data = data });
        }
    }
}
