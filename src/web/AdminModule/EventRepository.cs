using System;
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
        Task<CoreMessage> ProcessEvents(DateTime until);
        Task<Statistics> GetStatistics();
        public class Statistics
        {
            public int Processed { get; set; }
            public DateTime? LastProcessed { get; set; }
            public int Unprocessed { get; set; }
            public DateTime? FirstUnprocessed { get; set; }
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
                    , (select max(timestamp) from core.event where processed = TRUE) lastProcessed
                    , (select count(*) from core.event where processed = FALSE) unprocessed
                    , (select min(timestamp) from core.event where processed = FALSE) firstUnprocessed");      
        }
        public async Task<CoreMessage> Import(IEnumerable<Event> events)
        {
            var str = events.Select(e => e.ToJsonString()).ToArray();
            var import = await _db.QueryFirst<CoreMessage>("select * from core.import_events(@events::jsonb[]);", new { events = str });
            import.Key = $"Import.{import.Key}";

            return import;
        }
        public async Task<CoreMessage> ProcessEvents(DateTime until)
        {
            try
            {
                await _db.Execute(
                    @"do $$
                Declare
                    res core.message;
                begin
                    call ff.process_events(current_timestamp::timestamp, res);
                end; $$ language PLPGSQL;");
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
   
            return new CoreMessage
            {
                Key = "",
                Message = "OK",
                Status = 0
            };
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
