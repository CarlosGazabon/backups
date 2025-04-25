using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Data;
using Inventio.Models;
using System.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;

        public TelemetryController(ApplicationDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public struct TelemetryDTO
        {

            public string TenantId { get; set; }
            public string TenantEnvironment { get; set; }

            public string TenantImagePath { get; set; }
            public string TenantChangeOverString { get; set; }
            public string TenantWarehouseString { get; set; }

            public string DatabaseName { get; set; }
            public string Email { get; set; }
            public string FrontURL { get; set; }

            public string ServerTimeZone { get; set; }
            public DateTime ServerCurrentTime { get; set; }

        }

        // GET: api/Telemetry
        [HttpGet]
        [AllowAnonymous]
        public ActionResult<TelemetryDTO> GetTelemetry()
        {
            var tenantId = _configuration.GetSection("Tenant").GetSection("Id").Value;
            var tenantEnvironment = _configuration.GetSection("Tenant").GetSection("Environment").Value;
            var tenantImagePath = _configuration.GetSection("Tenant").GetSection("ImagePath").Value;
            var tenantChangeOverString = _configuration.GetSection("Tenant").GetSection("ChangeOverString").Value;
            var tenantWarehouseString = _configuration.GetSection("Tenant").GetSection("WarehouseString").Value;

            var databaseName = _context.Database.GetDbConnection().Database;
            var email = _configuration.GetSection("EmailSettings").GetSection("Mail").Value ?? "N/A";
            var frontURL = _configuration.GetSection("URLs").GetSection("FrontURL").Value ?? "N/A";

            var serverTimeZone = TimeZoneInfo.Local.DisplayName;
            var serverCurrentTime = DateTime.Now;


            TelemetryDTO result = new TelemetryDTO
            {
                TenantId = tenantId != null ? tenantId : "",
                TenantEnvironment = tenantEnvironment != null ? tenantEnvironment : "",
                TenantImagePath = tenantImagePath ?? throw new SettingsPropertyNotFoundException(),
                TenantChangeOverString = tenantChangeOverString ?? throw new SettingsPropertyNotFoundException(),
                TenantWarehouseString = tenantWarehouseString ?? throw new SettingsPropertyNotFoundException(),
                DatabaseName = databaseName,
                Email = email,
                FrontURL = frontURL,
                ServerTimeZone = serverTimeZone,
                ServerCurrentTime = serverCurrentTime
            };

            return result;
        }

    }
}
