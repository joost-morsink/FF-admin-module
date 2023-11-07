using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Calculator;
using FfAdminWeb.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/audit")]
    public class AuditController : Controller
    {
        private readonly IAuditRepository _auditRepository;

        public AuditController(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent()
        {
            var recent = await _auditRepository.GetRecentReport();
            return MakeExcel(recent, $"audit_{recent.Moment.Timestamp:yyyyMMdd_HHmmss}.xlsx");
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var rep = await _auditRepository.GetReport(id);
            return MakeExcel(rep, $"audit_{rep.Moment.Timestamp:yyyyMMdd_HHmmss}.xlsx");
        }
        [HttpGet("all")]
        public async Task<ActionResult<AuditMoment[]>> GetAll()
        {
            var all = await _auditRepository.GetReports();
            return all;
        }
        
        private IActionResult MakeExcel(IAuditRepository.AuditReport report, string filename = "audit.xlsx")
        {
            var excel = new[]
            {
                DataSheet.Build("Overview", b =>
                {
                    b.Write("Main audit information").RowFeed().RowFeed();
                    b.Empty().Write("Hash").RowFeed();
                    b.Write("Previous").Write(report.Moment.PreviousHashCode).RowFeed();
                    b.Write("Current").Write(report.Moment.HashCode).RowFeed().RowFeed();
                })
            }.ToExcel();
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }

    }
    internal static class AuditReportHelper
    {
        public static void Line<T>(this DataSheetWriter writer, string header, IEnumerable<T> objects, Func<T, object> getter)
        {
            writer.Write(header);
            foreach (var obj in objects)
                writer.Write(getter(obj));
            writer.RowFeed();
        }
    }

}
