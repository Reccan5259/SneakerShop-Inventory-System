using System.Drawing;
using System.Windows.Forms;
using SneakerShop.WinForms.Models;
using SneakerShop.WinForms.Services;
using SneakerShop.WinForms.Styles;

namespace SneakerShop.WinForms.Forms;

public class OrderHistoryControl : UserControl
{
    private readonly TextBox _searchTextBox;
    private readonly ComboBox _statusComboBox;
    private readonly DataGridView _ordersGrid;
    private readonly DataGridView _linesGrid;
    private readonly Label _statusLabel;

    private List<OrderResponse> _orders = new();

    public OrderHistoryControl()
    {
        Dock = DockStyle.Fill;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10);

        var toolbarPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 145,
            BackColor = Color.White
        };

        var sectionLabel = new Label
        {
            Text = "SALES RECORDS",
            Location = new Point(25, 15),
            Size = new Size(250, 20),
            ForeColor = AppTheme.PrimaryRed,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };

        var titleLabel = new Label
        {
            Text = "Order History",
            Location = new Point(25, 35),
            Size = new Size(350, 40),
            ForeColor = AppTheme.TextDark,
            Font = new Font("Segoe UI", 20, FontStyle.Bold)
        };

        _searchTextBox = new TextBox
        {
            Location = new Point(25, 92),
            Size = new Size(280, 32),
            PlaceholderText = "Search order or customer"
        };

        _searchTextBox.TextChanged += (_, _) =>
        {
            ApplyFilters();
        };

        var filterLabel = new Label
        {
            Text = "Status:",
            Location = new Point(325, 97),
            AutoSize = true,
            ForeColor = AppTheme.TextDark
        };

        _statusComboBox = new ComboBox
        {
            Location = new Point(385, 92),
            Size = new Size(150, 32),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _statusComboBox.Items.AddRange(
            new object[]
            {
                "All",
                "Completed",
                "Cancelled"
            });

        _statusComboBox.SelectedIndex = 0;

        _statusComboBox.SelectedIndexChanged += (_, _) =>
        {
            ApplyFilters();
        };

        var refreshButton = CreateButton(
            "REFRESH",
            550,
            88,
            105);

        AppTheme.StyleSecondaryButton(refreshButton);

        refreshButton.Click += async (_, _) =>
        {
            await LoadOrdersAsync();
        };

        var cancelButton = CreateButton(
            "CANCEL ORDER",
            670,
            88,
            135);

        AppTheme.StyleDangerButton(cancelButton);
        cancelButton.Click += CancelOrderButton_Click;

        var returnButton = CreateButton(
            "RETURN ITEM",
            820,
            88,
            125);

        AppTheme.StyleSecondaryButton(returnButton);
        returnButton.Click += ReturnItemButton_Click;

        var exchangeButton = CreateButton(
            "SIZE EXCHANGE",
            960,
            88,
            140);

        AppTheme.StylePrimaryButton(exchangeButton);
        exchangeButton.Click += ExchangeItemButton_Click;

        toolbarPanel.Controls.Add(sectionLabel);
        toolbarPanel.Controls.Add(titleLabel);
        toolbarPanel.Controls.Add(_searchTextBox);
        toolbarPanel.Controls.Add(filterLabel);
        toolbarPanel.Controls.Add(_statusComboBox);
        toolbarPanel.Controls.Add(refreshButton);
        toolbarPanel.Controls.Add(cancelButton);
        toolbarPanel.Controls.Add(returnButton);
        toolbarPanel.Controls.Add(exchangeButton);

        var tablesLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = AppTheme.PageBackground
        };

        tablesLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        tablesLayout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 50));

        tablesLayout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 50));

        var ordersPanel = new Panel
        {
            Dock = DockStyle.Fill
        };

        var ordersHeader = CreateTableHeader(
            "Orders — select an order to view its products");

        _ordersGrid = new DataGridView
        {
            Dock = DockStyle.Fill
        };

        AppTheme.StyleDataGridView(_ordersGrid);

        _ordersGrid.SelectionChanged += (_, _) =>
        {
            DisplaySelectedOrderLines();
        };

        ordersPanel.Controls.Add(_ordersGrid);
        ordersPanel.Controls.Add(ordersHeader);

        var linesPanel = new Panel
        {
            Dock = DockStyle.Fill
        };

        var linesHeader = CreateTableHeader(
            "Products in Selected Order");

        _linesGrid = new DataGridView
        {
            Dock = DockStyle.Fill
        };

        AppTheme.StyleDataGridView(_linesGrid);

        linesPanel.Controls.Add(_linesGrid);
        linesPanel.Controls.Add(linesHeader);

        tablesLayout.Controls.Add(ordersPanel, 0, 0);
        tablesLayout.Controls.Add(linesPanel, 0, 1);

        var statusPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            BackColor = Color.White
        };

        _statusLabel = new Label
        {
            Text = "Ready",
            Dock = DockStyle.Fill,
            Padding = new Padding(15, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = AppTheme.TextMuted
        };

        statusPanel.Controls.Add(_statusLabel);

        Controls.Add(tablesLayout);
        Controls.Add(statusPanel);
        Controls.Add(toolbarPanel);

        Load += async (_, _) =>
        {
            await LoadOrdersAsync();
        };
    }

    private static Button CreateButton(
        string text,
        int left,
        int top,
        int width)
    {
        return new Button
        {
            Text = text,
            Location = new Point(left, top),
            Size = new Size(width, 40),
            Font = new Font(
                "Segoe UI",
                9,
                FontStyle.Bold)
        };
    }

    private static Panel CreateTableHeader(string text)
    {
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = AppTheme.LightRed
        };

        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Padding = new Padding(15, 0, 0, 0),
            ForeColor = AppTheme.DarkRed,
            Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        headerPanel.Controls.Add(label);

        return headerPanel;
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            SetStatus("Loading order history...", false);

            _orders =
                await ApiService.Instance.GetOrdersAsync();

            ApplyFilters();

            SetStatus(
                $"{_orders.Count} order(s) loaded.",
                true);
        }
        catch (Exception ex)
        {
            _orders.Clear();
            _ordersGrid.DataSource = null;
            _linesGrid.DataSource = null;

            SetStatus("Unable to load orders.", false);

            MessageBox.Show(
                ex.Message,
                "Order History Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ApplyFilters()
    {
        string search =
            _searchTextBox.Text.Trim();

        string selectedStatus =
            _statusComboBox.SelectedItem?.ToString()
            ?? "All";

        IEnumerable<OrderResponse> filteredOrders =
            _orders;

        if (!string.IsNullOrWhiteSpace(search))
        {
            filteredOrders = filteredOrders.Where(order =>
                order.OrderNumber.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||

                order.CustomerName.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (selectedStatus != "All")
        {
            filteredOrders = filteredOrders.Where(order =>
                order.Status.Equals(
                    selectedStatus,
                    StringComparison.OrdinalIgnoreCase));
        }

        var rows = filteredOrders
            .OrderByDescending(order => order.OrderDate)
            .Select(order => new
            {
                order.Id,
                OrderNumber = order.OrderNumber,
                Date = order.OrderDate,
                Customer = order.CustomerName,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ProcessedBy = order.ProcessedBy
            })
            .ToList();

        _ordersGrid.DataSource = rows;

        if (_ordersGrid.Columns["Id"] != null)
        {
            _ordersGrid.Columns["Id"].Visible = false;
        }

        if (_ordersGrid.Columns["Date"] != null)
        {
            _ordersGrid.Columns["Date"]
                .DefaultCellStyle.Format =
                "MMM dd, yyyy hh:mm tt";
        }

        if (_ordersGrid.Columns["TotalAmount"] != null)
        {
            _ordersGrid.Columns["TotalAmount"]
                .DefaultCellStyle.Format =
                "₱#,##0.00";
        }

        DisplaySelectedOrderLines();
    }

    private void DisplaySelectedOrderLines()
    {
        OrderResponse? order = GetSelectedOrder();

        if (order == null)
        {
            _linesGrid.DataSource = null;
            return;
        }

        var lineRows = order.Lines
            .Select(line => new
            {
                line.Id,
                line.ItemId,
                Sneaker = line.ItemName,
                line.Brand,
                line.Colorway,
                line.Size,
                Ordered = line.Quantity,
                Returned = line.ReturnedQuantity,
                Exchanged = line.ExchangedQuantity,
                Available =
                    line.AvailableForReturnOrExchange,
                line.UnitPrice,
                line.Subtotal
            })
            .ToList();

        _linesGrid.DataSource = lineRows;

        if (_linesGrid.Columns["Id"] != null)
        {
            _linesGrid.Columns["Id"].Visible = false;
        }

        if (_linesGrid.Columns["ItemId"] != null)
        {
            _linesGrid.Columns["ItemId"].Visible = false;
        }

        if (_linesGrid.Columns["UnitPrice"] != null)
        {
            _linesGrid.Columns["UnitPrice"]
                .DefaultCellStyle.Format =
                "₱#,##0.00";
        }

        if (_linesGrid.Columns["Subtotal"] != null)
        {
            _linesGrid.Columns["Subtotal"]
                .DefaultCellStyle.Format =
                "₱#,##0.00";
        }
    }

    private OrderResponse? GetSelectedOrder()
    {
        if (_ordersGrid.CurrentRow == null)
        {
            return null;
        }

        object? idValue =
            _ordersGrid.CurrentRow.Cells["Id"].Value;

        if (!int.TryParse(
                idValue?.ToString(),
                out int orderId))
        {
            return null;
        }

        return _orders.FirstOrDefault(
            order => order.Id == orderId);
    }

    private OrderLineResponse? GetSelectedOrderLine(
        OrderResponse order)
    {
        if (_linesGrid.CurrentRow == null)
        {
            return null;
        }

        object? idValue =
            _linesGrid.CurrentRow.Cells["Id"].Value;

        if (!int.TryParse(
                idValue?.ToString(),
                out int lineId))
        {
            return null;
        }

        return order.Lines.FirstOrDefault(
            line => line.Id == lineId);
    }

    private async void CancelOrderButton_Click(
        object? sender,
        EventArgs e)
    {
        OrderResponse? order = GetSelectedOrder();

        if (order == null)
        {
            ShowWarning("Please select an order first.");
            return;
        }

        if (order.Status.Equals(
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
        {
            ShowWarning("This order is already cancelled.");
            return;
        }

        using var reasonDialog = new ReasonDialog(
            "Cancel Order",
            "Reason for cancellation");

        if (reasonDialog.ShowDialog(FindForm()) !=
            DialogResult.OK)
        {
            return;
        }

        DialogResult confirmation = MessageBox.Show(
            $"Cancel order {order.OrderNumber}?\n\n" +
            "The sold quantities will be returned to inventory.",

            "Confirm Cancellation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await ApiService.Instance.CancelOrderAsync(
                order.Id,
                new OrderActionRequest
                {
                    Reason = reasonDialog.Reason,
                    PerformedBy = UserSession.Username
                });

            MessageBox.Show(
                "Order cancelled successfully.",
                "Order Cancelled",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message, "Cancellation Failed");
        }
    }

    private async void ReturnItemButton_Click(
        object? sender,
        EventArgs e)
    {
        OrderResponse? order = GetSelectedOrder();

        if (order == null)
        {
            ShowWarning("Please select an order first.");
            return;
        }

        OrderLineResponse? line =
            GetSelectedOrderLine(order);

        if (line == null)
        {
            ShowWarning(
                "Please select a product from the lower table.");

            return;
        }

        if (line.AvailableForReturnOrExchange <= 0)
        {
            ShowWarning(
                "No remaining quantity is available for return.");

            return;
        }

        using var dialog = new QuantityReasonDialog(
            "Return Item",
            line.DisplayName,
            line.AvailableForReturnOrExchange);

        if (dialog.ShowDialog(FindForm()) !=
            DialogResult.OK)
        {
            return;
        }

        try
        {
            await ApiService.Instance.ReturnOrderItemAsync(
                order.Id,
                new ReturnOrderItemRequest
                {
                    OrderLineId = line.Id,
                    Quantity = dialog.Quantity,
                    Reason = dialog.Reason,
                    PerformedBy = UserSession.Username
                });

            MessageBox.Show(
                "Product return recorded successfully.\n" +
                "The returned quantity was added back to inventory.",

                "Return Completed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message, "Return Failed");
        }
    }

    private async void ExchangeItemButton_Click(
        object? sender,
        EventArgs e)
    {
        OrderResponse? order = GetSelectedOrder();

        if (order == null)
        {
            ShowWarning("Please select an order first.");
            return;
        }

        OrderLineResponse? line =
            GetSelectedOrderLine(order);

        if (line == null)
        {
            ShowWarning(
                "Please select a product from the lower table.");

            return;
        }

        if (line.AvailableForReturnOrExchange <= 0)
        {
            ShowWarning(
                "No remaining quantity is available for exchange.");

            return;
        }

        try
        {
            List<Item> allItems =
                await ApiService.Instance.GetItemsAsync();

            List<Item> exchangeChoices = allItems
                .Where(item =>
                    item.IsActive &&
                    item.Quantity > 0 &&
                    item.Id != line.ItemId &&
                    item.Name.Equals(
                        line.ItemName,
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Size)
                .ToList();

            if (exchangeChoices.Count == 0)
            {
                ShowWarning(
                    "No other available size was found " +
                    "for this sneaker model.");

                return;
            }

            using var dialog = new ExchangeDialog(
                line.DisplayName,
                line.AvailableForReturnOrExchange,
                exchangeChoices);

            if (dialog.ShowDialog(FindForm()) !=
                DialogResult.OK)
            {
                return;
            }

            await ApiService.Instance.ExchangeOrderItemAsync(
                order.Id,
                new ExchangeOrderItemRequest
                {
                    OrderLineId = line.Id,
                    NewItemId = dialog.NewItemId,
                    Quantity = dialog.Quantity,
                    Reason = dialog.Reason,
                    PerformedBy = UserSession.Username
                });

            MessageBox.Show(
                "Size exchange completed successfully.\n" +
                "Both sneaker stock levels were adjusted.",

                "Exchange Completed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadOrdersAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message, "Exchange Failed");
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

    private static void ShowWarning(string message)
    {
        MessageBox.Show(
            message,
            "Required Selection",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private static void ShowError(
        string message,
        string title)
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private sealed class ReasonDialog : Form
    {
        private readonly TextBox _reasonTextBox;

        public string Reason { get; private set; } =
            string.Empty;

        public ReasonDialog(
            string title,
            string labelText)
        {
            Text = title;
            ClientSize = new Size(460, 260);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = AppTheme.PageBackground;
            Font = new Font("Segoe UI", 10);

            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(30, 20),
                Size = new Size(390, 35),
                ForeColor = AppTheme.PrimaryRed,
                Font = new Font(
                    "Segoe UI",
                    18,
                    FontStyle.Bold)
            };

            var reasonLabel = new Label
            {
                Text = labelText,
                Location = new Point(30, 70),
                AutoSize = true
            };

            _reasonTextBox = new TextBox
            {
                Location = new Point(30, 95),
                Size = new Size(390, 80),
                Multiline = true
            };

            var cancelButton = new Button
            {
                Text = "CANCEL",
                Location = new Point(30, 195),
                Size = new Size(180, 40)
            };

            AppTheme.StyleSecondaryButton(cancelButton);

            cancelButton.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            var confirmButton = new Button
            {
                Text = "CONFIRM",
                Location = new Point(240, 195),
                Size = new Size(180, 40)
            };

            AppTheme.StylePrimaryButton(confirmButton);
            confirmButton.Click += ConfirmButton_Click;

            Controls.Add(titleLabel);
            Controls.Add(reasonLabel);
            Controls.Add(_reasonTextBox);
            Controls.Add(cancelButton);
            Controls.Add(confirmButton);

            AcceptButton = confirmButton;
            CancelButton = cancelButton;
        }

        private void ConfirmButton_Click(
            object? sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                    _reasonTextBox.Text))
            {
                ShowWarning("Please enter a reason.");
                return;
            }

            Reason = _reasonTextBox.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private sealed class QuantityReasonDialog : Form
    {
        private readonly NumericUpDown _quantityInput;
        private readonly TextBox _reasonTextBox;

        public int Quantity { get; private set; }

        public string Reason { get; private set; } =
            string.Empty;

        public QuantityReasonDialog(
            string title,
            string product,
            int maximumQuantity)
        {
            Text = title;
            ClientSize = new Size(480, 350);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = AppTheme.PageBackground;
            Font = new Font("Segoe UI", 10);

            var titleLabel = new Label
            {
                Text = title,
                Location = new Point(30, 20),
                Size = new Size(420, 35),
                ForeColor = AppTheme.PrimaryRed,
                Font = new Font(
                    "Segoe UI",
                    18,
                    FontStyle.Bold)
            };

            var productLabel = new Label
            {
                Text = product,
                Location = new Point(30, 60),
                Size = new Size(420, 45),
                ForeColor = AppTheme.TextDark
            };

            var quantityLabel = new Label
            {
                Text =
                    $"Quantity (maximum {maximumQuantity})",

                Location = new Point(30, 115),
                AutoSize = true
            };

            _quantityInput = new NumericUpDown
            {
                Location = new Point(30, 140),
                Size = new Size(420, 32),
                Minimum = 1,
                Maximum = maximumQuantity,
                Value = 1
            };

            var reasonLabel = new Label
            {
                Text = "Reason",
                Location = new Point(30, 190),
                AutoSize = true
            };

            _reasonTextBox = new TextBox
            {
                Location = new Point(30, 215),
                Size = new Size(420, 60),
                Multiline = true
            };

            var cancelButton = new Button
            {
                Text = "CANCEL",
                Location = new Point(30, 292),
                Size = new Size(190, 40)
            };

            AppTheme.StyleSecondaryButton(cancelButton);

            cancelButton.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            var confirmButton = new Button
            {
                Text = "CONFIRM",
                Location = new Point(260, 292),
                Size = new Size(190, 40)
            };

            AppTheme.StylePrimaryButton(confirmButton);
            confirmButton.Click += ConfirmButton_Click;

            Controls.Add(titleLabel);
            Controls.Add(productLabel);
            Controls.Add(quantityLabel);
            Controls.Add(_quantityInput);
            Controls.Add(reasonLabel);
            Controls.Add(_reasonTextBox);
            Controls.Add(cancelButton);
            Controls.Add(confirmButton);

            AcceptButton = confirmButton;
            CancelButton = cancelButton;
        }

        private void ConfirmButton_Click(
            object? sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                    _reasonTextBox.Text))
            {
                ShowWarning("Please enter a reason.");
                return;
            }

            Quantity = (int)_quantityInput.Value;
            Reason = _reasonTextBox.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private sealed class ExchangeDialog : Form
    {
        private readonly ComboBox _newItemComboBox;
        private readonly NumericUpDown _quantityInput;
        private readonly TextBox _reasonTextBox;

        public int NewItemId { get; private set; }

        public int Quantity { get; private set; }

        public string Reason { get; private set; } =
            string.Empty;

        public ExchangeDialog(
            string originalProduct,
            int maximumQuantity,
            List<Item> exchangeChoices)
        {
            Text = "Size Exchange";
            ClientSize = new Size(520, 430);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = AppTheme.PageBackground;
            Font = new Font("Segoe UI", 10);

            var titleLabel = new Label
            {
                Text = "SIZE EXCHANGE",
                Location = new Point(35, 20),
                Size = new Size(450, 38),
                ForeColor = AppTheme.PrimaryRed,
                Font = new Font(
                    "Segoe UI",
                    19,
                    FontStyle.Bold)
            };

            var originalLabel = new Label
            {
                Text = $"Original: {originalProduct}",
                Location = new Point(35, 65),
                Size = new Size(450, 45),
                ForeColor = AppTheme.TextDark
            };

            var replacementLabel = new Label
            {
                Text = "Replacement size",
                Location = new Point(35, 120),
                AutoSize = true
            };

            _newItemComboBox = new ComboBox
            {
                Location = new Point(35, 145),
                Size = new Size(450, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = nameof(Item.DisplayName),
                ValueMember = nameof(Item.Id),
                DataSource = exchangeChoices
            };

            var quantityLabel = new Label
            {
                Text =
                    $"Quantity (maximum {maximumQuantity})",

                Location = new Point(35, 195),
                AutoSize = true
            };

            _quantityInput = new NumericUpDown
            {
                Location = new Point(35, 220),
                Size = new Size(450, 32),
                Minimum = 1,
                Maximum = maximumQuantity,
                Value = 1
            };

            var reasonLabel = new Label
            {
                Text = "Reason for exchange",
                Location = new Point(35, 270),
                AutoSize = true
            };

            _reasonTextBox = new TextBox
            {
                Location = new Point(35, 295),
                Size = new Size(450, 60),
                Multiline = true
            };

            var cancelButton = new Button
            {
                Text = "CANCEL",
                Location = new Point(35, 373),
                Size = new Size(210, 40)
            };

            AppTheme.StyleSecondaryButton(cancelButton);

            cancelButton.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            var confirmButton = new Button
            {
                Text = "COMPLETE EXCHANGE",
                Location = new Point(275, 373),
                Size = new Size(210, 40)
            };

            AppTheme.StylePrimaryButton(confirmButton);
            confirmButton.Click += ConfirmButton_Click;

            Controls.Add(titleLabel);
            Controls.Add(originalLabel);
            Controls.Add(replacementLabel);
            Controls.Add(_newItemComboBox);
            Controls.Add(quantityLabel);
            Controls.Add(_quantityInput);
            Controls.Add(reasonLabel);
            Controls.Add(_reasonTextBox);
            Controls.Add(cancelButton);
            Controls.Add(confirmButton);

            AcceptButton = confirmButton;
            CancelButton = cancelButton;
        }

        private void ConfirmButton_Click(
            object? sender,
            EventArgs e)
        {
            if (_newItemComboBox.SelectedItem
                is not Item replacementItem)
            {
                ShowWarning(
                    "Please select a replacement size.");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    _reasonTextBox.Text))
            {
                ShowWarning(
                    "Please enter the reason for exchange.");

                return;
            }

            NewItemId = replacementItem.Id;
            Quantity = (int)_quantityInput.Value;
            Reason = _reasonTextBox.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}