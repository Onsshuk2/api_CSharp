using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using pd311_web_api.BLL.DTOs.Account;
using pd311_web_api.BLL.Services.Email;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static pd311_web_api.DAL.Entities.IdentityEntities;

namespace pd311_web_api.BLL.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        private readonly IMapper _mapper;

        public AccountService(UserManager<AppUser> userManager, IEmailService emailService, IMapper mapper, RoleManager<AppRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _mapper = mapper;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<bool> ConfirmEmailAsync(string id, string base64)
        {
            var user = await _userManager.FindByIdAsync(id);

            if(user != null)
            {
                var bytes = Convert.FromBase64String(base64);
                var token = Encoding.UTF8.GetString(bytes);
                var result = await _userManager.ConfirmEmailAsync(user, token);
                return result.Succeeded;
            }

            return false;
        }

        public async Task<ServiceResponse> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName ?? "");

            if (user == null)
                return new ServiceResponse($"Користувача з іменем '{dto.UserName}' не знайдено");

            var result = await _userManager.CheckPasswordAsync(user, dto.Password ?? "");

            if (!result)
                return new ServiceResponse($"Пароль вказано невірно");

            // Generate jwt token
            var claims = new List<Claim>
            {
                new Claim("id", user.Id),
                new Claim("email", user.Email ?? ""),
                new Claim("userName", user.UserName ?? ""),
                new Claim("image", user.Image ?? "")
            };

            var roles = await _userManager.GetRolesAsync(user);
            if(roles.Any())
            {
                var roleClaims = roles.Select(r => new Claim("role", r));
                claims.AddRange(roleClaims);
            }

            string secretKey = _configuration["JwtSettings:Key"] ?? "";
            string issuer = _configuration["JwtSettings:Issuer"] ?? "";
            string audience = _configuration["JwtSettings:Audience"] ?? "";
            int expMinutes = int.Parse(_configuration["JwtSettings:ExpTime"] ?? "");

            if (string.IsNullOrEmpty(secretKey) 
                || string.IsNullOrEmpty(issuer) 
                || string.IsNullOrEmpty(audience))
            {
                throw new ArgumentNullException("Jwt settings is null");
            }

            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims.ToArray(),
                expires: DateTime.UtcNow.AddMinutes(expMinutes),
                signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
                );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new ServiceResponse("Успішний вхід", true, jwtToken);
        }

        public async Task<ServiceResponse> RegisterAsync(RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return new ServiceResponse($"Email '{dto.Email}' зайнятий");

            if (await _userManager.FindByNameAsync(dto.UserName) != null)
                return new ServiceResponse($"Ім'я '{dto.UserName}' вже використовується");

            var user = _mapper.Map<AppUser>(dto);

            var result = await _userManager.CreateAsync(user, dto.Password);

            if(!result.Succeeded)
            {
                return new ServiceResponse(result.Errors.First().Description);
            }

            if(result.Succeeded && await _roleManager.RoleExistsAsync("user"))
            {
                result = await _userManager.AddToRoleAsync(user, "user");
            }

            if (!result.Succeeded)
                return new ServiceResponse(result.Errors.First().Description);

            await SendConfirmEmailTokenAsync(user.Id);

            // Generate jwt token
            var claims = new List<Claim>
            {
                new Claim("id", user.Id),
                new Claim("email", user.Email ?? ""),
                new Claim("userName", user.UserName ?? ""),
                new Claim("image", user.Image ?? "")
            };

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                var roleClaims = roles.Select(r => new Claim("role", r));
                claims.AddRange(roleClaims);
            }

            string secretKey = _configuration["JwtSettings:Key"] ?? "";
            string issuer = _configuration["JwtSettings:Issuer"] ?? "";
            string audience = _configuration["JwtSettings:Audience"] ?? "";
            int expMinutes = int.Parse(_configuration["JwtSettings:ExpTime"] ?? "");

            if (string.IsNullOrEmpty(secretKey)
                || string.IsNullOrEmpty(issuer)
                || string.IsNullOrEmpty(audience))
            {
                throw new ArgumentNullException("Jwt settings is null");
            }

            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims.ToArray(),
                expires: DateTime.UtcNow.AddMinutes(expMinutes),
                signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
                );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new ServiceResponse("Успішна реєстрація", true, jwtToken);
        }

        public async Task<bool> SendConfirmEmailTokenAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return false;
                }

                // Sent mail
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var bytes = Encoding.UTF8.GetBytes(token);
                var base64 = Convert.ToBase64String(bytes);

                var body = $"<a href='https://localhost:7223/api/account/confirmEmail?id={user.Id}&t={base64}'>Підтвердити пошту</a>";

                await _emailService.SendMailAsync(user.Email!, "Email confirm", body, true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
