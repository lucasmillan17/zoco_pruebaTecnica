using CMS.Application.Auditorias;
using CMS.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/auditoria")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _service;

    public AuditoriaController(IAuditoriaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? entidad,
        [FromQuery] string? usuario,
        [FromQuery] OperacionAuditoria? operacion,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new BuscarAuditoriaQuery(entidad, usuario, operacion, desde, hasta, pageNumber, pageSize);
        var resultado = await _service.GetAllAsync(query);
        return Ok(resultado);
    }
}
