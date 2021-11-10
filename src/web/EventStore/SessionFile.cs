using System;
using System.IO;

namespace FfAdmin.EventStore
{
    public class SessionFile : IDisposable
    {
        public SessionFile(string basepath, string path, DateTime fileTimestamp)
        {
            CurrentFile = path;
            var dir = Path.Combine(basepath, Path.GetDirectoryName(CurrentFile)!);
            MakeDir(dir);
            Stream = File.OpenWrite(Path.Combine(basepath, CurrentFile));
            Writer = new StreamWriter(Stream);
            Timestamp = fileTimestamp;
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
        public TextWriter Writer { get; }
        public DateTime Timestamp { get; }
        public void Dispose()
        {
            Writer.Flush();
            Stream.Flush();
            Writer.Dispose();
            Stream.Close();
            IsDisposed = true;
        }
        public bool IsDisposed { get; private set; }
    }
}
