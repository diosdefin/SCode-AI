using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Konvertor.Services
{
    public static class IconManager
    {
        private static readonly Dictionary<string, Image> _icons = new Dictionary<string, Image>();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                Console.WriteLine($"Ищем иконки в: {resourcesPath}");

                if (Directory.Exists(resourcesPath))
                {
                    var files = Directory.GetFiles(resourcesPath, "*.png");
                    Console.WriteLine($"Найдено {files.Length} PNG файлов");

                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                        Console.WriteLine($"Загружаем иконку: {fileName}");
                        _icons[fileName] = Image.FromFile(file);
                    }
                }
                else
                {
                    Console.WriteLine("Папка Resources не найдена!");
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки иконок: {ex.Message}");
            }
        }

        public static Image GetIcon(string languageId)
        {
            if (!_initialized) Initialize();

            string key = languageId.ToLower();

            // Маппинг алиасов
            if (key == "cs" || key == "c#") key = "csharp";
            if (key == "js") key = "javascript";
            if (key == "ts") key = "typescript";
            if (key == "c++") key = "cpp";

            return _icons.ContainsKey(key) ? _icons[key] : null;
        }
    }
}