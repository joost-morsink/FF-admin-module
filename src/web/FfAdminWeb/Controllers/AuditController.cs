﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdminWeb.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("audit")]
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
            return MakeExcel(recent);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var rep = await _auditRepository.GetReport(id);
            return MakeExcel(rep);
        }
        [HttpGet("all")]
        public async Task<ActionResult<IAuditRepository.AuditReportInfo[]>> GetAll()
        {
            var all = await _auditRepository.GetReports();
            return all;
        }
        private IActionResult MakeExcel(IAuditRepository.AuditReport report)
        {
            var excel = new[] { DataSheet.Build("Overview", b =>
                {
                    b.Write("Main audit information").RowFeed().RowFeed();
                    b.Empty().Write("Hash").RowFeed();
                    b.Write("Previous").Write(report.Previous.Main.Hashcode).RowFeed();
                    b.Write("Current").Write(report.Current.Main.Hashcode).RowFeed().RowFeed();
                    b.Empty().Write("Previous").Write("Current").RowFeed();
                    var objs = new[] { report.Previous, report.Current };
                    b.Line("Total number of events", objs, x => x.Main.Num_events);
                    b.Line("Processed events", objs, x => x.Main.Num_events - x.Main.Num_unprocessed_events);
                    b.Line("Unprocessed events", objs, x => x.Main.Num_unprocessed_events);
                    b.Line("Number of donations", objs, x => x.Main.Num_donations);
                    b.Line("Number of donors", objs, x => x.Main.Num_donors);
                    b.Line("Number of charities", objs, x => x.Main.Num_charities);
                })
            }.Concat(from curF in report.Current.Financials
                     join prevF in report.Previous.Financials on curF.Currency equals prevF.Currency into prevs
                     select DataSheet.Build(curF.Currency, b =>
                     {
                         b.Write($"Audit information for currency {curF.Currency}").RowFeed().RowFeed();
                         var objs = prevs.Concat(new[] { curF }).ToArray();
                         b.Empty();
                         if (objs.Length == 2)
                             b.Write("Previous");
                         b.Write("Current").RowFeed();
                         b.Line("Donations amount", objs, x => x.Donation_amount);
                         b.Line("Unentered donations amount", objs, x => x.Unentered_donation_amount);
                         b.Line("Invested amount", objs, x => x.Invested_amount);
                         b.Line("Cash amount", objs, x => x.Cash_amount);
                         b.Line("Total amount", objs, x => x.Invested_amount + x.Cash_amount + x.Unentered_donation_amount);
                         b.Line("Allocated amount", objs, x => x.Allocated_amount);
                         b.Line("Transferred amount", objs, x => x.Transferred_amount);
                         b.Line("Transfer pending amount", objs, x => x.Allocated_amount - x.Transferred_amount);
                     })).ToExcel();
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

    }
    internal static class AuditReportHelper
    {
        public static void Line<T>(this DataSheetWriter writer, string header, T[] objects, Func<T, object> getter)
        {
            writer.Write(header);
            foreach (var obj in objects)
                writer.Write(getter(obj));
            writer.RowFeed();
        }
    }

}