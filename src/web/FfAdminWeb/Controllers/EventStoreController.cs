using System;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FfAdmin.EventStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("eventstore")]
    public class EventStoreController : Controller
    {
        private readonly IEventStore _eventStore;
        private readonly IEventRepository _eventRepository;
        private readonly IOptions<JsonOptions> _jsonOptions;

        public EventStoreController(IEventStore eventStore, IEventRepository eventRepository, IOptions<JsonOptions> jsonOptions)
        {
            _eventStore = eventStore;
            _eventRepository = eventRepository;
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

        [HttpPost("import")]
        public async Task<IActionResult> PostEvent([FromBody] Event e)
        {
            var msgs = e.Validate().ToArray();
            if (!_eventStore.HasSession)
                return BadRequest(new ValidationMessage[] {
                    new("main","No session")
                });

            if (msgs.Length > 0)
                return BadRequest(msgs);

            _eventStore.WriteEvent(e);
            await _eventRepository.Import(new[] { e });

            return Ok();
        }
        [HttpPost("process")]
        public async Task<IActionResult> ProcessEvents()
        {
            var res = await _eventRepository.ProcessEvents(DateTime.UtcNow);

            if (res.Status >= 4)
                return StatusCode(500, new ValidationMessage[] { new ValidationMessage("Process", res.Message) });
            return Ok();
        }
        [HttpGet("statistics/main")]
        public async Task<ActionResult<IEventRepository.Statistics>> GetStatistics()
        {
            var stats = await _eventRepository.GetStatistics();
            return Ok(stats);
        }
    }
}
