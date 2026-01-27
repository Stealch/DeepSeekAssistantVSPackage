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
            var timer = new Timer();
            timer.Interval = 30000;
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

            var panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(20);

            statusLabel = new Label();
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            statusLabel.Font = new System.Drawing.Font("Segoe UI", 10);

            var cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Size = new System.Drawing.Size(80, 30);
            cancelButton.Location = new System.Drawing.Point(160, 80);
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