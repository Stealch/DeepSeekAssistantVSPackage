using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeepSeekAssistantVSPackage.Options
{
    public class LogFileDialog : Form
    {
        public string Action { get; private set; } = string.Empty;

        public LogFileDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Log File Actions";
            this.Size = new Size(300, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Open Log File Button
            var openBtn = new Button
            {
                Text = "📄 Open Log File",
                Size = new Size(200, 40),
                Location = new Point(50, 20)
            };
            openBtn.Click += (s, e) =>
            {
                Action = "open";
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            // Clear Log Button
            var clearBtn = new Button
            {
                Text = "🗑️ Clear Log",
                Size = new Size(200, 40),
                Location = new Point(50, 70)
            };
            clearBtn.Click += (s, e) =>
            {
                Action = "clear";
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            var copyBtn = new Button
            {
                Text = "📋 Copy Path",
                Size = new Size(200, 40),
                Location = new Point(50, 120) // Та же позиция
            };
            copyBtn.Click += (s, e) =>
            {
                Action = "copy";
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(copyBtn); // вместо folderBtn

            this.Controls.Add(openBtn);
            this.Controls.Add(clearBtn);
            this.Controls.Add(copyBtn);
        }
    }
}