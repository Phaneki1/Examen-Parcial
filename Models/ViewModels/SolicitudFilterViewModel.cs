using System.ComponentModel.DataAnnotations;

namespace Examen_Parcial.Models.ViewModels;

public class SolicitudFilterViewModel : IValidatableObject
{
    public EstadoSolicitud? Estado { get; set; }

    [Display(Name = "Monto Mínimo")]
    public decimal? MontoMinimo { get; set; }

    [Display(Name = "Monto Máximo")]
    public decimal? MontoMaximo { get; set; }

    [Display(Name = "Fecha Inicio")]
    [DataType(DataType.Date)]
    public DateTime? FechaInicio { get; set; }

    [Display(Name = "Fecha Fin")]
    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }

    // Propiedad para enviar las solicitudes filtradas a la vista
    public List<SolicitudCredito> Solicitudes { get; set; } = new List<SolicitudCredito>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // 1. No aceptar montos negativos
        if (MontoMinimo.HasValue && MontoMinimo.Value < 0)
        {
            yield return new ValidationResult("El monto mínimo no puede ser negativo.", new[] { nameof(MontoMinimo) });
        }

        if (MontoMaximo.HasValue && MontoMaximo.Value < 0)
        {
            yield return new ValidationResult("El monto máximo no puede ser negativo.", new[] { nameof(MontoMaximo) });
        }

        if (MontoMinimo.HasValue && MontoMaximo.HasValue && MontoMinimo > MontoMaximo)
        {
            yield return new ValidationResult("El monto mínimo no puede ser mayor que el máximo.", new[] { nameof(MontoMinimo), nameof(MontoMaximo) });
        }

        // 2. No aceptar rangos de fechas inválidos
        if (FechaInicio.HasValue && FechaFin.HasValue && FechaInicio.Value > FechaFin.Value)
        {
            yield return new ValidationResult("La fecha de inicio no puede ser mayor a la fecha de fin.", new[] { nameof(FechaInicio), nameof(FechaFin) });
        }
    }
}
