using backend.DTOs;
using static backend.DTOs.ServiceResponses;

namespace backend.Contracts
{
    public interface IUser
    {
        Task<GeneralResponse> CreateAccount(UserDTO userDto);
        Task<LoginResponse> LoginAccount(LoginDTO loginDto);
    }
}