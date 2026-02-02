
using CMS.Entities;

namespace CMS.Data
{
    public interface IMenuRepository
    {
        Task<IEnumerable<Menu>> GetAllAsync();
    }
}