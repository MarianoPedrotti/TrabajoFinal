using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrabajoPractico.Models
{
    
    
        [Table("Usuarios")] // asegúrate de esto si la tabla se llama "Usuarios"
        public class Usuario
        {
            [Key]
            public int Id { get; set; }

            public string Nombre { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }

            public decimal PesosArg { get; set; }
            public decimal BTC { get; set; }
            public decimal ETH { get; set; }
            public decimal USDS { get; set; }

            public List<Transaccion>? Historial { get; set; }
        }
    
}
