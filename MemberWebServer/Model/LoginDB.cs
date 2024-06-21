using System.ComponentModel.DataAnnotations;

namespace MemberWebServer.Model
{
	public class LoginDB
	{
		[Key]
		public string? email { get; set; }
		public string? password { get; set; }
	}
}
