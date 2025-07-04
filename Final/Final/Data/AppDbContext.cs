using Final.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Final.Data
{
    public class AppDbContext: DbContext
    {
       
        
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

            public DbSet<Moneda> Monedas { get; set; }
            public DbSet<Transaccion> Transacciones { get; set; }
        
    }
}
