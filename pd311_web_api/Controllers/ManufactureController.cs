using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pd311_web_api.BLL.DTOs.Manufactures;
using pd311_web_api.BLL.Services.Manufactures;
using System.Threading.Tasks;

namespace pd311_web_api.Controllers
{
    [ApiController]
    [Route("api/manufacture")]
    public class ManufactureController : BaseController
    {
        private readonly IManufactureService _manufactureService;

        public ManufactureController(IManufactureService manufactureService)
        {
            _manufactureService = manufactureService;
        }

        [HttpPost]
        [Authorize(Roles = "admin, manufacture manager")] // Дозволяємо тільки "admin" і "manufacture manager"
        public async Task<IActionResult> CreateAsync(CreateManufactureDto dto)
        {
            var result = await _manufactureService.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "admin, manufacture manager")]
        public async Task<IActionResult> UpdateAsync(UpdateManufactureDto dto)
        {
            var result = await _manufactureService.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpDelete]
        [Authorize(Roles = "admin, manufacture manager")]
        public async Task<IActionResult> DeleteAsync(string? id)
        {
            if (!ValidateId(id, out string error))
            {
                return BadRequest(error);
            }

            var result = await _manufactureService.DeleteAsync(id);
            return result ? Ok(result) : BadRequest(result);
        }

        [HttpGet("list")]
        [AllowAnonymous] // Дозволяємо всім
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _manufactureService.GetAllAsync();
            return CreateActionResult(response);
        }
    }
}
