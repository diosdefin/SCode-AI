using System;
using System.Collections.Generic;
using System.Drawing;
using Konvertor.Services;

namespace Konvertor.Models
{
    public class Language
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Image Icon
        {
            get
            {
                try
                {
                    return IconManager.GetIcon(Id);
                }
                catch
                {
                    return null;
                }
            }
        }

        public List<string> Keywords { get; set; }

        public Language(string id, string name)
        {
            Id = id;
            Name = name;
            Keywords = GetDefaultKeywords(id);
        }

        private List<string> GetDefaultKeywords(string languageId)
        {
            return languageId.ToLower() switch
            {
                "python" => new List<string>
                {
                    "def", "class", "import", "from", "if", "else", "elif",
                    "for", "while", "return", "print", "in", "is", "not",
                    "and", "or", "try", "except", "finally", "with", "as",
                    "lambda", "yield", "async", "await", "self", "__init__"
                },

                "csharp" => new List<string>
                {
                    "public", "private", "protected", "class", "interface",
                    "namespace", "using", "void", "int", "string", "bool",
                    "var", "if", "else", "for", "foreach", "while", "return",
                    "new", "this", "base", "static", "async", "await", "try",
                    "catch", "finally", "true", "false", "null", "Console.WriteLine"
                },

                "java" => new List<string>
                {
                    "public", "private", "protected", "class", "interface",
                    "package", "import", "void", "int", "String", "boolean",
                    "if", "else", "for", "while", "return", "new", "this",
                    "static", "System.out.println", "System.out.print", "throws"
                },

                "javascript" => new List<string>
                {
                    "function", "const", "let", "var", "if", "else", "for",
                    "while", "return", "console.log", "document", "window",
                    "addEventListener", "=>", "export", "import", "from",
                    "async", "await", "try", "catch", "finally", "class"
                },

                "typescript" => new List<string>
                {
                    "function", "const", "let", "var", "interface", "type",
                    "class", "if", "else", "for", "while", "return", "export",
                    "import", "from", ": string", ": number", ": boolean",
                    ": void", "any", "unknown", "never", "readonly"
                },

                "cpp" => new List<string>
                {
                    "#include", "using", "namespace", "int", "void", "class",
                    "if", "else", "for", "while", "return", "cout", "cin",
                    "std::", "endl", "main(", "printf", "scanf", "vector", "string"
                },

                "go" => new List<string>
                {
                    "func", "package", "import", "fmt.Println", "fmt.Printf",
                    ":= ", "var", "const", "if", "else", "for", "range",
                    "return", "struct", "interface", "error", "nil"
                },

                "rust" => new List<string>
                {
                    "fn", "let", "mut", "println!", "use", "impl", "struct",
                    "enum", "match", "if", "else", "for", "while", "loop",
                    "return", "->", "Result", "Option", "Some", "None"
                },

              



                _ => new List<string>()
            };
        }

        public override string ToString() => Name;

        public static IEnumerable<Language> GetAvailableLanguages()
        {
            // Инициализируем иконки
            IconManager.Initialize();

            return new List<Language>
            {
                new Language("python", "Python"),
                new Language("csharp", "C#"),
                new Language("java", "Java"),
                new Language("javascript", "JavaScript"),
                new Language("typescript", "TypeScript"),
                new Language("cpp", "C++"),
                new Language("go", "Go"),
                new Language("rust", "Rust"),
               
            };
        }
    }
}