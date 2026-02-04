using Microsoft.EntityFrameworkCore;
using CMS.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task<List<User>> GetAllAsync()
            => await _db.Users
                .OrderBy(u => u.USER_NAME)
                .AsNoTracking()
                .ToListAsync();

        public async Task<User?> GetByIdAsync(int id)
            => await _db.Users.FindAsync(id);

        public async Task<User?> GetByUsernameAsync(string username)
            => await _db.Users
                .FirstOrDefaultAsync(u => u.USER_NAME == username);

        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
            }
        }
    }
}