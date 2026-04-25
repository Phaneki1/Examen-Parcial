using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Examen_Parcial.Models;

public class Cliente
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [ForeignKey("UsuarioId")]
    public virtual IdentityUser? Usuario { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Los ingresos mensuales deben ser mayores a 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal IngresosMensuales { get; set; }

    public bool Activo { get; set; } = true;

    // Relación
    public virtual ICollection<SolicitudCredito> SolicitudesCredito { get; set; } = new List<SolicitudCredito>();
}
