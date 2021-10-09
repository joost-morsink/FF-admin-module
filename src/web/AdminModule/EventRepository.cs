﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FfAdmin.Common;
using Npgsql;

namespace FfAdmin.AdminModule
{
    public record CoreMessage
    {
        public string Key { get; set; } = "";
        public int Status { get; set; }
        public string Message { get; set; } = "";
    }
    public interface IEventRepository
    {
        Task<CoreMessage> Import(IEnumerable<Event> e);
        Task SetFileImported(string path);
        Task<CoreMessage> ProcessEvents(DateTime until);
        Task<Statistics> GetStatistics();
        Task ResetEvents();
        Task DeleteAllEvents();
        Task<string[]> GetProcessedFiles();

        public class Statistics
        {
            public int Processed { get; set; }
            public DateTimeOffset? LastProcessed { get; set; }
            public int Unprocessed { get; set; }
            public DateTimeOffset? FirstUnprocessed { get; set; }
        }
    }
    public class EventRepository : IEventRepository
    {
        private readonly IDatabase _db;

        public EventRepository(IDatabase db)
        {
            _db = db;
        }

        public Task<IEventRepository.Statistics> GetStatistics()
        {
            return _db.QueryFirst<IEventRepository.Statistics>(@"
                select (select count(*) from core.event where processed = TRUE) processed
                    , (select max(timestamp)::timestamp at time zone 'UTC' from core.event where processed = TRUE) lastProcessed
                    , (select count(*) from core.event where processed = FALSE) unprocessed
                    , (select min(timestamp)::timestamp at time zone 'UTC' from core.event where processed = FALSE) firstUnprocessed");
        }
        public async Task<CoreMessage> Import(IEnumerable<Event> events)
        {
            var str = events.Select(e => e.ToJsonString()).ToArray();
            var import = await _db.QueryFirst<CoreMessage>("select * from core.import_events(@events::jsonb[]);", new { events = str });
            import.Key = $"Import.{import.Key}";

            return import;
        }

        public async Task SetFileImported(string path)
        {
            await _db.Execute(@"insert into core.event_file(path)
                select @path where not exists (select 1 from core.event_file where path=@path);", new { path = path });
        }

        public async Task<CoreMessage> ProcessEvents(DateTime until)
        {
            try
            {
                return await _db.Run(async c =>
                {
                    var cmd = c.CreateCommand();
                    cmd.CommandText = "call ff.process_events(current_timestamp::timestamp, ROW(1,'','Nothing to process.')::core.message)";
                    var res = await cmd.ExecuteScalarAsync();
                    return (CoreMessage)res!;
                });
            }
            catch (Exception ex)
            {
                return new CoreMessage
                {
                    Key = "Process",
                    Status = 4,
                    Message = ex.Message
                };
            }
        }
        public Task ResetEvents()
        {
            return _db.Execute(@"truncate table ff.fraction cascade;
truncate table ff.fractionset cascade;
truncate table ff.option cascade;
truncate table ff.donation cascade;
truncate table ff.charity cascade;
truncate table ff.transfer cascade;
truncate table ff.allocation cascade;
update core.event set processed = FALSE;");
        }
        public async Task DeleteAllEvents()
        {
            await ResetEvents();
            await _db.Execute("truncate table core.event cascade; truncate table core.event_file cascade;");
        }
        private class EventFile
        {
            public string Path { get; set; } = "";
        }
        public async Task<string[]> GetProcessedFiles()
        {
            var res = await _db.Query<EventFile>("select * from core.event_file;");
            return res.Select(x => x.Path).ToArray();
        }
    }
    public class ImportException : ApplicationException
    {
        public ImportException(string message) : base(message) { }
    }
    public class ProcessException : ApplicationException
    {
        public ProcessException(string message) : base(message) { }
    }
}