namespace OasCommonLib.OasEventManager
{
    using System;

    public enum OasEventType
    {
        ErrorMessage, Message,

        // case broker events
        CaseDeleted, CaseAdded, CaseUpdate, NewCaseList, CaseAddInfoDeleted, CaseAssigned,

        // config events
        EMSCasePathCahnged, MCFPathCahnged, CasePathCahnged, ImagePathCahnged, UrlChanged, LogPathChanged,
        DataPathChanged, ServerPollTimeoutChanged, WebServerChanged, SqLitePathChanged, HowManyDaysToShowChanged,

        // offers
        NewOfferList, NewAddInfo,

        // oas web server
        CaseUpdated, DetailUpdated,

        // image checker events
        ImagesAdded, ImageDeleted,

        // log item was added
        NewLogItem,

        // refresh case list
        RefreshCaseList,

        // refresh images on USB
        RefreshImagesOnUSB, RemoveOldCasesOnUsb,

        // file uploaded/deleted
        FileUploaded, FileDeleted
    };

    public sealed class OasEventArgs : EventArgs
    {
        public OasEventType Type { get; set; }
        public object Data { get; set; }
    }

    public sealed class OasEventSource
    {
        public event EventHandler<OasEventArgs> OasEvent = delegate { };

        public void RaiseEvent(OasEventType type, object data = null)
        {
            OasEvent(this, new OasEventArgs() { Type = type, Data = data });
        }
    }
}
