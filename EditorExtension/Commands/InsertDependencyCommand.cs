using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using OpenNefia.EditorExtension;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using System.Security.AccessControl;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.LanguageServices;
using System.Collections.Generic;
using System.Windows;

namespace OpenNefia.EditorExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class InsertDependencyCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ff861f2c-6ee8-44e0-801d-1cb1a705f8e3");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly InsertDependencyCommandPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertDependencyCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private InsertDependencyCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = (package as InsertDependencyCommandPackage) ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static InsertDependencyCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in InsertDependencyCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new InsertDependencyCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            package.JoinableTaskFactory.RunAsync(async () =>
            {
                var types = await package.GetInsertableTypes();
                var modal = new InsertDependencyModal(types);
                modal.Owner = Application.Current.MainWindow;
                if (modal.ShowDialog() == true)
                {
                    await DoInsertDependency(modal.Result.TypeName, modal.Result.PropertyName);
                }
            });
        }

        private SyntaxTree UpdateUsingDirectives(SyntaxTree originalTree, UsingDirectiveSyntax[] newUsings)
        {
            var rootNode = originalTree.GetRoot() as CompilationUnitSyntax;

            var seen = new HashSet<string>();
            var usings = new List<UsingDirectiveSyntax>();

            foreach (var u in rootNode.Usings.Concat(newUsings))
            {
                var text = u.Name.GetText().ToString();
                if (!seen.Contains(text))
                {
                    seen.Add(text);
                    usings.Add(u);
                }
            }

            rootNode = rootNode.WithUsings(new SyntaxList<UsingDirectiveSyntax>(usings));
            return rootNode.SyntaxTree;
        }

        private static bool IsIoCDependencyField(FieldDeclarationSyntax node, string typeName)
        {
            var ident = (node.Declaration.Type as IdentifierNameSyntax);
            var sameType = ident != null && typeName.EndsWith("." + ident.Identifier.Text);

            bool hasIoCAttribute = HasIoCAttribute(node);

            return sameType && hasIoCAttribute;
        }

        private static bool HasIoCAttribute(FieldDeclarationSyntax node)
        {
            var hasIoCAttribute = false;
            foreach (var attribute in node.AttributeLists.SelectMany(al => al.Attributes))
            {
                // Doesn't use CsFiles, but close enough.
                if (attribute.Name.GetText().ToString().Contains("Dependency"))
                {
                    hasIoCAttribute = true;
                    break;
                }
            }

            return hasIoCAttribute;
        }

        private async Task DoInsertDependency(string typeName, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(propertyName))
                return;

            var textManager = await package.GetServiceAsync<SVsTextManager, IVsTextManager>();
            textManager.GetActiveView(1, null, out var textView);
            var componentModel = await package.GetServiceAsync<SComponentModel, IComponentModel>();
            var adaptersFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            var workspace = (Workspace)componentModel.GetService<VisualStudioWorkspace>();
            var wpfTextView = adaptersFactory.GetWpfTextView(textView);

            var caretPosition = wpfTextView.Caret.Position.BufferPosition;
            var document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            var syntaxRoot = await document.GetSyntaxRootAsync();

            var classDeclNode =
                syntaxRoot
                    .FindToken(caretPosition).Parent.AncestorsAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault();

            if (classDeclNode == null)
                return;

            var existing = classDeclNode.ChildNodes()
                .OfType<FieldDeclarationSyntax>()
                .Where(f => IsIoCDependencyField(f, typeName))
                .FirstOrDefault();

            if (existing != null)
                return;

            var index = typeName.LastIndexOf('.');
            if (index == -1)
                return;
            
            var ns = typeName.Remove(index);

            var newUsings = new UsingDirectiveSyntax[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("OpenNefia.Core.IoC")).NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns)).NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed),
            };

            var newSyntaxTree = UpdateUsingDirectives(syntaxRoot.SyntaxTree, newUsings);
            var newSyntaxRoot = await newSyntaxTree.GetRootAsync();

            var documentRoot = await document.GetSyntaxRootAsync();

            var newRoot = documentRoot.ReplaceNode(documentRoot, newSyntaxRoot);
            document = await Formatter.FormatAsync(document.WithSyntaxRoot(newRoot), Formatter.Annotation);
            workspace.TryApplyChanges(document.Project.Solution);

            caretPosition = wpfTextView.Caret.Position.BufferPosition;

            classDeclNode =
                newRoot
                    .FindToken(caretPosition).Parent.AncestorsAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault();

            if (classDeclNode == null)
                return;

            var shortName = typeName.Split('.').Last();

            var span = classDeclNode.OpenBraceToken.FullSpan;

            existing = classDeclNode.ChildNodes()
                .OfType<FieldDeclarationSyntax>()
                .Where(f => HasIoCAttribute(f))
                .LastOrDefault();

            string trivia;
            if (existing != null)
            {
                span = existing.FullSpan;
                trivia = "\r\n";
            }
            else
            {
                trivia = "\r\n\r\n";
            }

            wpfTextView.TextBuffer.Insert(span.End, $"        [Dependency] private readonly {shortName} {propertyName} = default!;{trivia}");

            document = await Formatter.FormatAsync(document, Formatter.Annotation, workspace.Options);
            workspace.TryApplyChanges(document.Project.Solution);
        }
    }
}