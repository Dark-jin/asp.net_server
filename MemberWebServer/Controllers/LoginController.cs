using MemberWebServer.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
using System.Collections.Concurrent;
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
        private readonly MemberContext _memberContext;
        private readonly IConfiguration _configuration;
        public static List<string> RefreshTokens = new List<string>();

        public LoginController(LoginContext context, RegistrationContext registrationContext, IConfiguration configuration, MemberContext memberContext)
        {
            _context = context;
            _registrationContext = registrationContext;
            _configuration = configuration;
            _memberContext = memberContext;
        }
        // GET: api/<LoginController>
        /// <summary>
        /// AccessToken으로 멤버 프로필 조회
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
		public async Task<ActionResult<MemberDB>> PostLogin()
		{
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var memberDB = await _memberContext.MemberDBs.SingleOrDefaultAsync(t=>t.accesstoken == token);

            if (memberDB == null)
			{
				return NotFound();
			}
            if(memberDB.accesstoken == null || memberDB.refreshtoken == null)
            {
                return BadRequest("로그인을 해주시길 바랍니다.");
            }

			return memberDB;
		}
		// POST api/<LoginController>
		/// <remarks>
		/// Sample request:
		///
		///     POST /Login
		///     {
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
                var memberDB = await _memberContext.MemberDBs.FindAsync(loginDB.email);
                if (validationcheck == false)
				{
                    return BadRequest("등록된 사용자가 없습니다.");
                    
                }
				else
				{
                    var token = GenerateAccessToken(loginDB.email, registrationDB.firstName + registrationDB.lastName);
                    // Generate refresh token
                    var refreshToken = Guid.NewGuid().ToString();
                    // Store the refresh token (in-memory for simplicity)
                    RefreshTokens.Add(refreshToken);

                    var membercheck = await _memberContext.MemberDBs.FirstOrDefaultAsync(m => m.email == loginDB.email);

                    if (membercheck == null)
                    {
                        var member = new MemberDB
                        {
                            firstName = registrationDB.firstName,
                            lastName = registrationDB.lastName,
                            gender = registrationDB.gender,
                            email = registrationDB.email,
                            password = loginDB.password,
                            userAvatar = registrationDB.userAvatar,
                            accesstoken = new JwtSecurityTokenHandler().WriteToken(token).ToString(),
                            refreshtoken = refreshToken
                        };

                        _memberContext.MemberDBs.Add(member);
                        await _memberContext.SaveChangesAsync();
                    }
                    else
                    {
                        membercheck.accesstoken = new JwtSecurityTokenHandler().WriteToken(token).ToString();
                        membercheck.refreshtoken = refreshToken;
                        await _memberContext.SaveChangesAsync();
                    }

                    // accesstoken, refreshtoken provide from client
                    return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token), RefreshToken = refreshToken });
                }

            }
			else
			{
				return BadRequest("이메일 형식에 맞추세요.");
			}
        }
        // refreshtoken to accesstoken reprovide
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {  
            if (RefreshTokens.Contains(request.RefreshToken))
            {
                var user = _registrationContext.Registrations.FirstOrDefault(r => r.email == request.Email);
                // Generate a new access token
                var token = GenerateAccessToken(user.email, user.firstName+user.lastName);

                // Return the new access token to the client
                return Ok(new { AccessToken = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return BadRequest("Invalid refresh token");
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<MemberDB>> Logout([FromQuery] string refresh)
        {
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var AccessToeken = _memberContext.MemberDBs.SingleOrDefault(r => r.accesstoken == token);
            var RefreshToken = _memberContext.MemberDBs.SingleOrDefault(r => r.refreshtoken == refresh);
           
            if (token != null && AccessToeken != null && RefreshToken != null)
            {
                
                AccessToeken.accesstoken = null;
                RefreshToken.refreshtoken = null;

                await _memberContext.SaveChangesAsync();

                return Ok(new { Message = "Logged out successfully" });
            }

            return BadRequest(new { Message = "Invalid token" });
        }
        private JwtSecurityToken GenerateAccessToken(string userEmail, string userName)
        {
            // create user claim
            var claims = new List<Claim>
        {
            new Claim("email", userEmail),
            new Claim("name", userName)
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
