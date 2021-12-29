using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FfAdmin.EventStore;
using FfAdmin.External.GiveWp;
using FfAdminWeb.Services;
using FfAdminWeb.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("eventstore")]
    public class EventStoreController : Controller
    {
        private readonly IEventStore _eventStore;
        private readonly IEventRepository _eventRepository;
        private readonly IEventingSystem _eventingSystem;
        private readonly IOptionRepository _optionRepository;
        private readonly ICharityRepository _charityRepository;
        private readonly IDonationRepository _donationRepository;

        public EventStoreController(IEventStore eventStore,
                                    IEventRepository eventRepository,
                                    IEventingSystem eventingSystem,
                                    IOptionRepository optionRepository,
                                    ICharityRepository charityRepository,
                                    IDonationRepository donationRepository)
        {
            _eventStore = eventStore;
            _eventRepository = eventRepository;
            _eventingSystem = eventingSystem;
            _optionRepository = optionRepository;
            _charityRepository = charityRepository;
            _donationRepository = donationRepository;
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
                    return StatusCode(500, new ValidationMessage[]
                    {
                        new("", "Failed to start session.")
                    });
                _eventRepository.SetFileImported(_eventStore.SessionFile ?? throw new Exception());
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ValidationMessage[]
                {
                    new("Exception", ex.Message)
                });
            }
        }
        public class StopRequest
        {
            public string? Message { get; set; }
        }
        [HttpPost("session/stop")]
        public IActionResult StopSession([FromBody] StopRequest body)
        {
            try
            {
                _eventStore.EndSession(body.Message);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ValidationMessage[]
                {
                    new("Exception", ex.Message)
                });
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> PostEvent([FromBody] Event e)
        {
            var msgs = e.Validate().ToArray();
            if (!_eventStore.HasSession)
                return BadRequest(new ValidationMessage[]
                {
                    new("main", "No session")
                });

            if (msgs.Length > 0)
                return BadRequest(msgs);

            await _eventingSystem.ImportEvent(e);

            return Ok();
        }

        [HttpPost("audit")]
        public async Task<IActionResult> Audit()
        {
            if (_eventStore.HasSession)
                return BadRequest(new ValidationMessage[]
                {
                    new("main", "Open session")
                });
            if ((await _eventRepository.GetStatistics()).Unprocessed > 0)
                return BadRequest(new ValidationMessage[]
                {
                    new("main", "Unprocessed events")
                });
            _eventStore.StartSession();
            await _eventRepository.SetFileImported(_eventStore.SessionFile ?? throw new Exception());
            var e = new FfAdmin.Common.Audit
            {
                Timestamp = DateTimeOffset.UtcNow, Hashcode = _eventStore.Hashcode()
            };
            _eventStore.WriteEvent(e);
            var timestamp = _eventStore.FileTimestamp!.Value;
            _eventStore.EndSession("Audit event");
            await _eventRepository.Import(timestamp, new[]
            {
                e
            });
            await _eventRepository.ProcessEvents(DateTime.UtcNow);
            return Ok();
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessEvents()
        {
            var res = await _eventRepository.ProcessEvents(DateTime.UtcNow);

            if (res.Status >= 4)
                return StatusCode(500, new ValidationMessage[]
                {
                    new ("Process", res.Message)
                });
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
        public async Task<IActionResult> ImportFiles([FromBody] string[] files)
        {
            foreach (var file in files)
            {
                var events = await _eventStore.GetEventsFromFile(file);
                var ts = GetFileTimestamp(file);
                var importmsg = await _eventRepository.Import(ts, events);
                if (importmsg.Status >= 4)
                    return StatusCode(500, importmsg);
                await _eventRepository.SetFileImported(file);
            }
            return Ok();
        }
        private static DateTime GetFileTimestamp(string name)
        {
            var parts = name.Split('/', '\\');

            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);
            var day = int.Parse(parts[2]);
            if (parts[3].Length == 11 && int.TryParse(parts[3][..2], out var hour)
                && int.TryParse(parts[3][2..4], out var minute)
                && int.TryParse(parts[3][4..6], out var second))
                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            else
                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        [HttpPost("donations/give")]
        public async Task<IActionResult> ImportGiveCsv()
        {
            var file = Request.Form.Files["file"];
            var mollie = Request.Form.Files["mollie"];
            if (file == null || mollie == null)
                return BadRequest(new ValidationMessage[]
                {
                    new("", "No file uploaded")
                });
            var content = await file.ReadFormFile();
            var mollieContent = await mollie.ReadFormFile();
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(mollieContent))
                return BadRequest(new ValidationMessage[]
                {
                    new("", "File is empty")
                });
            try
            {
                var rows = GiveExportRows.FromCsv(content);
                var mollieRows = MollieExportRows.FromCsv(mollieContent);
                var options = await _optionRepository.GetOptions();
                var charities = await _charityRepository.GetCharities();
                var events = rows.ToEvents(mollieRows, charities.Select(c => c.Charity_ext_id), options.Select(o => o.Option_ext_id)).ToArray();

                if (!_eventStore.HasSession)
                    return BadRequest(new ValidationMessage[]
                    {
                        new("main", "No session")
                    });

                var msgs = events.SelectMany(e => e.Validate()).ToArray();

                if (msgs.Length > 0)
                    return BadRequest(msgs);
                var alreadyImported = new HashSet<string>(
                    await _donationRepository.GetAlreadyImported(from e in events.OfType<NewDonation>()
                                                                 select e.Donation));
                await _eventingSystem.ImportEvents(events.Where(x => !(x is NewDonation nd && alreadyImported.Contains(nd.Donation))));

                return Ok();
            }
            catch (ValidationException vex)
            {
                return BadRequest(vex.Messages);
            }
            catch (Exception ex)
            {
                return BadRequest(new ValidationMessage[]
                {
                    new("", ex.Message)
                });
            }
        }
        [HttpGet("remote/status")]
        public RemoteStatus GetRemoteStatus()
        {
            return _eventStore.GetRemoteStatus();
        }
        [HttpPost("remote/push")]
        public async Task<IActionResult> Push()
        {
            await _eventStore.Push();
            return Ok();
        }
        [HttpPost("remote/pull")]
        public async Task<IActionResult> Pull()
        {
            await _eventStore.Pull();
            return Ok();
        }
    }
}
