// dehungarian - Copyright (c) 2016 CaptiveAire

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace dehungarian
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(DehungarianCodeFixProvider)), Shared]
    public class DehungarianCodeFixProvider : CodeFixProvider
    {
        const string Title = "Remove Hungarian Prefix";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DehungarianAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.FirstOrDefault();

            var diagnosticSpan = diagnostic?.Location?.SourceSpan;

            if (diagnosticSpan == null)
            {
                return;
            }

            switch (diagnostic.Descriptor.Category)
            {
                case DehungarianAnalyzer.Parameter:
                    var paramToken = root.FindToken(diagnosticSpan.Value.Start).Parent.AncestorsAndSelf().OfType<ParameterSyntax>().FirstOrDefault();

                    if (paramToken != null)
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(Title, c => this.RemoveHungarianPrefix(context.Document, paramToken, c), Title),
                            diagnostic);
                    }

                    break;
                case DehungarianAnalyzer.LocalVariable:
                    var variableToken =
                        root.FindToken(diagnosticSpan.Value.Start).Parent.AncestorsAndSelf().OfType<VariableDeclarationSyntax>().FirstOrDefault();

                    if (variableToken != null)
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(Title, c => this.RemoveHungarianPrefix(context.Document, variableToken, c), Title),
                            diagnostic);
                    }

                    break;
                default:
                    break;
            }
        }

        private async Task<Solution> RemoveHungarianPrefix(
            Document document,
            VariableDeclarationSyntax token,
            CancellationToken cancellationToken)
        {
            var identifierToken = token.Variables.First();
            var newName = DehungarianAnalyzer.SuggestDehungarianName(identifierToken.Identifier.Text);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var tokenSymbol = semanticModel.GetDeclaredSymbol(token.Variables.First(), cancellationToken);
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution =
                await
                Renamer.RenameSymbolAsync(document.Project.Solution, tokenSymbol, newName, optionSet, cancellationToken)
                    .ConfigureAwait(false);
            return newSolution;
        }

        private async Task<Solution> RemoveHungarianPrefix(Document document, ParameterSyntax token, CancellationToken cancellationToken)
        {
            var newName = DehungarianAnalyzer.SuggestDehungarianName(token.Identifier.Text);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var tokenSymbol = semanticModel.GetDeclaredSymbol(token, cancellationToken);
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution =
                await
                Renamer.RenameSymbolAsync(document.Project.Solution, tokenSymbol, newName, optionSet, cancellationToken)
                    .ConfigureAwait(false);
            return newSolution;
        }
    }
}