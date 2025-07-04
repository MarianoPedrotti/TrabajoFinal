
namespace webApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using webApi.Data;
    using webApi.Models;

    [ApiController]
    [Route("[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Test()
        {
            return Ok("API de usuarios funcionando");
        }

        // POST /usuarios - Registrar usuario
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody]Usuario nuevoUsuario)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == nuevoUsuario.Email))
                return BadRequest("Ya existe un usuario con ese email.");

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();
            return Ok("Usuario registrado con éxito.");
        }

        // POST /usuarios/login - Login básico
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest datos)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Historial)
                .ThenInclude(t => t.Moneda)
                .FirstOrDefaultAsync(u => u.Email == datos.Email && u.Password == datos.Password);

            if (usuario == null)
                return Unauthorized("Email o contraseña incorrectos.");

            return Ok(new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.PesosArg,
                usuario.BTC,
                usuario.ETH,
                usuario.USDS,
                Historial = usuario.Historial.Select(t => new
                {
                    t.Id,
                    t.Tipo,
                    t.Fecha,
                    t.Cantidad,
                    t.Cotizacion,
                    t.MontoTotal,
                    Moneda = t.Moneda != null ? t.Moneda.Abreviatura : null
                })
            });
        }

        // GET /usuarios/{id} - Obtener datos del usuario
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        // GET /usuarios/{id}/historial - Obtener historial de transacciones
        [HttpGet("{id}/historial")]
        public async Task<IActionResult> ObtenerHistorial(int id)
        {
            var historial = await _context.Transacciones
                .Include(t => t.Moneda)
                .Where(t => t.UsuarioId == id)
                .OrderByDescending(t => t.Fecha)
                .ToListAsync();

            return Ok(historial.Select(t => new
            {
                t.Id,
                t.Tipo,
                t.Fecha,
                t.Cantidad,
                t.Cotizacion,
                t.MontoTotal,
                Moneda = t.Moneda != null ? t.Moneda.Abreviatura : null
            }));
        }

        // PUT /usuarios/{id} - Actualizar saldo o criptos
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] Usuario datos)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            usuario.PesosArg = datos.PesosArg;
            usuario.BTC = datos.BTC;
            usuario.ETH = datos.ETH;
            usuario.USDS = datos.USDS;

            await _context.SaveChangesAsync();
            return Ok("Usuario actualizado.");
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

}
