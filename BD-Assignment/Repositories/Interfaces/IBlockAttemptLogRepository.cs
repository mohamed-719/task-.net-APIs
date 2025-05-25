using BD_Assignment.Models;

namespace BD_Assignment.Repositories.Interfaces;

public interface IBlockAttemptLogRepository
{
    Task AddAsync(BlockAttemptLog log);
    Task<IEnumerable<BlockAttemptLog>> GetAllAsync(int page = 1, int pageSize = 10);
}
