using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using pd311_web_api.BLL.DTOs.User;
using pd311_web_api.DAL; 
using System.Linq.Expressions;
using static pd311_web_api.DAL.Entities.IdentityEntities;

namespace pd311_web_api.BLL.Services.User
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserService(UserManager<AppUser> userManager, AppDbContext context, IMapper mapper) // Додаємо `ApplicationDbContext`
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var entities = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();

            return _mapper.Map<List<UserDto>>(entities);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync(int page, int pageSize)
        {
            var users = await _context.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role, int page, int pageSize)
        {
            var users = await _context.Users
                .Where(u => u.UserRoles.Any(r => r.Role.Name == role))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<IEnumerable<UserDto>> GetSortedUsersAsync(string sortBy, int page, int pageSize)
        {
            // Визначення поля для сортування
            Expression<Func<AppUser, object>> sortExpression = sortBy.ToLower() switch
            {
                "role" => u => u.UserRoles.First().Role.Name,
                "email" => u => u.Email,
                "username" => u => u.UserName,
                _ => u => u.UserName
            };

            var users = await _context.Users
                .OrderBy(sortExpression)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<UserDto>>(users);
        }
    }
}
