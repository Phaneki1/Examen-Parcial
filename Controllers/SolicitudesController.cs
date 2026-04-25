using Examen_Parcial.Data;
using Examen_Parcial.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Examen_Parcial.Controllers;

[Authorize]
public class SolicitudesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public SolicitudesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Solicitudes
    public async Task<IActionResult> Index(SolicitudFilterViewModel filter)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        // Buscar el cliente asociado al usuario logueado
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
        if (cliente == null)
        {
            // Si el usuario no tiene un perfil de cliente, retornamos una lista vacía
            return View(filter);
        }

        // Si el modelo no es válido por reglas de filtros mal enviados (ej. montos negativos),
        // devolvemos la vista para mostrar los mensajes de error.
        if (!ModelState.IsValid)
        {
            return View(filter);
        }

        // IQueryable para construir la consulta dinámicamente y asegurar que
        // SOLO vea sus propias solicitudes.
        var query = _context.Solicitudes
            .Include(s => s.Cliente)
            .Where(s => s.ClienteId == cliente.Id)
            .AsQueryable();

        // 1. Filtro por Estado
        if (filter.Estado.HasValue)
        {
            query = query.Where(s => s.Estado == filter.Estado.Value);
        }

        // 2. Filtros por Rango de Montos
        if (filter.MontoMinimo.HasValue)
        {
            query = query.Where(s => s.MontoSolicitado >= filter.MontoMinimo.Value);
        }

        if (filter.MontoMaximo.HasValue)
        {
            query = query.Where(s => s.MontoSolicitado <= filter.MontoMaximo.Value);
        }

        // 3. Filtros por Rango de Fechas
        if (filter.FechaInicio.HasValue)
        {
            query = query.Where(s => s.FechaSolicitud.Date >= filter.FechaInicio.Value.Date);
        }

        if (filter.FechaFin.HasValue)
        {
            query = query.Where(s => s.FechaSolicitud.Date <= filter.FechaFin.Value.Date);
        }

        // Ejecutar consulta y ordenar
        filter.Solicitudes = await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync();

        return View(filter);
    }

    // GET: Solicitudes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
        if (cliente == null)
        {
            return NotFound();
        }

        // Aseguramos que solo pueda ver el detalle si la solicitud le pertenece (ClienteId)
        var solicitud = await _context.Solicitudes
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(m => m.Id == id && m.ClienteId == cliente.Id);

        if (solicitud == null)
        {
            return NotFound();
        }

        return View(solicitud);
    }
}
