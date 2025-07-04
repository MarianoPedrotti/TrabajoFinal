using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Final.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int MonedaId { get; set; }
        [JsonIgnore]
        public Moneda? Moneda { get; set; }

        public decimal MontoTotal { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }
        public decimal Cotizacion { get; set; }
        public DateTime Fecha { get; set; }

        public string Tipo { get; set; }
    }
}
