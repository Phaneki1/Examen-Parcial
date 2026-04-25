using System.ComponentModel.DataAnnotations;

namespace Examen_Parcial.Models.ViewModels;

public class RechazarSolicitudViewModel
{
    [Required]
    public int SolicitudId { get; set; }

    [Required(ErrorMessage = "El motivo de rechazo es obligatorio.")]
    [MinLength(10, ErrorMessage = "El motivo debe tener al menos 10 caracteres.")]
    public string MotivoRechazo { get; set; } = string.Empty;
}
