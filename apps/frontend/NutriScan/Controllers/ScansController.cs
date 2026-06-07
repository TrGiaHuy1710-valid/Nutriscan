using Microsoft.AspNetCore.Mvc;
using NutriScan.Services.Scans;

namespace NutriScan.Controllers
{
    [ApiController]
    [Route("api/scans")]
    public class ScansController : ControllerBase
    {
        private readonly IScanAnalysisService _scanAnalysisService;
        private readonly ILogger<ScansController> _logger;

        public ScansController(
            IScanAnalysisService scanAnalysisService,
            ILogger<ScansController> logger)
        {
            _scanAnalysisService = scanAnalysisService;
            _logger = logger;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _scanAnalysisService.AnalyzeAsync(file, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(502, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scan analysis failed");
                return StatusCode(500, new { error = "Scan analysis failed" });
            }
        }
    }
}
