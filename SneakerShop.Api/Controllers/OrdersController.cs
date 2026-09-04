using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SneakerShop.Api.Data;
using SneakerShop.Api.DTOs;
using SneakerShop.Api.Models;

namespace SneakerShop.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly SneakerShopDbContext _context;

        public OrdersController(SneakerShopDbContext context)
        {
            _context = context;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult> GetAll(
            string? search = null,
            string? status = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            IQueryable<Order> query = _context.Orders
                .AsNoTracking()
                .Include(order => order.Lines)
                .ThenInclude(line => line.Item);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string value = search.Trim().ToLower();

                query = query.Where(order =>
                    order.OrderNumber.ToLower().Contains(value) ||
                    order.CustomerName.ToLower().Contains(value));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                string value = status.Trim().ToLower();

                query = query.Where(order =>
                    order.Status.ToLower() == value);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(order =>
                    order.OrderDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                DateTime endingDate =
                    dateTo.Value.Date.AddDays(1);

                query = query.Where(order =>
                    order.OrderDate < endingDate);
            }

            List<Order> orders = await query
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            return Ok(orders.Select(ToResponse));
        }

        // GET: api/orders/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            Order? order = await _context.Orders
                .AsNoTracking()
                .Include(record => record.Lines)
                .ThenInclude(line => line.Item)
                .FirstOrDefaultAsync(record =>
                    record.Id == id);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            return Ok(ToResponse(order));
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult> Create(
            CreateOrderRequest request)
        {
            var requestedLines = request.Lines
                .GroupBy(line => line.ItemId)
                .Select(group => new
                {
                    ItemId = group.Key,
                    Quantity = group.Sum(line =>
                        line.Quantity)
                })
                .ToList();

            if (requestedLines.Count == 0)
            {
                return BadRequest(new
                {
                    message =
                        "The order must contain at least one item."
                });
            }

            List<int> itemIds = requestedLines
                .Select(line => line.ItemId)
                .ToList();

            Dictionary<int, Item> items =
                await _context.Items
                    .Where(item =>
                        itemIds.Contains(item.Id))
                    .ToDictionaryAsync(item => item.Id);

            if (items.Count != itemIds.Count)
            {
                return BadRequest(new
                {
                    message =
                        "One or more sneaker items do not exist."
                });
            }

            foreach (var requestedLine in requestedLines)
            {
                Item item = items[requestedLine.ItemId];

                if (!item.IsActive)
                {
                    return BadRequest(new
                    {
                        message =
                            $"{item.Name}, size {item.Size}, is inactive."
                    });
                }

                if (!item.AuthenticityStatus.Equals(
                    "Verified",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message =
                            $"{item.Name}, size {item.Size}, cannot be sold because it is not verified."
                    });
                }

                if (item.Quantity <
                    requestedLine.Quantity)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Insufficient stock for {item.Name}, size {item.Size}. Only {item.Quantity} pair(s) are available."
                    });
                }
            }

            await using var databaseTransaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                string orderNumber =
                    $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Random.Shared.Next(100, 999)}";

                Order order = new()
                {
                    OrderNumber = orderNumber,
                    CustomerName =
                        request.CustomerName.Trim(),
                    OrderDate = DateTime.UtcNow,
                    Status = "Completed",
                    ProcessedBy =
                        request.ProcessedBy.Trim()
                };

                foreach (var requestedLine in requestedLines)
                {
                    Item item =
                        items[requestedLine.ItemId];

                    decimal subtotal =
                        item.UnitPrice *
                        requestedLine.Quantity;

                    order.Lines.Add(new OrderLine
                    {
                        ItemId = item.Id,
                        Item = item,
                        Quantity =
                            requestedLine.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = subtotal,
                        ReturnedQuantity = 0,
                        ExchangedQuantity = 0
                    });

                    item.Quantity -=
                        requestedLine.Quantity;

                    _context.InventoryTransactions.Add(
                        new InventoryTransaction
                        {
                            ItemId = item.Id,
                            TransactionType = "Sale",
                            Quantity =
                                requestedLine.Quantity,
                            ReferenceNumber =
                                orderNumber,
                            Notes =
                                $"Sold to {order.CustomerName}.",
                            PerformedBy =
                                order.ProcessedBy,
                            CreatedAt =
                                DateTime.UtcNow
                        });

                    order.TotalAmount += subtotal;
                }

                _context.Orders.Add(order);

                await _context.SaveChangesAsync();
                await databaseTransaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = order.Id },
                    ToResponse(order));
            }
            catch
            {
                await databaseTransaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message =
                        "The order could not be completed."
                });
            }
        }

        // POST: api/orders/1/cancel
        [HttpPost("{id:int}/cancel")]
        public async Task<ActionResult> Cancel(
            int id,
            CancelOrderRequest request)
        {
            Order? order = await LoadTrackedOrder(id);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            if (order.Status == "Cancelled")
            {
                return BadRequest(new
                {
                    message =
                        "The order is already cancelled."
                });
            }

            bool hasPreviousActions =
                order.Lines.Any(line =>
                    line.ReturnedQuantity > 0 ||
                    line.ExchangedQuantity > 0);

            if (hasPreviousActions)
            {
                return BadRequest(new
                {
                    message =
                        "An order with returns or exchanges cannot be cancelled."
                });
            }

            foreach (OrderLine line in order.Lines)
            {
                if (line.Item == null)
                {
                    continue;
                }

                line.Item.Quantity += line.Quantity;

                _context.InventoryTransactions.Add(
                    new InventoryTransaction
                    {
                        ItemId = line.ItemId,
                        TransactionType =
                            "OrderCancellation",
                        Quantity = line.Quantity,
                        ReferenceNumber =
                            order.OrderNumber,
                        Notes =
                            $"Cancelled: {request.Reason.Trim()}",
                        PerformedBy =
                            request.PerformedBy.Trim(),
                        CreatedAt = DateTime.UtcNow
                    });
            }

            order.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Order cancelled and stock restored.",
                orderNumber = order.OrderNumber,
                status = order.Status
            });
        }

        // POST: api/orders/1/return
        [HttpPost("{id:int}/return")]
        public async Task<ActionResult> ReturnItem(
            int id,
            ReturnOrderItemRequest request)
        {
            Order? order = await LoadTrackedOrder(id);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            if (order.Status == "Cancelled")
            {
                return BadRequest(new
                {
                    message =
                        "Items from a cancelled order cannot be returned."
                });
            }

            OrderLine? line = order.Lines
                .FirstOrDefault(record =>
                    record.Id == request.OrderLineId);

            if (line == null || line.Item == null)
            {
                return BadRequest(new
                {
                    message =
                        "The selected order item does not exist."
                });
            }

            int availableQuantity =
                line.Quantity -
                line.ReturnedQuantity -
                line.ExchangedQuantity;

            if (request.Quantity > availableQuantity)
            {
                return BadRequest(new
                {
                    message =
                        $"Only {availableQuantity} pair(s) can be returned."
                });
            }

            line.Item.Quantity += request.Quantity;
            line.ReturnedQuantity += request.Quantity;

            _context.InventoryTransactions.Add(
                new InventoryTransaction
                {
                    ItemId = line.ItemId,
                    TransactionType = "Return",
                    Quantity = request.Quantity,
                    ReferenceNumber =
                        order.OrderNumber,
                    Notes =
                        $"Customer return: {request.Reason.Trim()}",
                    PerformedBy =
                        request.PerformedBy.Trim(),
                    CreatedAt = DateTime.UtcNow
                });

            UpdateOrderStatus(order);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Return completed and stock restored.",
                orderNumber = order.OrderNumber,
                itemName = line.Item.Name,
                size = line.Item.Size,
                returnedQuantity =
                    request.Quantity,
                updatedStock =
                    line.Item.Quantity,
                orderStatus = order.Status
            });
        }

        // POST: api/orders/1/exchange
        [HttpPost("{id:int}/exchange")]
        public async Task<ActionResult> ExchangeSize(
            int id,
            ExchangeOrderItemRequest request)
        {
            Order? order = await LoadTrackedOrder(id);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            if (order.Status == "Cancelled")
            {
                return BadRequest(new
                {
                    message =
                        "Items from a cancelled order cannot be exchanged."
                });
            }

            OrderLine? oldLine = order.Lines
                .FirstOrDefault(line =>
                    line.Id == request.OrderLineId);

            if (oldLine == null ||
                oldLine.Item == null)
            {
                return BadRequest(new
                {
                    message =
                        "The original order item does not exist."
                });
            }

            Item oldItem = oldLine.Item;

            if (oldItem.Id == request.NewItemId)
            {
                return BadRequest(new
                {
                    message =
                        "Select a different shoe size."
                });
            }

            Item? newItem = await _context.Items
                .FindAsync(request.NewItemId);

            if (newItem == null || !newItem.IsActive)
            {
                return BadRequest(new
                {
                    message =
                        "The replacement item is unavailable."
                });
            }

            bool sameSneaker =
                oldItem.Name.Equals(
                    newItem.Name,
                    StringComparison.OrdinalIgnoreCase) &&
                oldItem.Brand.Equals(
                    newItem.Brand,
                    StringComparison.OrdinalIgnoreCase) &&
                oldItem.Colorway.Equals(
                    newItem.Colorway,
                    StringComparison.OrdinalIgnoreCase);

            if (!sameSneaker)
            {
                return BadRequest(new
                {
                    message =
                        "A size exchange must use the same sneaker model, brand, and colorway."
                });
            }

            if (oldItem.Size == newItem.Size)
            {
                return BadRequest(new
                {
                    message =
                        "The replacement must have a different size."
                });
            }

            if (oldItem.UnitPrice != newItem.UnitPrice)
            {
                return BadRequest(new
                {
                    message =
                        "The replacement size must have the same price."
                });
            }

            if (!newItem.AuthenticityStatus.Equals(
                "Verified",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "The replacement item is not verified."
                });
            }

            int exchangeableQuantity =
                oldLine.Quantity -
                oldLine.ReturnedQuantity -
                oldLine.ExchangedQuantity;

            if (request.Quantity >
                exchangeableQuantity)
            {
                return BadRequest(new
                {
                    message =
                        $"Only {exchangeableQuantity} pair(s) can be exchanged."
                });
            }

            if (request.Quantity > newItem.Quantity)
            {
                return BadRequest(new
                {
                    message =
                        $"Only {newItem.Quantity} replacement pair(s) are available."
                });
            }

            oldItem.Quantity += request.Quantity;
            newItem.Quantity -= request.Quantity;

            oldLine.ExchangedQuantity +=
                request.Quantity;

            string notes =
                $"Size exchange from {oldItem.Size} to {newItem.Size}: {request.Reason.Trim()}";

            _context.InventoryTransactions.AddRange(
                new InventoryTransaction
                {
                    ItemId = oldItem.Id,
                    TransactionType = "ExchangeIn",
                    Quantity = request.Quantity,
                    ReferenceNumber =
                        order.OrderNumber,
                    Notes = notes,
                    PerformedBy =
                        request.PerformedBy.Trim(),
                    CreatedAt = DateTime.UtcNow
                },
                new InventoryTransaction
                {
                    ItemId = newItem.Id,
                    TransactionType = "ExchangeOut",
                    Quantity = request.Quantity,
                    ReferenceNumber =
                        order.OrderNumber,
                    Notes = notes,
                    PerformedBy =
                        request.PerformedBy.Trim(),
                    CreatedAt = DateTime.UtcNow
                });

            UpdateOrderStatus(order);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Shoe-size exchange completed.",
                orderNumber = order.OrderNumber,
                sneaker = oldItem.Name,
                oldSize = oldItem.Size,
                newSize = newItem.Size,
                quantity = request.Quantity,
                oldSizeStock = oldItem.Quantity,
                newSizeStock = newItem.Quantity,
                orderStatus = order.Status
            });
        }

        private async Task<Order?> LoadTrackedOrder(
            int id)
        {
            return await _context.Orders
                .Include(order => order.Lines)
                .ThenInclude(line => line.Item)
                .FirstOrDefaultAsync(order =>
                    order.Id == id);
        }

        private static void UpdateOrderStatus(
            Order order)
        {
            int totalPurchased = order.Lines
                .Sum(line => line.Quantity);

            int totalReturned = order.Lines
                .Sum(line => line.ReturnedQuantity);

            int totalExchanged = order.Lines
                .Sum(line => line.ExchangedQuantity);

            int totalProcessed =
                totalReturned + totalExchanged;

            if (totalProcessed < totalPurchased)
            {
                order.Status =
                    totalReturned > 0
                        ? "Partially Returned"
                        : "Partially Exchanged";

                return;
            }

            if (totalReturned == totalPurchased)
            {
                order.Status = "Returned";
            }
            else if (totalExchanged == totalPurchased)
            {
                order.Status = "Exchanged";
            }
            else
            {
                order.Status = "Returned/Exchanged";
            }
        }

        private static object ToResponse(Order order)
        {
            return new
            {
                order.Id,
                order.OrderNumber,
                order.CustomerName,
                order.OrderDate,
                order.TotalAmount,
                order.Status,
                order.ProcessedBy,

                Lines = order.Lines.Select(line =>
                    new
                    {
                        line.Id,
                        line.ItemId,
                        ItemName = line.Item?.Name,
                        Brand = line.Item?.Brand,
                        Colorway = line.Item?.Colorway,
                        Size = line.Item?.Size,
                        line.Quantity,
                        line.UnitPrice,
                        line.Subtotal,
                        line.ReturnedQuantity,
                        line.ExchangedQuantity,
                        AvailableForReturnOrExchange =
                            line.Quantity -
                            line.ReturnedQuantity -
                            line.ExchangedQuantity
                    })
            };
        }
    }
}