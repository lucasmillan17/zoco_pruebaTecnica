namespace CMS.Application.Auth
{
    /// <summary>
    /// Configuración para emisión y validación de tokens JWT.
    /// Se carga desde la sección "Jwt" de la configuración.
    /// </summary>
    public record JwtOptions(
        string Issuer,
        string Audience,
        string Key,
        int ExpirationHours);
}
