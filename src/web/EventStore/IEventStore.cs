using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FfAdmin.Common;

namespace FfAdmin.EventStore
{
    public interface IEventStore : IDisposable
    {
        void StartSession();
        void EndSession(string? comment);
        bool HasSession { get; }
        string? SessionFile { get; }

        void WriteEvent(Event e);

        Task<Event[]> GetEventsFromFile(string path);

        IEnumerable<string> AllFiles();

        RemoteStatus GetRemoteStatus();
        Task Pull();
        Task Push();


        void IDisposable.Dispose()
        {
            if (HasSession)
                EndSession("Automatically closed.");
        }
    }
}
