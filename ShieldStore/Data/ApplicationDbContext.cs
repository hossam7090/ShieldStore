using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShieldStore.Models;

namespace ShieldStore.Data
{
	public class ApplicationDbContext : DbContext
	{
        public ApplicationDbContext(DbContextOptions options) : base(options)
		{
            
        }
		public DbSet<User> Users { get; set; }

	}
}
