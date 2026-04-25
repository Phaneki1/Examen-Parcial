using Examen_Parcial.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Examen_Parcial.Controllers;

[Authorize(Roles = "Analista")]
public class AnalistaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public AnalistaController(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET: Analista
    public async Task<IActionResult> Index()
    {
        // Traer todas las solicitudes pendientes incluyendo datos del cliente y su usuario Identity
        var solicitudesPendientes = await _context.Solicitudes
            .Include(s => s.Cliente)
            .ThenInclude(c => c!.Usuario)
            .Where(s => s.Estado == Models.EstadoSolicitud.Pendiente)
            .OrderBy(s => s.FechaSolicitud)
            .ToListAsync();

        return View(solicitudesPendientes);
    }

    // POST: Analista/Aprobar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        var solicitud = await _context.Solicitudes
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null) return NotFound();

        // Regla: No procesar solicitudes ya aprobadas o rechazadas
        if (solicitud.Estado != Models.EstadoSolicitud.Pendiente)
        {
            TempData["Error"] = "La solicitud ya ha sido procesada previamente.";
            return RedirectToAction(nameof(Index));
        }

        // Regla: No aprobar si el monto excede 5 veces los ingresos
        var maximoAprobable = solicitud.Cliente!.IngresosMensuales * 5;
        if (solicitud.MontoSolicitado > maximoAprobable)
        {
            TempData["Error"] = $"No se puede aprobar. El monto ({solicitud.MontoSolicitado:C}) supera 5 veces los ingresos del cliente ({maximoAprobable:C}).";
            return RedirectToAction(nameof(Index));
        }

        solicitud.Estado = Models.EstadoSolicitud.Aprobado;
        await _context.SaveChangesAsync();

        // Requerimiento Pregunta 4: Invalidar caché del cliente cuando cambia el estado
        await _cache.RemoveAsync($"solicitudes_cache_{solicitud.ClienteId}");

        TempData["Success"] = $"Solicitud #{id} aprobada con éxito.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Analista/Rechazar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(Examen_Parcial.Models.ViewModels.RechazarSolicitudViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "El motivo de rechazo es obligatorio y debe tener mínimo 10 caracteres.";
            return RedirectToAction(nameof(Index));
        }

        var solicitud = await _context.Solicitudes
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId);

        if (solicitud == null) return NotFound();

        // Regla: No procesar solicitudes ya aprobadas o rechazadas
        if (solicitud.Estado != Models.EstadoSolicitud.Pendiente)
        {
            TempData["Error"] = "La solicitud ya ha sido procesada previamente.";
            return RedirectToAction(nameof(Index));
        }

        solicitud.Estado = Models.EstadoSolicitud.Rechazado;
        solicitud.MotivoRechazo = model.MotivoRechazo;
        
        await _context.SaveChangesAsync();

        // Requerimiento Pregunta 4: Invalidar caché del cliente cuando cambia el estado
        await _cache.RemoveAsync($"solicitudes_cache_{solicitud.ClienteId}");

        TempData["Success"] = $"Solicitud #{model.SolicitudId} rechazada exitosamente.";
        return RedirectToAction(nameof(Index));
    }
}
