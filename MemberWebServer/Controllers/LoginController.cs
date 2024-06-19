using MemberWebServer.Model;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly IConfiguration _configuration;
        public static List<string> RefreshTokens = new List<string>();

        public LoginController(LoginContext context, RegistrationContext registrationContext, IConfiguration configuration)
		{
			_context = context;
            _registrationContext = registrationContext;
			_configuration = configuration;
		}
		// GET: api/<LoginController>
        /// <summary>
        /// Email로 멤버 프로필 조회
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
		[HttpGet("{email}")]
		public async Task<ActionResult<Registration>> PostLogin(string email)
		{
			var registrationDB = await _registrationContext.Registrations.FindAsync(email);

			if (registrationDB == null)
			{
				return NotFound();
			}

			return registrationDB;
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
                var validationcheck =  await _registrationContext.Registrations.AnyAsync(user => user.email == loginDB.email && user.password == loginDB.password);
                var registrationDB = await _registrationContext.Registrations.FindAsync(loginDB.email);
                if (validationcheck == false)
				{
                    return BadRequest("등록된 사용자가 없습니다.");
                    
                }
				else
				{
                    var token = GenerateAccessToken(loginDB.email, registrationDB.firstName + registrationDB.lastName, registrationDB.userAvatar);
                    // Generate refresh token
                    var refreshToken = Guid.NewGuid().ToString();
                    // Store the refresh token (in-memory for simplicity)
                    RefreshTokens.Add(refreshToken);

                    _context.LoginDBs.Add(loginDB);
                    await _context.SaveChangesAsync();


                    // accesstoken, refreshtoken provide from client
                    return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token), RefreshToken = refreshToken });
                }
            }
			else
			{
				return BadRequest("이메일 형식에 맞추세요.");
			}

            // 로그인 시 메모리에 저장
			return CreatedAtAction(nameof(PostLogin), new { id = loginDB.Id }, loginDB);
            
        }
        // refreshtoken to accesstoken reprovide
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {  
            if (RefreshTokens.Contains(request.RefreshToken))
            {
                var user = _registrationContext.Registrations.FirstOrDefault(r => r.email == request.Email);
                // Generate a new access token
                var token = GenerateAccessToken(user.email, user.firstName+user.lastName, user.userAvatar);

                // Return the new access token to the client
                return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return BadRequest("Invalid refresh token");
        }
        private JwtSecurityToken GenerateAccessToken(string userEmail, string userName, string userAvatar)
        {
            // create user claim
            var claims = new List<Claim>
        {
            new Claim("email", userEmail),
            new Claim("name", userName),
            new Claim("userAvatar", userAvatar)
        };

            // Create a JWT
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // token expiration time
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                    SecurityAlgorithms.HmacSha256)
            );

            return token;
        }
        public class RefreshRequest
        {
            public string RefreshToken { get; set; }
            public string Email { get; set; }
        }
    }
}
