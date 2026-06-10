// ================================================================================
// ARCHIVO: CMS.API/Controllers/WarehouseTypeController.cs
// PROPÓSITO: API REST para tipos de bodega - tabla CENTRAL admin.warehouse_type
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-01
// ================================================================================

using CMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/warehousetype")]
    public class WarehouseTypeController : ControllerBase
    {
        private readonly AppDbContext _db;

        public WarehouseTypeController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/warehousetype
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
        {
            var query = _db.WarehouseTypes.AsQueryable();
            if (isActive.HasValue)
                query = query.Where(w => w.IsActive == isActive.Value);

            var items = await query
                .OrderBy(w => w.SortOrder)
                .ThenBy(w => w.Name)
                .Select(w => new
                {
                    w.Id,
                    w.Code,
                    w.Name,
                    w.Description,
                    w.Icon,
                    w.Color,
                    w.SortOrder,
                    w.IsActive,
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
