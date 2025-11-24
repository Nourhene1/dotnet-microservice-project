using ClientsAPI.Data;
using ClientsAPI.models;
using ClientsAPI.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly ClientsDbContext _context;

        public ClientController(ClientsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Clients.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return Ok(client);
        }
    }
}
