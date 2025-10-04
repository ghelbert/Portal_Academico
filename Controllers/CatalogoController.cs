using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Portal_Academico.Data;
using Portal_Academico.Models;

namespace Portal_Academico.Controllers;

public class CatalogoController : Controller
{
    private readonly ILogger<CatalogoController> _logger;

    private readonly AppDbContext _dbContext; // Contexto de la base de datos

    public CatalogoController(ILogger<CatalogoController> logger, AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public IActionResult Index([FromQuery] CatalogoViewModel filters)
    {
        var viewModel = filters ?? new CatalogoViewModel();

        try
        {
            // Validaciones server-side
            if (viewModel.MinCreditos.HasValue && viewModel.MinCreditos < 0)
                ModelState.AddModelError(nameof(viewModel.MinCreditos), "No se aceptan créditos negativos.");
            if (viewModel.MaxCreditos.HasValue && viewModel.MaxCreditos < 0)
                ModelState.AddModelError(nameof(viewModel.MaxCreditos), "No se aceptan créditos negativos.");
            if (viewModel.MinCreditos.HasValue && viewModel.MaxCreditos.HasValue && viewModel.MinCreditos > viewModel.MaxCreditos)
                ModelState.AddModelError(string.Empty, "El rango de créditos es inválido (Min > Max).");
            if (viewModel.HorarioInicioFiltro.HasValue && viewModel.HorarioFinFiltro.HasValue
                && viewModel.HorarioFinFiltro < viewModel.HorarioInicioFiltro)
                ModelState.AddModelError(string.Empty, "Horario Fin no puede ser anterior a Horario Inicio.");

            // Iniciar consulta desde la base de datos y sólo cursos activos
            var query = _dbContext.Cursos.AsQueryable();
            query = query.Where(c => c.Activo == "true");

            // Aplicar filtros si se proporcionan
            if (!string.IsNullOrEmpty(viewModel.NombreFiltro))
                query = query.Where(c => c.Nombre.Contains(viewModel.NombreFiltro));
            if (viewModel.MinCreditos.HasValue)
                query = query.Where(c => c.Creditos >= viewModel.MinCreditos.Value);
            if (viewModel.MaxCreditos.HasValue)
                query = query.Where(c => c.Creditos <= viewModel.MaxCreditos.Value);

            // Horarios en la entidad están como strings "HH:mm"; EF no puede traducir TryParse,
            // así que si hay filtros de horario, materializamos la consulta y aplicamos el filtrado en memoria.
            if (viewModel.HorarioInicioFiltro.HasValue || viewModel.HorarioFinFiltro.HasValue)
            {
                var lista = query.ToList();
                if (viewModel.HorarioInicioFiltro.HasValue)
                {
                    var inicioFiltro = viewModel.HorarioInicioFiltro.Value;
                    lista = lista.Where(c => TimeSpan.TryParse(c.HorarioInicio, out var hi) && hi >= inicioFiltro).ToList();
                }
                if (viewModel.HorarioFinFiltro.HasValue)
                {
                    var finFiltro = viewModel.HorarioFinFiltro.Value;
                    lista = lista.Where(c => TimeSpan.TryParse(c.HorarioFin, out var hf) && hf <= finFiltro).ToList();
                }
                viewModel.Cursos = lista;
            }
            else
            {
                // No hay filtros de horario -> ejecutar en BD
                viewModel.Cursos = query.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los cursos de la base de datos.");
            viewModel.ErrorMessage = "Ocurrió un error al cargar los cursos. Por favor, intenta de nuevo.";
        }
        return View(viewModel);
    }

    // GET: /Catalogo/Details/5
    public IActionResult Details(int id)
    {
        var curso = _dbContext.Cursos.FirstOrDefault(c => c.Id == id && c.Activo == "true");
        if (curso == null) return NotFound();
        return View(curso);
    }

    // POST: /Catalogo/Inscribirse/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Inscribirse(int id)
    {
        var curso = _dbContext.Cursos.FirstOrDefault(c => c.Id == id && c.Activo == "true");
        if (curso == null) return NotFound();

        // Crear una matrícula simple. Si no hay autenticación, usamos 'demo-user'
    var userId = User?.Identity?.IsAuthenticated == true ? (User.Identity.Name ?? "demo-user") : "demo-user";

        var matricula = new Matricula
        {
            CursoId = curso.Id,
            UsuarioId = userId,
            FechaRegistro = DateTime.UtcNow.ToString("o"),
            Estado = EstadoMatricula.Pendiente
        };

        _dbContext.Matriculas.Add(matricula);
        _dbContext.SaveChanges();

        TempData["SuccessMessage"] = "Inscripción registrada correctamente.";
        return RedirectToAction(nameof(Details), new { id = curso.Id });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
