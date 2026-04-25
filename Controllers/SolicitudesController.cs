using Examen_Parcial.Data;
using Examen_Parcial.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Examen_Parcial.Controllers;

[Authorize]
public class SolicitudesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IDistributedCache _cache;

    public SolicitudesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IDistributedCache cache)
    {
        _context = context;
        _userManager = userManager;
        _cache = cache;
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

        // REDIS CACHE: Buscar solicitudes en caché por 60 segundos
        string cacheKey = $"solicitudes_cache_{cliente.Id}";
        string? cachedData = await _cache.GetStringAsync(cacheKey);
        
        List<Examen_Parcial.Models.SolicitudCredito> todasLasSolicitudes;
        var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };

        if (string.IsNullOrEmpty(cachedData))
        {
            // No está en caché -> Consultar BD
            todasLasSolicitudes = await _context.Solicitudes
                .Include(s => s.Cliente)
                .Where(s => s.ClienteId == cliente.Id)
                .ToListAsync();

            // Guardar en caché
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            };
            
            cachedData = JsonSerializer.Serialize(todasLasSolicitudes, jsonOptions);
            await _cache.SetStringAsync(cacheKey, cachedData, cacheOptions);
        }
        else
        {
            // Leer desde caché
            todasLasSolicitudes = JsonSerializer.Deserialize<List<Examen_Parcial.Models.SolicitudCredito>>(cachedData, jsonOptions) ?? new List<Examen_Parcial.Models.SolicitudCredito>();
        }

        IEnumerable<Examen_Parcial.Models.SolicitudCredito> query = todasLasSolicitudes;

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

        // Ejecutar consulta y ordenar en memoria
        filter.Solicitudes = query.OrderByDescending(s => s.FechaSolicitud).ToList();

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

        // GUARDAR EN SESIÓN LA ÚLTIMA SOLICITUD VISITADA
        HttpContext.Session.SetString("UltimaSolicitudId", solicitud.Id.ToString());
        HttpContext.Session.SetString("UltimaSolicitudMonto", solicitud.MontoSolicitado.ToString("C"));

        return View(solicitud);
    }

    // GET: Solicitudes/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
        if (cliente == null || !cliente.Activo)
        {
            TempData["Error"] = "Tu perfil de cliente no existe o está inactivo. No puedes solicitar créditos.";
            return RedirectToAction(nameof(Index));
        }

        var tienePendiente = await _context.Solicitudes
            .AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == Examen_Parcial.Models.EstadoSolicitud.Pendiente);

        if (tienePendiente)
        {
            TempData["Error"] = "Ya tienes una solicitud de crédito en estado Pendiente. Por favor, espera su evaluación.";
            return RedirectToAction(nameof(Index));
        }

        return View(new SolicitudCreateViewModel());
    }

    // POST: Solicitudes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SolicitudCreateViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == user.Id);
        
        // Validación 1: Cliente debe existir y estar activo
        if (cliente == null || !cliente.Activo)
        {
            ModelState.AddModelError(string.Empty, "Tu perfil de cliente no existe o está inactivo.");
            return View(model);
        }

        // Validación 2: No permitir más de una solicitud Pendiente
        var tienePendiente = await _context.Solicitudes
            .AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == Examen_Parcial.Models.EstadoSolicitud.Pendiente);

        if (tienePendiente)
        {
            ModelState.AddModelError(string.Empty, "Ya tienes una solicitud de crédito en estado Pendiente. No puedes registrar otra simultáneamente.");
            return View(model);
        }

        // Validación 3: El monto no puede superar 10 veces los ingresos mensuales
        var montoMaximoPermitido = cliente.IngresosMensuales * 10;
        if (model.MontoSolicitado > montoMaximoPermitido)
        {
            ModelState.AddModelError(string.Empty, $"El monto solicitado no puede superar 10 veces tus ingresos mensuales (Límite: {montoMaximoPermitido:C}).");
            return View(model);
        }

        if (ModelState.IsValid)
        {
            var nuevaSolicitud = new Examen_Parcial.Models.SolicitudCredito
            {
                ClienteId = cliente.Id,
                MontoSolicitado = model.MontoSolicitado,
                Estado = Examen_Parcial.Models.EstadoSolicitud.Pendiente,
                FechaSolicitud = DateTime.UtcNow
            };

            _context.Solicitudes.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            // REDIS INVALIDATION: Borrar la caché al agregar nueva solicitud
            await _cache.RemoveAsync($"solicitudes_cache_{cliente.Id}");

            TempData["Success"] = "¡Solicitud de crédito registrada con éxito! Entrará en proceso de evaluación.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }
}
