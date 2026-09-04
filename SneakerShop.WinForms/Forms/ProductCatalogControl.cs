using System.Drawing;
using SneakerShop.WinForms.Models;
using SneakerShop.WinForms.Services;
using SneakerShop.WinForms.Styles;

namespace SneakerShop.WinForms.Forms
{
    public class ProductCatalogControl : UserControl
    {
        private readonly TextBox _searchTextBox;
        private readonly ComboBox _brandComboBox;
        private readonly ComboBox _categoryComboBox;
        private readonly DataGridView _productGrid;
        private readonly Label _statusLabel;
        private readonly Label _productCountLabel;
        private readonly Label _stockCountLabel;
        private readonly Button _refreshButton;

        private List<Item> _allItems = new();

        public ProductCatalogControl()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.PageBackground;
            Padding = new Padding(24);

            // Header
            Panel headerPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            Label titleLabel = new()
            {
                Text = "Sneaker Catalog",
                Font = new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold),
                ForeColor = AppTheme.DarkRed,
                AutoSize = true,
                Location = new Point(20, 10)
            };

            Label descriptionLabel = new()
            {
                Text =
                    "Browse available sneakers, sizes, prices and stock.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(22, 49)
            };

            _refreshButton = new Button
            {
                Text = "Refresh",
                Width = 115,
                Height = 40,
                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Right
            };

            AppTheme.StylePrimaryButton(_refreshButton);

            headerPanel.Resize += (_, _) =>
            {
                _refreshButton.Left =
                    headerPanel.ClientSize.Width -
                    _refreshButton.Width - 20;

                _refreshButton.Top = 22;
            };

            _refreshButton.Click +=
                async (_, _) => await LoadProductsAsync();

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(descriptionLabel);
            headerPanel.Controls.Add(_refreshButton);

