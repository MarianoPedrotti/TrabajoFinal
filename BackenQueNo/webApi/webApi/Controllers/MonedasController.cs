using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using webApi.Data;

namespace webApi.Controllers
{
    
        [ApiController]
        [Route("[controller]")]
        public class MonedasController : ControllerBase
        {
            private readonly AppDbContext _context;

            public MonedasController(AppDbContext context)
            {
                _context = context;
            }


            [HttpGet]
            public async Task<IActionResult> ObtenerMonedas()
            {
                var monedas = await _context.Monedas
                    .Select(m => new
                    {
                        id = m.Id,
                        abreviatura = m.Abreviatura,
                        nombre = m.Nombre
                    })
                    .ToListAsync();

                return Ok(monedas);
            }
        }
    
   }
