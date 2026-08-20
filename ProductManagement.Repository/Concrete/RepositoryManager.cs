using ProductManagement.Domain.Abstract;
using ProductManagement.EfCore.Context;

namespace ProductManagement.Repository.Concrete
{
    public sealed class RepositoryManager : IRepositoryManager
    {
        private readonly ApplicationDbContext _context;

        public RepositoryManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
