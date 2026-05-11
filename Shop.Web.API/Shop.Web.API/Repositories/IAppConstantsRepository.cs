using Shop.Web.API.Models.Domain;

namespace Shop.Web.API.Repositories
{
    public interface IAppConstantsRepository
    {
        /// <summary>
        /// Executes usp_AppConstants_GetAll and returns every active constant row.
        /// </summary>
        Task<IEnumerable<AppConstantRecord>> GetAllAsync();
    }
}
