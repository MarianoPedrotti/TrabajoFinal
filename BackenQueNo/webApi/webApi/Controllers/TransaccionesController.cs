using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webApi.Data;
using webApi.Models;
using webApi.Service;

[ApiController]
[Route("[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CriptoYaService _criptoService;

    public TransactionsController(AppDbContext context, CriptoYaService criptoService)
    {
        _context = context;
        _criptoService = criptoService;
    }

    [HttpPost]
    public async Task<IActionResult> CrearTransaccion([FromBody] Transaccion transaccion)
    {
        if (transaccion.Cantidad <= 0)
            return BadRequest("La cantidad debe ser mayor a 0.");

        if (transaccion.Tipo != "purchase" && transaccion.Tipo != "sale")
            return BadRequest("Tipo de transacción inválido. Debe ser 'purchase' o 'sale'.");

        var moneda = await _context.Monedas.FindAsync(transaccion.MonedaId);
        if (moneda == null)
            return NotFound("Moneda no encontrada.");

        var usuario = await _context.Usuarios.FindAsync(transaccion.UsuarioId);
        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        decimal cotizacion = await _criptoService.ObtenerCotizacion(moneda.Abreviatura);
        transaccion.Cotizacion = cotizacion;
        transaccion.MontoTotal = cotizacion * transaccion.Cantidad;

        if (transaccion.Tipo == "purchase")
        {
            if (usuario.PesosArg < transaccion.MontoTotal)
                return BadRequest("Saldo en pesos insuficiente.");

            usuario.PesosArg -= transaccion.MontoTotal;

            switch (moneda.Abreviatura)
            {
                case "BTC": usuario.BTC += transaccion.Cantidad; break;
                case "ETH": usuario.ETH += transaccion.Cantidad; break;
                case "USDS": usuario.USDS += transaccion.Cantidad; break;
                default: return BadRequest("Moneda no válida para compra.");
            }
        }
        else if (transaccion.Tipo == "sale")
        {
            bool saldoSuficiente = moneda.Abreviatura switch
            {
                "BTC" => usuario.BTC >= transaccion.Cantidad,
                "ETH" => usuario.ETH >= transaccion.Cantidad,
                "USDS" => usuario.USDS >= transaccion.Cantidad,
                _ => false
            };

            if (!saldoSuficiente)
                return BadRequest("No tienes suficiente saldo en esa criptomoneda.");

            usuario.PesosArg += transaccion.MontoTotal;

            switch (moneda.Abreviatura)
            {
                case "BTC": usuario.BTC -= transaccion.Cantidad; break;
                case "ETH": usuario.ETH -= transaccion.Cantidad; break;
                case "USDS": usuario.USDS -= transaccion.Cantidad; break;
                default: return BadRequest("Moneda no válida para venta.");
            }
        }

        transaccion.Fecha = DateTime.UtcNow;

        _context.Transacciones.Add(transaccion);
        await _context.SaveChangesAsync();

        return Ok(transaccion);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTransacciones([FromQuery] int usuarioId)
    {
        var transacciones = await _context.Transacciones
            .Include(t => t.Moneda)
            .Where(t => t.UsuarioId == usuarioId)
            .OrderByDescending(t => t.Fecha)
            .ToListAsync();

        return Ok(transacciones);
    }

    [HttpGet("saldo")]
    public async Task<IActionResult> ObtenerSaldo([FromQuery] int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null) return NotFound("Usuario no encontrado.");

        var monedas = await _context.Monedas.ToListAsync();
        decimal saldoCriptoEnPesos = 0;

        foreach (var moneda in monedas)
        {
            decimal cotizacion = await _criptoService.ObtenerCotizacion(moneda.Abreviatura);
            saldoCriptoEnPesos += moneda.Abreviatura switch
            {
                "BTC" => usuario.BTC * cotizacion,
                "ETH" => usuario.ETH * cotizacion,
                "USDS" => usuario.USDS * cotizacion,
                _ => 0
            };
        }

        var saldoTotal = usuario.PesosArg + saldoCriptoEnPesos;

        return Ok(new
        {
            usuarioId = usuario.Id,
            nombre = usuario.Nombre,
            saldoPesos = usuario.PesosArg,
            saldoCriptoEnPesos,
            saldoTotal
        });
    }

    [HttpGet("estado")]
    public async Task<IActionResult> ObtenerEstadoActual([FromQuery] int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null) return NotFound("Usuario no encontrado.");

        var monedas = await _context.Monedas.ToListAsync();
        var estado = new List<object>();

        foreach (var moneda in monedas)
        {
            decimal cotizacion = await _criptoService.ObtenerCotizacion(moneda.Abreviatura);
            decimal cantidad = moneda.Abreviatura switch
            {
                "BTC" => usuario.BTC,
                "ETH" => usuario.ETH,
                "USDS" => usuario.USDS,
                _ => 0
            };

            estado.Add(new
            {
                Moneda = moneda.Abreviatura,
                Nombre = moneda.Nombre,
                Cantidad = Math.Round(cantidad, 6),
                Cotizacion = cotizacion,
                TotalEnPesos = Math.Round(cantidad * cotizacion, 2)
            });
        }

        return Ok(estado);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerTransaccionPorId(int id, [FromQuery] int usuarioId)
    {
        var transaccion = await _context.Transacciones
            .Include(t => t.Moneda)
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaccion == null)
            return NotFound("Transacción no encontrada.");

        if (transaccion.UsuarioId != usuarioId)
            return Unauthorized("No tenés permiso para ver esta transacción.");

        return Ok(new
        {
            transaccion.Id,
            transaccion.Fecha,
            transaccion.Tipo,
            transaccion.Cantidad,
            transaccion.Cotizacion,
            transaccion.MontoTotal,
            Moneda = transaccion.Moneda?.Abreviatura,
            Usuario = new
            {
                transaccion.Usuario?.Id,
                transaccion.Usuario?.Nombre,
                transaccion.Usuario?.Email
            }
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarTransaccion(int id, [FromQuery] int usuarioId)
    {
        var transaccion = await _context.Transacciones.FindAsync(id);

        if (transaccion == null)
            return NotFound("Transacción no encontrada.");

        if (transaccion.UsuarioId != usuarioId)
            return Unauthorized("No tenés permiso para eliminar esta transacción.");

        _context.Transacciones.Remove(transaccion);
        await _context.SaveChangesAsync();

        return Ok("Transacción eliminada correctamente.");
    }
}
