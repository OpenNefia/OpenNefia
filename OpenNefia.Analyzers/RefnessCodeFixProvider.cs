using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace OpenNefia.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RefnessCodeFixProvider)), Shared]
    internal sealed class RefnessCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(Diagnostics.InvalidEventSubscribingByRef.Id, Diagnostics.InvalidEventSubscribingByValue.Id); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

            var diagnostic = context.Diagnostics.First();
            var declSpan = diagnostic.AdditionalLocations.First().SourceSpan;

            var declaration = root.FindToken(declSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            if (diagnostic.Id == Diagnostics.InvalidEventSubscribingByRef.Id)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: CodeFixes.CodeFixDeleteRefTitle,
                        createChangedDocument: c => DeleteRefAsync(context.Document, declaration, c),
                        equivalenceKey: nameof(CodeFixes.CodeFixDeleteRefTitle)),
                    diagnostic);
            }
            else if (diagnostic.Id == Diagnostics.InvalidEventSubscribingByValue.Id)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: CodeFixes.CodeFixInsertRefTitle,
                        createChangedDocument: c => InsertRefAsync(context.Document, declaration, c),
                        equivalenceKey: nameof(CodeFixes.CodeFixInsertRefTitle)),
                    diagnostic);
            }
        }

        private static async Task<Document> DeleteRefAsync(Document document,
            MethodDeclarationSyntax decl,
            CancellationToken cancellationToken)
        {
            var oldParamNode = decl.ParameterList.Parameters[2];
            var newParamNode = oldParamNode.WithModifiers(new SyntaxTokenList());

            var newParameters = decl.ParameterList.Parameters.Replace(oldParamNode, newParamNode);
            var newParameterList = decl.ParameterList.WithParameters(newParameters);

            var newLocal = decl.WithParameterList(newParameterList);
            var formattedLocal = newLocal.WithAdditionalAnnotations(Formatter.Annotation);

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxNode newRoot = oldRoot.ReplaceNode(decl, formattedLocal);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> InsertRefAsync(Document document,
            MethodDeclarationSyntax decl,
            CancellationToken cancellationToken)
        {
            var oldParamNode = decl.ParameterList.Parameters[2];
            var newParamNode = oldParamNode.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword)));

            var newParameters = decl.ParameterList.Parameters.Replace(oldParamNode, newParamNode);
            var newParameterList = decl.ParameterList.WithParameters(newParameters);

            var newLocal = decl.WithParameterList(newParameterList);
            var formattedLocal = newLocal.WithAdditionalAnnotations(Formatter.Annotation);

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxNode newRoot = oldRoot.ReplaceNode(decl, formattedLocal);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
