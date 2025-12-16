using System;

namespace Konvertor.Models
{
    public class ConversionHistory
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string SourceCode { get; set; } = string.Empty;
        public string ConvertedCode { get; set; } = string.Empty;
        public string FromLanguage { get; set; } = string.Empty;
        public string ToLanguage { get; set; } = string.Empty;
        public bool Success { get; set; }
        public bool IsAnalysis { get; set; } = false; // Новое поле для анализа

        // Для отображения в ListBox/DataGridView
        public string DisplayText =>
            IsAnalysis
                ? $"[{Timestamp:HH:mm}] 🔍 Анализ: {FromLanguage} - {GetPreview(SourceCode)}"
                : $"[{Timestamp:HH:mm}] {FromLanguage} → {ToLanguage} - {GetPreview(SourceCode)}";

        private string GetPreview(string code)
        {
            if (string.IsNullOrEmpty(code))
                return "Пустой код";

            var preview = code.Length > 30 ? code.Substring(0, 30) + "..." : code;
            return preview.Replace("\n", " ").Replace("\r", " ");
        }
    }
}