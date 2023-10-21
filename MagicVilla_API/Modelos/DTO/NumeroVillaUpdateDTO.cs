using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MagicVilla_API.Modelos.DTO
{
    public class NumeroVillaUpdateDTO
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)] // no se asigna automáticamente. El usuario lo maneja
        public int VillaNo { get; set; }
        [Required]
        public int VillaId { get; set; }
        public string DetalleEspecial { get; set; }
    }
}
