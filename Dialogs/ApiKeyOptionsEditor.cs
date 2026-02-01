using DeepSeekAssistantVSPackage.Options;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

// Dialogs\ApiKeyOptionsEditor.cs

public class ApiKeyOptionsEditor : UITypeEditor
{
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
        return UITypeEditorEditStyle.Modal;
    }

    public override object EditValue(ITypeDescriptorContext context,
                                    IServiceProvider provider,
                                    object value)
    {
        if (provider != null)
        {
            var editorService = (IWindowsFormsEditorService)
                provider.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null && context.Instance is DeepSeekOptionsPage page)
            {
                using (var form = new Form())
                {
                    form.Text = "DeepSeek API Settings";
                    form.Size = new Size(500, 250);
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.MaximizeBox = false;
                    form.MinimizeBox = false;

                    // Добавляем наш UserControl
                    var control = new ApiKeyOptionsControl(page);
                    control.Dock = DockStyle.Fill;
                    form.Controls.Add(control);

                    // Кнопки OK/Cancel
                    var okButton = new Button { Text = "OK", DialogResult = DialogResult.OK };
                    var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };

                    var flowPanel = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.RightToLeft,
                        Dock = DockStyle.Bottom,
                        Height = 40,
                        Padding = new Padding(10)
                    };
                    flowPanel.Controls.AddRange(new[] { cancelButton, okButton });
                    form.Controls.Add(flowPanel);

                    form.AcceptButton = okButton;
                    form.CancelButton = cancelButton;

                    if (editorService.ShowDialog(form) == DialogResult.OK)
                    {
                        control.SaveSettings();
                        return page.ApiKey;
                    }
                }
            }
        }

        return value;
    }
}