// ================================================================================
// ARCHIVO: CMS.API/Controllers/CurrencyController.cs
// PROPÓSITO: API para gestión de monedas (currencies)
// DESCRIPCIÓN: Endpoint para obtener la lista de monedas activas del sistema
// AUTOR: BITI SOLUTIONS S.A
// CREADO: 2025-01-XX
// ================================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Entities;

namespace CMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CurrencyController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(
            AppDbContext context,
            ILogger<CurrencyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las monedas activas ordenadas por sort_order
        /// </summary>
        /// <returns>Lista de monedas activas</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CurrencyDto>>> GetActiveCurrencies()
        {
            try
            {
                var currencies = await _context.Currencies
                    .Where(c => c.IS_ACTIVE)
                    .OrderBy(c => c.SORT_ORDER)
                    .ThenBy(c => c.CURRENCY_NAME)
                    .Select(c => new CurrencyDto
                    {
                        Id = c.ID_CURRENCY,
                        Code = c.CURRENCY_CODE,
                        Name = c.CURRENCY_NAME,
                        Symbol = c.CURRENCY_SYMBOL,
                        MinorUnit = c.MINOR_UNIT,
                        IsCrypto = c.IS_CRYPTO
                    })
                    .ToListAsync();

                _logger.LogInformation("Obtenidas {Count} monedas activas", currencies.Count);

                return Ok(currencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener monedas activas");
                return StatusCode(500, new { message = "Error al obtener la lista de monedas" });
            }
        }

        /// <summary>
        /// Obtiene una moneda por su código ISO (ej: USD, EUR, CRC)
        /// </summary>
        /// <param name="code">Código ISO de la moneda (3 caracteres)</param>
        /// <returns>Datos de la moneda</returns>
        [HttpGet("code/{code}")]
        public async Task<ActionResult<CurrencyDto>> GetCurrencyByCode(string code)
        {
            try
            {
                var currency = await _context.Currencies
                    .Where(c => c.CURRENCY_CODE == code.ToUpper())
                    .Select(c => new CurrencyDto
                    {
                        Id = c.ID_CURRENCY,
                        Code = c.CURRENCY_CODE,
                        Name = c.CURRENCY_NAME,
                        Symbol = c.CURRENCY_SYMBOL,
                        MinorUnit = c.MINOR_UNIT,
                        IsCrypto = c.IS_CRYPTO
                    })
                    .FirstOrDefaultAsync();

                if (currency == null)
                {
                    return NotFound(new { message = $"Moneda con código '{code}' no encontrada" });
                }

                return Ok(currency);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener moneda por código {Code}", code);
                return StatusCode(500, new { message = "Error al obtener la moneda" });
            }
        }
    }

    #region DTOs

    /// <summary>
    /// DTO para Currency usado en los endpoints de API
    /// </summary>
    public class CurrencyDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Symbol { get; set; } = default!;
        public short MinorUnit { get; set; }
        public bool IsCrypto { get; set; }
    }

    #endregion
}
