// Dialogs\ApiKeyEditor.cs
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace DeepSeekAssistantVSPackage.Options
{
    public class ApiKeyEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // Modal диалог с кнопками
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

                if (editorService != null)
                {
                    // Используем существующую форму настроек или создаем новую
                    using (var dialog = new ApiKeyDialog())
                    {
                        dialog.ApiKey = value as string ?? string.Empty;

                        if (editorService.ShowDialog(dialog) == DialogResult.OK)
                        {
                            return dialog.ApiKey;
                        }
                    }
                }
            }

            return value;
        }
    }
}