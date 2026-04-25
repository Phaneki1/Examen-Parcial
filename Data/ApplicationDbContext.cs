using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Examen_Parcial.Models;

namespace Examen_Parcial.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<SolicitudCredito> Solicitudes { get; set; }
}
