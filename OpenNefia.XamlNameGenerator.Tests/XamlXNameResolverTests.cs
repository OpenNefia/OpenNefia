﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.NameGenerator.Compiler;
using Avalonia.NameGenerator.Domain;
using Avalonia.NameGenerator.Generator;
using Avalonia.ReactiveUI;
using Avalonia.NameGenerator.Tests.Views;
using Xunit;

namespace Avalonia.NameGenerator.Tests;

public class XamlXNameResolverTests
{
    [Theory]
    [InlineData(View.NamedControl)]
    [InlineData(View.XNamedControl)]
    [InlineData(View.AttachedProps)]
    public async Task Should_Resolve_Types_From_Avalonia_Markup_File_With_Named_Control(string resource)
    {
        var xaml = await View.Load(resource);
        var controls = ResolveNames(xaml);

        Assert.NotEmpty(controls);
        Assert.Equal(1, controls.Count);
        Assert.Equal("UserNameTextBox", controls[0].Name);
        Assert.Equal(typeof(TextBox).FullName, controls[0].TypeName);
    }

    [Theory]
    [InlineData(View.NamedControls)]
    [InlineData(View.XNamedControls)]
    public async Task Should_Resolve_Types_From_Avalonia_Markup_File_With_Named_Controls(string resource)
    {
        var xaml = await View.Load(resource);
        var controls = ResolveNames(xaml);

        Assert.NotEmpty(controls);
        Assert.Equal(3, controls.Count);
        Assert.Equal("UserNameTextBox", controls[0].Name);
        Assert.Equal("PasswordTextBox", controls[1].Name);
        Assert.Equal("SignUpButton", controls[2].Name);
        Assert.Equal(typeof(TextBox).FullName, controls[0].TypeName);
        Assert.Equal(typeof(TextBox).FullName, controls[1].TypeName);
        Assert.Equal(typeof(Button).FullName, controls[2].TypeName);
    }

    [Fact]
    public async Task Should_Resolve_Types_From_Avalonia_Markup_File_With_Custom_Controls()
    {
        var xaml = await View.Load(View.CustomControls);
        var controls = ResolveNames(xaml);

        Assert.NotEmpty(controls);
        Assert.Equal(3, controls.Count);
        Assert.Equal("ClrNamespaceRoutedViewHost", controls[0].Name);
        Assert.Equal("UriRoutedViewHost", controls[1].Name);
        Assert.Equal("UserNameTextBox", controls[2].Name);
        Assert.Equal(typeof(RoutedViewHost).FullName, controls[0].TypeName);
        Assert.Equal(typeof(RoutedViewHost).FullName, controls[1].TypeName);
        Assert.Equal("Controls.CustomTextBox", controls[2].TypeName);
    }

    [Fact]
    public async Task Should_Not_Resolve_Named_Controls_From_Avalonia_Markup_File_Without_Named_Controls()
    {
        var xaml = await View.Load(View.NoNamedControls);
        var controls = ResolveNames(xaml);

        Assert.Empty(controls);
    }

    [Fact]
    public async Task Should_Not_Resolve_Elements_From_DataTemplates()
    {
        var xaml = await View.Load(View.DataTemplates);
        var controls = ResolveNames(xaml);

        Assert.NotEmpty(controls);
        Assert.Equal(2, controls.Count);
        Assert.Equal("UserNameTextBox", controls[0].Name);
        Assert.Equal("NamedListBox", controls[1].Name);
        Assert.Equal(typeof(TextBox).FullName, controls[0].TypeName);
        Assert.Equal(typeof(ListBox).FullName, controls[1].TypeName);
    }

    [Fact]
    public async Task Should_Resolve_Names_From_Complex_Views()
    {
        var xaml = await View.Load(View.SignUpView);
        var controls = ResolveNames(xaml);

        Assert.NotEmpty(controls);
        Assert.Equal(9, controls.Count);
        Assert.Equal("UserNameTextBox", controls[0].Name);
        Assert.Equal("UserNameValidation", controls[1].Name);
        Assert.Equal("PasswordTextBox", controls[2].Name);
        Assert.Equal("PasswordValidation", controls[3].Name);
        Assert.Equal("AwesomeListView", controls[4].Name);
        Assert.Equal("ConfirmPasswordTextBox", controls[5].Name);
        Assert.Equal("ConfirmPasswordValidation", controls[6].Name);
        Assert.Equal("SignUpButton", controls[7].Name);
        Assert.Equal("CompoundValidation", controls[8].Name);
    }

    private static IReadOnlyList<ResolvedName> ResolveNames(string xaml)
    {
        var compilation =
            View.CreateAvaloniaCompilation()
                .WithCustomTextBox();

        var classResolver = new XamlXViewResolver(
            new RoslynTypeSystem(compilation),
            MiniCompiler.CreateDefault(
                new RoslynTypeSystem(compilation),
                MiniCompiler.AvaloniaXmlnsDefinitionAttribute));

        var classInfo = classResolver.ResolveView(xaml);
        var nameResolver = new XamlXNameResolver();
        return nameResolver.ResolveNames(classInfo.Xaml);
    }
}