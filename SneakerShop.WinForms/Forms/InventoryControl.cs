using System.Drawing;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using SneakerShop.WinForms.Services;
using SneakerShop.WinForms.Styles;

namespace SneakerShop.WinForms.Forms;

public class InventoryControl : UserControl
{
    private static readonly HttpClient HttpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5000/")
    };

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly DataGridView _inventoryGrid;
    private readonly TextBox _searchTextBox;
    private readonly CheckBox _showArchivedCheckBox;
    private readonly Label _statusLabel;

    private List<SneakerItemDto> _items = new();

    public InventoryControl()
    {
        Dock = DockStyle.Fill;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 145,
            BackColor = Color.White
        };

        var sectionLabel = new Label
        {
            Text = "PRODUCT MANAGEMENT",
            Location = new Point(25, 15),
            Size = new Size(250, 20),
            ForeColor = AppTheme.PrimaryRed,
            Font = new Font("Segoe UI", 8, FontStyle.Bold)
        };

        var titleLabel = new Label
        {
            Text = "Sneaker Inventory",
            Location = new Point(25, 35),
            Size = new Size(350, 38),
            ForeColor = AppTheme.TextDark,
            Font = new Font("Segoe UI", 20, FontStyle.Bold)
        };

        _searchTextBox = new TextBox
        {
            Location = new Point(25, 90),
            Size = new Size(300, 32),
            PlaceholderText = "Search by name, code, or brand"
        };

        AppTheme.StyleTextBox(_searchTextBox);
        _searchTextBox.TextChanged += (_, _) => ApplySearch();

        _showArchivedCheckBox = new CheckBox
        {
            Text = "Show archived",
            Location = new Point(340, 94),
            AutoSize = true,
            ForeColor = AppTheme.TextMuted
        };

        _showArchivedCheckBox.CheckedChanged += async (_, _) =>
        {
            await LoadItemsAsync();
        };

        var refreshButton = new Button
        {
            Text = "REFRESH",
            Location = new Point(480, 86),
            Size = new Size(105, 38)
        };

        AppTheme.StyleSecondaryButton(refreshButton);

        refreshButton.Click += async (_, _) =>
        {
            await LoadItemsAsync();
        };

        var addButton = new Button
        {
            Text = "+ ADD SNEAKER",
            Location = new Point(600, 86),
            Size = new Size(140, 38),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        AppTheme.StylePrimaryButton(addButton);
        addButton.Click += AddButton_Click;

        var editButton = new Button
        {
            Text = "EDIT",
            Location = new Point(750, 86),
            Size = new Size(90, 38)
        };

        AppTheme.StyleSecondaryButton(editButton);
        editButton.Click += EditButton_Click;

        var deleteButton = new Button
        {
            Text = "ARCHIVE",
            Location = new Point(850, 86),
            Size = new Size(100, 38)
        };

        AppTheme.StyleDangerButton(deleteButton);
        deleteButton.Click += DeleteButton_Click;

        var restoreButton = new Button
        {
            Text = "RESTORE",
            Location = new Point(960, 86),
            Size = new Size(100, 38)
        };

        AppTheme.StyleSecondaryButton(restoreButton);
        restoreButton.Click += RestoreButton_Click;

        headerPanel.Controls.Add(sectionLabel);
        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(_searchTextBox);
        headerPanel.Controls.Add(_showArchivedCheckBox);
        headerPanel.Controls.Add(refreshButton);
        headerPanel.Controls.Add(addButton);
        headerPanel.Controls.Add(editButton);
        headerPanel.Controls.Add(deleteButton);
        headerPanel.Controls.Add(restoreButton);

        _inventoryGrid = new DataGridView
        {
            Dock = DockStyle.Fill
        };

        AppTheme.StyleDataGridView(_inventoryGrid);

        _inventoryGrid.CellDoubleClick += (_, eventArgs) =>
        {
            if (eventArgs.RowIndex >= 0)
            {
                EditSelectedItem();
            }
        };

        var statusPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 42,
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

        Controls.Add(_inventoryGrid);
        Controls.Add(statusPanel);
        Controls.Add(headerPanel);

        Load += async (_, _) =>
        {
            await LoadItemsAsync();
        };
    }

    private async Task LoadItemsAsync()
    {
        try
        {
            SetStatus("Loading sneaker inventory...", false);

            string endpoint = _showArchivedCheckBox.Checked
                ? "api/items?includeInactive=true"
                : "api/items";

            using HttpResponseMessage response =
                await HttpClient.GetAsync(endpoint);

            await EnsureSuccessAsync(response);

            string json = await response.Content.ReadAsStringAsync();

            _items =
                JsonSerializer.Deserialize<List<SneakerItemDto>>(
                    json,
                    JsonOptions)
                ?? new List<SneakerItemDto>();

            ApplySearch();

            SetStatus(
                $"{_items.Count} sneaker record(s) loaded.",
                true);
        }
        catch (Exception ex)
        {
            _items.Clear();
            _inventoryGrid.DataSource = null;

            SetStatus("Unable to load inventory.", false);

            MessageBox.Show(
                ex.Message,
                "Inventory Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ApplySearch()
    {
        string search = _searchTextBox.Text.Trim();

        IEnumerable<SneakerItemDto> filteredItems = _items;

        if (!string.IsNullOrWhiteSpace(search))
        {
            filteredItems = filteredItems.Where(item =>
                item.Name.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||

                item.Code.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||

                item.Brand.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||

                item.Colorway.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||

                item.BoxCode.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase));
        }

        var rows = filteredItems
            .Select(item => new
            {
                item.Id,
                Sneaker = item.Name,
                item.Code,
                item.Brand,
                item.Colorway,
                Size = item.DisplaySize,
                UnitPrice = item.UnitPrice,
                Stock = item.AvailableQuantity,
                ReorderLevel = item.ReorderLevel,
                Authenticity = item.AuthenticityStatus,
                Status = item.IsActive ? "Active" : "Archived"
            })
            .ToList();

        _inventoryGrid.DataSource = rows;

        if (_inventoryGrid.Columns["Id"] != null)
        {
            _inventoryGrid.Columns["Id"].Visible = false;
        }

        if (_inventoryGrid.Columns["UnitPrice"] != null)
        {
            _inventoryGrid.Columns["UnitPrice"]
                .DefaultCellStyle.Format = "₱#,##0.00";
        }

        if (_inventoryGrid.Columns["Sneaker"] != null)
        {
            _inventoryGrid.Columns["Sneaker"].FillWeight = 160;
        }
    }

    private async void AddButton_Click(
        object? sender,
        EventArgs e)
    {
        using var editor = new SneakerEditorForm();

        if (editor.ShowDialog(FindForm()) != DialogResult.OK)
        {
            return;
        }

        try
        {
            SetStatus("Adding sneaker...", false);

            using HttpResponseMessage response =
                await HttpClient.PostAsJsonAsync(
                    "api/items",
                    editor.Payload);

            await EnsureSuccessAsync(response);

            MessageBox.Show(
                "Sneaker added successfully.",
                "Add Sneaker",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Add Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void EditButton_Click(
        object? sender,
        EventArgs e)
    {
        EditSelectedItem();
    }

    private async void EditSelectedItem()
    {
        SneakerItemDto? selectedItem = GetSelectedItem();

        if (selectedItem == null)
        {
            MessageBox.Show(
                "Please select a sneaker first.",
                "No Selection",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (!selectedItem.IsActive)
        {
            MessageBox.Show(
                "Restore this sneaker before editing it.",
                "Archived Sneaker",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        using var editor =
            new SneakerEditorForm(selectedItem);

        if (editor.ShowDialog(FindForm()) != DialogResult.OK)
        {
            return;
        }

        try
        {
            SetStatus("Updating sneaker...", false);

            using HttpResponseMessage response =
                await HttpClient.PutAsJsonAsync(
                    $"api/items/{selectedItem.Id}",
                    editor.Payload);

            await EnsureSuccessAsync(response);

            MessageBox.Show(
                "Sneaker updated successfully.",
                "Update Sneaker",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Update Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async void DeleteButton_Click(
        object? sender,
        EventArgs e)
    {
        SneakerItemDto? selectedItem = GetSelectedItem();

        if (selectedItem == null)
        {
            MessageBox.Show(
                "Please select a sneaker first.",
                "No Selection",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        if (!selectedItem.IsActive)
        {
            MessageBox.Show(
                "The selected sneaker is already archived.",
                "Already Archived",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            return;
        }

        DialogResult answer = MessageBox.Show(
            $"Archive {selectedItem.Name}?",
            "Confirm Archive",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != DialogResult.Yes)
        {
            return;
        }

        try
        {
            using HttpResponseMessage response =
                await HttpClient.DeleteAsync(
                    $"api/items/{selectedItem.Id}");

            await EnsureSuccessAsync(response);

            MessageBox.Show(
                "Sneaker archived successfully.",
                "Archive Sneaker",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await LoadItemsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Archive Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

private async void RestoreButton_Click(
    object? sender,
    EventArgs e)
{
    SneakerItemDto? selectedItem =
        GetSelectedItem();

    if (selectedItem == null)
    {
        MessageBox.Show(
            "Select an archived sneaker first.",
            "No Selection",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);

        return;
    }

    if (selectedItem.IsActive)
    {
        MessageBox.Show(
            "The selected sneaker is already active.",
            "Active Sneaker",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        return;
    }

    try
    {
        SetStatus("Restoring sneaker...", false);

        using StringContent content = new(
            "{}",
            System.Text.Encoding.UTF8,
            "application/json");

        using HttpResponseMessage response =
            await HttpClient.PatchAsync(
                $"api/items/{selectedItem.Id}/restore",
                content);

        await EnsureSuccessAsync(response);

        MessageBox.Show(
            "Sneaker restored successfully.",
            "Restore Sneaker",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        await LoadItemsAsync();

        SetStatus(
            "Sneaker restored successfully.",
            false);
    }
    catch (Exception ex)
    {
        SetStatus("Restore failed.", true);

        MessageBox.Show(
            ex.Message,
            "Restore Failed",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}

    private SneakerItemDto? GetSelectedItem()
    {
        if (_inventoryGrid.CurrentRow == null)
        {
            return null;
        }

        object? idValue =
            _inventoryGrid.CurrentRow.Cells["Id"].Value;

        if (!int.TryParse(
                idValue?.ToString(),
                out int selectedId))
        {
            return null;
        }

        return _items.FirstOrDefault(
            item => item.Id == selectedId);
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

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string responseText =
            await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseText))
        {
            responseText =
                $"Server returned {response.StatusCode}.";
        }

        throw new Exception(responseText);
    }

    private sealed class SneakerItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; } =
            string.Empty;

        public string Code { get; set; } =
            string.Empty;

        public string Brand { get; set; } =
            string.Empty;

        public string Colorway { get; set; } =
            string.Empty;

        public string Category { get; set; } =
            string.Empty;

        public string ReleaseType { get; set; } =
            "Regular";

        public string AuthenticityStatus { get; set; } =
            "Pending";

        public string BoxCode { get; set; } =
            string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public int Stock { get; set; }

        public decimal ShoeSize { get; set; }

        public decimal Size { get; set; }

        public int ReorderLevel { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public int AvailableQuantity =>
            Quantity != 0 ? Quantity : Stock;

        [JsonIgnore]
        public decimal DisplaySize =>
            ShoeSize != 0 ? ShoeSize : Size;
    }

    private sealed class SneakerPayload
    {
        public string Name { get; set; } =
            string.Empty;

        public string Code { get; set; } =
            string.Empty;

        public string Brand { get; set; } =
            string.Empty;

        public string Colorway { get; set; } =
            string.Empty;

        public string Category { get; set; } =
            string.Empty;

        public string ReleaseType { get; set; } =
            "Regular";

        public string AuthenticityStatus { get; set; } =
            "Pending";

        public string BoxCode { get; set; } =
            string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public int Stock { get; set; }

        public decimal ShoeSize { get; set; }

        public decimal Size { get; set; }

        public int ReorderLevel { get; set; }

        public bool IsActive { get; set; } = true;

        public string PerformedBy { get; set; } =
            string.Empty;
    }

    private sealed class SneakerEditorForm : Form
    {
        private readonly TextBox _nameTextBox;
        private readonly TextBox _codeTextBox;
        private readonly TextBox _brandTextBox;
        private readonly TextBox _colorwayTextBox;
        private readonly TextBox _boxCodeTextBox;
        private readonly ComboBox _categoryComboBox;
        private readonly ComboBox _releaseTypeComboBox;
        private readonly ComboBox _authenticityComboBox;
        private readonly NumericUpDown _priceInput;
        private readonly NumericUpDown _quantityInput;
        private readonly ComboBox _sizeComboBox;
        private readonly NumericUpDown _reorderInput;

        public SneakerPayload Payload { get; private set; } =
            new();

        public SneakerEditorForm(
            SneakerItemDto? existingItem = null)
        {
            Text = existingItem == null
                ? "Add Sneaker"
                : "Edit Sneaker";

            ClientSize = new Size(850, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = AppTheme.PageBackground;
            Font = new Font("Segoe UI", 10);

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = AppTheme.DarkRed
            };

            var titleLabel = new Label
            {
                Text = existingItem == null
                    ? "ADD NEW SNEAKER"
                    : "EDIT SNEAKER",

                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            headerPanel.Controls.Add(titleLabel);
            Controls.Add(headerPanel);

            _nameTextBox =
                AddTextField(
                    "Sneaker name",
                    40,
                    105,
                    360);

            _codeTextBox =
                AddTextField(
                    "Product code",
                    40,
                    175,
                    360);

            _brandTextBox =
                AddTextField(
                    "Brand",
                    40,
                    245,
                    360);

            _colorwayTextBox =
                AddTextField(
                    "Colorway",
                    40,
                    315,
                    360);

            _categoryComboBox =
                AddComboField(
                    "Category",
                    40,
                    385,
                    360,
                    new[]
                    {
                        "Lifestyle",
                        "Running",
                        "Basketball",
                        "Training",
                        "Skateboarding",
                        "Limited Edition"
                    });

            _boxCodeTextBox =
                AddTextField(
                    "Box code / serial reference",
                    40,
                    455,
                    360);

            _priceInput =
                AddNumberField(
                    "Unit price",
                    450,
                    105,
                    360,
                    0,
                    1000000,
                    2);

            _quantityInput =
                AddNumberField(
                    "Available quantity",
                    450,
                    175,
                    360,
                    0,
                    100000,
                    0);

            _sizeComboBox =
                AddSizeField(
                    "Available size",
                    450,
                    245,
                    360);

            _reorderInput =
                AddNumberField(
                    "Reorder level",
                    450,
                    315,
                    360,
                    0,
                    10000,
                    0);

            _releaseTypeComboBox =
                AddComboField(
                    "Release type",
                    450,
                    385,
                    360,
                    new[]
                    {
                        "Regular",
                        "Limited",
                        "Exclusive",
                        "Collaboration"
                    });

            _authenticityComboBox =
                AddComboField(
                    "Authenticity status",
                    450,
                    455,
                    360,
                    new[]
                    {
                        "Pending",
                        "Verified",
                        "Rejected"
                    });

            var saveButton = new Button
            {
                Text = existingItem == null
                    ? "SAVE SNEAKER"
                    : "SAVE CHANGES",

                Location = new Point(610, 565),
                Size = new Size(200, 44),
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold)
            };

            AppTheme.StylePrimaryButton(saveButton);
            saveButton.Click += SaveButton_Click;

            var cancelButton = new Button
            {
                Text = "CANCEL",
                Location = new Point(390, 565),
                Size = new Size(200, 44)
            };

            AppTheme.StyleSecondaryButton(cancelButton);

            cancelButton.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(saveButton);
            Controls.Add(cancelButton);

            if (existingItem != null)
            {
                _nameTextBox.Text = existingItem.Name;
                _codeTextBox.Text = existingItem.Code;
                _brandTextBox.Text = existingItem.Brand;
                _colorwayTextBox.Text = existingItem.Colorway;
                _boxCodeTextBox.Text = existingItem.BoxCode;

                SelectComboValue(
                    _categoryComboBox,
                    existingItem.Category,
                    "Lifestyle");

                SelectComboValue(
                    _releaseTypeComboBox,
                    existingItem.ReleaseType,
                    "Regular");

                SelectComboValue(
                    _authenticityComboBox,
                    existingItem.AuthenticityStatus,
                    "Pending");

                _priceInput.Value = LimitValue(
                    _priceInput,
                    existingItem.UnitPrice);

                _quantityInput.Value = LimitValue(
                    _quantityInput,
                    existingItem.AvailableQuantity);

                SelectSizeValue(
                    _sizeComboBox,
                    existingItem.DisplaySize <= 0
                        ? 4
                        : existingItem.DisplaySize);

                _reorderInput.Value = LimitValue(
                    _reorderInput,
                    existingItem.ReorderLevel);
            }

            AcceptButton = saveButton;
            CancelButton = cancelButton;
        }

        private TextBox AddTextField(
            string labelText,
            int x,
            int y,
            int width)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = AppTheme.TextDark
            };

            var textBox = new TextBox
            {
                Location = new Point(x, y + 24),
                Size = new Size(width, 32)
            };

            AppTheme.StyleTextBox(textBox);

            Controls.Add(label);
            Controls.Add(textBox);

            return textBox;
        }

        private ComboBox AddComboField(
            string labelText,
            int x,
            int y,
            int width,
            IEnumerable<string> choices)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = AppTheme.TextDark
            };

            var comboBox = new ComboBox
            {
                Location = new Point(x, y + 24),
                Size = new Size(width, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };

            foreach (string choice in choices)
            {
                comboBox.Items.Add(choice);
            }

            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }

            Controls.Add(label);
            Controls.Add(comboBox);

            return comboBox;
        }

        private NumericUpDown AddNumberField(
            string labelText,
            int x,
            int y,
            int width,
            decimal minimum,
            decimal maximum,
            int decimalPlaces)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = AppTheme.TextDark
            };

            var numberInput = new NumericUpDown
            {
                Location = new Point(x, y + 24),
                Size = new Size(width, 32),
                Minimum = minimum,
                Maximum = maximum,
                DecimalPlaces = decimalPlaces,
                ThousandsSeparator = true,
                Font = new Font("Segoe UI", 11)
            };

            Controls.Add(label);
            Controls.Add(numberInput);

            return numberInput;
        }

        private ComboBox AddSizeField(
            string labelText,
            int x,
            int y,
            int width)
        {
            ComboBox comboBox = AddComboField(
                labelText,
                x,
                y,
                width,
                Enumerable.Range(8, 23)
                    .Select(value => (value / 2M)
                        .ToString("0.0")));

            return comboBox;
        }

        private static void SelectSizeValue(
            ComboBox comboBox,
            decimal size)
        {
            string sizeText = size.ToString("0.0");
            object? match = comboBox.Items
                .Cast<object>()
                .FirstOrDefault(item =>
                    string.Equals(
                        item.ToString(),
                        sizeText,
                        StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                comboBox.Items.Add(sizeText);
                match = sizeText;
            }

            comboBox.SelectedItem = match;
        }

        private static decimal LimitValue(
            NumericUpDown input,
            decimal value)
        {
            return Math.Min(
                input.Maximum,
                Math.Max(input.Minimum, value));
        }

        private static void SelectComboValue(
            ComboBox comboBox,
            string? value,
            string defaultValue)
        {
            string selectedValue =
                string.IsNullOrWhiteSpace(value)
                    ? defaultValue
                    : value;

            object? match = comboBox.Items
                .Cast<object>()
                .FirstOrDefault(item =>
                    string.Equals(
                        item.ToString(),
                        selectedValue,
                        StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                comboBox.Items.Add(selectedValue);
                match = selectedValue;
            }

            comboBox.SelectedItem = match;
        }

        private void SaveButton_Click(
            object? sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                    _nameTextBox.Text))
            {
                ShowValidation(
                    "Sneaker name is required.");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    _codeTextBox.Text))
            {
                ShowValidation(
                    "Product code is required.");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    _brandTextBox.Text))
            {
                ShowValidation(
                    "Brand is required.");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    _colorwayTextBox.Text))
            {
                ShowValidation(
                    "Colorway is required.");

                return;
            }

            if (_priceInput.Value <= 0)
            {
                ShowValidation(
                    "Unit price must be greater than zero.");

                return;
            }

            string authenticityStatus =
                _authenticityComboBox.SelectedItem?
                    .ToString() ?? "Pending";

            if (authenticityStatus == "Verified" &&
                string.IsNullOrWhiteSpace(
                    _boxCodeTextBox.Text))
            {
                ShowValidation(
                    "A box code is required before a sneaker can be marked Verified.");

                return;
            }

            if (!decimal.TryParse(
                    _sizeComboBox.SelectedItem?.ToString(),
                    out decimal selectedSize))
            {
                ShowValidation(
                    "Select an available shoe size.");

                return;
            }

            Payload = new SneakerPayload
            {
                Name = _nameTextBox.Text.Trim(),
                Code = _codeTextBox.Text.Trim(),
                Brand = _brandTextBox.Text.Trim(),
                Colorway = _colorwayTextBox.Text.Trim(),
                Category =
                    _categoryComboBox.SelectedItem?
                        .ToString() ?? "Lifestyle",
                ReleaseType =
                    _releaseTypeComboBox.SelectedItem?
                        .ToString() ?? "Regular",
                AuthenticityStatus =
                    authenticityStatus,
                BoxCode = _boxCodeTextBox.Text.Trim(),
                UnitPrice = _priceInput.Value,
                Quantity = (int)_quantityInput.Value,
                Stock = (int)_quantityInput.Value,
                ShoeSize = selectedSize,
                Size = selectedSize,
                ReorderLevel = (int)_reorderInput.Value,
                IsActive = true,
                PerformedBy = UserSession.Username
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private static void ShowValidation(
            string message)
        {
            MessageBox.Show(
                message,
                "Invalid Sneaker Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
