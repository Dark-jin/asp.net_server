using Microsoft.EntityFrameworkCore;

namespace MemberWebServer.Model
{
	public class LoginContext : DbContext
	{
		public LoginContext(DbContextOptions<LoginContext> options) : base(options)
		{
		}
		public DbSet<LoginDB> LoginDBs { get; set; } = null!;
	}
}
