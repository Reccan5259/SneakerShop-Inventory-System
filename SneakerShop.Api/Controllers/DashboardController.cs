using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerShop.Api.Data;
using SneakerShop.Api.Models;

namespace SneakerShop.Api.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly SneakerShopDbContext _context;

        public DashboardController(
            SneakerShopDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard/summary
        [HttpGet("summary")]
        public async Task<ActionResult> GetSummary()
        {
            List<Item> items = await _context.Items
                .AsNoTracking()
                .Where(item => item.IsActive)
                .ToListAsync();

            List<Order> orders = await _context.Orders
                .AsNoTracking()
                .ToListAsync();

            List<SalesMovement> movements =
                await LoadSalesMovements();

            int totalProducts = items.Count;

            int totalModels = items
                .Select(item =>
                    $"{item.Brand}|{item.Name}".ToLower())
                .Distinct()
                .Count();

            int totalBrands = items
                .Select(item => item.Brand.ToLower())
                .Distinct()
                .Count();

            int totalStock = items.Sum(item =>
                item.Quantity);

            decimal inventoryValue = items.Sum(item =>
                item.UnitPrice * item.Quantity);

            int lowStockCount = items.Count(item =>
                item.Quantity > 0 &&
                item.Quantity <= item.ReorderLevel);

            int outOfStockCount = items.Count(item =>
                item.Quantity == 0);

            int verifiedCount = items.Count(item =>
                item.AuthenticityStatus.Equals(
                    "Verified",
                    StringComparison.OrdinalIgnoreCase));

            int pendingAuthenticity = items.Count(item =>
                item.AuthenticityStatus.Equals(
                    "Pending",
                    StringComparison.OrdinalIgnoreCase));

            List<Order> validOrders = orders
                .Where(order =>
                    order.Status != "Cancelled")
                .ToList();

            decimal grossSales = validOrders.Sum(order =>
                order.TotalAmount);

            int netUnitsSold = Math.Max(
                movements.Sum(record =>
                    record.SignedQuantity),
                0);

            var bestSize = movements
                .GroupBy(record => record.Size)
                .Select(group => new
                {
                    Size = group.Key,
                    UnitsSold = group.Sum(record =>
                        record.SignedQuantity)
                })
                .Where(result => result.UnitsSold > 0)
                .OrderByDescending(result =>
                    result.UnitsSold)
                .FirstOrDefault();

            var bestBrand = movements
                .GroupBy(record => record.Brand)
                .Select(group => new
                {
                    Brand = group.Key,
                    UnitsSold = group.Sum(record =>
                        record.SignedQuantity)
                })
                .Where(result => result.UnitsSold > 0)
                .OrderByDescending(result =>
                    result.UnitsSold)
                .FirstOrDefault();

            int userCount = await _context.Users.CountAsync();

            return Ok(new
            {
                totalProducts,
                totalModels,
                totalBrands,
                totalStock,
                inventoryValue,
                lowStockCount,
                outOfStockCount,
                verifiedCount,
                pendingAuthenticity,
                totalOrders = validOrders.Count,
                cancelledOrders = orders.Count(order =>
                    order.Status == "Cancelled"),
                grossSales,
                netUnitsSold,
                bestSellingSize = bestSize?.Size,
                bestSellingSizeUnits =
                    bestSize?.UnitsSold ?? 0,
                bestSellingBrand = bestBrand?.Brand,
                bestSellingBrandUnits =
                    bestBrand?.UnitsSold ?? 0,
                registeredUsers = userCount
            });
        }

        // GET: api/dashboard/best-selling-sizes
        [HttpGet("best-selling-sizes")]
        public async Task<ActionResult> GetBestSellingSizes(
            int top = 5)
        {
            top = Math.Clamp(top, 1, 20);

            List<SalesMovement> movements =
                await LoadSalesMovements();

            var results = movements
                .GroupBy(record => record.Size)
                .Select(group => new
                {
                    Size = group.Key,
                    UnitsSold = group.Sum(record =>
                        record.SignedQuantity)
                })
                .Where(result => result.UnitsSold > 0)
                .OrderByDescending(result =>
                    result.UnitsSold)
                .Take(top)
                .Select((result, index) => new
                {
                    Rank = index + 1,
                    result.Size,
                    result.UnitsSold
                })
                .ToList();

            return Ok(results);
        }

        // GET: api/dashboard/best-selling-brands
        [HttpGet("best-selling-brands")]
        public async Task<ActionResult> GetBestSellingBrands(
            int top = 5)
        {
            top = Math.Clamp(top, 1, 20);

            List<SalesMovement> movements =
                await LoadSalesMovements();

            var results = movements
                .GroupBy(record => record.Brand)
                .Select(group => new
                {
                    Brand = group.Key,
                    UnitsSold = group.Sum(record =>
                        record.SignedQuantity)
                })
                .Where(result => result.UnitsSold > 0)
                .OrderByDescending(result =>
                    result.UnitsSold)
                .Take(top)
                .Select((result, index) => new
                {
                    Rank = index + 1,
                    result.Brand,
                    result.UnitsSold
                })
                .ToList();

            return Ok(results);
        }

        // GET: api/dashboard/slow-moving
        [HttpGet("slow-moving")]
        public async Task<ActionResult> GetSlowMovingItems(
            int days = 30,
            int maximumSales = 1)
        {
            days = Math.Clamp(days, 1, 365);
            maximumSales = Math.Max(maximumSales, 0);

            DateTime cutoffDate =
                DateTime.UtcNow.AddDays(-days);

            List<Item> items = await _context.Items
                .AsNoTracking()
                .Where(item =>
                    item.IsActive &&
                    item.Quantity > 0)
                .ToListAsync();

            List<SalesMovement> movements =
                await LoadSalesMovements(cutoffDate);

            Dictionary<int, int> salesByItem = movements
                .GroupBy(record => record.ItemId)
                .ToDictionary(
                    group => group.Key,
                    group => Math.Max(
                        group.Sum(record =>
                            record.SignedQuantity),
                        0));

            var slowMoving = items
                .Select(item =>
                {
                    int unitsSold = salesByItem.TryGetValue(
                        item.Id,
                        out int value)
                        ? value
                        : 0;

                    return new
                    {
                        item.Id,
                        item.Name,
                        item.Code,
                        item.Brand,
                        item.Colorway,
                        item.Size,
                        item.Quantity,
                        UnitsSold = unitsSold,
                        DaysAnalyzed = days,
                        Status = unitsSold == 0
                            ? "No Sales"
                            : "Slow Moving"
                    };
                })
                .Where(item =>
                    item.UnitsSold <= maximumSales)
                .OrderBy(item => item.UnitsSold)
                .ThenByDescending(item => item.Quantity)
                .ToList();

            return Ok(slowMoving);
        }

        // GET: api/dashboard/restock-suggestions
        [HttpGet("restock-suggestions")]
        public async Task<ActionResult>
            GetRestockSuggestions()
        {
            DateTime cutoffDate =
                DateTime.UtcNow.AddDays(-30);

            List<Item> items = await _context.Items
                .AsNoTracking()
                .Where(item =>
                    item.IsActive &&
                    item.Quantity <= item.ReorderLevel)
                .ToListAsync();

            List<SalesMovement> movements =
                await LoadSalesMovements(cutoffDate);

            Dictionary<int, int> recentSales = movements
                .GroupBy(record => record.ItemId)
                .ToDictionary(
                    group => group.Key,
                    group => Math.Max(
                        group.Sum(record =>
                            record.SignedQuantity),
                        0));

            var suggestions = items
                .Select(item =>
                {
                    int unitsSold30Days =
                        recentSales.TryGetValue(
                            item.Id,
                            out int value)
                            ? value
                            : 0;

                    int targetStock = Math.Max(
                        item.ReorderLevel * 2,
                        unitsSold30Days +
                        item.ReorderLevel);

                    int suggestedOrderQuantity =
                        Math.Max(
                            targetStock - item.Quantity,
                            1);

                    string urgency;

                    if (item.Quantity == 0)
                    {
                        urgency = "Critical";
                    }
                    else if (item.Quantity <=
                        Math.Max(
                            item.ReorderLevel / 2,
                            1))
                    {
                        urgency = "High";
                    }
                    else
                    {
                        urgency = "Medium";
                    }

                    return new
                    {
                        item.Id,
                        item.Name,
                        item.Code,
                        item.Brand,
                        item.Colorway,
                        item.Size,
                        CurrentQuantity = item.Quantity,
                        item.ReorderLevel,
                        UnitsSoldLast30Days =
                            unitsSold30Days,
                        TargetStock = targetStock,
                        SuggestedOrderQuantity =
                            suggestedOrderQuantity,
                        Urgency = urgency
                    };
                })
                .OrderBy(suggestion =>
                    suggestion.Urgency == "Critical"
                        ? 1
                        : suggestion.Urgency == "High"
                            ? 2
                            : 3)
                .ThenBy(suggestion =>
                    suggestion.CurrentQuantity)
                .ToList();

            return Ok(suggestions);
        }

        private async Task<List<SalesMovement>>
            LoadSalesMovements(
                DateTime? cutoffDate = null)
        {
            string[] salesTypes =
            {
                "Sale",
                "Return",
                "OrderCancellation",
                "ExchangeIn",
                "ExchangeOut"
            };

            IQueryable<InventoryTransaction> query =
                _context.InventoryTransactions
                    .AsNoTracking()
                    .Include(transaction =>
                        transaction.Item)
                    .Where(transaction =>
                        salesTypes.Contains(
                            transaction.TransactionType));

            if (cutoffDate.HasValue)
            {
                query = query.Where(transaction =>
                    transaction.CreatedAt >=
                    cutoffDate.Value);
            }

            List<InventoryTransaction> records =
                await query.ToListAsync();

            return records
                .Where(record => record.Item != null)
                .Select(record => new SalesMovement
                {
                    ItemId = record.ItemId,
                    Size = record.Item!.Size,
                    Brand = record.Item.Brand,
                    SignedQuantity =
                        GetSignedQuantity(record)
                })
                .ToList();
        }

        private static int GetSignedQuantity(
            InventoryTransaction transaction)
        {
            return transaction.TransactionType switch
            {
                "Sale" => transaction.Quantity,
                "ExchangeOut" => transaction.Quantity,
                "Return" => -transaction.Quantity,
                "OrderCancellation" =>
                    -transaction.Quantity,
                "ExchangeIn" => -transaction.Quantity,
                _ => 0
            };
        }

        private sealed class SalesMovement
        {
            public int ItemId { get; set; }

            public decimal Size { get; set; }

            public string Brand { get; set; } =
                string.Empty;

            public int SignedQuantity { get; set; }
        }
    }
}