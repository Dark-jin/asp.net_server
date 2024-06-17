using Microsoft.EntityFrameworkCore;

namespace MemberWebServer.Model
{
	public class RegistrationContext : DbContext
	{
		public RegistrationContext(DbContextOptions<RegistrationContext> options) : base(options) { }

		public DbSet<RegistrationContext> RegistrationContexts { get; set; } = null!;
	}
}
