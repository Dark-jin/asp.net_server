using Microsoft.EntityFrameworkCore;

namespace MemberWebServer.Model
{
	public class RegistrationContext : DbContext
	{
		public RegistrationContext(DbContextOptions<RegistrationContext> options) : base(options) {}

		public DbSet<Registration> Registrations { get; set; } = null!;

        internal bool FindAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
