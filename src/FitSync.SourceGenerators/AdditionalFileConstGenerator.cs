namespace FitSync.SourceGenerators;

using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class AdditionalFileConstGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        foreach (AdditionalText file in context.AdditionalFiles)
        {
            SourceText? text = file.GetText(context.CancellationToken);
            if (text is null)
                continue;

            string fileName = Path.GetFileNameWithoutExtension(file.Path);
            string className = ToPascalCase(fileName);
            string escaped = text.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");

            string source = $@"namespace FitSync.Api.Generated;

public static class {className}
{{
    public const string Text = ""{escaped}"";
}}
";
            context.AddSource($"{className}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string ToPascalCase(string fileName)
    {
        StringBuilder sb = new();
        bool upper = true;
        foreach (char c in fileName)
        {
            if (c == '-' || c == '_')
            {
                upper = true;
            }
            else
            {
                sb.Append(upper ? char.ToUpper(c) : c);
                upper = false;
            }
        }
        return sb.ToString();
    }
}
