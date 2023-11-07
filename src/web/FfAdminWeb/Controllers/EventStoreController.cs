using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FfAdmin.EventStore;
using FfAdmin.External.GiveWp;
using FfAdminWeb.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("eventstore")]
    public class EventStoreController : Controller
    {
        private readonly IEventStore _oldEventStore;
        private readonly FfAdmin.EventStore.Abstractions.IEventStore _eventStore;
        private readonly IEventRepository _eventRepository;
        private readonly IOptionRepository _optionRepository;
        private readonly ICharityRepository _charityRepository;
        private readonly IDonationRepository _donationRepository;
        private readonly IAuditRepository _auditRepository;

        public EventStoreController(IEventStore oldEventStore,
                                    FfAdmin.EventStore.Abstractions.IEventStore eventStore,
                                    IEventRepository eventRepository,
                                    IOptionRepository optionRepository,
                                    ICharityRepository charityRepository,
                                    IDonationRepository donationRepository,
                                    IAuditRepository auditRepository
                                    )
        {
            _oldEventStore = oldEventStore;
            _eventStore = eventStore;
            _eventRepository = eventRepository;
            _optionRepository = optionRepository;
            _charityRepository = charityRepository;
            _donationRepository = donationRepository;
            _auditRepository = auditRepository;
        }
        [HttpGet("session/is-available")]
        public bool HasSession()
        {
            return _oldEventStore.HasSession;
        }
        [HttpPut("session/is-available")]
        public void SetSession([FromBody] bool available)
        {
            if (available != _oldEventStore.HasSession)
            {
                if (available)
                    _oldEventStore.StartSession();
                else
                    _oldEventStore.EndSession(null);
            }
        }
        [HttpPost("session/start")]
        public IActionResult StartSession()
        {
            try
            {
                _oldEventStore.StartSession();
                if (_oldEventStore.SessionFile == null)
                    return StatusCode(500, new ValidationMessage[]
                    {
                        new("", "Failed to start session.")
                    });
                _eventRepository.SetFileImported(_oldEventStore.SessionFile ?? throw new Exception());
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
                _oldEventStore.EndSession(body.Message);
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
            if (msgs.Length > 0)
                return BadRequest(msgs);

            await _eventRepository.Import(new[] {e});

            return Ok();
        }

        [HttpPost("audit")]
        public async Task<IActionResult> Audit()
        {
            await _auditRepository.AddAuditMoment();
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
            var allFiles = _oldEventStore.AllFiles();
            var importedFiles = new HashSet<string>(await _eventRepository.GetProcessedFiles());
            return allFiles.Where(f => !importedFiles.Contains(f));
        }
        [HttpPost("files/import")]
        public async Task<IActionResult> ImportFiles([FromBody] string[] files)
        {
            foreach (var file in files)
            {
                var events = await _oldEventStore.GetEventsFromFile(file);
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
                var events = rows.ToEvents(mollieRows, charities.Select(c => c.Id), options.Select(o => o.Id)).ToArray();

                if (!_oldEventStore.HasSession)
                    return BadRequest(new ValidationMessage[]
                    {
                        new("main", "No session")
                    });

                var msgs = events.SelectMany(e => e.Validate()).ToArray();

                if (msgs.Length > 0)
                    return BadRequest(msgs);

                var alreadyImported = await _donationRepository.GetAlreadyImported(
                    from e in events.OfType<NewDonation>()
                    select e.Donation);
                var donationCharityMap =alreadyImported.ToDictionary(i => i.DonationId, i => i.CharityId);
           
                await _eventRepository.Import(events.SelectMany(TransformEvent));

                return Ok();

                IEnumerable<Event> TransformEvent(Event e)
                {
                    if (e is NewDonation nd)
                    {
                        if (donationCharityMap.TryGetValue(nd.Donation, out var charity)) {
                            if (charity != nd.Charity)
                                yield return new UpdateCharityForDonation
                                {
                                    Charity = nd.Charity,
                                    Donation = nd.Donation
                                };
                        } else
                            yield return e;
                    }
                    else
                        yield return e;
                }
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
            return _oldEventStore.GetRemoteStatus();
        }
        [HttpPost("remote/push")]
        public async Task<IActionResult> Push()
        {
            await _oldEventStore.Push();
            return Ok();
        }
        [HttpPost("remote/pull")]
        public async Task<IActionResult> Pull()
        {
            await _oldEventStore.Pull();
            return Ok();
        }
    }
}
