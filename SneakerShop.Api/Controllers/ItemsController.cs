using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerShop.Api.Data;
using SneakerShop.Api.DTOs;
using SneakerShop.Api.Models;

namespace SneakerShop.Api.Controllers
{
    [Route("api/items")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly SneakerShopDbContext _context;

        private static readonly string[] ValidReleaseTypes =
        {
            "Regular",
            "Limited Edition",
            "Collaboration",
            "Exclusive"
        };

        private static readonly string[] ValidAuthenticityStatuses =
        {
            "Pending",
            "Verified",
            "Rejected"
        };

        public ItemsController(SneakerShopDbContext context)
        {
            _context = context;
        }

        // GET: api/items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetAll(
            string? search = null,
            string? brand = null,
            decimal? size = null,
            string? colorway = null,
            string? category = null,
            string? releaseType = null,
            string? authenticityStatus = null,
            bool lowStockOnly = false,
            bool includeInactive = false)
        {
            IQueryable<Item> query = _context.Items
                .AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(item => item.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string value = search.Trim().ToLower();

                query = query.Where(item =>
                    item.Name.ToLower().Contains(value) ||
                    item.Code.ToLower().Contains(value) ||
                    item.Brand.ToLower().Contains(value) ||
                    item.BoxCode.ToLower().Contains(value));
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                string value = brand.Trim().ToLower();

                query = query.Where(item =>
                    item.Brand.ToLower() == value);
            }

            if (size.HasValue)
            {
                query = query.Where(item =>
                    item.Size == size.Value);
            }

            if (!string.IsNullOrWhiteSpace(colorway))
            {
                string value = colorway.Trim().ToLower();

                query = query.Where(item =>
                    item.Colorway.ToLower().Contains(value));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                string value = category.Trim().ToLower();

                query = query.Where(item =>
                    item.Category.ToLower() == value);
            }

            if (!string.IsNullOrWhiteSpace(releaseType))
            {
                string value = releaseType.Trim().ToLower();

                query = query.Where(item =>
                    item.ReleaseType.ToLower() == value);
            }

            if (!string.IsNullOrWhiteSpace(
                authenticityStatus))
            {
                string value =
                    authenticityStatus.Trim().ToLower();

                query = query.Where(item =>
                    item.AuthenticityStatus.ToLower() == value);
            }

            if (lowStockOnly)
            {
                query = query.Where(item =>
                    item.Quantity <= item.ReorderLevel);
            }

            List<Item> results = await query
                .OrderBy(item => item.Name)
                .ThenBy(item => item.Brand)
                .ToListAsync();

            results = results
                .OrderBy(item => item.Name)
                .ThenBy(item => item.Size)
                .ToList();

            return Ok(results);
        }

