
using CMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext _context;

        public MenuRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Menu>> GetAllAsync()
        {
            try
            {
                // SIMPLE y directo
                return await _context.Menus
                    .AsNoTracking()  // Solo lectura
                    .OrderBy(m => m.ORDER)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log error
                throw new Exception($"Error obteniendo menús: {ex.Message}", ex);
            }
        }
    }
}