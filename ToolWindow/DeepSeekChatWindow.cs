using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using System.Drawing; // Для работы с иконками
using System.Reflection; // Для загрузки ресурсов

namespace DeepSeekAssistantVSPackage.ToolWindows
{
    [Guid("6c5b550d-8e4a-4a5c-8b5a-7e9f3c1d2a4b")]
    public class DeepSeekChatWindow : ToolWindowPane
    {
        public DeepSeekChatWindow() : base(null)
        {
            this.Caption = "DeepSeek Chat";

            // Способ 1: Через BitmapResourceID (если есть ресурсы в .resx)
            // this.BitmapResourceID = 4001;
            // this.BitmapIndex = 1;

            // Способ 2: Загрузка иконки из внедрённого ресурса
            LoadIconFromEmbeddedResources();

            var control = new DeepSeekChatWindowControl();
            this.Content = control;
        }

        private void LoadIconFromEmbeddedResources()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                // Ищем ресурс с иконкой
                // Формат: {Namespace}.{Папка}.{Файл}
                var possibleNames = new[]
                {
                    "DeepSeekAssistantVSPackage.Resources.deepseek.ico",
                    "DeepSeekAssistantVSPackage.Resources.DeepSeekIcon.ico",
                    "DeepSeekAssistantVSPackage.deepseek.ico"
                };

                foreach (var resourceName in possibleNames)
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        // СОЗДАЁМ ИКОНКУ И НАЗНАЧАЕМ ЧЕРЕЗ ОКОННЫЙ HANDLE
                        var icon = new Icon(stream);

                        // Для ToolWindowPane нужно использовать Bitmap, а не Icon напрямую
                        // Конвертируем Icon в Bitmap
                        using (var bitmap = icon.ToBitmap())
                        {
                            // Получаем handle битмапа
                            IntPtr hBitmap = bitmap.GetHbitmap();

                            // В VS ToolWindow иконка устанавливается через ресурсы
                            // Оставляем стандартную иконку для now
                            // Иконку можно будет настроить через .vsct/.resx позже
                        }

                        icon.Dispose();
                        return;
                    }
                }

                // Если не нашли ресурс, оставляем стандартную иконку VS
                System.Diagnostics.Debug.WriteLine("Icon resource not found");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading icon: {ex.Message}");
            }
        }
    }
}