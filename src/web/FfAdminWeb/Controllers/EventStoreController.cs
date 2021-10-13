using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FfAdmin.EventStore;
using FfAdmin.External.GiveWp;
using Microsoft.AspNetCore.Http;
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
        private readonly IOptionRepository _optionRepository;
        private readonly ICharityRepository _charityRepository;
        private readonly IOptions<JsonOptions> _jsonOptions;

        public EventStoreController(IEventStore eventStore,
                                    IEventRepository eventRepository,
                                    IOptionRepository optionRepository,
                                    ICharityRepository charityRepository,
                                    IOptions<JsonOptions> jsonOptions)
        {
            _eventStore = eventStore;
            _eventRepository = eventRepository;
            _optionRepository = optionRepository;
            _charityRepository = charityRepository;
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
        public IActionResult StartSession()
        {
            try
            {
                _eventStore.StartSession();
                if (_eventStore.SessionFile == null)
                    return StatusCode(500,new ValidationMessage[] { new("", "Failed to start session.") });
                _eventRepository.SetFileImported(_eventStore.SessionFile ?? throw new Exception());
                return Ok();
            } catch (Exception ex) {
                return StatusCode(500, new ValidationMessage[] { new("Exception", ex.Message) });
            }
        }
        public class StopRequest
        {
            public string? Message { get; set; }
        }
        [HttpPost("session/stop")]
        public IActionResult StopSession([FromBody]StopRequest body)
        {
            try
            {
                _eventStore.EndSession(body.Message);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ValidationMessage[] { new("Exception", ex.Message) });
            }
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
        [HttpPost("reset")]
        public async Task<IActionResult> Reset()
        {
            await _eventRepository.ResetEvents();
            return Ok();
        }
        [HttpPost("deleteAll")]
        public async Task<IActionResult> DeleteAll()
        {
            await _eventRepository.DeleteAllEvents();
            return Ok();
        }
        [HttpGet("unprocessed")]
        public Task<IEventRepository.DbEvent[]> UnprocessedEvents()
        {
            return _eventRepository.GetUnprocessedEvents();
        }
        [HttpGet("files/unimported")]
        public async Task<IEnumerable<string>> UnimportedFiles()
        {
            var allFiles = _eventStore.AllFiles();
            var importedFiles = new HashSet<string>(await _eventRepository.GetProcessedFiles());
            return allFiles.Where(f => !importedFiles.Contains(f));
        }
        [HttpPost("files/import")]
        public async Task ImportFiles([FromBody] string[] files)
        {
            foreach (var file in files)
            {
                var events = await _eventStore.GetEventsFromFile(file);
                await _eventRepository.Import(events);
                await _eventRepository.SetFileImported(file);
            }
        }
        [HttpPost("donations/give")]
        public async Task<IActionResult> ImportGiveCsv()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null)
                return BadRequest(new ValidationMessage[] { new("", "No file uploaded") });
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            if(content==null)
                return BadRequest(new ValidationMessage[] { new("", "File is empty") });
            try
            {
                var rows = GiveExportRows.FromCsv(content);
                var options = await _optionRepository.GetOptions();
                var charities = await _charityRepository.GetCharities();
                var events = rows.ToEvents(charities.Select(c => c.Charity_ext_id), options.Select(o => o.Option_ext_id)).ToArray();

                if (!_eventStore.HasSession)
                    return BadRequest(new ValidationMessage[] {
                    new("main","No session")
                });
                var msgs = events.SelectMany(e => e.Validate()).ToArray();

                if (msgs.Length > 0)
                    return BadRequest(msgs);
                foreach (var e in events)
                    _eventStore.WriteEvent(e);

                await _eventRepository.Import(events);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new ValidationMessage[] { new("", ex.Message) });
            }
        }
    }
}
