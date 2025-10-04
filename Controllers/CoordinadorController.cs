using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Portal_Academico.Data;
using Portal_Academico.Models;

namespace Portal_Academico.Controllers;

[Authorize(Roles = "Coordinador")]
public class CoordinadorController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<CoordinadorController> _logger;
    private readonly IDistributedCache _cache;

    public CoordinadorController(AppDbContext db, ILogger<CoordinadorController> logger, IDistributedCache cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
    }

    public IActionResult Index()
    {
        var cursos = _db.Cursos.OrderBy(c => c.Nombre).ToList();
        return View(cursos);
    }

    public IActionResult Create()
    {
        return View(new Curso());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Curso model)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["FormErrors"] = errors;
            return View(model);
        }

        model.Activo = "true";
        _db.Cursos.Add(model);
        _db.SaveChanges();
        try { _cache.Remove("catalogo:cursos:activos"); } catch { }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var curso = _db.Cursos.Find(id);
        if (curso == null) return NotFound();
        return View(curso);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Curso model)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["FormErrors"] = errors;
            return View(model);
        }

        var curso = _db.Cursos.Find(model.Id);
        if (curso == null) return NotFound();
        curso.Codigo = model.Codigo;
        curso.Nombre = model.Nombre;
        curso.Creditos = model.Creditos;
        curso.CupoMaximo = model.CupoMaximo;
        curso.HorarioInicio = model.HorarioInicio;
        curso.HorarioFin = model.HorarioFin;
        // Also persist Activo if changed from the form
        curso.Activo = model.Activo;
        _db.SaveChanges();
        try { _cache.Remove("catalogo:cursos:activos"); } catch { }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActive(int id)
    {
        var curso = _db.Cursos.Find(id);
        if (curso == null) return NotFound();
        curso.Activo = curso.Activo == "true" ? "false" : "true";
        _db.SaveChanges();
        try { _cache.Remove("catalogo:cursos:activos"); } catch { }
        return RedirectToAction(nameof(Index));
    }

    // Lista de matrículas por curso
    public IActionResult Matriculas(int id)
    {
        var curso = _db.Cursos.Find(id);
        if (curso == null) return NotFound();
        var matriculas = _db.Matriculas.Where(m => m.CursoId == id).ToList();
        ViewBag.Curso = curso;
        return View(matriculas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Confirmar(int id)
    {
        var matricula = _db.Matriculas.Find(id);
        if (matricula == null) return NotFound();
        matricula.Estado = EstadoMatricula.Confirmada;
        _db.SaveChanges();
        return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(int id)
    {
        var matricula = _db.Matriculas.Find(id);
        if (matricula == null) return NotFound();
        matricula.Estado = EstadoMatricula.Cancelada;
        _db.SaveChanges();
        return RedirectToAction(nameof(Matriculas), new { id = matricula.CursoId });
    }
}