        // GET: api/items/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Item>> GetById(int id)
        {
            Item? item = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "Sneaker item not found."
                });
            }

            return Ok(item);
        }

        // GET: api/items/low-stock
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Item>>>
            GetLowStock()
        {
            List<Item> items = await _context.Items
                .AsNoTracking()
                .Where(item =>
                    item.IsActive &&
                    item.Quantity <= item.ReorderLevel)
                .OrderBy(item => item.Quantity)
                .ThenBy(item => item.Name)
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/items/filters
        [HttpGet("filters")]
        public async Task<ActionResult> GetFilters()
        {
            var activeItems = _context.Items
                .AsNoTracking()
                .Where(item => item.IsActive);

            List<string> brands = await activeItems
                .Select(item => item.Brand)
                .Distinct()
                .OrderBy(value => value)
                .ToListAsync();

            List<decimal> sizes = await activeItems
                .Select(item => item.Size)
                .Distinct()
                .ToListAsync();

            sizes = sizes.OrderBy(value => value).ToList();

            List<string> categories = await activeItems
                .Select(item => item.Category)
                .Distinct()
                .OrderBy(value => value)
                .ToListAsync();

            List<string> colorways = await activeItems
                .Select(item => item.Colorway)
                .Distinct()
                .OrderBy(value => value)
                .ToListAsync();

            return Ok(new
            {
                brands,
                sizes,
                categories,
                colorways,
                releaseTypes = ValidReleaseTypes,
                authenticityStatuses =
                    ValidAuthenticityStatuses
            });
        }

        // POST: api/items
        [HttpPost]
        public async Task<ActionResult<Item>> Create(
            ItemRequest request)
        {
            string? validationError =
                ValidateChoices(request);

            if (validationError != null)
            {
                return BadRequest(new
                {
                    message = validationError
                });
            }

            string code = request.Code.Trim().ToUpper();

            bool codeExists = await _context.Items.AnyAsync(
                item => item.Code.ToUpper() == code);

            if (codeExists)
            {
                return Conflict(new
                {
                    message =
                        "An item with this product code already exists."
                });
            }

            Item item = new()
            {
                Name = request.Name.Trim(),
                Code = code,
                Brand = request.Brand.Trim(),
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity,
                Size = request.Size,
                Colorway = request.Colorway.Trim(),
                Category = request.Category.Trim(),
                ReleaseType = GetCanonicalValue(
                    request.ReleaseType,
                    ValidReleaseTypes),
                AuthenticityStatus = GetCanonicalValue(
                    request.AuthenticityStatus,
                    ValidAuthenticityStatuses),
                BoxCode = request.BoxCode.Trim().ToUpper(),
                ReorderLevel = request.ReorderLevel,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            if (item.Quantity > 0)
            {
                InventoryTransaction openingStock = new()
                {
                    ItemId = item.Id,
                    TransactionType = "OpeningStock",
                    Quantity = item.Quantity,
                    ReferenceNumber =
                        $"OPEN-{item.Id:D5}",
                    Notes = "Opening inventory quantity.",
                    PerformedBy =
                        CleanUser(request.PerformedBy),
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryTransactions.Add(
                    openingStock);

                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = item.Id },
                item);
        }

        // PUT: api/items/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            ItemRequest request)
        {
            Item? item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "Sneaker item not found."
                });
            }

            string? validationError =
                ValidateChoices(request);

            if (validationError != null)
            {
                return BadRequest(new
                {
                    message = validationError
                });
            }

            string code = request.Code.Trim().ToUpper();

            bool duplicateCode =
                await _context.Items.AnyAsync(
                    existingItem =>
                        existingItem.Id != id &&
                        existingItem.Code.ToUpper() == code);

            if (duplicateCode)
            {
                return Conflict(new
                {
                    message =
                        "Another item already uses this product code."
                });
            }

            int oldQuantity = item.Quantity;
            int difference = request.Quantity - oldQuantity;

            item.Name = request.Name.Trim();
            item.Code = code;
            item.Brand = request.Brand.Trim();
            item.UnitPrice = request.UnitPrice;
            item.Quantity = request.Quantity;
            item.Size = request.Size;
            item.Colorway = request.Colorway.Trim();
            item.Category = request.Category.Trim();
            item.ReleaseType = GetCanonicalValue(
                request.ReleaseType,
                ValidReleaseTypes);
            item.AuthenticityStatus = GetCanonicalValue(
                request.AuthenticityStatus,
                ValidAuthenticityStatuses);
            item.BoxCode =
                request.BoxCode.Trim().ToUpper();
            item.ReorderLevel = request.ReorderLevel;

            if (difference != 0)
            {
                InventoryTransaction adjustment = new()
                {
                    ItemId = item.Id,
                    TransactionType = "Adjustment",
                    Quantity = Math.Abs(difference),
                    ReferenceNumber =
                        $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Notes = difference > 0
                        ? $"Quantity increased from {oldQuantity} to {request.Quantity}."
                        : $"Quantity decreased from {oldQuantity} to {request.Quantity}.",
                    PerformedBy =
                        CleanUser(request.PerformedBy),
                    CreatedAt = DateTime.UtcNow
                };

                _context.InventoryTransactions.Add(adjustment);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/items/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            Item? item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "Sneaker item not found."
                });
            }

            item.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Sneaker item deactivated."
            });
        }

        // PATCH: api/items/1/restore
        [HttpPatch("{id:int}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            Item? item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "Sneaker item not found."
                });
            }

            item.IsActive = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Sneaker item restored."
            });
        }

        private static string? ValidateChoices(
            ItemRequest request)
        {
            bool validRelease = ValidReleaseTypes.Any(
                value => value.Equals(
                    request.ReleaseType.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (!validRelease)
            {
                return "Release type must be Regular, Limited Edition, Collaboration, or Exclusive.";
            }

            bool validAuthenticity =
                ValidAuthenticityStatuses.Any(
                    value => value.Equals(
                        request.AuthenticityStatus.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            if (!validAuthenticity)
            {
                return "Authenticity status must be Pending, Verified, or Rejected.";
            }

            return null;
        }

        private static string GetCanonicalValue(
            string enteredValue,
            IEnumerable<string> validValues)
        {
            return validValues.First(value =>
                value.Equals(
                    enteredValue.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        private static string CleanUser(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "System"
                : value.Trim();
        }
    }
}