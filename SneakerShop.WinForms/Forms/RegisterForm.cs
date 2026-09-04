using System.Drawing;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SneakerShop.WinForms.Models;
using SneakerShop.WinForms.Services;
using SneakerShop.WinForms.Styles;

namespace SneakerShop.WinForms.Forms;

public class RegisterForm : Form
{
    private readonly TextBox _fullNameTextBox;
    private readonly TextBox _usernameTextBox;
    private readonly TextBox _emailTextBox;
    private readonly TextBox _passwordTextBox;
    private readonly TextBox _confirmPasswordTextBox;
    private readonly Label _passwordRulesLabel;
    private readonly Label _statusLabel;
    private readonly Button _registerButton;

    public RegisterForm()
    {
        Text = "SoleStock - Register Account";
        ClientSize = new Size(540, 750);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 105,
            BackColor = AppTheme.DarkRed
        };

        var titleLabel = new Label
        {
            Text = "CREATE ACCOUNT",
            Font = new Font("Segoe UI", 23, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(50, 15),
            Size = new Size(440, 45),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var subtitleLabel = new Label
        {
            Text = "Register a new SoleStock employee account",
            ForeColor = Color.FromArgb(255, 205, 205),
            Location = new Point(50, 60),
            Size = new Size(440, 28),
            TextAlign = ContentAlignment.MiddleCenter
        };

        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(subtitleLabel);
        Controls.Add(headerPanel);

        int y = 125;

        _fullNameTextBox =
            CreateField("Full name", ref y);

        _usernameTextBox =
            CreateField("Username", ref y);

        _emailTextBox =
            CreateField("Email address", ref y);

        _passwordTextBox =
            CreateField("Password", ref y, true);

        _confirmPasswordTextBox =
            CreateField("Confirm password", ref y, true);

        _passwordRulesLabel = new Label
        {
            Location = new Point(50, y),
            Size = new Size(440, 70),
            ForeColor = AppTheme.Danger,
            Font = new Font("Segoe UI", 9)
        };

        y += 70;

        var showPasswordCheckBox = new CheckBox
        {
            Text = "Show passwords",
            Location = new Point(50, y),
            ForeColor = AppTheme.TextMuted,
            AutoSize = true
        };

        showPasswordCheckBox.CheckedChanged += (_, _) =>
        {
            bool hidePasswords =
                !showPasswordCheckBox.Checked;

            _passwordTextBox.UseSystemPasswordChar =
                hidePasswords;

            _confirmPasswordTextBox.UseSystemPasswordChar =
                hidePasswords;
        };

        y += 40;

        _registerButton = new Button
        {
            Text = "REGISTER",
            Location = new Point(50, y),
            Size = new Size(210, 45),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        AppTheme.StylePrimaryButton(_registerButton);
        _registerButton.Click += RegisterButton_Click;

        var cancelButton = new Button
        {
            Text = "CANCEL",
            Location = new Point(280, y),
            Size = new Size(210, 45),
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        AppTheme.StyleSecondaryButton(cancelButton);
        cancelButton.Click += (_, _) => Close();

        y += 55;

        _statusLabel = new Label
        {
            Location = new Point(50, y),
            Size = new Size(440, 35),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = AppTheme.TextMuted
        };

        Controls.Add(_passwordRulesLabel);
        Controls.Add(showPasswordCheckBox);
        Controls.Add(_registerButton);
        Controls.Add(cancelButton);
        Controls.Add(_statusLabel);

        _passwordTextBox.TextChanged += (_, _) =>
            UpdatePasswordRules();

        UpdatePasswordRules();
        AcceptButton = _registerButton;
    }

    private TextBox CreateField(
        string labelText,
        ref int y,
        bool password = false)
    {
        var label = new Label
        {
            Text = labelText,
            Location = new Point(50, y),
            ForeColor = AppTheme.TextDark,
            AutoSize = true
        };

        var textBox = new TextBox
        {
            Location = new Point(50, y + 25),
            Size = new Size(440, 32),
            UseSystemPasswordChar = password
        };

        AppTheme.StyleTextBox(textBox);

        Controls.Add(label);
        Controls.Add(textBox);

        y += 72;
        return textBox;
    }

    private void UpdatePasswordRules()
    {
        string password = _passwordTextBox.Text;

        bool hasLength = password.Length >= 8;
        bool hasUppercase = password.Any(char.IsUpper);
        bool hasLowercase = password.Any(char.IsLower);
        bool hasNumber = password.Any(char.IsDigit);
        bool hasSymbol =
            password.Any(character =>
                !char.IsLetterOrDigit(character));

        _passwordRulesLabel.Text =
            $"{Mark(hasLength)} Minimum 8 characters     " +
            $"{Mark(hasUppercase)} Uppercase letter\n" +
            $"{Mark(hasLowercase)} Lowercase letter      " +
            $"{Mark(hasNumber)} Number     " +
            $"{Mark(hasSymbol)} Symbol";

        bool passwordIsValid =
            hasLength &&
            hasUppercase &&
            hasLowercase &&
            hasNumber &&
            hasSymbol;

        _passwordRulesLabel.ForeColor =
            passwordIsValid
                ? AppTheme.Success
                : AppTheme.Danger;
    }

    private static string Mark(bool valid)
    {
        return valid ? "✓" : "✗";
    }

    private bool ValidateInputs(
        out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(
                _fullNameTextBox.Text))
        {
            errorMessage = "Full name is required.";
            return false;
        }

        if (!Regex.IsMatch(
                _usernameTextBox.Text.Trim(),
                @"^[a-zA-Z0-9_]{4,20}$"))
        {
            errorMessage =
                "Username must contain 4-20 letters, " +
                "numbers, or underscores.";

            return false;
        }

        if (!MailAddress.TryCreate(
                _emailTextBox.Text.Trim(),
                out _))
        {
            errorMessage =
                "Please enter a valid email address.";

            return false;
        }

        if (!Regex.IsMatch(
                _passwordTextBox.Text,
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)" +
                @"(?=.*[^a-zA-Z0-9]).{8,}$"))
        {
            errorMessage =
                "Password must contain at least 8 characters, " +
                "an uppercase letter, lowercase letter, " +
                "number, and symbol.";

            return false;
        }

        if (_passwordTextBox.Text !=
            _confirmPasswordTextBox.Text)
        {
            errorMessage = "Passwords do not match.";
            return false;
        }

        return true;
    }

    private async void RegisterButton_Click(
        object? sender,
        EventArgs e)
    {
        if (!ValidateInputs(out string errorMessage))
        {
            MessageBox.Show(
                errorMessage,
                "Invalid Registration",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return;
        }

        try
        {
            _registerButton.Enabled = false;
            _registerButton.Text = "REGISTERING...";

            _statusLabel.Text = "Creating account...";
            _statusLabel.ForeColor = AppTheme.TextMuted;

            AuthResponse response =
                await ApiService.Instance.RegisterAsync(
                    new RegisterRequest
                    {
                        FullName =
                            _fullNameTextBox.Text.Trim(),

                        Username =
                            _usernameTextBox.Text.Trim(),

                        Email =
                            _emailTextBox.Text.Trim(),

                        Password =
                            _passwordTextBox.Text,
                            
                        ConfirmPassword = 
                            _confirmPasswordTextBox.Text,
                    });

            if (!response.Success)
            {
                throw new Exception(response.Message);
            }

            _statusLabel.Text =
                "Account created successfully.";

            _statusLabel.ForeColor =
                AppTheme.Success;

            MessageBox.Show(
                "Registration successful. You may now log in.",
                "Account Created",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Close();
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Registration failed.";
            _statusLabel.ForeColor = AppTheme.Danger;

            MessageBox.Show(
                ex.Message,
                "Registration Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _registerButton.Enabled = true;
            _registerButton.Text = "REGISTER";
        }
    }
}