using Microsoft.AspNetCore.Mvc;
using NavExpo.Models;   // Added
using NavExpo.Services; // Added

namespace NavExpo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController : ControllerBase // Inherit from ControllerBase for API
    {
        private readonly IWebHostEnvironment _environment;
        private readonly MapService _mapService; // 1. Inject the service

        public MapController(IWebHostEnvironment environment, MapService mapService)
        {
            _environment = environment;
            _mapService = mapService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadMap(IFormFile file)
        {
            // 1. Validate File
            if (file == null || file.Length == 0)
            {
                return BadRequest("No File Uploaded");
            }

            // 2. Ensure "wwwroot" exists (Safety check)
            if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
            {
                _environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // 3. Prepare Folder
            string uploadFolder = Path.Combine(_environment.WebRootPath, "maps");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // 4. Generate Filename and Path
            string uniqueFilename = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadFolder, uniqueFilename);

            // 5. Save physical file to disk
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 6. Create Database Record
            var newMap = new MapDocument
            {
                FileName = uniqueFilename,
                OriginalName = file.FileName,
                FileUrl = $"/maps/{uniqueFilename}",
                UploadDate = DateTime.UtcNow
            };

            // 7. Save to MongoDB
            await _mapService.CreateAsync(newMap);

            // 8. Return the full object (including the new DB ID)
            return Ok(newMap);
        }

        // Optional: Endpoint to see uploaded maps
        [HttpGet]
        public async Task<IActionResult> GetAllMaps()
        {
            var maps = await _mapService.GetAllAsync();
            return Ok(maps);
        }
    }
}