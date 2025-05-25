using BD_Assignment.Models;
using BD_Assignment.Repositories.Interfaces;
using System.Collections.Concurrent;

namespace BD_Assignment.Repositories;

public class BlockAttemptLogRepository : IBlockAttemptLogRepository
{
   
        private readonly ConcurrentQueue<BlockAttemptLog> _logs = new();

        public Task AddAsync(BlockAttemptLog log)
        {
            _logs.Enqueue(log);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<BlockAttemptLog>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var result = _logs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .AsEnumerable();

            return Task.FromResult(result);
        }
    }

