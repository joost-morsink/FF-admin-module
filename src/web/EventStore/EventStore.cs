using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventStore
{
    public interface IEventStore : IDisposable
    {
        void StartSession();
        void EndSession(string? comment);
        bool HasSession { get; }
        void IDisposable.Dispose() => EndSession("Automatically closed.");
    }
    public class SessionFile : IDisposable
    {
        public SessionFile(string basepath, string path)
        {
            CurrentFile = path;
            var dir = Path.Combine(basepath, Path.GetDirectoryName(CurrentFile)!);
            MakeDir(dir);
            Stream = File.OpenWrite(Path.Combine(basepath, CurrentFile));
            JsonWriter = new Utf8JsonWriter(Stream);
        }
        private void MakeDir(string dir)
        {
            var parent = Path.GetDirectoryName(dir);
            if (!string.IsNullOrWhiteSpace(parent) && !Directory.Exists(parent))
                MakeDir(parent);
            
            Directory.CreateDirectory(dir);
        }
        public string CurrentFile { get; }
        public FileStream Stream { get; }
        public Utf8JsonWriter JsonWriter { get; }
        public void Dispose()
        {
            JsonWriter.Flush();
            Stream.Flush();
            JsonWriter.Dispose();
            Stream.Close();
            IsDisposed = true;
        }
        public bool IsDisposed { get; private set; }
    }
   
    public class EventStore : IEventStore
    {
        public EventStore() {
            _git = new Git("data");
            _jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                WriteIndented = false
            };
        }
        private SessionFile? _sessionFile;
        private Git _git;
        private readonly JsonSerializerOptions _jsonOptions;
        private static readonly byte[] CRLF = new byte[] { 0x0a, 0x0d };

        public bool HasSession => _sessionFile != null;

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
                    $"{now.Hour.ToString("D2")}{now.Minute.ToString("D2")}{now.Second.ToString("D2")}.json"));
        }
        public void WriteEvent(Event e)
        {
            if (_sessionFile == null)
                throw new InvalidOperationException("No open session.");
            JsonSerializer.Serialize(_sessionFile.JsonWriter, _jsonOptions);
            _sessionFile.JsonWriter.Flush();
            _sessionFile.Stream.Write(CRLF.AsSpan());

        }
    }

    public class Git
    {
        public string Path { get; }
        public Git(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }
        public void Add(string argument)
        {
            ExecuteShell("add", argument);
        }
        public void Commit(string message)
        {
            ExecuteShell("commit", "-m", message);
        }
        public void PushOrigin()
        {
            ExecuteShell("push", "origin");
        }
        private int ExecuteShell(string command, params string[] arguments)
        {
            var psi = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = "git",
                UseShellExecute = true,
                WorkingDirectory = System.IO.Path.Combine(Directory.GetCurrentDirectory(), Path)
            };
            psi.ArgumentList.Add(command);
            foreach (var arg in arguments)
                psi.ArgumentList.Add(arg);
            var p = new Process { StartInfo = psi };
            p.Start();
            p.WaitForExit();
            return p.ExitCode;
        }
    }
}
