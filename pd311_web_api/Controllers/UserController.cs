using Microsoft.AspNetCore.Mvc;
using pd311_web_api.BLL.DTOs.User;
using pd311_web_api.BLL.Services.User;

namespace pd311_web_api.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Отримати список всіх користувачів із пагінацією
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var users = await _userService.GetAllAsync(page, pageSize);
            return Ok(users);
        }

        /// <summary>
        /// Отримати всіх користувачів певної ролі із пагінацією
        /// </summary>
        [HttpGet("by-role")]
        public async Task<IActionResult> GetUsersByRoleAsync(string role, int page = 1, int pageSize = 10)
        {
            var users = await _userService.GetUsersByRoleAsync(role, page, pageSize);
            return Ok(users);
        }

        /// <summary>
        /// Отримати відсортований список користувачів за вказаним полем із пагінацією
        /// </summary>
        [HttpGet("sorted")]
        public async Task<IActionResult> GetSortedUsersAsync(string sortBy = "userName", int page = 1, int pageSize = 10)
        {
            var users = await _userService.GetSortedUsersAsync(sortBy, page, pageSize);
            return Ok(users);
        }
    }
}
