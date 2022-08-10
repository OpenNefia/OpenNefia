using System.Windows;
using System.Windows.Input;

namespace OpenNefia.EditorExtension
{
    public static class KeyboardFocus
    {
        public static readonly DependencyProperty OnProperty;

        public static void SetOn(UIElement element, FrameworkElement value)
        {
            element.SetValue(OnProperty, value);
        }

        public static FrameworkElement GetOn(UIElement element)
        {
            return (FrameworkElement)element.GetValue(OnProperty);
        }

        static KeyboardFocus()
        {
            OnProperty = DependencyProperty.RegisterAttached("On", typeof(FrameworkElement), typeof(KeyboardFocus), new PropertyMetadata(OnSetCallback));
        }

        private static void OnSetCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var frameworkElement = (FrameworkElement)dependencyObject;
            var target = GetOn(frameworkElement);

            if (target == null)
                return;

            frameworkElement.Loaded += (s, e) => Keyboard.Focus(target);
        }
    }
}