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
        internal const string Title = "contains a Hungarian style prefix";
        public const string LocalVariable = "Local variable";
        public const string Parameter = "Parameter";

        public static string[] HungarianPrefixes = { "str", "s", "c", "ch", "n", "f", "i", "l", "p", "d", "b", "bln" };
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, "", "Naming", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxParameterNode, SyntaxKind.Parameter);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxVariableNode, SyntaxKind.VariableDeclaration);
        }
        private static void AnalyzeSyntaxParameterNode(SyntaxNodeAnalysisContext context)
        {
            try
            {
                ParameterSyntax param = (ParameterSyntax)context.Node;
                var foundHungarianPrefix = IdentifierStartsWithHungarian(param.Identifier.ToString());
                if (foundHungarianPrefix.Length > 0)
                {
                    var rule = new DiagnosticDescriptor(DiagnosticId, "\{Parameter} \{Title}", "\{Parameter} '{0}' contains a Hungarian style prefix: '\{foundHungarianPrefix}'", Parameter, DiagnosticSeverity.Warning, isEnabledByDefault: true);
                    var diagnostic = Diagnostic.Create(rule, param.GetLocation(), param.Identifier.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
            catch (Exception) { }
        }

        private static void AnalyzeSyntaxVariableNode(SyntaxNodeAnalysisContext context)
        {
            try
            {
                VariableDeclarationSyntax variable = (VariableDeclarationSyntax)context.Node;
                string variableName = variable.Variables.First().Identifier.ToString();

                var foundHungarianPrefix = IdentifierStartsWithHungarian(variableName);
                if (foundHungarianPrefix.Length > 0)
                {
                    var rule = new DiagnosticDescriptor(DiagnosticId, "\{Parameter} \{Title}", "\{LocalVariable} '{0}' contains a Hungarian style prefix: '\{foundHungarianPrefix}'", LocalVariable, DiagnosticSeverity.Warning, isEnabledByDefault: true);
                    var diagnostic = Diagnostic.Create(rule, variable.GetLocation(), variableName);
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
        private static string IdentifierStartsWithHungarian(string testIdentifier)
        {
            //TODO refactor to use LINQ and Regex instead. Not happy with this current implementation
            foreach (var prefix in HungarianPrefixes)
            {
                if (testIdentifier.StartsWith(prefix) || testIdentifier.StartsWith("_" + prefix))
                {
                    if (char.IsUpper(testIdentifier.Substring(prefix.Length, 1)[0]))
                    //todo: oops, take into account for length when is a field with underscore
                    {
                        return prefix;
                    }
                }
            }
            return "";
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
                    string newIdentifier = identifierToRename.Substring(prefix.Length);
                    return newIdentifier[0].ToString().ToLower() + newIdentifier.Substring(1);
                }
            }
            return identifierToRename;
        }
    }
}
