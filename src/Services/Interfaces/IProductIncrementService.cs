using ProjectManagementApplication.Dto.Read.ProductIncrementDtos;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface IProductIncrementService
    {
        public Task<ProductIncrementDto?> GetProductIncrementAsync(int projectId);
    }
}
