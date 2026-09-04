using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerShop.Api.Data;
using SneakerShop.Api.DTOs;
using SneakerShop.Api.Models;

namespace SneakerShop.Api.Controllers
{
    [Route("api/inventory-transactions")]
    [ApiController]
    public class InventoryTransactionsController :
        ControllerBase
    {
        private readonly SneakerShopDbContext _context;

        public InventoryTransactionsController(
            SneakerShopDbContext context)
        {
            _context = context;
        }

        // GET: api/inventory-transactions
        [HttpGet]
        public async Task<ActionResult<
            IEnumerable<InventoryTransaction>>> GetAll(
            int? itemId = null,
            string? transactionType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            IQueryable<InventoryTransaction> query =
                _context.InventoryTransactions
                    .AsNoTracking()
                    .Include(transaction => transaction.Item);

            if (itemId.HasValue)
            {
                query = query.Where(transaction =>
                    transaction.ItemId == itemId.Value);
            }

            if (!string.IsNullOrWhiteSpace(
                transactionType))
            {
                string value =
                    transactionType.Trim().ToLower();

                query = query.Where(transaction =>
                    transaction.TransactionType.ToLower() ==
                    value);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(transaction =>
                    transaction.CreatedAt >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                DateTime endingDate =
                    dateTo.Value.Date.AddDays(1);

                query = query.Where(transaction =>
                    transaction.CreatedAt < endingDate);
            }

            List<InventoryTransaction> transactions =
                await query
                    .OrderByDescending(transaction =>
                        transaction.CreatedAt)
                    .ToListAsync();

            return Ok(transactions);
        }

        // GET: api/inventory-transactions/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<
            InventoryTransaction>> GetById(int id)
        {
            InventoryTransaction? transaction =
                await _context.InventoryTransactions
                    .AsNoTracking()
                    .Include(record => record.Item)
                    .FirstOrDefaultAsync(record =>
                        record.Id == id);

            if (transaction == null)
            {
                return NotFound(new
                {
                    message =
                        "Inventory transaction not found."
                });
            }

            return Ok(transaction);
        }

        // POST: api/inventory-transactions/stock-in
        [HttpPost("stock-in")]
        public async Task<ActionResult> StockIn(
            StockMovementRequest request)
        {
            return await ProcessMovement(
                request,
                "StockIn",
                addsStock: true);
        }

        // POST: api/inventory-transactions/stock-out
        [HttpPost("stock-out")]
        public async Task<ActionResult> StockOut(
            StockMovementRequest request)
        {
            return await ProcessMovement(
                request,
                "StockOut",
                addsStock: false);
        }

        // POST: api/inventory-transactions/damaged
        [HttpPost("damaged")]
        public async Task<ActionResult> RecordDamaged(
            StockMovementRequest request)
        {
            return await ProcessMovement(
                request,
                "Damaged",
                addsStock: false);
        }

        // POST: api/inventory-transactions/return
        [HttpPost("return")]
        public async Task<ActionResult> CustomerReturn(
            StockMovementRequest request)
        {
            return await ProcessMovement(
                request,
                "Return",
                addsStock: true);
        }

        private async Task<ActionResult> ProcessMovement(
            StockMovementRequest request,
            string transactionType,
            bool addsStock)
        {
            Item? item = await _context.Items.FindAsync(
                request.ItemId);

            if (item == null)
            {
                return NotFound(new
                {
                    message = "Sneaker item not found."
                });
            }

            if (!item.IsActive)
            {
                return BadRequest(new
                {
                    message =
                        "Transactions cannot be recorded for an inactive item."
                });
            }

            if (!addsStock &&
                item.Quantity < request.Quantity)
            {
                return BadRequest(new
                {
                    message =
                        $"Insufficient stock. Only {item.Quantity} pair(s) are available."
                });
            }

            int previousQuantity = item.Quantity;

            if (addsStock)
            {
                item.Quantity += request.Quantity;
            }
            else
            {
                item.Quantity -= request.Quantity;
            }

            InventoryTransaction transaction = new()
            {
                ItemId = item.Id,
                TransactionType = transactionType,
                Quantity = request.Quantity,
                ReferenceNumber =
                    request.ReferenceNumber.Trim().ToUpper(),
                Notes = request.Notes.Trim(),
                PerformedBy =
                    request.PerformedBy.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(
                transaction);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    $"{transactionType} recorded successfully.",
                transactionId = transaction.Id,
                itemId = item.Id,
                itemName = item.Name,
                size = item.Size,
                colorway = item.Colorway,
                previousQuantity,
                movementQuantity = request.Quantity,
                updatedQuantity = item.Quantity,
                reorderLevel = item.ReorderLevel,
                lowStock =
                    item.Quantity <= item.ReorderLevel
            });
        }
    }
}