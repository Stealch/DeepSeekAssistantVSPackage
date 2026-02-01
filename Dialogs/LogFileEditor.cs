using DeepseekAPILib.Utilities;
using DeepSeekAssistantVSPackage.Options;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

public class LogFileEditor : UITypeEditor
{
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        => UITypeEditorEditStyle.Modal;

    public override object EditValue(ITypeDescriptorContext context,
                                    IServiceProvider provider,
                                    object value)
    {
        if (provider != null)
        {
            var editorService = (IWindowsFormsEditorService)
                provider.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null)
            {
                using (var form = new LogFileDialog())
                {
                    if (editorService.ShowDialog(form) == DialogResult.OK)
                    {
                        // Выполняем действия
                        if (form.Action == "open")
                            Logger.OpenLogFile();
                        else if (form.Action == "clear")
                            Logger.ClearLog();
                        else if (form.Action == "copy")
                        {
                            var path = Logger.GetLogFilePath();
                            if (!string.IsNullOrEmpty(path))
                            {
                                try
                                {
                                    Clipboard.SetText(path);
                                    MessageBox.Show($"Path copied to clipboard:\n{path}",
                                        "Copy Path", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Failed to copy: {ex.Message}",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
        }

        return value;
    }
}