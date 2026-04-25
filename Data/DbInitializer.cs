using Examen_Parcial.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Examen_Parcial.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Asegurar que la BD se cree y aplique migraciones
        await context.Database.MigrateAsync();

        // 1. Roles
        if (!await roleManager.RoleExistsAsync("Analista"))
        {
            await roleManager.CreateAsync(new IdentityRole("Analista"));
        }

        // 2. Analista
        if (await userManager.FindByEmailAsync("analista@banco.com") == null)
        {
            var analista = new IdentityUser { UserName = "analista@banco.com", Email = "analista@banco.com", EmailConfirmed = true };
            await userManager.CreateAsync(analista, "Password123!");
            await userManager.AddToRoleAsync(analista, "Analista");
        }

        // 3. Cliente
        var clienteUser1 = await userManager.FindByEmailAsync("cliente1@banco.com");
        if (clienteUser1 == null)
        {
            clienteUser1 = new IdentityUser { UserName = "cliente1@banco.com", Email = "cliente1@banco.com", EmailConfirmed = true };
            await userManager.CreateAsync(clienteUser1, "Password123!");
            
            context.Clientes.Add(new Cliente 
            { 
                UsuarioId = clienteUser1.Id, 
                IngresosMensuales = 2000, 
                Activo = true 
            });
        }

        await context.SaveChangesAsync();

        // 4. Solicitud de ejemplo (1 Pendiente para cliente1)
        if (!await context.Solicitudes.AnyAsync())
        {
            var cl1 = await context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == clienteUser1.Id);

            if (cl1 != null)
            {
                context.Solicitudes.Add(new SolicitudCredito
                {
                    ClienteId = cl1.Id,
                    MontoSolicitado = 3000,
                    Estado = EstadoSolicitud.Pendiente,
                    FechaSolicitud = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();
        }
    }
}

