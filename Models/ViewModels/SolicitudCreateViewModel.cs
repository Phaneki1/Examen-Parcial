using System.ComponentModel.DataAnnotations;

namespace Examen_Parcial.Models.ViewModels;

public class SolicitudCreateViewModel
{
    [Required(ErrorMessage = "El monto solicitado es obligatorio.")]
    [Display(Name = "Monto a Solicitar")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto solicitado debe ser mayor a 0.")]
    public decimal MontoSolicitado { get; set; }
}
