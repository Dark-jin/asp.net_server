using MemberWebServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MemberWebServer.Controllers
{
	[Route("api/[controller]")]
	[Produces("application/json")]
	[ApiController]
	public class LoginController : ControllerBase
	{
		private readonly LoginContext _context;

		public LoginController(LoginContext context)
		{
			_context = context;
		}
		// GET: api/<LoginController>
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
		/// <remarks>
		/// Sample request:
		///
		///     POST /Login
		///     {
		///        "id": 0, - 자동으로 1씩 증가
		///        "email" : test@test.com,
		///        "password": test
		///     }
		///
		/// </remarks>
		[HttpPost]
		public async  Task<ActionResult<LoginDB>> PostLogin(LoginDB loginDB)
		{
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
			Match match = regex.Match(loginDB.email);

            if(match.Success)
			{
                _context.LoginDBs.Add(loginDB);
                await _context.SaveChangesAsync();
            }
			else
			{
				return BadRequest();
			}

			return CreatedAtAction(nameof(PostLogin), new { id = loginDB.Id }, loginDB);
		}
	}
}
