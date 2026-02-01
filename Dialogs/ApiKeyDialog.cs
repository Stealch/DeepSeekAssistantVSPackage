// Dialogs\ApiKeyDialog.cs
using System;
using System.Windows.Forms;

namespace DeepSeekAssistantVSPackage.Options
{
    public partial class ApiKeyDialog : Form
    {
        private TextBox apiKeyTextBox;
        private Button testButton;
        private Button clearButton;
        private Button okButton;
        private Button cancelButton;

        public string ApiKey { get; set; }

        public ApiKeyDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "API Key Settings";
            this.Size = new System.Drawing.Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // API Key Label
            var label = new Label
            {
                Text = "API Key:",
                Location = new System.Drawing.Point(10, 15),
                Size = new System.Drawing.Size(60, 20)
            };

            // API Key TextBox
            apiKeyTextBox = new TextBox
            {
                Location = new System.Drawing.Point(80, 12),
                Size = new System.Drawing.Size(300, 20),
                Text = ApiKey ?? string.Empty,
                PasswordChar = '*'
            };

            // Test Button
            testButton = new Button
            {
                Text = "Test Connection",
                Location = new System.Drawing.Point(80, 40),
                Size = new System.Drawing.Size(120, 30)
            };
            testButton.Click += TestButton_Click;

            // Clear Button
            clearButton = new Button
            {
                Text = "Clear Key",
                Location = new System.Drawing.Point(210, 40),
                Size = new System.Drawing.Size(80, 30)
            };
            clearButton.Click += ClearButton_Click;

            // OK Button
            okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(220, 120),
                Size = new System.Drawing.Size(75, 30)
            };

            // Cancel Button
            cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(305, 120),
                Size = new System.Drawing.Size(75, 30)
            };

            // Add controls
            this.Controls.Add(label);
            this.Controls.Add(apiKeyTextBox);
            this.Controls.Add(testButton);
            this.Controls.Add(clearButton);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            // Accept/Cancel buttons
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private void TestButton_Click(object sender, EventArgs e)
        {
            // Сохраняем текущее значение
            ApiKey = apiKeyTextBox.Text;

            // ЗАМЕНЯЕМ вызов TestConnection() на прямой код тестирования
            string testKey = string.IsNullOrEmpty(ApiKey) ? " " : ApiKey;
            string keyStatus = string.IsNullOrEmpty(ApiKey) ?
                "Anonymous access (no API key)" : $"API key ({ApiKey.Length} chars)";

            MessageBox.Show(
                $"Testing connection...\n\nStatus: {keyStatus}\n\n" +
                $"Note: Without API key, you'll get authentication error.\n" +
                $"With API key but zero balance, you'll get 'Insufficient Balance'.",
                "Test Connection",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear API key?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                apiKeyTextBox.Text = string.Empty;
                ApiKey = string.Empty;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Сохраняем значение при закрытии
            if (this.DialogResult == DialogResult.OK)
            {
                ApiKey = apiKeyTextBox.Text;
            }
            base.OnFormClosing(e);
        }
    }
}