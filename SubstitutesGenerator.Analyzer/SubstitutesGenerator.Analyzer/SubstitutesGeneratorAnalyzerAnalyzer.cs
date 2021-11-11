using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace SubstitutesGenerator.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SubstitutesGeneratorAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SubstitutesGeneratorAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ObjectCreationExpression);
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is ObjectCreationExpressionSyntax objectCreation)
            {
                var assign = objectCreation.AncestorsAndSelf().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
                var paramertersAreDefault = objectCreation.ArgumentList.DescendantNodes().OfType<ArgumentSyntax>().All(argument =>
                {
                    return argument.Expression.IsKind(SyntaxKind.DefaultLiteralExpression);
                });

                if (assign.Left is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == "_sut" && paramertersAreDefault)
                {
                    var symbol = context.SemanticModel.GetSymbolInfo(objectCreation.Type);
                    if (symbol.Symbol is INamedTypeSymbol namedTypeSymbol)
                    {
                        var constructor = namedTypeSymbol.Constructors.FirstOrDefault(x => x.Parameters.Length > 0);
                        var names = constructor.Parameters.Select(parameter => parameter.Type.Name).ToArray();
                        var msg = string.Join(", ", names);

                        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
                    }
                }

            }
        }
    }
}
