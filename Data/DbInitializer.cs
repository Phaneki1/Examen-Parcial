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

        // 3. Clientes (Usuarios Identity y Entidad de Dominio)
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

        var clienteUser2 = await userManager.FindByEmailAsync("cliente2@banco.com");
        if (clienteUser2 == null)
        {
            clienteUser2 = new IdentityUser { UserName = "cliente2@banco.com", Email = "cliente2@banco.com", EmailConfirmed = true };
            await userManager.CreateAsync(clienteUser2, "Password123!");

            context.Clientes.Add(new Cliente 
            { 
                UsuarioId = clienteUser2.Id, 
                IngresosMensuales = 5000, 
                Activo = true 
            });
        }

        await context.SaveChangesAsync(); // Guardamos los clientes antes de insertar las solicitudes

        // 4. Solicitudes (1 Pendiente, 1 Aprobada)
        if (!await context.Solicitudes.AnyAsync())
        {
            var cl1 = await context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == clienteUser1.Id);
            var cl2 = await context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == clienteUser2.Id);

            if (cl1 != null)
            {
                // Pendiente (no mayor a 5 veces sus ingresos de 2000 = 10000 max)
                context.Solicitudes.Add(new SolicitudCredito
                {
                    ClienteId = cl1.Id,
                    MontoSolicitado = 3000,
                    Estado = EstadoSolicitud.Pendiente,
                    FechaSolicitud = DateTime.UtcNow
                });
            }

            if (cl2 != null)
            {
                // Aprobada (no mayor a 5 veces sus ingresos de 5000 = 25000 max)
                context.Solicitudes.Add(new SolicitudCredito
                {
                    ClienteId = cl2.Id,
                    MontoSolicitado = 10000,
                    Estado = EstadoSolicitud.Aprobado,
                    FechaSolicitud = DateTime.UtcNow.AddDays(-1)
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
