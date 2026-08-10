namespace CMS.Application.Auth
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<UsuarioDto> MeAsync(Guid usuarioId);
        Task<UsuarioDto> CambiarPasswordAsync(Guid usuarioId, CambiarPasswordDto dto);
        Task<UsuarioDto> CrearUsuarioAsync(CrearUsuarioDto dto);
        Task<List<UsuarioDto>> ListarUsuariosAsync();
        Task<UsuarioDto> DesactivarUsuarioAsync(Guid actorId, Guid idObjetivo);
    }
}
