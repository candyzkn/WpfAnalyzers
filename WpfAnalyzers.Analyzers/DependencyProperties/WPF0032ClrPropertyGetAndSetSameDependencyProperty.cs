﻿namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WPF0032ClrPropertyGetAndSetSameDependencyProperty : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WPF0032";
        private const string Title = "Use same dependency property in get and set.";
        private const string MessageFormat = "Property '{0}' must access same dependency property in getter and setter";
        private const string Description = Title;
        private static readonly string HelpLink = WpfAnalyzers.HelpLink.ForId(DiagnosticId);

        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
                                                                      DiagnosticId,
                                                                      Title,
                                                                      MessageFormat,
                                                                      AnalyzerCategory.DependencyProperties,
                                                                      DiagnosticSeverity.Error,
                                                                      AnalyzerConstants.EnabledByDefault,
                                                                      Description,
                                                                      HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void HandleDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            var propertyDeclaration = context.Node as PropertyDeclarationSyntax;
            if (propertyDeclaration == null || propertyDeclaration.IsMissing)
            {
                return;
            }

            if (ClrProperty.TryGetBackingFields(
propertyDeclaration,
context.SemanticModel,
context.CancellationToken,
out IFieldSymbol getter,
out IFieldSymbol setter))
            {
                if (DependencyProperty.TryGetDependencyPropertyKeyField(
    getter,
    context.SemanticModel,
    context.CancellationToken,
    out IFieldSymbol keyField))
                {
                    getter = keyField;
                }

                if (ReferenceEquals(getter, setter))
                {
                    return;
                }

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptor,
                        propertyDeclaration.GetLocation(),
                        context.ContainingSymbol.Name));
            }
        }
    }
}