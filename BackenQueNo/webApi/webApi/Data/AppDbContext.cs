using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using webApi.Models;

namespace webApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Moneda> Monedas { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
