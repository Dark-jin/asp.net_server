using Microsoft.EntityFrameworkCore;

namespace MemberWebServer.Model
{
    public class MemberContext : DbContext
    {
        public MemberContext(DbContextOptions<MemberContext> options) : base(options)
        {
        }
        public DbSet<MemberDB> MemberDBs { get; set; } = null!;
    }
}
