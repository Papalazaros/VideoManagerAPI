using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VideoManager.Services;

namespace VideoManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly ILogger<RoomsController> _logger;
        private readonly IRoomService _roomService;

        public RoomsController(ILogger<RoomsController> logger,
            IRoomService roomService)
        {
            _logger = logger;
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _roomService.GetAll());
        }
    }
}
