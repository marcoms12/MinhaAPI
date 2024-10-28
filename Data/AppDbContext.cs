using Microsoft.EntityFrameworkCore;
using MinhaAPI.Models;

namespace MinhaAPI.Data
{
    
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

        // Define o DbSet na tabela usuario
        public DbSet<User> Users { get; set; }

        // Protege a tabela já existente de modificações
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{       
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>().ToTable("usuario"); 
        modelBuilder.Entity<User>().Property(u => u.Id).HasColumnName("id");
        modelBuilder.Entity<User>().Property(u => u.Name).HasColumnName("Name");
        modelBuilder.Entity<User>().Property(u => u.Email).HasColumnName("Email");
        modelBuilder.Entity<User>().Property(u => u.Password).HasColumnName("Password");
        modelBuilder.Entity<User>().Property(u => u.CreateTime).HasColumnName("create_time");

}
    }

    
    

}

