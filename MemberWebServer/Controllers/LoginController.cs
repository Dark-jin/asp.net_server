using MemberWebServer.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
		private readonly RegistrationContext _registrationContext;

		public LoginController(LoginContext context, RegistrationContext registrationContext)
		{
			_context = context;
            _registrationContext = registrationContext;
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
           
            if (match.Success)
			{
				var emailcheck = await _registrationContext.Registrations.FindAsync(loginDB.email);
				var passwordcheck = await _registrationContext.Registrations.FindAsync(loginDB.password);
                if (emailcheck == null)
				{
                    return BadRequest("이메일이 틀렸습니다.");
                    
                }
				else if(passwordcheck == null)
				{
					return BadRequest("비밀번호가 틀렸습니다.");
				}
				else
				{
                    _context.LoginDBs.Add(loginDB);
                    await _context.SaveChangesAsync();
                }
                
            }
			else
			{
				return BadRequest("이메일 형식에 맞추세요.");
			}

			return CreatedAtAction(nameof(PostLogin), new { id = loginDB.Id }, loginDB);
		}
	}
}
