using System.Drawing;
using System.Windows.Forms;
using SneakerShop.WinForms.Models;
using SneakerShop.WinForms.Services;
using SneakerShop.WinForms.Styles;

namespace SneakerShop.WinForms.Forms;

public class LoginForm : Form
{
    private readonly TextBox _usernameTextBox;
    private readonly TextBox _passwordTextBox;
    private readonly Button _loginButton;
    private readonly Label _statusLabel;

    public LoginForm()
    {
        Text = "SoleStock Sneaker Shop - Login";
        ClientSize = new Size(470, 570);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 125,
            BackColor = AppTheme.DarkRed
        };

        var logoLabel = new Label
        {
            Text = "SOLESTOCK",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(30, 20),
            Size = new Size(410, 50),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var subtitleLabel = new Label
        {
            Text = "Sneaker Inventory & Order Management",
            ForeColor = Color.FromArgb(255, 205, 205),
            Location = new Point(30, 72),
            Size = new Size(410, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };

        headerPanel.Controls.Add(logoLabel);
        headerPanel.Controls.Add(subtitleLabel);

        var loginTitleLabel = new Label
        {
            Text = "Sign in to your account",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = AppTheme.TextDark,
            Location = new Point(50, 155),
            Size = new Size(370, 38)
        };

        var usernameLabel = new Label
        {
            Text = "Username",
            ForeColor = AppTheme.TextDark,
            Location = new Point(50, 210),
            AutoSize = true
        };

        _usernameTextBox = new TextBox
        {
            Location = new Point(50, 237),
            Size = new Size(370, 32)
        };

        AppTheme.StyleTextBox(_usernameTextBox);

        var passwordLabel = new Label
        {
            Text = "Password",
            ForeColor = AppTheme.TextDark,
            Location = new Point(50, 290),
            AutoSize = true
        };

        _passwordTextBox = new TextBox
        {
            Location = new Point(50, 317),
            Size = new Size(370, 32),
            UseSystemPasswordChar = true
        };

        AppTheme.StyleTextBox(_passwordTextBox);

        var showPasswordCheckBox = new CheckBox
        {
            Text = "Show password",
            ForeColor = AppTheme.TextMuted,
            Location = new Point(50, 360),
            AutoSize = true
        };

        showPasswordCheckBox.CheckedChanged += (_, _) =>
        {
            _passwordTextBox.UseSystemPasswordChar =
                !showPasswordCheckBox.Checked;
        };

        _loginButton = new Button
        {
            Text = "LOGIN",
            Location = new Point(50, 400),
            Size = new Size(370, 46),
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };

        AppTheme.StylePrimaryButton(_loginButton);
        _loginButton.Click += LoginButton_Click;

        var registerButton = new Button
        {
            Text = "CREATE NEW ACCOUNT",
            Location = new Point(50, 457),
            Size = new Size(370, 42),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        AppTheme.StyleSecondaryButton(registerButton);

        registerButton.Click += (_, _) =>
        {
            using var registerForm = new RegisterForm();
            registerForm.ShowDialog(this);
        };

        _statusLabel = new Label
        {
            Text = "Checking API connection...",
            ForeColor = AppTheme.TextMuted,
            Location = new Point(50, 515),
            Size = new Size(370, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };

        Controls.Add(headerPanel);
        Controls.Add(loginTitleLabel);
        Controls.Add(usernameLabel);
        Controls.Add(_usernameTextBox);
        Controls.Add(passwordLabel);
        Controls.Add(_passwordTextBox);
        Controls.Add(showPasswordCheckBox);
        Controls.Add(_loginButton);
        Controls.Add(registerButton);
        Controls.Add(_statusLabel);

        AcceptButton = _loginButton;
        Load += LoginForm_Load;
    }

    private async void LoginForm_Load(
        object? sender,
        EventArgs e)
    {
        await CheckApiConnectionAsync();
    }

    private async Task CheckApiConnectionAsync()
    {
        _statusLabel.Text = "Checking API connection...";
        _statusLabel.ForeColor = AppTheme.TextMuted;

        bool connected =
            await ApiService.Instance.CheckConnectionAsync();

        if (connected)
        {
            _statusLabel.Text = "● REST API connected";
            _statusLabel.ForeColor = AppTheme.Success;
        }
        else
        {
            _statusLabel.Text =
                "● API is not running on localhost:5000";

            _statusLabel.ForeColor = AppTheme.Danger;
        }
    }

    private async void LoginButton_Click(
        object? sender,
        EventArgs e)
    {
        string username = _usernameTextBox.Text.Trim();
        string password = _passwordTextBox.Text;

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show(
                "Please enter your username and password.",
                "Required Fields",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        try
        {
            _loginButton.Enabled = false;
            _loginButton.Text = "SIGNING IN...";

            _statusLabel.Text = "Verifying account...";
            _statusLabel.ForeColor = AppTheme.TextMuted;

            AuthResponse response =
                await ApiService.Instance.LoginAsync(
                    new LoginRequest
                    {
                        Username = username,
                        Password = password
                    });

            if (!response.Success)
            {
                throw new Exception(response.Message);
            }

            UserSession.CurrentUser = response;

            _statusLabel.Text = "Login successful";
            _statusLabel.ForeColor = AppTheme.Success;

            Hide();

            using var mainForm = new MainForm();
            mainForm.ShowDialog();

            if (mainForm.LoggedOut)
            {
                UserSession.Logout();
                _passwordTextBox.Clear();

                Show();
                Activate();

                await CheckApiConnectionAsync();
            }
            else
            {
                Close();
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Login failed";
            _statusLabel.ForeColor = AppTheme.Danger;

            MessageBox.Show(
                ex.Message,
                "Login Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _loginButton.Enabled = true;
            _loginButton.Text = "LOGIN";
        }
    }
}