            // Search and filters
            Panel filterPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = AppTheme.PageBackground,
                Padding = new Padding(0, 15, 0, 10)
            };

            Label searchLabel = CreateFilterLabel(
                "Search",
                new Point(4, 5));

            _searchTextBox = new TextBox
            {
                Width = 250,
                Height = 32,
                Location = new Point(4, 29),
                PlaceholderText =
                    "Search name, code, brand or colorway..."
            };

            AppTheme.StyleTextBox(_searchTextBox);

            Label brandLabel = CreateFilterLabel(
                "Brand",
                new Point(275, 5));

            _brandComboBox = CreateComboBox(
                new Point(275, 29),
                170);

            Label categoryLabel = CreateFilterLabel(
                "Category",
                new Point(465, 5));

            _categoryComboBox = CreateComboBox(
                new Point(465, 29),
                170);

            Button clearButton = new()
            {
                Text = "Clear Filters",
                Width = 125,
                Height = 34,
                Location = new Point(655, 28)
            };

            AppTheme.StyleSecondaryButton(clearButton);

            clearButton.Click += (_, _) =>
            {
                _searchTextBox.Clear();
                _brandComboBox.SelectedIndex = 0;
                _categoryComboBox.SelectedIndex = 0;
                ApplyFilters();
            };

            _searchTextBox.TextChanged +=
                (_, _) => ApplyFilters();

            _brandComboBox.SelectedIndexChanged +=
                (_, _) => ApplyFilters();

            _categoryComboBox.SelectedIndexChanged +=
                (_, _) => ApplyFilters();

            filterPanel.Controls.Add(searchLabel);
            filterPanel.Controls.Add(_searchTextBox);
            filterPanel.Controls.Add(brandLabel);
            filterPanel.Controls.Add(_brandComboBox);
            filterPanel.Controls.Add(categoryLabel);
            filterPanel.Controls.Add(_categoryComboBox);
            filterPanel.Controls.Add(clearButton);

            // Summary
            Panel summaryPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            _productCountLabel = new Label
            {
                Text = "Products: 0",
                Font = new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold),
                ForeColor = AppTheme.DarkRed,
                AutoSize = true,
                Location = new Point(20, 15)
            };

            _stockCountLabel = new Label
            {
                Text = "Available Units: 0",
                Font = new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold),
                ForeColor = AppTheme.Success,
                AutoSize = true,
                Location = new Point(200, 15)
            };

            _statusLabel = new Label
            {
                Text = "Loading catalog...",
                Font = new Font("Segoe UI", 9),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(20, 43)
            };

            summaryPanel.Controls.Add(_productCountLabel);
            summaryPanel.Controls.Add(_stockCountLabel);
            summaryPanel.Controls.Add(_statusLabel);

            // Grid
            _productGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                MultiSelect = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };

            AppTheme.StyleDataGridView(_productGrid);
            ConfigureGrid();

            _productGrid.CellFormatting +=
                ProductGrid_CellFormatting;

            _productGrid.CellDoubleClick +=
                ProductGrid_CellDoubleClick;

            Controls.Add(_productGrid);
            Controls.Add(summaryPanel);
            Controls.Add(filterPanel);
            Controls.Add(headerPanel);

            Load += async (_, _) =>
                await LoadProductsAsync();
        }

        private static Label CreateFilterLabel(
            string text,
            Point location)
        {
            return new Label
            {
                Text = text,
                Font = new Font(
                    "Segoe UI",
                    8.5f,
                    FontStyle.Bold),
                ForeColor = AppTheme.TextDark,
                AutoSize = true,
                Location = location
            };
        }

        private static ComboBox CreateComboBox(
            Point location,
            int width)
        {
            return new ComboBox
            {
                Width = width,
                Height = 32,
                Location = location,
                DropDownStyle =
                    ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
        }

        private void ConfigureGrid()
        {
            AddColumn("Code", "Code", 70);
            AddColumn("Sneaker", "Name", 145);
            AddColumn("Brand", "Brand", 90);
            AddColumn("Colorway", "Colorway", 110);
            AddColumn("Size", "Size", 55, "0.##");
            AddColumn(
                "Price",
                "UnitPrice",
                80,
                "₱#,##0.00");
            AddColumn("Stock", "Quantity", 55);
            AddColumn("Category", "Category", 85);
            AddColumn(
                "Release",
                "ReleaseType",
                75);
            AddColumn(
                "Authenticity",
                "AuthenticityStatus",
                85);
        }

        private void AddColumn(
            string header,
            string property,
            float fillWeight,
            string? format = null)
        {
            DataGridViewTextBoxColumn column = new()
            {
                HeaderText = header,
                DataPropertyName = property,
                FillWeight = fillWeight,
                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill
            };

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DefaultCellStyle.Format = format;
            }

            _productGrid.Columns.Add(column);
        }

        private async Task LoadProductsAsync()
        {
            SetLoading(true);

            try
            {
                _allItems =
                    await ApiService.Instance
                        .GetItemsAsync(
                            search: null,
                            includeInactive: false);

                PopulateFilters();
                ApplyFilters();

                _statusLabel.Text =
                    "Double-click a sneaker to view its details.";

                _statusLabel.ForeColor =
                    AppTheme.Success;
            }
            catch (Exception ex)
            {
                _statusLabel.Text =
                    $"Unable to load products: {ex.Message}";

                _statusLabel.ForeColor =
                    AppTheme.Danger;
            }
            finally
            {
                SetLoading(false);
                       }
        }

        private void PopulateFilters()
        {
            string selectedBrand =
                _brandComboBox.SelectedItem?.ToString() ??
                "All Brands";

            string selectedCategory =
                _categoryComboBox.SelectedItem?.ToString() ??
                "All Categories";

            List<string> brands =
                _allItems
                    .Where(item =>
                        !string.IsNullOrWhiteSpace(
                            item.Brand))
                    .Select(item => item.Brand)
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(brand => brand)
                    .ToList();

            List<string> categories =
                _allItems
                    .Where(item =>
                        !string.IsNullOrWhiteSpace(
                            item.Category))
                    .Select(item => item.Category)
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(category => category)
                    .ToList();

            _brandComboBox.Items.Clear();
            _brandComboBox.Items.Add("All Brands");

            foreach (string brand in brands)
            {
                _brandComboBox.Items.Add(brand);
            }

            _categoryComboBox.Items.Clear();
            _categoryComboBox.Items.Add(
                "All Categories");

            foreach (string category in categories)
            {
                _categoryComboBox.Items.Add(category);
            }

            _brandComboBox.SelectedItem =
                _brandComboBox.Items.Contains(selectedBrand)
                    ? selectedBrand
                    : "All Brands";

            _categoryComboBox.SelectedItem =
                _categoryComboBox.Items.Contains(
                    selectedCategory)
                    ? selectedCategory
                    : "All Categories";
        }

        private void ApplyFilters()
        {
            if (_brandComboBox.Items.Count == 0 ||
                _categoryComboBox.Items.Count == 0)
            {
                return;
            }

            string search =
                _searchTextBox.Text
                    .Trim()
                    .ToLowerInvariant();

            string brand =
                _brandComboBox.SelectedItem?.ToString() ??
                "All Brands";

            string category =
                _categoryComboBox.SelectedItem?.ToString() ??
                "All Categories";

            List<Item> filteredItems =
                _allItems
                    .Where(item =>
                        string.IsNullOrWhiteSpace(search) ||
                        item.Name.ToLowerInvariant()
                            .Contains(search) ||
                        item.Code.ToLowerInvariant()
                            .Contains(search) ||
                        item.Brand.ToLowerInvariant()
                            .Contains(search) ||
                        item.Colorway.ToLowerInvariant()
                            .Contains(search))
                    .Where(item =>
                        brand == "All Brands" ||
                        item.Brand.Equals(
                            brand,
                            StringComparison.OrdinalIgnoreCase))
                    .Where(item =>
                        category == "All Categories" ||
                        item.Category.Equals(
                            category,
                            StringComparison.OrdinalIgnoreCase))
                    .OrderBy(item => item.Brand)
                    .ThenBy(item => item.Name)
                    .ThenBy(item => item.Size)
                    .ToList();

            _productGrid.DataSource = null;
            _productGrid.DataSource = filteredItems;

            _productCountLabel.Text =
                $"Products: {filteredItems.Count}";

            _stockCountLabel.Text =
                $"Available Units: " +
                $"{filteredItems.Sum(item => item.Quantity)}";
        }

        private void ProductGrid_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                _productGrid.Rows[e.RowIndex]
                    .DataBoundItem is not Item item)
            {
                return;
            }

            string property =
                _productGrid.Columns[e.ColumnIndex]
                    .DataPropertyName;

            if (property == "Quantity")
            {
                e.CellStyle.Font = new Font(
                    _productGrid.Font,
                    FontStyle.Bold);

                if (item.Quantity == 0)
                {
                    e.CellStyle.ForeColor =
                        AppTheme.Danger;

                    e.Value = "Out";
                }
                else if (item.Quantity <= item.ReorderLevel)
                {
                    e.CellStyle.ForeColor =
                        AppTheme.Warning;
                }
                else
                {
                    e.CellStyle.ForeColor =
                        AppTheme.Success;
                }
            }

            if (property == "AuthenticityStatus")
            {
                e.CellStyle.Font = new Font(
                    _productGrid.Font,
                    FontStyle.Bold);

                e.CellStyle.ForeColor =
                    item.AuthenticityStatus.Equals(
                        "Verified",
                        StringComparison.OrdinalIgnoreCase)
                        ? AppTheme.Success
                        : AppTheme.Warning;
            }
        }

        private void ProductGrid_CellDoubleClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                _productGrid.Rows[e.RowIndex]
                    .DataBoundItem is not Item item)
            {
                return;
            }

            MessageBox.Show(
                $"Sneaker: {item.Name}\n" +
                $"Code: {item.Code}\n" +
                $"Brand: {item.Brand}\n" +
                $"Colorway: {item.Colorway}\n" +
                $"Size: {item.Size:0.##}\n" +
                $"Category: {item.Category}\n" +
                $"Release: {item.ReleaseType}\n" +
                $"Price: ₱{item.UnitPrice:N2}\n" +
                $"Available Stock: {item.Quantity}\n" +
                $"Authenticity: " +
                $"{item.AuthenticityStatus}",
                "Sneaker Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void SetLoading(bool loading)
        {
            _refreshButton.Enabled = !loading;
            UseWaitCursor = loading;

            if (loading)
            {
                _statusLabel.Text =
                    "Loading sneaker catalog...";

                _statusLabel.ForeColor =
                    AppTheme.TextMuted;
            }
        }
    }
}