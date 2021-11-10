using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FfAdmin.Common;

namespace FfAdmin.EventStore
{

    public class EventStore : IEventStore
    {
        public EventStore()
        {
            _git = new Git("data");

        }
        private SessionFile? _sessionFile;
        private Git _git;
        private static readonly byte[] CRLF = new byte[] { 0x0a, 0x0d };

        public bool HasSession => _sessionFile != null;
        public string? SessionFile => _sessionFile?.CurrentFile;
        public DateTime? FileTimestamp => _sessionFile?.Timestamp;

        public void EndSession(string? comment)
        {
            if (_sessionFile == null)
                throw new InvalidOperationException("No open session.");
            var file = _sessionFile.CurrentFile;
            _sessionFile.Dispose();
            _sessionFile = null;

            _git.Add(file);
            _git.Commit(comment ?? $"Add {file}.");
            _git.PushOrigin();
        }

        public void StartSession()
        {
            var now = DateTime.UtcNow;
            _sessionFile = new SessionFile(_git.Path,
                string.Join(Path.DirectorySeparatorChar,
                    now.Year,
                    now.Month.ToString("D2"),
                    now.Day.ToString("D2"),
                    $"{now.Hour.ToString("D2")}{now.Minute.ToString("D2")}{now.Second.ToString("D2")}.json"),
                now - TimeSpan.FromTicks(now.Ticks % TimeSpan.FromSeconds(1).Ticks));
        }
        public void WriteEvent(Event e)
        {
            if (_sessionFile == null)
                throw new InvalidOperationException("No open session.");
            _sessionFile.Writer.WriteLine(e.ToJsonString());
            _sessionFile.Writer.Flush();
        }
        public IEnumerable<string> AllFiles()
        {
            var baseDir = _git.Path;
            return from dir in Directory.GetDirectories(baseDir)
                   from path in ProcessDir(Path.GetFileName(dir))
                   select path;

            IEnumerable<string> ProcessDir(string prefix)
                => (from dir in Directory.GetDirectories(Path.Combine(baseDir, prefix))
                    from path in ProcessDir(Path.Combine(prefix, Path.GetFileName(dir)))
                    select path)
                    .Concat(from file in Directory.GetFiles(Path.Combine(baseDir, prefix), "*.json")
                            select Path.Combine(prefix, Path.GetFileName(file)));
        }
        public async Task<Event[]> GetEventsFromFile(string path)
        {
            var filename = Path.Combine(_git.Path, path);
            if (!File.Exists(filename))
                return new Event[0];
            using var stream = File.OpenRead(filename);
            var res = new List<Event>();
            return await Event.ReadAll(stream);
        }
        public RemoteStatus GetRemoteStatus()
            => _git.GetRemoteStatus();
        public Task Pull()
        {
            _git.PullOrigin();
            return Task.CompletedTask;
        }
        public Task Push()
        {
            _git.PushOrigin();
            return Task.CompletedTask;
        }
        public string Hashcode()
            => _git.GetCurrentSha();
    }
}
