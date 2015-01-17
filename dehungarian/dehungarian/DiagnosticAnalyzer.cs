using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace dehungarian
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DehungarianAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "dehungarian";
        internal const string Title = "Identifier contains a Hungarian style prefix";
        internal const string MessageFormat = "Identifier '{0}' contains a Hungarian style prefix";
        internal const string Category = "Naming";

        public static string[] HungarianPrefixes = new[] { "str", "s", "c", "ch", "n", "f", "i", "l", "p", "d", "b", "bln" };

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            //problem: trying to have local variables, parameters, class fields, range variables
            //unsure whether to use SymbolAction or SyntaxNodeAction.
            //using SymbolKind.Local doesn't trigger for function local variables when I run+debug a console app
            var symbolsToActOn = new[] { SymbolKind.Local, SymbolKind.Parameter, SymbolKind.Field, SymbolKind.RangeVariable }; //Enumerable.Range(0, 18).Select(x => (SymbolKind)Enum.ToObject(typeof(SymbolKind), x)).ToArray();
            context.RegisterSymbolAction(AnalyzeSymbol, symbolsToActOn);

            var syntaxTypes = new[] { SyntaxKind.IdentifierName, SyntaxKind.IdentifierToken, SyntaxKind.Parameter };
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, syntaxTypes);
        }
        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            try
            {
                var namedTypeSymbol = (INamedTypeSymbol)context.Node;

                if (IdentifierStartsWithHungarian(namedTypeSymbol.Name))
                {
                    var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
            catch (Exception) { }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            try
            {
                var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

                if (IdentifierStartsWithHungarian(namedTypeSymbol.Name))
                {
                    var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Determine if this identifier starts with a hungarian prefix. Checks the common hungarian prefixes.
        /// For fields, check also that it starts with an underscore before the common prefixes.
        /// Assumes camel casing names.
        /// Will positively identify (strCountry or iAmountOwing)
        /// but not strangeName, insider, boatName, or longRoad (str, i, b, and l respectively)
        /// </summary>
        private static bool IdentifierStartsWithHungarian(string testIdentifier)
        {
            //TODO refactor to use LINQ and Regex instead. Not happy with this current implementation
            foreach (var prefix in HungarianPrefixes)
            {
                if (testIdentifier.StartsWith(prefix) || testIdentifier.StartsWith("_" + prefix))
                {
                    return char.IsUpper(testIdentifier.Substring(0, prefix.Length)[0]);  //todo: oops, take into account for length when is a field with underscore
                }
            }
            return false;
        }

        /// <summary>
        /// Suggest the name to be simply what the user had without the hungarian prefix.
        /// Provides the suggestion with the first letter lowercased.
        /// </summary>
        public static string SuggestDehungarianName(string identifierToRename)
        {
            //todo: all kinds of empty string checking etc. what happens if substring returns empty string, etc.
            foreach (var prefix in HungarianPrefixes)
            {
                if (identifierToRename.StartsWith(prefix))
                {
                    string newIdentifier = identifierToRename.Substring(0, prefix.Length);
                    return newIdentifier[0].ToString().ToLower() + newIdentifier.Substring(1);
                }
            }

            return identifierToRename;
        }

    }
}
