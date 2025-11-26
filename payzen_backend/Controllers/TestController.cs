using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using System.Reflection;

namespace payzen_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public TestController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        /// <summary>
        /// Health check endpoint - Statut de l'API
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var status = new
            {
                Status = "Healthy",
                Timestamp = DateTimeOffset.UtcNow,
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production"
            };

            return Ok(status);
        }

        /// <summary>
        /// Vérifie la connexion à la base de données
        /// </summary>
        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var canConnect = await _db.Database.CanConnectAsync();

                return Ok(new
                {
                    Status = canConnect ? "Connected" : "Disconnected",
                    Database = _db.Database.GetConnectionString()?.Split(';')[0],
                    Timestamp = DateTimeOffset.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = ex.Message,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }
        }

        /// <summary>
        /// Ping simple pour vérifier que l'API répond
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                Message = "Pong",
                Timestamp = DateTimeOffset.UtcNow
            });
        }
    }
}