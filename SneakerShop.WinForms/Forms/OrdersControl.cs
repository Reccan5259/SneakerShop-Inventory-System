using System.Drawing;
using System.Windows.Forms;
using SneakerShop.WinForms.Models;
using SneakerShop.WinForms.Services;
using SneakerShop.WinForms.Styles;

namespace SneakerShop.WinForms.Forms;

public class OrdersControl : UserControl
{
    private readonly TextBox _customerNameTextBox;
    private readonly ComboBox _itemComboBox;
    private readonly ComboBox _sizeComboBox;
    private readonly NumericUpDown _quantityInput;
    private readonly Label _availableStockLabel;
    private readonly Label _unitPriceLabel;
    private readonly Label _totalLabel;
    private readonly Label _statusLabel;
    private readonly DataGridView _cartGrid;
    private readonly Button _completeOrderButton;

    private List<Item> _items = new();
    private readonly List<CartLine> _cart = new();

    public OrdersControl()
    {
        Dock = DockStyle.Fill;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10);

        // ORDER INFORMATION AREA
        var orderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 245,
            BackColor = Color.White
        };

        var sectionLabel = new Label
        {
            Text = "SALES AND ORDER PROCESSING",
            Location = new Point(25, 15),
            Size = new Size(300, 20),
            ForeColor = AppTheme.PrimaryRed,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };

        var titleLabel = new Label
        {
            Text = "Create Customer Order",
            Location = new Point(25, 35),
            Size = new Size(420, 40),
            ForeColor = AppTheme.TextDark,
            Font = new Font("Segoe UI", 20, FontStyle.Bold)
        };

        var customerLabel = CreateLabel(
            "Customer name",
            25,
            90);

        _customerNameTextBox = new TextBox
        {
            Location = new Point(25, 115),
            Size = new Size(280, 32),
            PlaceholderText = "Enter the customer's full name"
        };

        AppTheme.StyleTextBox(_customerNameTextBox);

        var itemLabel = CreateLabel(
            "Sneaker product",
            325,
            90);

        _itemComboBox = new ComboBox
        {
            Location = new Point(325, 115),
            Size = new Size(310, 32),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _itemComboBox.SelectedIndexChanged += (_, _) =>
        {
            PopulateAvailableSizes();
        };

        var sizeLabel = CreateLabel(
            "Available size",
            650,
            90);

        _sizeComboBox = new ComboBox
        {
            Location = new Point(650, 115),
            Size = new Size(120, 32),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _sizeComboBox.SelectedIndexChanged += (_, _) =>
        {
            UpdateSelectedItemDetails();
        };

        var quantityLabel = CreateLabel(
            "Quantity",
            785,
            90);

        _quantityInput = new NumericUpDown
        {
            Location = new Point(785, 115),
            Size = new Size(95, 32),
            Minimum = 1,
            Maximum = 1000,
            Value = 1,
            Font = new Font("Segoe UI", 11)
        };

        var addToCartButton = new Button
        {
            Text = "ADD TO ORDER",
            Location = new Point(895, 110),
            Size = new Size(165, 42),
            Font = new Font(
                "Segoe UI",
                9,
                FontStyle.Bold)
        };

        AppTheme.StylePrimaryButton(addToCartButton);
        addToCartButton.Click += AddToCartButton_Click;

        _availableStockLabel = new Label
        {
            Text = "Available: 0 pair(s)",
            Location = new Point(325, 165),
            Size = new Size(230, 25),
            ForeColor = AppTheme.Success,
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Bold)
        };

        _unitPriceLabel = new Label
        {
            Text = "Unit price: ₱0.00",
            Location = new Point(570, 165),
            Size = new Size(230, 25),
            ForeColor = AppTheme.PrimaryRed,
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Bold)
        };

        var instructionLabel = new Label
        {
            Text =
                "Select a sneaker, available size and quantity, then click " +
                "Add to Order. You may add multiple products.",

            Location = new Point(25, 202),
            Size = new Size(750, 25),
            ForeColor = AppTheme.TextMuted
        };

        orderPanel.Controls.Add(sectionLabel);
        orderPanel.Controls.Add(titleLabel);
        orderPanel.Controls.Add(customerLabel);
        orderPanel.Controls.Add(_customerNameTextBox);
        orderPanel.Controls.Add(itemLabel);
        orderPanel.Controls.Add(_itemComboBox);
        orderPanel.Controls.Add(sizeLabel);
        orderPanel.Controls.Add(_sizeComboBox);
        orderPanel.Controls.Add(quantityLabel);
        orderPanel.Controls.Add(_quantityInput);
        orderPanel.Controls.Add(addToCartButton);
        orderPanel.Controls.Add(_availableStockLabel);
        orderPanel.Controls.Add(_unitPriceLabel);
        orderPanel.Controls.Add(instructionLabel);

        // CART HEADER
        var cartHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = AppTheme.LightRed
        };

        var cartTitleLabel = new Label
        {
            Text = "Current Order Items",
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 0, 0, 0),
            ForeColor = AppTheme.DarkRed,
            Font = new Font(
                "Segoe UI",
                14,
                FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        cartHeaderPanel.Controls.Add(cartTitleLabel);

        // CART TABLE
        _cartGrid = new DataGridView
        {
            Dock = DockStyle.Fill
        };

        AppTheme.StyleDataGridView(_cartGrid);

        _cartGrid.CellDoubleClick += (_, eventArgs) =>
        {
            if (eventArgs.RowIndex >= 0)
            {
                RemoveSelectedCartItem();
            }
        };

        // ORDER FOOTER
        var footerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 115,
            BackColor = Color.White
        };

        var orderTotalTextLabel = new Label
        {
            Text = "ORDER TOTAL",
            Location = new Point(25, 18),
            Size = new Size(180, 20),
            ForeColor = AppTheme.TextMuted,
            Font = new Font(
                "Segoe UI",
                8,
                FontStyle.Bold)
        };

        _totalLabel = new Label
        {
            Text = "₱0.00",
            Location = new Point(25, 40),
            Size = new Size(350, 42),
            ForeColor = AppTheme.PrimaryRed,
            Font = new Font(
                "Segoe UI",
                23,
                FontStyle.Bold)
        };

        var removeButton = new Button
        {
            Text = "REMOVE SELECTED",
            Location = new Point(430, 28),
            Size = new Size(170, 42)
        };

        AppTheme.StyleDangerButton(removeButton);

        removeButton.Click += (_, _) =>
        {
            RemoveSelectedCartItem();
        };

        var clearButton = new Button
        {
            Text = "CLEAR ORDER",
            Location = new Point(615, 28),
            Size = new Size(145, 42)
        };

        AppTheme.StyleSecondaryButton(clearButton);

        clearButton.Click += (_, _) =>
        {
            if (_cart.Count == 0)
            {
                return;
            }

            DialogResult result = MessageBox.Show(
                "Remove all products from this order?",
                "Clear Order",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _cart.Clear();
                RefreshCart();
            }
        };

        _completeOrderButton = new Button
        {
            Text = "COMPLETE ORDER",
            Location = new Point(775, 28),
            Size = new Size(200, 45),
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Bold)
        };

        AppTheme.StylePrimaryButton(
            _completeOrderButton);

        _completeOrderButton.Click +=
            CompleteOrderButton_Click;

        _statusLabel = new Label
        {
            Text = "Ready",
            Location = new Point(430, 80),
            Size = new Size(545, 25),
            ForeColor = AppTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleRight
        };

        footerPanel.Controls.Add(orderTotalTextLabel);
        footerPanel.Controls.Add(_totalLabel);
        footerPanel.Controls.Add(removeButton);
        footerPanel.Controls.Add(clearButton);
        footerPanel.Controls.Add(_completeOrderButton);
        footerPanel.Controls.Add(_statusLabel);

        Controls.Add(_cartGrid);
        Controls.Add(cartHeaderPanel);
        Controls.Add(footerPanel);
        Controls.Add(orderPanel);

        Load += async (_, _) =>
        {
            await LoadItemsAsync();
        };
    }

    private static Label CreateLabel(
        string text,
        int left,
        int top)
    {
        return new Label
        {
            Text = text,
            Location = new Point(left, top),
            AutoSize = true,
            ForeColor = AppTheme.TextDark
        };
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            SetStatus("Loading available sneakers...", false);

            _items =
                await ApiService.Instance.GetItemsAsync();

            List<Item> availableItems = _items
                .Where(item =>
                    item.IsActive &&
                    item.Quantity > 0)
                .OrderBy(item => item.Brand)
                .ThenBy(item => item.Name)
                .ThenBy(item => item.Size)
                .ToList();

            List<ProductChoice> availableProducts = availableItems
                .GroupBy(item => new
                {
                    item.Brand,
                    item.Name,
                    item.Colorway
                })
                .Select(group => new ProductChoice
                {
                    Brand = group.Key.Brand,
                    Name = group.Key.Name,
                    Colorway = group.Key.Colorway
                })
                .OrderBy(product => product.Brand)
                .ThenBy(product => product.Name)
                .ThenBy(product => product.Colorway)
                .ToList();

            _itemComboBox.DataSource = null;
            _itemComboBox.DisplayMember =
                nameof(ProductChoice.DisplayName);
            _itemComboBox.DataSource =
                availableProducts;

            PopulateAvailableSizes();

            SetStatus(
                $"{availableProducts.Count} sneaker product(s) and " +
                $"{availableItems.Count} size option(s) available.",
                true);
        }
        catch (Exception ex)
        {
            SetStatus("Unable to load sneakers.", false);

            MessageBox.Show(
                ex.Message,
                "Order Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void PopulateAvailableSizes()
    {
        _sizeComboBox.DataSource = null;

        if (_itemComboBox.SelectedItem is not ProductChoice product)
        {
            UpdateSelectedItemDetails();
            return;
        }

        List<SizeChoice> availableSizes = _items
            .Where(item =>
                item.IsActive &&
                item.Quantity > 0 &&
                string.Equals(item.Brand, product.Brand,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.Name, product.Name,
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.Colorway, product.Colorway,
                    StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Size)
            .Select(item => new SizeChoice
            {
                Item = item
            })
            .ToList();

        _sizeComboBox.DisplayMember = nameof(SizeChoice.DisplayName);
        _sizeComboBox.ValueMember = nameof(SizeChoice.ItemId);
        _sizeComboBox.DataSource = availableSizes;

        UpdateSelectedItemDetails();
    }

    private void UpdateSelectedItemDetails()
    {
        if (_sizeComboBox.SelectedItem is not SizeChoice sizeChoice)
        {
            _availableStockLabel.Text =
                "Available: 0 pair(s)";

            _unitPriceLabel.Text =
                "Unit price: ₱0.00";

            return;
        }

        Item item = sizeChoice.Item;

        int quantityAlreadyInCart = _cart
            .Where(line => line.ItemId == item.Id)
            .Sum(line => line.Quantity);

        int remainingQuantity =
            item.Quantity - quantityAlreadyInCart;

        _availableStockLabel.Text =
            $"Available: {remainingQuantity} pair(s)";

        _availableStockLabel.ForeColor =
            remainingQuantity <= item.ReorderLevel
                ? AppTheme.Danger
                : AppTheme.Success;

        _unitPriceLabel.Text =
            $"Unit price: ₱{item.UnitPrice:N2}";

        _quantityInput.Maximum =
            Math.Max(1, remainingQuantity);

        if (_quantityInput.Value >
            _quantityInput.Maximum)
        {
            _quantityInput.Value =
                _quantityInput.Maximum;
        }
    }

    private void AddToCartButton_Click(
        object? sender,
        EventArgs e)
    {
        if (_itemComboBox.SelectedItem is not ProductChoice)
        {
            MessageBox.Show(
                "Please select a sneaker product.",
                "No Sneaker Selected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (_sizeComboBox.SelectedItem is not SizeChoice sizeChoice)
        {
            MessageBox.Show(
                "Please select an available shoe size.",
                "No Size Selected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        Item item = sizeChoice.Item;

        int requestedQuantity =
            (int)_quantityInput.Value;

        CartLine? existingLine =
            _cart.FirstOrDefault(
                line => line.ItemId == item.Id);

        int existingQuantity =
            existingLine?.Quantity ?? 0;

        int newQuantity =
            existingQuantity + requestedQuantity;

        if (newQuantity > item.Quantity)
        {
            MessageBox.Show(
                $"Only {item.Quantity} pair(s) are available.",
                "Insufficient Stock",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (existingLine == null)
        {
            _cart.Add(new CartLine
            {
                ItemId = item.Id,
                Sneaker = $"{item.Brand} {item.Name} | {item.Colorway}",
                Code = item.Code,
                Size = item.Size,
                Quantity = requestedQuantity,
                UnitPrice = item.UnitPrice
            });
        }
        else
        {
            existingLine.Quantity =
                newQuantity;
        }

        _quantityInput.Value = 1;

        RefreshCart();
        UpdateSelectedItemDetails();

        SetStatus(
            $"{item.Name} added to the order.",
            true);
    }

    private void RefreshCart()
    {
        var rows = _cart
            .Select(line => new
            {
                line.ItemId,
                line.Sneaker,
                line.Code,
                line.Size,
                line.Quantity,
                line.UnitPrice,
                line.Subtotal
            })
            .ToList();

        _cartGrid.DataSource = rows;

        if (_cartGrid.Columns["ItemId"] != null)
        {
            _cartGrid.Columns["ItemId"].Visible = false;
        }

        if (_cartGrid.Columns["UnitPrice"] != null)
        {
            _cartGrid.Columns["UnitPrice"]
                .DefaultCellStyle.Format =
                "₱#,##0.00";
        }

        if (_cartGrid.Columns["Subtotal"] != null)
        {
            _cartGrid.Columns["Subtotal"]
                .DefaultCellStyle.Format =
                "₱#,##0.00";
        }

        if (_cartGrid.Columns["Sneaker"] != null)
        {
            _cartGrid.Columns["Sneaker"]
                .FillWeight = 180;
        }

        decimal total =
            _cart.Sum(line => line.Subtotal);

        _totalLabel.Text = $"₱{total:N2}";
    }

    private void RemoveSelectedCartItem()
    {
        if (_cartGrid.CurrentRow == null)
        {
            MessageBox.Show(
                "Select a product from the order first.",
                "No Selection",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        object? idValue =
            _cartGrid.CurrentRow
                .Cells["ItemId"].Value;

        if (!int.TryParse(
                idValue?.ToString(),
                out int itemId))
        {
            return;
        }

        CartLine? line =
            _cart.FirstOrDefault(
                cartLine =>
                    cartLine.ItemId == itemId);

        if (line != null)
        {
            _cart.Remove(line);
        }

        RefreshCart();
        UpdateSelectedItemDetails();

        SetStatus(
            "Product removed from the order.",
            true);
    }

    private async void CompleteOrderButton_Click(
        object? sender,
        EventArgs e)
    {
        string customerName =
            _customerNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(customerName))
        {
            MessageBox.Show(
                "Please enter the customer's name.",
                "Customer Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            _customerNameTextBox.Focus();
            return;
        }

        if (_cart.Count == 0)
        {
            MessageBox.Show(
                "Add at least one sneaker to the order.",
                "Empty Order",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        decimal total =
            _cart.Sum(line => line.Subtotal);

        DialogResult answer = MessageBox.Show(
            $"Complete this order for {customerName}?\n\n" +
            $"Total amount: ₱{total:N2}",

            "Confirm Customer Order",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _completeOrderButton.Enabled = false;
            _completeOrderButton.Text =
                "PROCESSING...";

            SetStatus(
                "Submitting customer order...",
                false);

            var request = new CreateOrderRequest
            {
                CustomerName = customerName,
                ProcessedBy = UserSession.Username,
                Lines = _cart
                    .Select(line =>
                        new CreateOrderLineRequest
                        {
                            ItemId = line.ItemId,
                            Quantity = line.Quantity
                        })
                    .ToList()
            };

            OrderResponse completedOrder =
                await ApiService.Instance
                    .CreateOrderAsync(request);

            MessageBox.Show(
                $"Order completed successfully.\n\n" +
                $"Order number: {completedOrder.OrderNumber}\n" +
                $"Customer: {completedOrder.CustomerName}\n" +
                $"Total: ₱{completedOrder.TotalAmount:N2}",

                "Order Completed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            _cart.Clear();
            _customerNameTextBox.Clear();

            RefreshCart();
            await LoadItemsAsync();

            SetStatus(
                $"Order {completedOrder.OrderNumber} completed.",
                true);
        }
        catch (Exception ex)
        {
            SetStatus("Unable to complete order.", false);

            MessageBox.Show(
                ex.Message,
                "Order Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _completeOrderButton.Enabled = true;
            _completeOrderButton.Text =
                "COMPLETE ORDER";
        }
    }

    private void SetStatus(
        string message,
        bool success)
    {
        _statusLabel.Text = message;

        _statusLabel.ForeColor = success
            ? AppTheme.Success
            : AppTheme.TextMuted;
    }

    private sealed class CartLine
    {
        public int ItemId { get; set; }

        public string Sneaker { get; set; } =
            string.Empty;

        public string Code { get; set; } =
            string.Empty;

        public decimal Size { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Subtotal =>
            Quantity * UnitPrice;
    }

    private sealed class ProductChoice
    {
        public string Brand { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Colorway { get; set; } = string.Empty;

        public string DisplayName =>
            $"{Brand} {Name} | {Colorway}";
    }

    private sealed class SizeChoice
    {
        public Item Item { get; set; } = new();

        public int ItemId => Item.Id;

        public string DisplayName =>
            $"Size {Item.Size:0.##}";
    }
}