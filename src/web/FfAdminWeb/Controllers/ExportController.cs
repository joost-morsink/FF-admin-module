using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FfAdminWeb.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FfAdminWeb.Controllers
{
    [ApiController]
    [Route("admin/export")]
    public class ExportController : Controller
    {
        private readonly IExportRepository _exportRepository;

        public ExportController(IExportRepository exportRepository)
        {
            _exportRepository = exportRepository;
        }
        [HttpGet("json")]
        public async Task<IActionResult> Json()
        {
            var data = await _exportRepository.GetExportRows();
            var res = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, Event.DefaultJsonOptions));
            return File(res, "application/json", "web_export.json");
        }
        [HttpGet("csv")]
        public async Task<IActionResult> Csv()
        {
            var data = await _exportRepository.GetExportRows();
            var res = Encoding.UTF8.GetBytes(data.ToCsv());
            return File(res, "text/csv", "web_export.csv");
        }

        [HttpGet("sql")]
        public async Task<IActionResult> Sql()
        {
            var data = await _exportRepository.GetExportRows();
            var res = Encoding.UTF8.GetBytes(data.ToSql());
            return File(res, "text/sql", "web_export.sql");
        }

        [HttpGet("history/json")]
        public async Task<IActionResult> HistoryJson(DateTime? from)
        {
            var date = from ?? new DateTime(2000, 1, 1);
            var data = await _exportRepository.GetHistoricRows(date);
            var res = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, Event.DefaultJsonOptions));
            return File(res, "application/json", "web_export_history.json");
        }

        [HttpGet("history/csv")]
        public async Task<IActionResult> HistoryCsv(DateTime? from)
        {
            var date = from ?? new DateTime(2000, 1, 1);
            var data = await _exportRepository.GetHistoricRows(date);
            var res = Encoding.UTF8.GetBytes(data.ToCsv());
            return File(res, "text/csv", "web_export_history.csv");            
        }
        [HttpGet("history/sql")]
        public async Task<IActionResult> HistorySql(DateTime? from, bool? truncate)
        {
            var date = from ?? new DateTime(2000, 1, 1);
            var data = await _exportRepository.GetHistoricRows(date);
            var res = Encoding.UTF8.GetBytes(data.ToSql(truncate ?? !from.HasValue));
            return File(res, "text/sql", "web_export_history.sql");
        }
    }
}
