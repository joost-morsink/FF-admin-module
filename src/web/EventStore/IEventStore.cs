using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FfAdmin.Common;

namespace FfAdmin.EventStore
{
    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public interface IEventStore : IDisposable
    {
        void StartSession();
        void EndSession(string? comment);
        bool HasSession { get; }
        string? SessionFile { get; }
        DateTime? FileTimestamp { get; }

        void WriteEvent(Event e);
        string Hashcode();
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
