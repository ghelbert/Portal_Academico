using Microsoft.AspNetCore.Identity;
using Portal_Academico.Models;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<Usuario> userManager)
    {
        var user = await userManager.FindByEmailAsync("gustavo@hotmail.com");
        Console.WriteLine($"Usuario encontrado: {user != null}");

        if (user == null)
        {
            Console.WriteLine("Creando usuario...");
            user = new Usuario
            {
                UserName = "gustavo@hotmail.com",  // Cambia a esto
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
    }
}
