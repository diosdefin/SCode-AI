using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Konvertor.Models;
using Newtonsoft.Json;
using System.Linq;


namespace Konvertor.Services
{
    public class PythonService
    {
        private readonly string _apiKey;

        public class AnalyzeResult
        {
            public bool Success { get; set; }
            public string Explanation { get; set; } = "";
            public string ErrorMessage { get; set; } = "";
        }

        public PythonService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<ConversionResult> ConvertCodeAsync(string sourceCode, string fromLang, string toLang)
        {
            var result = new ConversionResult
            {
                SourceCode = sourceCode,

                FromLanguage = fromLang,
                ToLanguage = toLang,
                Timestamp = DateTime.Now
            };

            try
            {
                // Создаем Python скрипт временно
                string pythonScript = CreatePythonScript(sourceCode, fromLang, toLang);
                string tempFile = Path.GetTempFileName() + ".py";
                File.WriteAllText(tempFile, pythonScript);

                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"-X utf8 \"{tempFile}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                processInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        throw new Exception("Не удалось запустить Python процесс");
                    }

                    // Устанавливаем таймаут 60 секунд
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
                    var readTask = Task.Run(async () =>
                    {
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        return new { Output = output, Error = error };
                    });

                    var completedTask = await Task.WhenAny(readTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        try { process.Kill(); } catch { }
                        result.ErrorMessage = "Таймаут выполнения (60 секунд)";
                        result.Success = false;
                        return result;
                    }

                    var readResult = await readTask;

                    if (!string.IsNullOrEmpty(readResult.Error))
                    {
                        result.ErrorMessage = readResult.Error;
                        result.Success = false;
                    }
                    else
                    {
                        // Парсим JSON ответ от Python скрипта
                        try
                        {
                            var jsonResult = JsonConvert.DeserializeObject<PythonResponse>(readResult.Output);

                            if (jsonResult != null && jsonResult.success)
                            {
                                string convertedCode = jsonResult.converted_code ?? "";

                                // ↓↓↓ ВЫЗЫВАЕМ ОЧИСТКУ ПЕРЕД ФОРМАТИРОВАНИЕМ ↓↓↓
                                convertedCode = CleanConvertedCode(convertedCode, toLang);
                                // ↑↑↑ ВЫЗЫВАЕМ ОЧИСТКУ ПЕРЕД ФОРМАТИРОВАНИЕМ ↑↑↑

                                // ФОРМАТИРУЕМ код после конвертации
                                convertedCode = FormatCode(convertedCode, toLang);

                                result.ConvertedCode = convertedCode;
                                result.Success = true;
                            }
                            else
                            {
                                result.ErrorMessage = jsonResult?.error ?? "Неизвестная ошибка";
                                result.Success = false;
                            }
                        }
                        catch (JsonException ex)
                        {
                            // Если не JSON, возможно Python вернул просто текст
                            string convertedCode = readResult.Output.Trim();

                            // ↓↓↓ И ЗДЕСЬ ТОЖЕ ОЧИСТКА ↓↓↓
                            convertedCode = CleanConvertedCode(convertedCode, toLang);
                            // ↑↑↑ И ЗДЕСЬ ТОЖЕ ОЧИСТКА ↑↑↑

                            // ФОРМАТИРУЕМ код после конвертации
                            convertedCode = FormatCode(convertedCode, toLang);

                            result.ConvertedCode = convertedCode;
                            result.Success = !string.IsNullOrWhiteSpace(result.ConvertedCode);
                            if (!result.Success)
                            {
                                result.ErrorMessage = $"Ошибка парсинга JSON: {ex.Message}";
                            }
                        }
                    }
                }

                File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Ошибка: {ex.Message}";
                result.Success = false;
            }

