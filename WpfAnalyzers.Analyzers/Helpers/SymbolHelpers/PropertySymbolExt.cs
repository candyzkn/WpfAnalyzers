﻿namespace WpfAnalyzers
{
    using System.Linq;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using WpfAnalyzers.DependencyProperties;

    internal static class PropertySymbolExt
    {
        internal static bool TryGetSetterSyntax(this IPropertySymbol property, out AccessorDeclarationSyntax setter)
        {
            setter = null;
            if (property?.SetMethod.DeclaringSyntaxReferences.Length != 1)
            {
                return false;
            }

            var reference = property.SetMethod.DeclaringSyntaxReferences[0];
            setter = reference.SyntaxTree.GetRoot()
                              .FindNode(reference.Span) as AccessorDeclarationSyntax;
            return setter != null;
        }

        internal static bool IsPotentialDependencyPropertyAccessor(this IPropertySymbol property)
        {
            return property != null &&
                   !property.IsIndexer &&
                   !property.IsReadOnly &&
                   !property.IsWriteOnly &&
                   !property.IsStatic && 
                   property.ContainingType.IsAssignableToDependencyObject();
        }

        internal static bool TryGetMutableDependencyPropertyField(this IPropertySymbol property, SemanticModel semanticModel, CancellationToken cancellationToken, out IFieldSymbol result)
        {
            result = null;
            if (!property.IsPotentialDependencyPropertyAccessor())
            {
                return false;
            }

            AccessorDeclarationSyntax setter;
            if (TryGetSetterSyntax(property, out setter))
            {
                FieldDeclarationSyntax fieldDeclaration;
                if (setter.TryGetDependencyPropertyFromSetter(out fieldDeclaration))
                {
                    if (fieldDeclaration.IsDependencyPropertyKeyField())
                    {
                        return false;
                    }

                    result = property.ContainingType.GetMembers(fieldDeclaration.Name())
                                     .OfType<IFieldSymbol>()
                                     .FirstOrDefault();
                    return result != null;
                }
            }

            foreach (var field in property.ContainingType.GetMembers().OfType<IFieldSymbol>())
            {
                if (field.Name.IsParts(property.Name, "Property"))
                {
                    if (!field.IsPotentialDependencyPropertyBackingField())
                    {
                        return false;
                    }

                    result = field;
                }

                if (field.Name.IsParts(property.Name, "PropertyKey"))
                {
                    return false;
                }
            }

            return result != null;
        }
    }
}