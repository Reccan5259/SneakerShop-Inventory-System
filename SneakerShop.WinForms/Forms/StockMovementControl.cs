using System.Drawing;
using System.Windows.Forms;
using SneakerShop.WinForms.Models;
using SneakerShop.WinForms.Services;
using SneakerShop.WinForms.Styles;

namespace SneakerShop.WinForms.Forms;

public class StockMovementControl : UserControl
{
    private readonly ComboBox _itemComboBox;
    private readonly ComboBox _operationComboBox;
    private readonly NumericUpDown _quantityInput;
    private readonly TextBox _referenceTextBox;
    private readonly TextBox _notesTextBox;
    private readonly DataGridView _historyGrid;
    private readonly Label _availableStockLabel;
    private readonly Label _statusLabel;
    private readonly Button _submitButton;

    public StockMovementControl()
    {
        Dock = DockStyle.Fill;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10);

        var formPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 290,
            BackColor = Color.White
        };

        var sectionLabel = new Label
        {
            Text = "INVENTORY CONTROL",
            Location = new Point(25, 15),
            Size = new Size(250, 20),
            ForeColor = AppTheme.PrimaryRed,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };

        var titleLabel = new Label
        {
            Text = "Record Stock Movement",
            Location = new Point(25, 35),
            Size = new Size(400, 38),
            ForeColor = AppTheme.TextDark,
            Font = new Font("Segoe UI", 20, FontStyle.Bold)
        };

        var itemLabel = CreateLabel(
            "Sneaker product",
            25,
            90);

        _itemComboBox = new ComboBox
        {
            Location = new Point(25, 115),
            Size = new Size(350, 32),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _itemComboBox.SelectedIndexChanged += (_, _) =>
        {
            UpdateAvailableStock();
        };

        var operationLabel = CreateLabel(
            "Movement type",
            395,
            90);

        _operationComboBox = new ComboBox
        {
            Location = new Point(395, 115),
            Size = new Size(180, 32),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _operationComboBox.Items.AddRange(
            new object[]
            {
                "Stock In",
                "Stock Out",
                "Damaged",
                "Customer Return"
            });

        _operationComboBox.SelectedIndex = 0;

        var quantityLabel = CreateLabel(
            "Quantity",
            595,
            90);

        _quantityInput = new NumericUpDown
        {
            Location = new Point(595, 115),
            Size = new Size(130, 32),
            Minimum = 1,
            Maximum = 100000,
            Value = 1,
            ThousandsSeparator = true,
            Font = new Font("Segoe UI", 11)
        };

        _availableStockLabel = new Label
        {
            Text = "Available stock: 0",
            Location = new Point(745, 117),
            Size = new Size(220, 28),
            ForeColor = AppTheme.PrimaryRed,
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Bold)
        };

        var referenceLabel = CreateLabel(
            "Reference number",
            25,
            165);

        _referenceTextBox = new TextBox
        {
            Location = new Point(25, 190),
            Size = new Size(250, 32),
            PlaceholderText = "Example: PO-2026-001"
        };

        AppTheme.StyleTextBox(_referenceTextBox);

        var notesLabel = CreateLabel(
            "Notes",
            295,
            165);

        _notesTextBox = new TextBox
        {
            Location = new Point(295, 190),
            Size = new Size(430, 55),
            Multiline = true,
            PlaceholderText = "Enter reason or additional details"
        };

        AppTheme.StyleTextBox(_notesTextBox);

        _submitButton = new Button
        {
            Text = "SAVE MOVEMENT",
            Location = new Point(745, 190),
            Size = new Size(180, 45),
            Font = new Font(
                "Segoe UI",
                10,
                FontStyle.Bold)
        };

        AppTheme.StylePrimaryButton(_submitButton);
        _submitButton.Click += SubmitButton_Click;

        var refreshButton = new Button
        {
            Text = "REFRESH HISTORY",
            Location = new Point(935, 190),
            Size = new Size(150, 45)
        };

        AppTheme.StyleSecondaryButton(refreshButton);

        refreshButton.Click += async (_, _) =>
        {
            await LoadDataAsync();
        };

        formPanel.Controls.Add(sectionLabel);
        formPanel.Controls.Add(titleLabel);
        formPanel.Controls.Add(itemLabel);
        formPanel.Controls.Add(_itemComboBox);
        formPanel.Controls.Add(operationLabel);
        formPanel.Controls.Add(_operationComboBox);
        formPanel.Controls.Add(quantityLabel);
        formPanel.Controls.Add(_quantityInput);
        formPanel.Controls.Add(_availableStockLabel);
        formPanel.Controls.Add(referenceLabel);
        formPanel.Controls.Add(_referenceTextBox);
        formPanel.Controls.Add(notesLabel);
        formPanel.Controls.Add(_notesTextBox);
        formPanel.Controls.Add(_submitButton);
        formPanel.Controls.Add(refreshButton);

        var historyHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = AppTheme.LightRed
        };

        var historyTitleLabel = new Label
        {
            Text = "Stock Movement History",
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 0, 0, 0),
            ForeColor = AppTheme.DarkRed,
            Font = new Font(
                "Segoe UI",
                14,
                FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        historyHeaderPanel.Controls.Add(historyTitleLabel);

        _historyGrid = new DataGridView
        {
            Dock = DockStyle.Fill
        };

        AppTheme.StyleDataGridView(_historyGrid);

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

        Controls.Add(_historyGrid);
        Controls.Add(historyHeaderPanel);
        Controls.Add(statusPanel);
        Controls.Add(formPanel);

        Load += async (_, _) =>
        {
            await LoadDataAsync();
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

    private async Task LoadDataAsync()
    {
        try
        {
            SetStatus(
                "Loading inventory and transaction history...",
                false);

            List<Item> items =
                await ApiService.Instance.GetItemsAsync();

            List<Item> activeItems = items
                .Where(item => item.IsActive)
                .OrderBy(item => item.Brand)
                .ThenBy(item => item.Name)
                .ThenBy(item => item.Size)
                .ToList();

            _itemComboBox.DataSource = null;
            _itemComboBox.DisplayMember =
                nameof(Item.DisplayName);
            _itemComboBox.ValueMember =
                nameof(Item.Id);
            _itemComboBox.DataSource = activeItems;

            List<InventoryTransactionRecord> transactions =
                await ApiService.Instance
                    .GetTransactionsAsync();

            var transactionRows = transactions
                .OrderByDescending(
                    transaction => transaction.CreatedAt)
                .Select(transaction => new
                {
                    transaction.Id,
                    Date = transaction.CreatedAt,
                    Type = transaction.TransactionType,
                    Sneaker =
                        transaction.Item?.DisplayName ??
                        $"Item #{transaction.ItemId}",
                    Code =
                        transaction.Item?.Code ??
                        string.Empty,
                    transaction.Quantity,
                    Reference =
                        transaction.ReferenceNumber,
                    Notes = transaction.Notes,
                    PerformedBy =
                        transaction.PerformedBy
                })
                .ToList();

            _historyGrid.DataSource = transactionRows;

            if (_historyGrid.Columns["Id"] != null)
            {
                _historyGrid.Columns["Id"].Visible = false;
            }

            if (_historyGrid.Columns["Date"] != null)
            {
                _historyGrid.Columns["Date"]
                    .DefaultCellStyle.Format =
                    "MMM dd, yyyy hh:mm tt";
            }

            UpdateAvailableStock();

            SetStatus(
                $"{transactionRows.Count} transaction(s) loaded.",
                true);
        }
        catch (Exception ex)
        {
            SetStatus("Unable to load stock movement data.", false);

            MessageBox.Show(
                ex.Message,
                "Stock Movement Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateAvailableStock()
    {
        if (_itemComboBox.SelectedItem is not Item item)
        {
            _availableStockLabel.Text =
                "Available stock: 0";

            return;
        }

        _availableStockLabel.Text =
            $"Available stock: {item.Quantity} pair(s)";

        _availableStockLabel.ForeColor =
            item.Quantity <= item.ReorderLevel
                ? AppTheme.Danger
                : AppTheme.Success;
    }

    private async void SubmitButton_Click(
        object? sender,
        EventArgs e)
    {
        if (_itemComboBox.SelectedItem is not Item selectedItem)
        {
            MessageBox.Show(
                "Please select a sneaker product.",
                "Required Field",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (_operationComboBox.SelectedItem == null)
        {
            MessageBox.Show(
                "Please select a movement type.",
                "Required Field",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        int quantity = (int)_quantityInput.Value;

        string displayedOperation =
            _operationComboBox.SelectedItem.ToString() ??
            string.Empty;

        string operation = displayedOperation switch
        {
            "Stock In" => "stock-in",
            "Stock Out" => "stock-out",
            "Damaged" => "damaged",
            "Customer Return" => "return",
            _ => string.Empty
        };

        bool removesStock =
            operation == "stock-out" ||
            operation == "damaged";

        if (removesStock &&
            quantity > selectedItem.Quantity)
        {
            MessageBox.Show(
                $"Only {selectedItem.Quantity} pair(s) are available.",
                "Insufficient Stock",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (string.IsNullOrWhiteSpace(
                _referenceTextBox.Text))
        {
            MessageBox.Show(
                "Please enter a reference number.",
                "Required Field",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if ((operation == "damaged" ||
             operation == "return") &&
            string.IsNullOrWhiteSpace(_notesTextBox.Text))
        {
            MessageBox.Show(
                "Please enter the reason in the Notes field.",
                "Notes Required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        DialogResult answer = MessageBox.Show(
            $"Record {displayedOperation} of " +
            $"{quantity} pair(s) for\n" +
            $"{selectedItem.DisplayName}?",

            "Confirm Stock Movement",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _submitButton.Enabled = false;
            _submitButton.Text = "SAVING...";

            var request = new StockMovementRequest
            {
                ItemId = selectedItem.Id,
                Quantity = quantity,
                ReferenceNumber =
                    _referenceTextBox.Text.Trim(),
                Notes = _notesTextBox.Text.Trim(),
                PerformedBy =
                    UserSession.Username
            };

            await ApiService.Instance
                .SendStockMovementAsync(
                    operation,
                    request);

            MessageBox.Show(
                "Stock movement recorded successfully.",
                "Movement Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            _quantityInput.Value = 1;
            _referenceTextBox.Clear();
            _notesTextBox.Clear();

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Movement Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _submitButton.Enabled = true;
            _submitButton.Text = "SAVE MOVEMENT";
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
}