using System;
using EventStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("eventstore")]
    public class EventStoreController : Controller
    {
        private readonly IEventStore _eventStore;
        private readonly IOptions<JsonOptions> _jsonOptions;

        public EventStoreController(IEventStore eventStore, IOptions<JsonOptions> jsonOptions)
        {
            _eventStore = eventStore;
            _jsonOptions = jsonOptions;
        }
        [HttpGet("session/is-available")]
        public bool HasSession()
        {
            return _eventStore.HasSession;
        }
        [HttpPut("session/is-available")]
        public void SetSession([FromBody] bool available)
        {
            if (available != _eventStore.HasSession)
            {
                if (available)
                    _eventStore.StartSession();
                else
                    _eventStore.EndSession(null);
            }
        }
        [HttpPost("session/start")]
        public void StartSession()
        {
            _eventStore.StartSession();
        }
        public class StopRequest
        {
            public string? Message { get; set; }
        }
        [HttpPost("session/stop")]
        public void StopSession([FromBody]StopRequest body)
        {
            _eventStore.EndSession(body.Message);
        }
        [HttpPost]
        public void PostEvent([FromBody] Event e)
        {
            //_jsonOptions.Value.JsonSerializerOptions
        }
    }
}
