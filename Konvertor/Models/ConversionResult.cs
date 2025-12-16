using System;

namespace Konvertor.Models
{
    public class ConversionResult
    {
        public string SourceCode { get; set; } = "";
        public string ConvertedCode { get; set; } = "";
        public string FromLanguage { get; set; } = "";
        public string ToLanguage { get; set; } = "";
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}