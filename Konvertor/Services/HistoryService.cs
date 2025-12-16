using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Konvertor.Models;
using Newtonsoft.Json;

namespace Konvertor.Services
{
    public class HistoryService
    {
        private readonly string _historyFilePath;
        private List<ConversionHistory> _history;

        public HistoryService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "KonvertorAI");

            if (!Directory.Exists(appFolder))
                Directory.CreateDirectory(appFolder);

            _historyFilePath = Path.Combine(appFolder, "history.json");
            _history = LoadHistory();
        }

        private List<ConversionHistory> LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    string json = File.ReadAllText(_historyFilePath);
                    return JsonConvert.DeserializeObject<List<ConversionHistory>>(json)
                        ?? new List<ConversionHistory>();
                }
            }
            catch { }

            return new List<ConversionHistory>();
        }

        private void SaveHistory()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_history, Formatting.Indented);
                File.WriteAllText(_historyFilePath, json);
            }
            catch { }
        }

        public void AddToHistory(ConversionResult result)
        {
            try
            {
                var historyItem = new ConversionHistory
                {
                    Id = _history.Count > 0 ? _history.Max(h => h.Id) + 1 : 1,
                    Timestamp = DateTime.Now,
                    SourceCode = result.SourceCode,
                    ConvertedCode = result.ConvertedCode,
                    FromLanguage = result.FromLanguage,
                    ToLanguage = result.ToLanguage,
                    Success = result.Success
                };

                _history.Add(historyItem);

                // Ограничиваем историю 100 записями
                if (_history.Count > 100)
                    _history = _history.Skip(_history.Count - 100).ToList();

                SaveHistory();
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не падаем
                Console.WriteLine($"Error adding to history: {ex.Message}");
            }
        }

        public void AddAnalysisToHistory(string sourceCode, string detectedLanguage)
        {
            try
            {
                var historyItem = new ConversionHistory
                {
                    Id = _history.Count > 0 ? _history.Max(h => h.Id) + 1 : 1,
                    Timestamp = DateTime.Now,
                    SourceCode = sourceCode,
                    FromLanguage = detectedLanguage,
                    Success = true,
                    IsAnalysis = true
                };

                _history.Add(historyItem);

                // Ограничиваем историю 100 записями
                if (_history.Count > 100)
                    _history = _history.Skip(_history.Count - 100).ToList();

                SaveHistory();
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не падаем
                Console.WriteLine($"Error adding analysis to history: {ex.Message}");
            }
        }

        public List<ConversionHistory> GetHistory()
        {
            return _history.OrderByDescending(h => h.Timestamp).ToList();
        }

        public void ClearHistory()
        {
            _history.Clear();
            SaveHistory();
        }

        public ConversionHistory GetHistoryItem(int id)
        {
            return _history.FirstOrDefault(h => h.Id == id);
        }
    }
}