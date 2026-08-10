using CMS.Application.Comercios;
using CMS.Application.Oportunidad;
using CMS.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/comercios")]
public class ComerciosController : ControllerBase
{
    private readonly IComercioService _comercioService;
    private readonly IAnalisisOportunidadService _oportunidadService;

    public ComerciosController(IComercioService comercioService, IAnalisisOportunidadService oportunidadService)
    {
        _comercioService = comercioService;
        _oportunidadService = oportunidadService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? busqueda = null,
        [FromQuery] EstadoComercio? estado = null,
        [FromQuery] string? rubro = null,
        [FromQuery] OrdenComercio? ordenarPor = null,
        [FromQuery] OrdenDireccion? orden = null,
        [FromQuery] EstadoActivo estadoActivo = EstadoActivo.Activos,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new BuscarComerciosQuery(busqueda, estado, rubro, ordenarPor, orden, estadoActivo, pageNumber, pageSize);
        var resultado = await _comercioService.GetAllAsync(query);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resultado = await _comercioService.GetByIdAsync(id);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpGet("validar-cuit")]
    public async Task<IActionResult> ValidarCuit([FromQuery] string cuit)
    {
        var resultado = await _comercioService.ValidarCuitAsync(cuit);
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearComercioDto dto)
    {
        var resultado = await _comercioService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarComercioDto dto)
    {
        var resultado = await _comercioService.UpdateAsync(id, dto);
        return Ok(resultado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _comercioService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivar")]
    public async Task<IActionResult> Reactivar(Guid id)
    {
        var resultado = await _comercioService.ReactivarAsync(id);
        return Ok(resultado);
    }

    [HttpPost("{id:guid}/oportunidad")]
    public async Task<IActionResult> AnalizarOportunidad(Guid id, CancellationToken ct)
    {
        var resultado = await _oportunidadService.AnalizarAsync(id, ct);
        return Ok(resultado);
    }
}
