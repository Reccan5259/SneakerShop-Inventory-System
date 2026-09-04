using System.Drawing;
using SneakerShop.WinForms.Models;
using SneakerShop.WinForms.Services;
using SneakerShop.WinForms.Styles;

namespace SneakerShop.WinForms.Forms
{
    public class MainForm : Form
    {
        public bool LoggedOut { get; private set; }

        private readonly Panel _contentPanel;
        private readonly Label _pageTitleLabel;
        private readonly Label _apiStatusLabel;
        private readonly Label _currentUserLabel;

        private readonly Button _dashboardButton;
        private readonly Button _inventoryButton;
        private readonly Button _catalogButton;
        private readonly Button _stockButton;
        private readonly Button _ordersButton;
        private readonly Button _historyButton;
        private readonly Button _reportsButton;

        private Button? _activeButton;

        public MainForm()
        {
            Text = UserSession.IsAdmin
                ? "Sneaker Shop - Administrator"
                : "Sneaker Shop - Staff";

            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1100, 700);
            Size = new Size(1450, 850);
            WindowState = FormWindowState.Maximized;
            BackColor = AppTheme.PageBackground;
            Font = new Font("Segoe UI", 9);

            Panel sidebar = new()
            {
                Dock = DockStyle.Left,
                Width = 245,
                BackColor = AppTheme.SidebarRed
            };

            Panel logoPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = AppTheme.DarkRed
            };

            Label logoLabel = new()
            {
                Text = "SOLE\nSYSTEM",
                Font = new Font(
                    "Segoe UI",
                    18,
                    FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(25, 17)
            };

            Label portalLabel = new()
            {
                Text = UserSession.IsAdmin
                    ? "ADMIN PORTAL"
                    : "STAFF SALES PORTAL",
                Font = new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold),
                ForeColor = Color.FromArgb(
                    255,
                    210,
                    210),
                AutoSize = true,
                Location = new Point(27, 80)
            };

            logoPanel.Controls.Add(logoLabel);
            logoPanel.Controls.Add(portalLabel);