            return result;
        }

        public async Task<AnalyzeResult> AnalyzeCodeAsync(string sourceCode, string language)
        {
            var result = new AnalyzeResult();

            try
            {
                string pythonScript = CreatePythonAnalysisScript(sourceCode, language);
                string tempFile = Path.GetTempFileName() + ".py";
                File.WriteAllText(tempFile, pythonScript);

                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"-X utf8 \"{tempFile}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                processInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                        throw new Exception("Не удалось запустить Python процесс");

                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
                    var readTask = Task.Run(async () =>
                    {
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();
                        return new { Output = output, Error = error };
                    });

                    var completedTask = await Task.WhenAny(readTask, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        try { process.Kill(); } catch { }
                        result.Success = false;
                        result.ErrorMessage = "Таймаут выполнения (60 секунд)";
                        return result;
                    }

                    var readResult = await readTask;
                    if (!string.IsNullOrEmpty(readResult.Error))
                    {
                        result.Success = false;
                        result.ErrorMessage = readResult.Error;
                    }
                    else
                    {
                        try
                        {
                            var jsonResult = JsonConvert.DeserializeObject<PythonAnalysisResponse>(readResult.Output);
                            if (jsonResult != null && jsonResult.success)
                            {
                                result.Success = true;
                                result.Explanation = (jsonResult.explanation ?? string.Empty).Trim();
                            }
                            else
                            {
                                result.Success = false;
                                result.ErrorMessage = jsonResult?.error ?? "Неизвестная ошибка";
                            }
                        }
                        catch (JsonException)
                        {
                            // Если не JSON, покажем как есть
                            var text = readResult.Output.Trim();
                            result.Success = !string.IsNullOrWhiteSpace(text);
                            result.Explanation = text;
                            if (!result.Success)
                                result.ErrorMessage = "Пустой ответ";
                        }
                    }
                }

                File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Ошибка: {ex.Message}";
            }

            return result;
        }

        private class PythonResponse
        {
            public bool success { get; set; }
            public string? converted_code { get; set; }
            public string? error { get; set; }
        }

        private class PythonAnalysisResponse
        {
            public bool success { get; set; }
            public string? explanation { get; set; }
            public string? error { get; set; }
        }

        // ↓↓↓ МЕТОД ДЛЯ ОЧИСТКИ КОДА ОТ MARKDOWN И НАЗВАНИЙ ЯЗЫКОВ ↓↓↓
        private string CleanConvertedCode(string code, string toLang)
        {
            if (string.IsNullOrEmpty(code))
                return code;

            // Удаляем markdown блоки
            if (code.StartsWith("```"))
            {
                var lines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                {
                    code = string.Join("\n", lines.Skip(1));
                    if (code.EndsWith("```"))
                        code = code.Substring(0, code.Length - 3);
                }
                else
                {
                    code = code.Replace("```", "");
                }
            }

            // Удаляем названия языков в начале КАЖДОЙ строки
            var languageNames = new[] { "python", "csharp", "c#", "java", "javascript", "js",
                                        "typescript", "ts", "cpp", "c++", "go", "rust", "php",
                                        "html", "css" };

            var linesArray = code.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var cleanedLines = new List<string>();

            foreach (var line in linesArray)
            {
                var trimmedLine = line.Trim();
                bool skipLine = false;

                // Пропускаем строки которые содержат только название языка
                foreach (var lang in languageNames)
                {
                    if (trimmedLine.Equals(lang, StringComparison.OrdinalIgnoreCase))
                    {
                        skipLine = true;
                        break;
                    }
                }

                if (!skipLine)
                {
                    // Удаляем название языка в начале строки
                    string cleanedLine = line;
                    foreach (var lang in languageNames)
                    {
                        if (cleanedLine.StartsWith(lang, StringComparison.OrdinalIgnoreCase))
                        {
                            cleanedLine = cleanedLine.Substring(lang.Length).TrimStart();
                        }
                    }
                    cleanedLines.Add(cleanedLine);
                }
            }

            code = string.Join("\n", cleanedLines);

            // Удаляем оставшиеся ```
            code = code.Replace("```", "");

            return code.Trim();
        }
        // ↑↑↑ МЕТОД ДЛЯ ОЧИСТКИ КОДА ОТ MARKDOWN И НАЗВАНИЙ ЯЗЫКОВ ↑↑↑

        private string FormatCode(string code, string language)
        {
            if (string.IsNullOrWhiteSpace(code))
                return code;

            language = language.ToLower();

            switch (language)
            {
                case "python":
                    return FormatPythonCode(code);
                case "javascript":
                case "js":
                    return FormatJavaScriptCode(code);
                case "java":
                    return FormatJavaCode(code);
                case "c#":
                case "csharp":
                case "cs":
                    return FormatCSharpCode(code);
                case "c++":
                case "cpp":
                    return FormatCppCode(code);
                default:
                    // Для остальных языков просто убираем лишние пробелы
                    return NormalizeWhitespace(code);
            }
        }

        private string FormatPythonCode(string code)
        {
            try
            {
                // Используем black для форматирования Python кода
                return FormatWithExternalTool(code, "python", "-m black --quiet -");
            }
            catch
            {
                // Если black не установлен, делаем базовое форматирование
                return NormalizePythonIndentation(code);
            }
        }

        private string NormalizePythonIndentation(string code)
        {
            var lines = code.Split('\n');
            var result = new StringBuilder();
            int currentIndent = 0;

            foreach (var line in lines)
            {
                string trimmed = line.TrimEnd();

                // Уменьшаем отступ при закрытии блоков
                if (trimmed.StartsWith("except") ||
                    trimmed.StartsWith("elif") ||
                    trimmed.StartsWith("else") ||
                    trimmed.StartsWith("finally"))
                {
                    currentIndent = Math.Max(0, currentIndent - 4);
                }

                // Добавляем правильный отступ
                result.Append(new string(' ', currentIndent));
                result.AppendLine(trimmed);

                // Увеличиваем отступ для новых блоков
                if (trimmed.EndsWith(":") &&
                    !trimmed.Contains("#") &&
                    !trimmed.StartsWith("class") &&
                    !trimmed.StartsWith("def") &&
                    !trimmed.StartsWith("except") &&
                    !trimmed.StartsWith("elif") &&
                    !trimmed.StartsWith("else") &&
                    !trimmed.StartsWith("finally"))
                {
                    currentIndent += 4;
                }

                // Уменьшаем отступ после return, break, continue, pass
                if (trimmed.StartsWith("return ") ||
                    trimmed == "return" ||
                    trimmed.StartsWith("break") ||
                    trimmed.StartsWith("continue") ||
                    trimmed == "pass")
                {
                    currentIndent = Math.Max(0, currentIndent - 4);
                }
            }

            return result.ToString().TrimEnd();
        }

        private string FormatJavaScriptCode(string code)
        {
            try
            {
                return FormatWithExternalTool(code, "npx", "prettier --stdin-filepath script.js");
            }
            catch
            {
                try
                {
                    return FormatWithExternalTool(code, "prettier", "--stdin-filepath script.js");
                }
                catch
                {
                    return NormalizeWhitespace(code);
                }
            }
        }

        private string FormatJavaCode(string code)
        {
            // Базовое форматирование для Java (похоже на C#)
            return FormatCSharpWithBasicRules(code);
        }

        private string FormatCSharpCode(string code)
        {
            try
            {
                return FormatCSharpWithBasicRules(code);
            }
            catch
            {
                return NormalizeWhitespace(code);
            }
        }

        private string FormatCSharpWithBasicRules(string code)
        {
            var lines = code.Split('\n');
            var result = new StringBuilder();
            int indentLevel = 0;
            const int indentSize = 4;

            foreach (var line in lines)
            {
                string trimmed = line.TrimEnd();

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    result.AppendLine();
                    continue;
                }

                // Уменьшаем отступ перед закрывающей скобкой
                if (trimmed.StartsWith("}") || trimmed.StartsWith("]"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                // Добавляем отступ
                result.Append(new string(' ', indentLevel * indentSize));
                result.AppendLine(trimmed);

                // Увеличиваем отступ после открывающей скобки
                if (trimmed.EndsWith("{") || trimmed.EndsWith("["))
                {
                    indentLevel++;
                }
                // Увеличиваем отступ для многострочных выражений
                else if (trimmed.EndsWith(";") == false &&
                         trimmed.EndsWith("{") == false &&
                         trimmed.EndsWith("}") == false &&
                         !trimmed.StartsWith("#") &&
                         !trimmed.StartsWith("//") &&
                         !trimmed.StartsWith("/*") &&
                         !trimmed.StartsWith("*"))
                {
                    // Проверяем, есть ли открывающая скобка в строке
                    int openBraces = trimmed.Count(c => c == '(' || c == '[' || c == '{');
                    int closeBraces = trimmed.Count(c => c == ')' || c == ']' || c == '}');

                    if (openBraces > closeBraces)
                    {
                        indentLevel++;
                    }
                }
            }

            return result.ToString().TrimEnd();
        }

        private string FormatCppCode(string code)
        {
            try
            {
                return FormatWithExternalTool(code, "clang-format", "--style=Microsoft");
            }
            catch
            {
                return FormatCSharpWithBasicRules(code); // Похожий синтаксис с C#
            }
        }

        private string FormatWithExternalTool(string code, string tool, string args)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = tool,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        // Пишем код в stdin
                        process.StandardInput.Write(code);
                        process.StandardInput.Close();

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            return output;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Debug.WriteLine($"Ошибка форматирования с {tool}: {ex.Message}");
            }

            // Если форматирование не удалось, возвращаем исходный код
            return code;
        }

        private string NormalizeWhitespace(string code)
        {
            // Убираем лишние пробелы в конце строк
            var lines = code.Split('\n');
            var result = new StringBuilder();

            foreach (var line in lines)
            {
                result.AppendLine(line.TrimEnd());
            }

            return result.ToString().TrimEnd();
        }

        private string CreatePythonScript(string sourceCode, string fromLang, string toLang)
        {
            string groqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "YOUR_GROQ_API_KEY_HERE";
            string modelForRequest = "llama-3.1-8b-instant";

            return $@"
import requests
import json
import re
import base64

GROQ_API_KEY = '{groqApiKey}'
MODEL = '{modelForRequest}'

try:
    # Base64 для русских символов
    encoded_code = '{Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceCode))}'
    source_bytes = base64.b64decode(encoded_code)
    source_code = source_bytes.decode('utf-8')
    
    # СУПЕР-СТРОГИЙ промпт - ИСПРАВЛЕННЫЙ (ToUpper вместо upper)
    prompt = f'''CONVERT THIS CODE:

{{source_code}}

CONVERT FROM {{'{fromLang}'.upper()}} TO {{'{toLang}'.upper()}}.

STRICT RULES:
1. OUTPUT ONLY THE CONVERTED CODE
2. NO MARKDOWN (NO ```)
3. NO LANGUAGE NAMES IN OUTPUT
4. NO EXPLANATIONS
5. PRESERVE ALL COMMENTS AND STRINGS

{{'{toLang}'.upper()}} CODE:'''
    
    response = requests.post(
        'https://api.groq.com/openai/v1/chat/completions',
        headers={{
            'Authorization': f'Bearer {{GROQ_API_KEY}}',
            'Content-Type': 'application/json'
        }},
        json={{
            'model': MODEL,
            'messages': [{{'role': 'user', 'content': prompt}}],
            'temperature': 0.1,
            'max_tokens': 2000
        }}
    )
    
    result = response.json()
    code = result['choices'][0]['message']['content'].strip()
    
    # МЕГА-ОЧИСТКА
    lines = code.split('\\n')
    cleaned_lines = []
    
    for line in lines:
        stripped = line.strip()
        
        # Пропускаем строки которые ТОЛЬКО язык
        if stripped.lower() in ['python', 'csharp', 'c#', 'java', 'javascript', 
                               'js', 'typescript', 'ts', 'cpp', 'c++', 'go', 
                               'rust', 'php', 'html', 'css']:
            continue
            
        # Удаляем ``` в начале строки
        if stripped.startswith('```'):
            line = line.replace('```', '', 1)
            
        cleaned_lines.append(line)
    
    code = '\\n'.join(cleaned_lines)
    
    # Удаляем оставшиеся ```
    code = re.sub(r'^```+', '', code)
    code = re.sub(r'```+$', '', code)
    
    # Удаляем пустые строки в начале
    while code.startswith('\\n'):
        code = code[1:]
        
    code = code.strip()
    
    print(json.dumps({{
        'success': True,
        'converted_code': code,
        'error': None
    }}, ensure_ascii=False))
    
except Exception as e:
    print(json.dumps({{
        'success': False,
        'converted_code': '',
        'error': str(e)
    }}, ensure_ascii=False))
";
        }

        private string CreatePythonAnalysisScript(string sourceCode, string language)
        {
            string groqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "YOUR_GROQ_API_KEY_HERE";
            string modelForRequest = "llama-3.1-8b-instant";

            return $@"
import requests
import json
import base64

GROQ_API_KEY = '{groqApiKey}'
MODEL = '{modelForRequest}'

try:
    encoded_code = '{Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceCode))}'
    source_bytes = base64.b64decode(encoded_code)
    source_code = source_bytes.decode('utf-8')

    lang = '{language}'.strip()
    prompt = f'''Explain the following code briefly in Russian.
Give 5-10 short bullet points max.
No markdown fences. No code blocks.
If language is known, mention it.

LANGUAGE: {{lang}}

CODE:
{{source_code}}
'''

    response = requests.post(
        'https://api.groq.com/openai/v1/chat/completions',
        headers={{
            'Authorization': f'Bearer {{GROQ_API_KEY}}',
            'Content-Type': 'application/json'
        }},
        json={{
            'model': MODEL,
            'messages': [{{'role': 'user', 'content': prompt}}],
            'temperature': 0.2,
            'max_tokens': 800
        }}
    )

    result = response.json()
    text = result['choices'][0]['message']['content'].strip()

    print(json.dumps({{
        'success': True,
        'explanation': text,
        'error': None
    }}, ensure_ascii=False))

except Exception as e:
    print(json.dumps({{
        'success': False,
        'explanation': '',
        'error': str(e)
    }}, ensure_ascii=False))
";
        }
    }
}