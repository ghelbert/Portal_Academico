using Microsoft.AspNetCore.Identity;
using Portal_Academico.Models;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<Usuario> userManager, RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager)
    {
        var user = await userManager.FindByEmailAsync("gustavo@hotmail.com");
        var coordinador = await userManager.FindByEmailAsync("coordinador@hotmail.com");

        // Asegurar que exista el rol Coordinador
        var roleName = "Coordinador";
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(roleName));
            Console.WriteLine("Rol 'Coordinador' creado.");
        }
        Console.WriteLine($"Usuario encontrado: {user != null}");

        if (user == null)
        {
            Console.WriteLine("Creando usuario...");
            user = new Usuario
            {
                UserName = "Gustavo",  // Cambia a esto
                Email = "gustavo@hotmail.com",
                Rol = "Usuario",
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, "@Gustavo123");
            if (result.Succeeded)
            {
                Console.WriteLine("Usuario creado exitosamente.");
            }
            else
            {
                Console.WriteLine("Errores al crear: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            Console.WriteLine("Usuario ya existe, no se crea.");
        }

        if (coordinador == null)
        {
            Console.WriteLine("Creando Coordinador...");
            coordinador = new Usuario
            {
                UserName = "coordi@hotmail.com",  // Cambia a esto
                Email = "coordi@hotmail.com",
                Rol = "Coordinador",
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(coordinador, "@Coordi123");
            if (result.Succeeded)
            {
                Console.WriteLine("Coordinador creado exitosamente.");
                // asignar rol
                await userManager.AddToRoleAsync(coordinador, roleName);
            }
            else
            {
                Console.WriteLine("Errores al crear: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            Console.WriteLine("Coordinador ya existe, no se crea.");
            // asegurar que tenga el rol
            if (!await userManager.IsInRoleAsync(coordinador, roleName))
            {
                await userManager.AddToRoleAsync(coordinador, roleName);
            }
        }
    }
}
