namespace CMS.Application.Exceptions;

/// <summary>
/// Error al invocar un servicio externo (proveedor de IA, etc.).
/// </summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message)
    {
    }
}
