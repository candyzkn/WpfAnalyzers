﻿namespace WpfAnalyzers.DependencyProperties
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class WA1211ClrPropertyTypeMustMatchRegisteredType : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "WA1211";
        private const string Title = "DependencyProperty CLR property type must match registered type.";
        private const string MessageFormat = "Property '{0}' must be of type {1}";
        private const string Description = Title;
        private const string HelpLink = "http://stackoverflow.com/";

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
            var propertyDeclaration = context.Node as PropertyDeclarationSyntax;
            if (propertyDeclaration == null || propertyDeclaration.IsMissing)
            {
                return;
            }

            var propertySymbol = context.ContainingSymbol as IPropertySymbol;
            if (propertySymbol == null)
            {
                return;
            }

            TypeSyntax registeredType;
            if (!propertyDeclaration.TryGetDependencyPropertyRegisteredType(out registeredType))
            {
                return;
            }

            var actualTypeSymbol = propertySymbol.Type;
            var registeredTypeSymbol = context.SemanticModel.GetTypeInfo(registeredType);
            if (!TypeHelper.IsSameType(actualTypeSymbol, registeredTypeSymbol.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, propertyDeclaration.Type.GetLocation(), propertySymbol, registeredTypeSymbol.Type));
            }
        }
    }
}