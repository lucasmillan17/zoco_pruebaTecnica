namespace CMS.Application.Exceptions
{
    /// <summary>
    /// Se lanza cuando las credenciales son inválidas o el usuario no tiene sesión válida.
    /// Se mapea a HTTP 401 por el manejador global de excepciones.
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}
