using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MemberWebServer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RegistrationController : ControllerBase
	{
		// POST api/<RegistrationController>
		[HttpPost]
		public void Post([FromBody] string value)
		{

		}
	}
}
