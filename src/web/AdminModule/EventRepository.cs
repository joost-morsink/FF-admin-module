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
    }
    public class EventRepository : IEventRepository
    {
        private readonly IDatabase _db;

        public EventRepository(IDatabase db)
        {
            _db = db;
        }
        public async Task<CoreMessage> Import(IEnumerable<Event> events)
        {
            var str = events.Select(e => e.ToJsonString()).ToArray();
            var import = await _db.QueryFirst<CoreMessage>("select * from core.import_events(@events::jsonb[]);", new { events = str });
            import.Key = $"Import.{import.Key}";
            if (import.Status >= 3)
                return import;
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