            FlowLayoutPanel navigationPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 470,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(12, 22, 12, 0),
                BackColor = AppTheme.SidebarRed
            };

            _dashboardButton = CreateNavigationButton(
                UserSession.IsAdmin
                    ? "Dashboard"
                    : "Staff Dashboard",
                async (_, _) => await ShowDashboardAsync());

            _inventoryButton = CreateNavigationButton(
                "Sneaker Inventory",
                (_, _) => ShowInventory());

            _catalogButton = CreateNavigationButton(
                "Browse Sneakers",
                (_, _) => ShowCatalog());

            _stockButton = CreateNavigationButton(
                "Stock Movement",
                (_, _) => ShowStockMovement());

            _ordersButton = CreateNavigationButton(
                UserSession.IsAdmin
                    ? "Customer Orders"
                    : "Create Order",
                (_, _) => ShowCustomerOrders());

            _historyButton = CreateNavigationButton(
                "Order History",
                (_, _) => ShowOrderHistory());

            _reportsButton = CreateNavigationButton(
                "Reports & Analytics",
                (_, _) => ShowReports());

            navigationPanel.Controls.Add(
                _dashboardButton);

            if (UserSession.IsAdmin)
            {
                navigationPanel.Controls.Add(
                    _inventoryButton);

                navigationPanel.Controls.Add(
                    _stockButton);

                navigationPanel.Controls.Add(
                    _ordersButton);

                navigationPanel.Controls.Add(
                    _historyButton);

                navigationPanel.Controls.Add(
                    _reportsButton);
            }
            else
            {
                navigationPanel.Controls.Add(
                    _catalogButton);

                navigationPanel.Controls.Add(
                    _ordersButton);

                navigationPanel.Controls.Add(
                    _historyButton);
            }

            Panel footerPanel = new()
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                Padding = new Padding(12),
                BackColor = AppTheme.DarkRed
            };

            Label roleLabel = new()
            {
                Text = UserSession.IsAdmin
                    ? "Administrator Account"
                    : "Sales Staff Account",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = Color.White,
                TextAlign =
                    ContentAlignment.MiddleCenter,
                Font = new Font(
                    "Segoe UI",
                    8.5f,
                    FontStyle.Bold)
            };

            Button logoutButton = new()
            {
                Text = "Logout",
                Dock = DockStyle.Bottom,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = AppTheme.DarkRed,
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            logoutButton.FlatAppearance.BorderSize = 0;
            logoutButton.Click += LogoutButton_Click;

            footerPanel.Controls.Add(roleLabel);
            footerPanel.Controls.Add(logoutButton);

            sidebar.Controls.Add(footerPanel);
            sidebar.Controls.Add(navigationPanel);
            sidebar.Controls.Add(logoPanel);

            Panel headerPanel = new()
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = Color.White,
                Padding = new Padding(24, 10, 24, 10)
            };

            _pageTitleLabel = new Label
            {
                Text = UserSession.IsAdmin
                    ? "Admin Dashboard"
                    : "Staff Dashboard",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    19,
                    FontStyle.Bold),
                ForeColor = AppTheme.TextDark,
                Location = new Point(24, 19)
            };

            _currentUserLabel = new Label
            {
                Text =
                    $"Signed in as: {UserSession.Username}",
                AutoSize = true,
                Font = new Font("Segoe UI", 9),
                ForeColor = AppTheme.TextMuted,
                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Right
            };

            _apiStatusLabel = new Label
            {
                Text = "Checking API...",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    8.5f,
                    FontStyle.Bold),
                ForeColor = AppTheme.Warning,
                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Right
            };

            Panel headerBorder = new()
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = AppTheme.BorderColor
            };

            headerPanel.Resize += (_, _) =>
            {
                _currentUserLabel.Left =
                    headerPanel.ClientSize.Width -
                    _currentUserLabel.Width - 25;

                _currentUserLabel.Top = 16;

                _apiStatusLabel.Left =
                    headerPanel.ClientSize.Width -
                    _apiStatusLabel.Width - 25;

                _apiStatusLabel.Top = 42;
            };

            headerPanel.Controls.Add(_pageTitleLabel);
            headerPanel.Controls.Add(_currentUserLabel);
            headerPanel.Controls.Add(_apiStatusLabel);
            headerPanel.Controls.Add(headerBorder);

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.PageBackground
            };

            Controls.Add(_contentPanel);
            Controls.Add(headerPanel);
            Controls.Add(sidebar);

            Load += MainForm_Load;
        }

        private Button CreateNavigationButton(
            string text,
            EventHandler handler)
        {
            Button button = new()
            {
                Text = text,
                Width = 215,
                Height = 52,
                Margin = new Padding(0, 0, 0, 7),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.SidebarRed,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold),
                TextAlign =
                    ContentAlignment.MiddleLeft,
                Padding = new Padding(18, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor =
                AppTheme.HoverRed;

            button.Click += handler;

            button.Click += (_, _) =>
                SetActiveButton(button);

            return button;
        }

        private void SetActiveButton(
            Button selectedButton)
        {
            if (_activeButton != null)
            {
                _activeButton.BackColor =
                    AppTheme.SidebarRed;

                _activeButton.ForeColor =
                    Color.White;
            }

            _activeButton = selectedButton;
            _activeButton.BackColor = Color.White;
            _activeButton.ForeColor =
                AppTheme.DarkRed;
        }

        private async void MainForm_Load(
            object? sender,
            EventArgs e)
        {
            bool connected =
                await ApiService.Instance
                    .CheckConnectionAsync();

            _apiStatusLabel.Text = connected
                ? "● API CONNECTED"
                : "● API OFFLINE";

            _apiStatusLabel.ForeColor = connected
                ? AppTheme.Success
                : AppTheme.Danger;

            if (!connected)
            {
                MessageBox.Show(
                    "The API is not running.\n\n" +
                    "Start it using:\n" +
                    "dotnet run --urls " +
                    "http://localhost:5000",
                    "API Connection",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            _dashboardButton.PerformClick();
        }

        private async Task ShowDashboardAsync()
        {
            _pageTitleLabel.Text =
                UserSession.IsAdmin
                    ? "Admin Dashboard"
                    : "Staff Dashboard";

            _contentPanel.Controls.Clear();

            Panel page = new()
            {
                Dock = DockStyle.Fill,
                BackColor = AppTheme.PageBackground,
                Padding = new Padding(25),
                AutoScroll = true
            };

            string name =
                UserSession.CurrentUser?.FullName ??
                UserSession.Username;

            Label welcomeLabel = new()
            {
                Text = $"Welcome, {name}",
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold),
                ForeColor = AppTheme.DarkRed,
                AutoSize = true,
                Location = new Point(25, 20)
            };

            Label descriptionLabel = new()
            {
                Text = UserSession.IsAdmin
                    ? "Manage the complete sneaker shop system."
                    : "Browse sneakers and process customer orders.",
                Font = new Font("Segoe UI", 10),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(27, 62)
            };

            Label loadingLabel = new()
            {
                Text = "Loading dashboard...",
                Font = new Font("Segoe UI", 10),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(27, 96)
            };

            FlowLayoutPanel cardsPanel = new()
            {
                Location = new Point(25, 115),
                Width = 1100,
                Height = 330,
                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Left |
                    AnchorStyles.Right,
                AutoScroll = true,
                WrapContents = true,
                BackColor = AppTheme.PageBackground
            };

            Panel actionPanel = new()
            {
                Location = new Point(25, 470),
                Width = 1100,
                Height = 130,
                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Left |
                    AnchorStyles.Right,
                BackColor = Color.White
            };

            page.Resize += (_, _) =>
            {
                int width =
                    page.ClientSize.Width - 60;

                if (width > 400)
                {
                    cardsPanel.Width = width;
                    actionPanel.Width = width;
                }
            };

            page.Controls.Add(welcomeLabel);
            page.Controls.Add(descriptionLabel);
            page.Controls.Add(loadingLabel);
            page.Controls.Add(cardsPanel);
            page.Controls.Add(actionPanel);

            _contentPanel.Controls.Add(page);

            try
            {
                DashboardSummary summary =
                    await ApiService.Instance
                        .GetDashboardAsync();

                loadingLabel.Visible = false;

                if (UserSession.IsAdmin)
                {
                    AddAdminDashboardCards(
                        cardsPanel,
                        summary);

                    BuildAdminActionPanel(
                        actionPanel,
                        summary);
                }
                else
                {
                    AddStaffDashboardCards(
                        cardsPanel,
                        summary);

                    BuildStaffActionPanel(
                        actionPanel);
                }
            }
            catch (Exception ex)
            {
                loadingLabel.Text =
                    $"Unable to load dashboard: {ex.Message}";

                loadingLabel.ForeColor =
                    AppTheme.Danger;
            }
        }

        private void AddAdminDashboardCards(
            FlowLayoutPanel panel,
            DashboardSummary summary)
        {
            panel.Controls.Add(CreateMetricCard(
                "TOTAL PRODUCTS",
                summary.TotalProducts.ToString(),
                "Sneaker variations",
                AppTheme.PrimaryRed));

            panel.Controls.Add(CreateMetricCard(
                "TOTAL STOCK",
                summary.TotalStock.ToString(),
                "Available units",
                AppTheme.Success));

            panel.Controls.Add(CreateMetricCard(
                "INVENTORY VALUE",
                $"₱{summary.InventoryValue:N2}",
                "Current inventory value",
                AppTheme.PrimaryRed));

            panel.Controls.Add(CreateMetricCard(
                "LOW STOCK",
                summary.LowStockCount.ToString(),
                $"{summary.OutOfStockCount} out of stock",
                AppTheme.Warning));

            panel.Controls.Add(CreateMetricCard(
                "TOTAL ORDERS",
                summary.TotalOrders.ToString(),
                $"{summary.CancelledOrders} cancelled",
                AppTheme.DarkRed));

            panel.Controls.Add(CreateMetricCard(
                "GROSS SALES",
                $"₱{summary.GrossSales:N2}",
                "Total sales revenue",
                AppTheme.Success));

            panel.Controls.Add(CreateMetricCard(
                "VERIFIED",
                summary.VerifiedCount.ToString(),
                $"{summary.PendingAuthenticity} pending",
                AppTheme.PrimaryRed));

            panel.Controls.Add(CreateMetricCard(
                "USERS",
                summary.RegisteredUsers.ToString(),
                "Registered accounts",
                AppTheme.DarkRed));
        }

        private void AddStaffDashboardCards(
            FlowLayoutPanel panel,
            DashboardSummary summary)
        {
            panel.Controls.Add(CreateMetricCard(
                "SNEAKER PRODUCTS",
                summary.TotalProducts.ToString(),
                "Available variations",
                AppTheme.PrimaryRed));

            panel.Controls.Add(CreateMetricCard(
                "AVAILABLE STOCK",
                summary.TotalStock.ToString(),
                "Units available for sale",
                AppTheme.Success));

            panel.Controls.Add(CreateMetricCard(
                "BRANDS",
                summary.TotalBrands.ToString(),
                "Available sneaker brands",
                AppTheme.DarkRed));

            panel.Controls.Add(CreateMetricCard(
                "VERIFIED",
                summary.VerifiedCount.ToString(),
                "Authenticated products",
                AppTheme.Success));

            panel.Controls.Add(CreateMetricCard(
                "ORDERS",
                summary.TotalOrders.ToString(),
                "Processed shop orders",
                AppTheme.PrimaryRed));
        }

        private Panel CreateMetricCard(
            string title,
            string value,
            string description,
            Color accentColor)
        {
            Panel card = new()
            {
                Width = 250,
                Height = 135,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 18, 18)
            };

            Panel accent = new()
            {
                Dock = DockStyle.Left,
                Width = 6,
                BackColor = accentColor
            };

            Label titleLabel = new()
            {
                Text = title,
                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(22, 16)
            };

            Label valueLabel = new()
            {
                Text = value,
                Font = new Font(
                    "Segoe UI",
                    21,
                    FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = true,
                Location = new Point(20, 45)
            };

            Label descriptionLabel = new()
            {
                Text = description,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(22, 101)
            };

            card.Controls.Add(accent);
            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            card.Controls.Add(descriptionLabel);

            return card;
        }

        private void BuildAdminActionPanel(
            Panel panel,
            DashboardSummary summary)
        {
            panel.Controls.Clear();

            Label titleLabel = new()
            {
                Text = summary.LowStockCount > 0
                    ? "Inventory Attention Required"
                    : "Inventory Status Is Healthy",
                Font = new Font(
                    "Segoe UI",
                    13,
                    FontStyle.Bold),
                ForeColor = AppTheme.TextDark,
                AutoSize = true,
                Location = new Point(25, 22)
            };

            Label messageLabel = new()
            {
                Text =
                    $"{summary.LowStockCount} low-stock " +
                    $"and {summary.OutOfStockCount} " +
                    "out-of-stock item(s).",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(27, 57)
            };

            Button button = new()
            {
                Text = "Manage Inventory",
                Width = 160,
                Height = 42
            };

            AppTheme.StylePrimaryButton(button);

            button.Location =
                new Point(27, 82);

            button.Click += (_, _) =>
                _inventoryButton.PerformClick();

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(messageLabel);
            panel.Controls.Add(button);
        }

        private void BuildStaffActionPanel(Panel panel)
        {
            panel.Controls.Clear();

            Label titleLabel = new()
            {
                Text = "Start a Customer Transaction",
                Font = new Font(
                    "Segoe UI",
                    13,
                    FontStyle.Bold),
                ForeColor = AppTheme.TextDark,
                AutoSize = true,
                Location = new Point(25, 22)
            };

            Label messageLabel = new()
            {
                Text =
                    "Browse available sneakers or create a new order.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppTheme.TextMuted,
                AutoSize = true,
                Location = new Point(27, 54)
            };

            Button browseButton = new()
            {
                Text = "Browse Sneakers",
                Width = 155,
                Height = 42,
                Location = new Point(27, 80)
            };

            Button orderButton = new()
            {
                Text = "Create Order",
                Width = 145,
                Height = 42,
                Location = new Point(195, 80)
            };

            AppTheme.StyleSecondaryButton(
                browseButton);

            AppTheme.StylePrimaryButton(
                orderButton);

            browseButton.Click += (_, _) =>
                _catalogButton.PerformClick();

            orderButton.Click += (_, _) =>
                _ordersButton.PerformClick();

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(messageLabel);
            panel.Controls.Add(browseButton);
            panel.Controls.Add(orderButton);
        }

        private void ShowInventory()
        {
            if (!UserSession.IsAdmin)
            {
                ShowAccessDenied();
                return;
            }

            ShowControl(
                "Sneaker Inventory",
                new InventoryControl());
        }

        private void ShowCatalog()
        {
            ShowControl(
                "Browse Sneakers",
                new ProductCatalogControl());
        }

        private void ShowStockMovement()
        {
            if (!UserSession.IsAdmin)
            {
                ShowAccessDenied();
                return;
            }

            ShowControl(
                "Stock Movement",
                new StockMovementControl());
        }

        private void ShowCustomerOrders()
        {
            ShowControl(
                UserSession.IsAdmin
                    ? "Customer Orders"
                    : "Create Order",
                new OrdersControl());
        }

        private void ShowOrderHistory()
        {
            ShowControl(
                "Order History",
                new OrderHistoryControl());
        }

        private void ShowReports()
        {
            if (!UserSession.IsAdmin)
            {
                ShowAccessDenied();
                return;
            }

            ShowControl(
                "Reports & Analytics",
                new ReportsControl());
        }

        private void ShowControl(
            string title,
            UserControl control)
        {
            _pageTitleLabel.Text = title;

            _contentPanel.SuspendLayout();
            _contentPanel.Controls.Clear();

            control.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(control);

            _contentPanel.ResumeLayout();
        }

        private static void ShowAccessDenied()
        {
            MessageBox.Show(
                "Only administrators can access this feature.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void LogoutButton_Click(
            object? sender,
            EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to log out?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            UserSession.Logout();
            LoggedOut = true;
            Close();
        }
    }
}