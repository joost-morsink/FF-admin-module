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
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace FfAdminWeb.Controllers;

[ApiController]
[Route("eventstore")]
public class EventStoreController : Controller
{
    private readonly IEventRepository _eventRepository;
    private readonly IOptionRepository _optionRepository;
    private readonly ICharityRepository _charityRepository;
    private readonly IDonationRepository _donationRepository;
    private readonly IAuditRepository _auditRepository;

    public EventStoreController(IEventRepository eventRepository,
        IOptionRepository optionRepository,
        ICharityRepository charityRepository,
        IDonationRepository donationRepository,
        IAuditRepository auditRepository
    )
    {
        _eventRepository = eventRepository;
        _optionRepository = optionRepository;
        _charityRepository = charityRepository;
        _donationRepository = donationRepository;
        _auditRepository = auditRepository;
    }

    [HttpPost("import")]
    public async Task<IActionResult> PostEvent([FromBody] Event e)
    {
        var msgs = e.Validate().ToArray();
        if (msgs.Length > 0)
            return BadRequest(msgs);

        try
        {
            await _eventRepository.Import(new[] {e});
            return Ok();
        }
        catch (ValidationException vex)
        {
            return BadRequest(vex.Messages);
        }
    }

    [HttpPost("import-many")]
    public async Task<IActionResult> PostEvents([FromBody] Event[] events)
    {
        var msgs = events.SelectMany(e => e.Validate()).ToArray();
        if (msgs.Length > 0)
            return BadRequest(msgs);
        try
        {
            await _eventRepository.Import(events);
            return Ok();
        }
        catch (ValidationException vex)
        {
            return BadRequest(vex.Messages);
        }
    }
    
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents([FromQuery] int skip = 0, [FromQuery] int? limit = null)
    {

        var events = await (limit.HasValue && limit.Value < 0 
            ? _eventRepository.GetLastEvents(-limit.Value) 
            : _eventRepository.GetEvents(skip, limit));
        return Ok(events);
    }
    
    [HttpGet("branches")]
    public Task<string[]> GetBranches()
        => _eventRepository.GetBranchNames();

    public record BranchRequest(string To);

    [HttpPost("branch")]
    public Task Branch([FromBody] BranchRequest req)
        => _eventRepository.Branch(req.To);

    public record GenericBranchRequest(string Name);
    [HttpPost("new-branch")]
    public Task NewBranch([FromBody] GenericBranchRequest req) 
        => _eventRepository.CreateEmptyBranch(req.Name);
    
    [HttpDelete("branch/{name}")]
    public Task RemoveBranch(string name)
        => _eventRepository.RemoveBranch(name);
    
    [HttpPost("fast-forward")]
    public Task FastForward([FromBody] GenericBranchRequest req)
        => _eventRepository.FastForward(req.Name);
    
    public record RebaseRequest(string On);
    [HttpPost("rebase")]
    public Task Rebase([FromBody] RebaseRequest req)
        => _eventRepository.Rebase(req.On);
    
    [HttpPost("audit")]
    public async Task<IActionResult> Audit()
    {
        await _auditRepository.AddAuditMoment();
        return Ok();
    }
        
    [HttpGet("statistics/main")]
    public async Task<ActionResult<IEventRepository.Statistics>> GetStatistics()
    {
        var stats = await _eventRepository.GetStatistics();
        return Ok(stats);
    }
}
