using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Portal_Academico.Data;
using Portal_Academico.Models;

namespace Portal_Academico.Controllers;

public class CatalogoController : Controller
{
    private readonly ILogger<CatalogoController> _logger;

    private readonly AppDbContext _dbContext; // Contexto de la base de datos
    private readonly Microsoft.AspNetCore.Identity.UserManager<Portal_Academico.Models.Usuario> _userManager;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

    public CatalogoController(ILogger<CatalogoController> logger, AppDbContext dbContext, Microsoft.AspNetCore.Identity.UserManager<Portal_Academico.Models.Usuario> userManager, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
        _logger = logger;
        _dbContext = dbContext;
        _userManager = userManager;
        _cache = cache;
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

            // Usar cache distribuida para el listado de cursos activos (60s)
            var cacheKey = "catalogo:cursos:activos";
            List<Curso> cursosActivos;
            try
            {
                var cachedBytes = _cache.GetAsync(cacheKey).GetAwaiter().GetResult();
                if (cachedBytes != null && cachedBytes.Length > 0)
                {
                    var json = System.Text.Encoding.UTF8.GetString(cachedBytes);
                    var cachedDto = System.Text.Json.JsonSerializer.Deserialize<List<CursoCacheDto>>(json) ?? new List<CursoCacheDto>();
                    // Reconstruir entidades Curso ligeras para la vista (no trackeadas)
                    cursosActivos = cachedDto.Select(d => new Curso
                    {
                        Id = d.Id,
                        Codigo = d.Codigo,
                        Nombre = d.Nombre,
                        Creditos = d.Creditos,
                        CupoMaximo = d.CupoMaximo,
                        HorarioInicio = d.HorarioInicio,
                        HorarioFin = d.HorarioFin,
                        Activo = "true"
                    }).ToList();
                }
                else
                {
                    var query = _dbContext.Cursos.Where(c => c.Activo == "true");
                    cursosActivos = query.ToList();
                    var dto = cursosActivos.Select(c => new CursoCacheDto
                    {
                        Id = c.Id,
                        Codigo = c.Codigo,
                        Nombre = c.Nombre,
                        Creditos = c.Creditos,
                        CupoMaximo = c.CupoMaximo,
                        HorarioInicio = c.HorarioInicio,
                        HorarioFin = c.HorarioFin
                    }).ToList();
                    var json = System.Text.Json.JsonSerializer.Serialize(dto);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    var options = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                    };
                    _cache.SetAsync(cacheKey, bytes, options).GetAwaiter().GetResult();
                }
            }
            catch (Exception exCache)
            {
                _logger.LogWarning(exCache, "No se pudo usar la cache distribuida, se procederá a consultar la BD.");
                var query = _dbContext.Cursos.Where(c => c.Activo == "true");
                cursosActivos = query.ToList();
            }

            // Aplicar filtros si se proporcionan sobre la lista materializada
            var filtered = cursosActivos.AsQueryable();
            if (!string.IsNullOrEmpty(viewModel.NombreFiltro))
                filtered = filtered.Where(c => c.Nombre != null && c.Nombre.Contains(viewModel.NombreFiltro));
            if (viewModel.MinCreditos.HasValue)
                filtered = filtered.Where(c => c.Creditos >= viewModel.MinCreditos.Value);
            if (viewModel.MaxCreditos.HasValue)
                filtered = filtered.Where(c => c.Creditos <= viewModel.MaxCreditos.Value);

            if (viewModel.HorarioInicioFiltro.HasValue || viewModel.HorarioFinFiltro.HasValue)
            {
                var lista = filtered.ToList();
                if (viewModel.HorarioInicioFiltro.HasValue)
                {
                    var inicioFiltro = viewModel.HorarioInicioFiltro.Value;
                    lista = lista.Where(c => c.HorarioInicio != null && TimeSpan.TryParse(c.HorarioInicio, out var hi) && hi >= inicioFiltro).ToList();
                }
                if (viewModel.HorarioFinFiltro.HasValue)
                {
                    var finFiltro = viewModel.HorarioFinFiltro.Value;
                    lista = lista.Where(c => c.HorarioFin != null && TimeSpan.TryParse(c.HorarioFin, out var hf) && hf <= finFiltro).ToList();
                }
                viewModel.Cursos = lista;
            }
            else
            {
                viewModel.Cursos = filtered.ToList();
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
        // Guardar en sesión el último curso visitado (id y nombre)
        try
        {
            HttpContext.Session.SetString("UltimoCursoId", curso.Id.ToString());
            HttpContext.Session.SetString("UltimoCursoNombre", curso.Nombre ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "No se pudo almacenar sesión de último curso");
        }
        return View(curso);
    }

