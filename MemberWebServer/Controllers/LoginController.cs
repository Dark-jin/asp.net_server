using MemberWebServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MemberWebServer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LoginController : ControllerBase
	{
		private readonly LoginContext _context;

		public LoginController(LoginContext context)
		{
			_context = context;
		}
		// GET: api/<LoginController>
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}
		[HttpGet("{id}")]
		public async Task<ActionResult<LoginDB>> PostLogin(long id)
		{
			var loginDB = await _context.LoginDBs.FindAsync(id);

			if (loginDB == null)
			{
				return NotFound();
			}

			return loginDB;
		}

		// POST api/<LoginController>
		[HttpPost]
		public async  Task<ActionResult<LoginDB>> PostLogin(LoginDB loginDB)
		{
			
			_context.LoginDBs.Add(loginDB);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(PostLogin), new { id = loginDB.Id }, loginDB);
		}
	}
}
