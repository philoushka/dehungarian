using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace dehungarian
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DehungarianCodeFixProvider)), Shared]
    public class DehungarianCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DehungarianAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            const string RemoveHungarianPrefixCommand = "Remove Hungarian prefix";
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            switch (diagnostic.Descriptor.Category)
            {
                case DehungarianAnalyzer.Parameter:
                    var paramToken = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ParameterSyntax>().First();

                    context.RegisterCodeFix(
                        CodeAction.Create(RemoveHungarianPrefixCommand, c => RemoveHungarianPrefix(context.Document, paramToken, c)),
                        diagnostic);
                    break;
                case DehungarianAnalyzer.LocalVariable:
                    var variableToken = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclarationSyntax>().First();

                    context.RegisterCodeFix(
                        CodeAction.Create(RemoveHungarianPrefixCommand, c => RemoveHungarianPrefix(context.Document, variableToken, c)),
                        diagnostic);
                    break;
                default:
                    break;
            }
        }

        private async Task<Solution> RemoveHungarianPrefix(Document document, VariableDeclarationSyntax token, CancellationToken cancellationToken)
        {
            var identifierToken = token.Variables.First();
            var newName = DehungarianAnalyzer.SuggestDehungarianName(identifierToken.Identifier.Text);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var tokenSymbol = semanticModel.GetDeclaredSymbol(token.Variables.First(), cancellationToken);
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, tokenSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);
            return newSolution;
        }

        private async Task<Solution> RemoveHungarianPrefix(Document document, ParameterSyntax token, CancellationToken cancellationToken)
        {
            var newName = DehungarianAnalyzer.SuggestDehungarianName(token.Identifier.Text);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var tokenSymbol = semanticModel.GetDeclaredSymbol(token, cancellationToken);
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, tokenSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);
            return newSolution;
        }        
    }
}