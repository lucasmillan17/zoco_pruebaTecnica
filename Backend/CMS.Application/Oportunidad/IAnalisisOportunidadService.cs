namespace CMS.Application.Oportunidad;

public interface IAnalisisOportunidadService
{
    Task<AnalisisOportunidadResult> AnalizarAsync(Guid comercioId, CancellationToken ct = default);
}