    // POST: /Catalogo/Inscribirse/5
    [HttpPost]
    [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Inscribirse(int id)
    {
        var curso = _dbContext.Cursos.FirstOrDefault(c => c.Id == id && c.Activo == "true");
        if (curso == null) return NotFound();

            // Obtener/crear usuario
            // El usuario debe estar autenticado para inscribirse
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para inscribirte en un curso.";
                return RedirectToAction(nameof(Details), new { id = curso.Id });
            }

            // Intentar obtener el user id desde las claims (NameIdentifier)
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Portal_Academico.Models.Usuario? user = null;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                user = await _userManager.FindByIdAsync(userIdClaim);
                _logger.LogDebug("Buscar usuario por Id claim: {UserIdClaim} -> {Found}", userIdClaim, user != null);
            }
            if (user == null)
            {
                var userName = User.Identity?.Name ?? string.Empty;
                if (!string.IsNullOrEmpty(userName))
                {
                    user = await _userManager.FindByNameAsync(userName);
                    _logger.LogDebug("Buscar usuario por Name: {UserName} -> {Found}", userName, user != null);
                }
            }
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado. Inicia sesión nuevamente.";
                return RedirectToAction(nameof(Details), new { id = curso.Id });
            }
            var userId = user.Id;

        // Validación: el usuario no puede estar matriculado más de una vez en el mismo curso (estado distinto de Cancelada)
            var existe = _dbContext.Matriculas.Any(m => m.CursoId == curso.Id && m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada);
        if (existe)
        {
            TempData["ErrorMessage"] = "Ya estás matriculado en este curso.";
            return RedirectToAction(nameof(Details), new { id = curso.Id });
        }

        // Validación: no exceder CupoMaximo (contar matrículas activas: Pendiente o Confirmada)
        var inscritos = _dbContext.Matriculas.Count(m => m.CursoId == curso.Id && m.Estado != EstadoMatricula.Cancelada);
        if (curso.CupoMaximo > 0 && inscritos >= curso.CupoMaximo)
        {
            TempData["ErrorMessage"] = "No es posible inscribirse: cupo máximo alcanzado.";
            return RedirectToAction(nameof(Details), new { id = curso.Id });
        }

        // Validación adicional: evitar solapamiento de horarios con otros cursos en los que el usuario ya está matriculado (Pendiente o Confirmada)
        try
        {
            var matr = _dbContext.Matriculas
                .Where(m => m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada)
                .Select(m => m.Curso)
                .ToList();

            if (matr != null && matr.Any())
            {
                TimeSpan curStart, curEnd;
                var parseStart = TimeSpan.TryParse(curso.HorarioInicio, out curStart);
                var parseEnd = TimeSpan.TryParse(curso.HorarioFin, out curEnd);
                bool parseCur = parseStart && parseEnd;
                if (parseCur)
                {
                    foreach (var other in matr)
                    {
                        if (other == null) continue;
                        if (other.Id == curso.Id) continue; // mismo curso ya cubierto
                        if (!TimeSpan.TryParse(other.HorarioInicio, out var oStart) || !TimeSpan.TryParse(other.HorarioFin, out var oEnd))
                        {
                            _logger.LogDebug("No se pudo parsear horario de curso {CursoId}", other.Id);
                            continue;
                        }
                        // Comprobar intersección estricta: start1 < end2 && start2 < end1
                        if (curStart < oEnd && oStart < curEnd)
                        {
                            TempData["ErrorMessage"] = "No puedes inscribirte: el horario se solapa con otro curso en el que estás matriculado.";
                            return RedirectToAction(nameof(Details), new { id = curso.Id });
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("No se pudo parsear horario del curso actual Id={CursoId}", curso.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar solapamiento de horarios para usuario {UserId} y curso {CursoId}", userId, curso.Id);
            // No bloquear inscripción por error de validación de horario; mejor permitir y registrar
        }

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
