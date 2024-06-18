using MemberWebServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MemberWebServer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RegistrationController : ControllerBase
	{
        private readonly RegistrationContext _context;

        public RegistrationController(RegistrationContext context)
        {
            _context = context;
        }
        // POST api/<RegistrationController>
        [HttpPost]
        [Consumes("application/json")]
        public async Task<ActionResult<Registration>> PostRegistration(Registration registration)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existingRegistration = await _context.Registrations.FirstOrDefaultAsync(r => r.Email == registration.Email);
            if (existingRegistration != null)
            {
                return BadRequest("등록된 정보가 있습니다.");
            }
            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(PostRegistration), new { email = registration.Email }, registration);
        }
	}
}
