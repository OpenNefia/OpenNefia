using System;

namespace OpenNefia.Core.UserInterface.XAML
{
    public sealed class ContentAttribute : Attribute
    {
    }

    public sealed class UsableDuringInitializationAttribute : Attribute
    {
        public UsableDuringInitializationAttribute(bool usable)
        {
        }
    }

    public sealed class DeferredContentAttribute : Attribute
    {
    }

    public interface ITestRootObjectProvider
    {
        object RootObject { get; }
    }

    public interface ITestProvideValueTarget
    {
        object TargetObject { get; }
        object TargetProperty { get; }
    }

    public interface ITestUriContext
    {
        Uri BaseUri { get; set; }
    }
}
