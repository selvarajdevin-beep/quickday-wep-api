using Dapper;
using Microsoft.Data.SqlClient;
using Shop.Web.API.Models.Domain;
using System.Data;

namespace Shop.Web.API.Repositories
{
    public sealed class AppConstantsRepository : IAppConstantsRepository
    {
        private readonly string _conn;
        private readonly ILogger<AppConstantsRepository> _logger;

        public AppConstantsRepository(
            IConfiguration config,
            ILogger<AppConstantsRepository> logger)
        {
            _conn = config.GetConnectionString("Default")
                      ?? throw new InvalidOperationException("Connection string missing.");
            _logger = logger;
        }

        public async Task<IEnumerable<AppConstantRecord>> GetAllAsync()
        {
            using var db = new SqlConnection(_conn);
            try
            {
                return await db.QueryAsync<AppConstantRecord>(
                    "dbo.usp_AppConstants_GetAll",
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error in AppConstantsRepository.GetAllAsync");
                throw;
            }
        }
    }

}
