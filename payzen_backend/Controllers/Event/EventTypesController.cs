using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using payzen_backend.Data;
using payzen_backend.Models.Event;
using payzen_backend.Models.Users.Dtos;

namespace payzen_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/eventtypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventType>>> GetEventTypes()
        {
            return await _context.EventTypes.ToListAsync();
        }

        // GET: api/eventtypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventType>> GetEventType(int id)
        {
            var eventType = await _context.EventTypes.FindAsync(id);

            if (eventType == null)
                return NotFound();

            return eventType;
        }

        // POST: api/eventtypes
        [HttpPost]
        public async Task<ActionResult<EventType>> PostEventType(EventType eventType)
        {
            _context.EventTypes.Add(eventType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventType), new { id = eventType.Id }, eventType);
        }

        // PUT: api/eventtypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEventType(int id, EventType eventType)
        {
            if (id != eventType.Id)
                return BadRequest();

            _context.Entry(eventType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.EventTypes.Any(e => e.Id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

        // DELETE: api/eventtypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType == null)
                return NotFound();

            _context.EventTypes.Remove(eventType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
