
using Microsoft.EntityFrameworkCore;
using DEMO.Models;
namespace DEMO.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
       
        public DbSet<Customer> Customers { get; set; }
    }
}


