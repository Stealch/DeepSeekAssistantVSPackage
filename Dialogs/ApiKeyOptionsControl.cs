using System;
using System.Drawing;
using System.Windows.Forms;

// Dialogs\ApiKeyOptionsControl.cs

namespace DeepSeekAssistantVSPackage.Options
{
    public class ApiKeyOptionsControl : UserControl
    {
        private DeepSeekOptionsPage _optionsPage;
        private Panel _mainPanel;
        private TextBox _apiKeyTextBox;
        private CheckBox _showKeyCheckBox;
        private Button _testButton;
        private Button _clearButton;
        private Label _statusLabel;
        private bool _hasExistingKey = false;
        private string _originalKey = string.Empty;

        public ApiKeyOptionsControl(DeepSeekOptionsPage page)
        {
            _optionsPage = page;
            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            this.BackColor = SystemColors.Control;
            // Основная панель с рамкой как в нативных плагинах
            _mainPanel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SystemColors.Control,
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            this.Controls.Add(_mainPanel);

            // Заголовок
            var titleLabel = new Label
            {
                Text = "DeepSeek API Settings",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            _mainPanel.Controls.Add(titleLabel);

            // Label для API Key
            var apiKeyLabel = new Label
            {
                Text = "API Key:",
                AutoSize = true,
                Location = new Point(10, 45)
            };
            _mainPanel.Controls.Add(apiKeyLabel);

            // TextBox для API Key
            _apiKeyTextBox = new TextBox
            {
                Width = 350,
                Location = new Point(80, 42),
                BackColor = SystemColors.Window,
                ForeColor = SystemColors.WindowText,
                ReadOnly = true // По умолчанию только для чтения
            };
            _mainPanel.Controls.Add(_apiKeyTextBox);

            // Кнопка Test Connection
            _testButton = new Button
            {
                Text = "Test Connection",
                Location = new Point(80, 70),
                Size = new Size(120, 30)
            };
            _testButton.Click += TestButton_Click;
            _mainPanel.Controls.Add(_testButton);

            // Кнопка Clear Key
            _clearButton = new Button
            {
                Text = "Clear Key",
                Location = new Point(210, 70),
                Size = new Size(80, 30)
            };
            _clearButton.Click += ClearButton_Click;
            _mainPanel.Controls.Add(_clearButton);

            // Чекбокс Show Key
            _showKeyCheckBox = new CheckBox
            {
                Text = "Show key",
                Location = new Point(300, 75),
                AutoSize = true,
                Enabled = false // По умолчанию выключен
            };
            _showKeyCheckBox.CheckedChanged += ShowKeyCheckBox_CheckedChanged;
            _mainPanel.Controls.Add(_showKeyCheckBox);

            // Статусная метка
            _statusLabel = new Label
            {
                Text = string.Empty,
                AutoSize = true,
                Location = new Point(80, 110),
                ForeColor = SystemColors.GrayText
            };
            _mainPanel.Controls.Add(_statusLabel);

            this.Size = new Size(450, 150);
        }

        private void LoadSettings()
        {
            _originalKey = _optionsPage.ApiKey ?? string.Empty;
            _hasExistingKey = !string.IsNullOrEmpty(_originalKey);

            if (_hasExistingKey)
            {
                // Изначально: ЧАСТИЧНАЯ маскировка (sk-32ee*******16be)
                _apiKeyTextBox.Text = MaskApiKey(_originalKey); // ← Используем MaskApiKey
                _apiKeyTextBox.PasswordChar = '\0'; // НЕ звездочки в TextBox
                _apiKeyTextBox.ReadOnly = true;
                _apiKeyTextBox.BackColor = SystemColors.Control;
                _showKeyCheckBox.Enabled = true;
                _statusLabel.Text = $"Key configured ({_originalKey.Length} chars)";
            }
            else
            {
                // Нет ключа
                _apiKeyTextBox.Text = "[Click 'Clear Key' to enter new key]";
                _apiKeyTextBox.PasswordChar = '\0';
                _apiKeyTextBox.ReadOnly = true;
                _apiKeyTextBox.BackColor = SystemColors.Control;
                _showKeyCheckBox.Enabled = false;
                _statusLabel.Text = "No API key configured";
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            if (_hasExistingKey)
            {
                // Подтверждение очистки существующего ключа
                if (MessageBox.Show("Clear existing API key?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;
            }

            // Очищаем и готовим к новому вводу
            _originalKey = string.Empty;
            _hasExistingKey = false;
            _apiKeyTextBox.Text = string.Empty;
            _apiKeyTextBox.PasswordChar = '\0';
            _apiKeyTextBox.ReadOnly = false;
            _apiKeyTextBox.BackColor = SystemColors.Window;
            _showKeyCheckBox.Enabled = false;
            _showKeyCheckBox.Checked = false;
            _statusLabel.Text = "Enter new API key";
            _apiKeyTextBox.Focus();
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            string currentKey = _hasExistingKey ? _originalKey : _apiKeyTextBox.Text;
            string status = string.IsNullOrEmpty(currentKey) ?
                "Anonymous access" : $"API key ({currentKey.Length} chars)";

            MessageBox.Show($"Test connection with:\n\n{status}",
                "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowKeyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!_hasExistingKey) return;

            if (_showKeyCheckBox.Checked)
            {
                // Показываем реальный ключ
                _apiKeyTextBox.Text = _originalKey;
                _apiKeyTextBox.PasswordChar = '\0';
            }
            else
            {
                // ВОЗВРАЩАЕМ ЧАСТИЧНУЮ маскировку, не полную!
                _apiKeyTextBox.Text = MaskApiKey(_originalKey); // ← Это наш метод частичной маскировки
                _apiKeyTextBox.PasswordChar = '\0'; // НЕ звездочки в TextBox
            }
        }

        public void SaveSettings()
        {
            if (!_hasExistingKey && !string.IsNullOrEmpty(_apiKeyTextBox.Text))
            {
                // Сохраняем новый ключ
                _optionsPage.ApiKey = _apiKeyTextBox.Text.Trim();
            }
            else
            {
                // Сохраняем существующий ключ (или пустой если очистили)
                _optionsPage.ApiKey = _originalKey;
            }
        }

        private string MaskApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 10)
                return new string('*', apiKey?.Length ?? 0);

            return $"{apiKey.Substring(0, 8)}{new string('*', Math.Max(0, apiKey.Length - 12))}{apiKey.Substring(apiKey.Length - 4)}";
        }
    }
}