using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xavalon.XamlStyler;
using Xavalon.XamlStyler.Options;

namespace XamlStyler.CodeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class XamlStyleAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new NotImplementedException();

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
            context.RegisterCompilationAction(this.AnalyzeXaml);
        }

        private void AnalyzeXaml(CompilationAnalysisContext context)
        {
            var additionalFiles = context.Options.AdditionalFiles;
            var xamlFiles = additionalFiles.Where(file => Path.GetExtension(file.Path) == "xaml");

            var stylerOptions = new StylerOptions();
            var styleService = new StylerService(stylerOptions);

            foreach (var xamlFile in xamlFiles)
            {
                // Process each XAML file
                var originalContents = xamlFile.GetText().ToString();
                var originalContentsHash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(originalContents));

                var styledContents = styleService.StyleDocument(originalContents);
                var styledContentsHash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(originalContents));

                if (originalContentsHash != styledContentsHash)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "id",
                            "title",
                            "msg format",
                            "category",
                            DiagnosticSeverity.Warning,
                            true
                        ),
                        Location.Create(
                            xamlFile.Path,
                            TextSpan.FromBounds(0, originalContents.Length - 1),
                            new LinePositionSpan(
                                new LinePosition(0, 0),
                                new LinePosition(0, 0)
                            )
                        )
                    ));
                }
            }
        }
    }
}
