using pd311_web_api.BLL.DTOs.User;

namespace pd311_web_api.BLL.Services.User
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<IEnumerable<UserDto>> GetAllAsync(int page, int pageSize);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role, int page, int pageSize);
        Task<IEnumerable<UserDto>> GetSortedUsersAsync(string sortBy, int page, int pageSize);
    }
}
