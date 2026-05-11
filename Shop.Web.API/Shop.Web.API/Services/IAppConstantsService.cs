using Shop.Web.API.Models.Domain;

namespace Shop.Web.API.Services
{
    public interface IAppConstantsService
    {
        /// <summary>
        /// Returns the full constant catalogue, built from AppConstants table.
        /// Result is memory-cached after first load (constants rarely change).
        /// </summary>
        Task<AppConstantsDto> GetAllAsync();
    }
}
