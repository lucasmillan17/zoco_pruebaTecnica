using CMS.Application.TiposInteraccion;
using CMS.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tipos-interaccion")]
public class TiposInteraccionController : ControllerBase
{
    private readonly ITipoInteraccionService _service;

    public TiposInteraccionController(ITipoInteraccionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] EstadoActivo estadoActivo = EstadoActivo.Activos,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var resultado = await _service.GetAllAsync(estadoActivo, pageNumber, pageSize);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create([FromBody] CrearTipoInteraccionDto dto)
    {
        var resultado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarTipoInteraccionDto dto)
    {
        var resultado = await _service.UpdateAsync(id, dto);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Reactivar(Guid id)
    {
        var resultado = await _service.ReactivarAsync(id);
        return Ok(resultado);
    }
}
