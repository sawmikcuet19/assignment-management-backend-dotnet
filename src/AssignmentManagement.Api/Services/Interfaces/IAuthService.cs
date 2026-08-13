using AssignmentManagement.Api.Dtos;

namespace AssignmentManagement.Api.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> Login(LoginRequest request);
    Task<LoginResponse> Register(RegisterRequest request);
}