using System;
using System.Collections.Generic;
using System.Linq;
using Konvertor.Models;

namespace Konvertor.Services
{
    public class CodeDetector
    {
        /// <summary>
        /// Детектирует язык программирования по коду
        /// </summary>
        public static Language DetectLanguage(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            // Приводим код к нижнему регистру для анализа
            var codeLower = code.ToLower();

            // Получаем все доступные языки
            var languages = Language.GetAvailableLanguages();
            var scores = new Dictionary<Language, int>();

            // Для каждого языка считаем "очки" по ключевым словам
            foreach (var lang in languages)
            {
                int score = 0;

                // Проверяем наличие ключевых слов
                foreach (var keyword in lang.Keywords)
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        score += CountOccurrences(codeLower, keyword.ToLower());
                    }
                }

                // Дополнительные эвристики для каждого языка
                score += CalculateHeuristicScore(lang.Id, code);

                scores[lang] = score;
            }

            // Сортируем по количеству очков (по убыванию)
            var sortedLanguages = scores.OrderByDescending(kv => kv.Value).ToList();

            // Возвращаем язык с наибольшим количеством очков
            var bestMatch = sortedLanguages.FirstOrDefault();

            // Если есть хоть какие-то очки, возвращаем лучший вариант
            if (bestMatch.Value > 0)
                return bestMatch.Key;

