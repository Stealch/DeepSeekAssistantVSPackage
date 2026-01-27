// Dialogs\TestConnectionForm.cs
using System;
using System.Windows.Forms;

namespace DeepSeekAssistantVSPackage.Options
{
    public partial class TestConnectionForm : Form
    {
        private readonly System.Threading.CancellationTokenSource _cancellationTokenSource;

        public TestConnectionForm(string status)
        {
            InitializeComponent();

            _cancellationTokenSource = new System.Threading.CancellationTokenSource();
            statusLabel.Text = $"Testing connection...\n{status}";

            // Автоматически закрываем через 30 секунд
            Timer timer = new Timer
            {
                Interval = 30000
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                if (!this.IsDisposed)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            };
            timer.Start();
        }

        private void InitializeComponent()
        {
            this.Text = "Testing DeepSeek API Connection";
            this.Size = new System.Drawing.Size(400, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            Button cancelButton = new Button
            {
                Text = "Cancel",
                Size = new System.Drawing.Size(80, 30),
                Location = new System.Drawing.Point(160, 80)
            };
            cancelButton.Click += (s, e) =>
            {
                _cancellationTokenSource.Cancel();
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            panel.Controls.Add(statusLabel);
            panel.Controls.Add(cancelButton);

            this.Controls.Add(panel);
        }

        private Label statusLabel;

        public System.Threading.CancellationToken CancellationToken => _cancellationTokenSource.Token;

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _cancellationTokenSource?.Dispose();
            base.OnFormClosed(e);
        }
    }
}