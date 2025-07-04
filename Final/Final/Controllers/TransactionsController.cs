using Final.Data;
using Final.Models;
using Final.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Final.Controllers
{

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

            if (transaccion.Fecha == default)
                transaccion.Fecha = DateTime.Now;
            else if (transaccion.Fecha > DateTime.Now)
                return BadRequest("La fecha no puede ser en el futuro.");

            decimal cotizacion = await _criptoService.ObtenerCotizacion(moneda.Abreviatura);

            transaccion.Cotizacion = cotizacion;
            transaccion.MontoTotal = cotizacion * transaccion.Cantidad;


            if (transaccion.Tipo == "purchase")
            {
                var compras = await _context.Transacciones
                    .Where(t => t.Tipo == "purchase")
                    .SumAsync(t => t.MontoTotal);

                var ventas = await _context.Transacciones
                    .Where(t => t.Tipo == "sale")
                    .SumAsync(t => t.MontoTotal);

                decimal saldoInicial = 1000;
                decimal saldoActual = saldoInicial - compras + ventas;

                if (saldoActual < transaccion.MontoTotal)
                    return BadRequest("Saldo insuficiente.");
            }


            if (transaccion.Tipo == "sale")
            {
                var cantidadComprada = await _context.Transacciones
                    .Where(t => t.MonedaId == transaccion.MonedaId && t.Tipo == "purchase")
                    .SumAsync(t => t.Cantidad);

                var cantidadVendida = await _context.Transacciones
                    .Where(t => t.MonedaId == transaccion.MonedaId && t.Tipo == "sale")
                    .SumAsync(t => t.Cantidad);

                if ((cantidadComprada - cantidadVendida) < transaccion.Cantidad)
                    return BadRequest("No tienes suficiente cantidad para vender.");
            }

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return Ok(transaccion);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerTransacciones()
        {
            var transacciones = await _context.Transacciones
                .Include(t => t.Moneda)
                .OrderByDescending(t => t.Fecha)
                .Select(t => new
                {
                    id = t.Id,
                    crypto_code = t.Moneda.Abreviatura,
                    crypto_amount = t.Cantidad,
                    money = t.MontoTotal,
                    action = t.Tipo,
                    datetime = t.Fecha
                })
                .ToListAsync();

            return Ok(transacciones);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerTransaccion(int id)
        {
            var transaccion = await _context.Transacciones
                .Include(t => t.Moneda)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaccion == null)
                return NotFound("Transacción no encontrada.");

            return Ok(new
            {
                id = transaccion.Id,
                crypto_code = transaccion.Moneda.Abreviatura,
                crypto_amount = transaccion.Cantidad,
                money = transaccion.MontoTotal,
                action = transaccion.Tipo,
                datetime = transaccion.Fecha
            });
        }


        [HttpGet("estado")]
        public async Task<IActionResult> ObtenerEstadoActual()
        {
            var monedas = await _context.Monedas.ToListAsync();
            var resultado = new List<object>();
            decimal total = 0;

            foreach (var moneda in monedas)
            {
                var compradas = await _context.Transacciones
                    .Where(t => t.MonedaId == moneda.Id && t.Tipo == "purchase")
                    .SumAsync(t => t.Cantidad);

                var vendidas = await _context.Transacciones
                    .Where(t => t.MonedaId == moneda.Id && t.Tipo == "sale")
                    .SumAsync(t => t.Cantidad);

                var saldo = compradas - vendidas;

                if (saldo > 0)
                {
                    decimal cotizacion = await _criptoService.ObtenerCotizacion(moneda.Abreviatura);
                    decimal valorEnPesos = saldo * cotizacion;
                    total += valorEnPesos;

                    resultado.Add(new
                    {
                        moneda = moneda.Nombre,
                        cantidad = saldo,
                        valor = Math.Round(valorEnPesos, 2)
                    });
                }
            }

            return Ok(new
            {
                resumen = resultado,
                total = Math.Round(total, 2)
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarTransaccion(int id)
        {
            var transaccion = await _context.Transacciones.FindAsync(id);

            if (transaccion == null)
                return NotFound("Transacción no encontrada.");

            _context.Transacciones.Remove(transaccion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("saldo")]
        public async Task<IActionResult> ObtenerSaldo()
        {
            var compras = await _context.Transacciones
                .Where(t => t.Tipo == "purchase")
                .SumAsync(t => t.MontoTotal);

            var ventas = await _context.Transacciones
                .Where(t => t.Tipo == "sale")
                .SumAsync(t => t.MontoTotal);

            decimal saldoInicial = 1000;
            decimal saldoActual = saldoInicial - compras + ventas;

            return Ok(new
            {
                saldoARS = Math.Round(saldoActual, 2)
            });
        }
    }

}