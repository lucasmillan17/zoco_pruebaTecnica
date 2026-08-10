using CMS.Application.Interacciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/interacciones")]
public class InteraccionesController : ControllerBase
{
    private readonly IInteraccionService _service;

    public InteraccionesController(IInteraccionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetByComercio(
        [FromQuery] Guid comercioId,
        [FromQuery] Guid? tipoInteraccionId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var resultado = await _service.GetByComercioAsync(comercioId, tipoInteraccionId, desde, hasta, pageNumber, pageSize);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resultado = await _service.GetByIdAsync(id);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearInteraccionDto dto)
    {
        var resultado = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarInteraccionDto dto)
    {
        var resultado = await _service.UpdateAsync(id, dto);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
