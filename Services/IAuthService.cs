using SupportPulse.Api.DTOs;

namespace SupportPulse.Api.Services;

public interface IAuthService
{
	Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
	Task<AuthResponseDto> LoginAsync(LoginDto dto);
}
