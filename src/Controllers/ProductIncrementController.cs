using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.ProductIncrementDtos;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Controllers
{
    public class ProductIncrementController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IProductIncrementService _productIncrementService;

        public ProductIncrementController(IAuthorizationService authorizationService, IProductIncrementService productIncrementService)
        {
            _authorizationService = authorizationService;
            _productIncrementService = productIncrementService;
        }
        

        public async Task<IActionResult> Index(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Forbid();

            ProductIncrementDto? dto = await _productIncrementService.GetProductIncrementAsync(id);
            if (dto == null) return NotFound();

            return View(dto);
        }
    }
}
