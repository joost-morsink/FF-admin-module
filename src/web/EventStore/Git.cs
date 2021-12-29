using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace FfAdmin.EventStore
{
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
        public void PullOrigin()
        {
            ExecuteShell("pull", "origin", "--ff-only");
        }
        public void PullMergeOrigin()
        {
            ExecuteShell("pull", "origin", "--no-rebase");
        }
        public string CurrentBranch()
        {
            return ExecuteShell("branch", "--show-current").Output.TrimEnd();
        }
        public string? RemoteBranch()
        {
            var res = ExecuteShell("status", "-sb").Output;
            return (from part in res.Split('\n')
                    where part.TrimStart().StartsWith("##")
                    let index = part.IndexOf("...", StringComparison.Ordinal)
                    where index > 0
                    let remoteAndJunk = part[(index + 3)..].TrimEnd()
                    let space = remoteAndJunk.IndexOf(' ')
                    select space < 0 ? remoteAndJunk : remoteAndJunk[..space]).FirstOrDefault();
        }
        public (int behind, int ahead)? BehindAhead(string compare, string to)
        {
            var res = ExecuteShell("rev-list", "--left-right", "--count", $"{to}...{compare}").Output;
            var parts = res.Split('\t').Select(Parse).Where(x => x != null).Select(x => x!.Value).ToArray();

            return parts.Length == 2 ? (parts[0], parts[1]) : null;

            int? Parse(string str)
                => int.TryParse(str, out var r) ? r : null;
        }
        public (int behind, int ahead)? CurrentBehindAhead()
        {
            var current = CurrentBranch();
            var remote = RemoteBranch();
            return remote == null ? null : BehindAhead(current, remote);
        }
        public RemoteStatus GetRemoteStatus()
        {
            var current = CurrentBranch();
            var remote = RemoteBranch();
            if (remote == null)
                return new RemoteStatus
                {
                    Name = current,
                    HasRemote = false
                };
            var rel = BehindAhead(current, remote);
            return new RemoteStatus
            {
                Name = current,
                HasRemote = true,
                Behind = rel?.behind,
                Ahead = rel?.ahead
            };
        }
        public string GetCurrentSha()
        {
            return ExecuteShell("rev-parse", "HEAD").Output.TrimEnd();
        }
        private ProcessResult ExecuteShell(string command, params string[] arguments)
        {
            var psi = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = "git",
                UseShellExecute = false,
                WorkingDirectory = System.IO.Path.Combine(Directory.GetCurrentDirectory(), Path),
                RedirectStandardOutput = true
            };

            psi.ArgumentList.Add(command);
            foreach (var arg in arguments)
                psi.ArgumentList.Add(arg);
            var p = new Process { StartInfo = psi };
            p.Start();
            p.WaitForExit();
            var output = p.StandardOutput.ReadToEnd();
            return new ProcessResult(p.ExitCode, output);
        }
        public struct ProcessResult
        {
            public ProcessResult(int returnCode, string output)
            {
                ReturnCode = returnCode;
                Output = output;
            }

            public int ReturnCode { get; }
            public string Output { get; }
        }
    }
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
    public class RemoteStatus
    {
        public string Name { get; set; } = "";
        public bool HasRemote { get; set; }
        public int? Ahead { get; set; }
        public int? Behind { get; set; }
    }

}