            // Если не определили, пробуем определить по синтаксису
            var fallback = DetectBySyntax(code);
            return fallback ?? languages.First(); // По умолчанию Python
        }

        /// <summary>
        /// Подсчитывает количество вхождений подстроки в тексте
        /// </summary>
        private static int CountOccurrences(string text, string pattern)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
                return 0;

            int count = 0;
            int i = 0;

            while ((i = text.IndexOf(pattern, i, StringComparison.Ordinal)) != -1)
            {
                i += pattern.Length;
                count++;
            }

            return count;
        }

        /// <summary>
        /// Дополнительные эвристики для определения языка
        /// </summary>
        private static int CalculateHeuristicScore(string languageId, string code)
        {
            int score = 0;
            string codeLower = code.ToLower();

            switch (languageId.ToLower())
            {
                case "python":
                    // Python: отступы, def, lambda, self, __init__, import
                    if (code.Contains("def ") || code.Contains("  ") && code.Contains(":\n"))
                        score += 10;
                    if (code.Contains("lambda ") || code.Contains("self."))
                        score += 5;
                    if (code.Contains("__init__") || code.Contains("if __name__"))
                        score += 8;
                    if (code.Contains("import ") && !code.Contains("import java"))
                        score += 3;
                    break;

                case "csharp":
                    // C#: using System, class, public, private, namespace
                    if (code.Contains("using ") && code.Contains("System."))
                        score += 10;
                    if (code.Contains("class ") && code.Contains("{"))
                        score += 8;
                    if (code.Contains("public ") || code.Contains("private ") || code.Contains("protected "))
                        score += 5;
                    if (code.Contains("namespace "))
                        score += 7;
                    if (code.Contains("Console.WriteLine") || code.Contains("Console.Write"))
                        score += 6;
                    break;

                case "java":
                    // Java: public class, import, System.out, package
                    if (code.Contains("public class") && code.Contains("{"))
                        score += 12;
                    if (code.Contains("import java."))
                        score += 10;
                    if (code.Contains("System.out.print"))
                        score += 8;
                    if (code.Contains("package "))
                        score += 7;
                    if (code.Contains("String[] args"))
                        score += 5;
                    break;

                case "javascript":
                    // JavaScript: function, var/let/const, console.log, =>, document.
                    if (code.Contains("function ") || code.Contains("=>"))
                        score += 10;
                    if (code.Contains("console.log") || code.Contains("console.error"))
                        score += 8;
                    if (code.Contains("document.") || code.Contains("window."))
                        score += 7;
                    if (code.Contains("var ") || code.Contains("let ") || code.Contains("const "))
                        score += 5;
                    if (code.Contains("addEventListener"))
                        score += 4;
                    break;

                case "typescript":
                    // TypeScript: interface, type, :string, :number, export
                    if (code.Contains("interface ") || code.Contains("type "))
                        score += 12;
                    if (code.Contains(": string") || code.Contains(": number") || code.Contains(": boolean"))
                        score += 10;
                    if (code.Contains("export "))
                        score += 8;
                    if (code.Contains("import ") && code.Contains("from "))
                        score += 7;
                    break;

                case "cpp":
                    // C++: #include, using namespace, cout <<, std::
                    if (code.Contains("#include"))
                        score += 15;
                    if (code.Contains("using namespace"))
                        score += 10;
                    if (code.Contains("cout <<") || code.Contains("cin >>"))
                        score += 8;
                    if (code.Contains("std::"))
                        score += 7;
                    if (code.Contains("int main("))
                        score += 12;
                    break;

                case "go":
                    // Go: func, package, import, fmt.Println, :=
                    if (code.Contains("func ") && code.Contains("()"))
                        score += 10;
                    if (code.Contains("package "))
                        score += 8;
                    if (code.Contains("fmt.Println") || code.Contains("fmt.Printf"))
                        score += 7;
                    if (code.Contains(":= "))
                        score += 6;
                    if (code.Contains("import ("))
                        score += 5;
                    break;

                case "rust":
                    // Rust: fn, let mut, println!, use, impl
                    if (code.Contains("fn ") && code.Contains("->"))
                        score += 10;
                    if (code.Contains("let mut"))
                        score += 8;
                    if (code.Contains("println!"))
                        score += 7;
                    if (code.Contains("use ") && code.Contains("::"))
                        score += 6;
                    if (code.Contains("impl "))
                        score += 5;
                    break;
            }

            return score;
        }

        /// <summary>
        /// Определение языка по синтаксису (резервный метод)
        /// </summary>
        private static Language DetectBySyntax(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            // Проверяем наличие специфичных синтаксических конструкций
            var lines = code.Split('\n');

            // Python: отступы, двоеточия в конце строк
            bool hasPythonIndentation = false;
            bool hasPythonColon = false;

            // C#/Java: фигурные скобки, точки с запятой
            bool hasCurlyBraces = code.Contains("{") && code.Contains("}");
            bool hasSemicolons = code.Contains(";");

            // JavaScript: стрелочные функции
            bool hasArrowFunctions = code.Contains("=>");

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // Python: строки заканчивающиеся на :
                if (trimmed.EndsWith(":"))
                    hasPythonColon = true;

                // Python: отступы пробелами (но не табами)
                if (line.StartsWith("    ") && !line.StartsWith("\t"))
                    hasPythonIndentation = true;
            }

            // Логика определения
            if (hasPythonColon && hasPythonIndentation && !hasCurlyBraces)
            {
                return Language.GetAvailableLanguages().First(l => l.Id == "python");
            }
            else if (hasCurlyBraces && hasSemicolons && code.Contains("class "))
            {
                // Различаем C# и Java
                if (code.Contains("using System") || code.Contains("namespace "))
                    return Language.GetAvailableLanguages().First(l => l.Id == "csharp");
                else if (code.Contains("import java") || code.Contains("public class"))
                    return Language.GetAvailableLanguages().First(l => l.Id == "java");
            }
            else if (hasArrowFunctions || code.Contains("function "))
            {
                // Различаем JavaScript и TypeScript
                if (code.Contains(": string") || code.Contains("interface "))
                    return Language.GetAvailableLanguages().First(l => l.Id == "typescript");
                else
                    return Language.GetAvailableLanguages().First(l => l.Id == "javascript");
            }
            else if (code.Contains("#include") || code.Contains("using namespace"))
            {
                return Language.GetAvailableLanguages().First(l => l.Id == "cpp");
            }

            return null;
        }

        /// <summary>
        /// Определяет, является ли код на определенном языке (для проверки)
        /// </summary>
        public static bool IsLanguage(string code, string languageId)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var detected = DetectLanguage(code);
            return detected?.Id == languageId?.ToLower();
        }

        /// <summary>
        /// Возвращает список возможных языков с вероятностями
        /// </summary>
        public static List<KeyValuePair<Language, int>> GetLanguageProbabilities(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return new List<KeyValuePair<Language, int>>();

            var codeLower = code.ToLower();
            var languages = Language.GetAvailableLanguages();
            var results = new List<KeyValuePair<Language, int>>();

            foreach (var lang in languages)
            {
                int score = 0;

                // Базовые ключевые слова
                foreach (var keyword in lang.Keywords)
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        score += CountOccurrences(codeLower, keyword.ToLower()) * 2;
                    }
                }

                // Эвристики
                score += CalculateHeuristicScore(lang.Id, code);

                results.Add(new KeyValuePair<Language, int>(lang, score));
            }

            // Сортируем по убыванию очков
            return results.OrderByDescending(x => x.Value).ToList();
        }
    }
}