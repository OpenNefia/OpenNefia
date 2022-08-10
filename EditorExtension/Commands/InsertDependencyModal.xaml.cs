using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections;
using System.Windows.Automation.Peers;
using static OpenNefia.EditorExtension.InsertDependencyCommandPackage;
using System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;
using System.Reflection;

namespace OpenNefia.EditorExtension
{
    public sealed class DependencyCandidate
    {
        public DependencyCandidate(TypeName typeName)
        {
            TypeName = typeName;
        }

        public TypeName TypeName { get; }

        public override string ToString()
        {
            return TypeName.FullName;
        }
    }

    public sealed class InsertDependencyModalViewModel
    {
        public InsertDependencyModalViewModel(IEnumerable<TypeName> types)
        {
            TestItems = new ObservableCollection<DependencyCandidate>(types.Select(t => new DependencyCandidate(t)));
        }

        public ObservableCollection<DependencyCandidate> TestItems { get; private set; }

        public string PropertyName { get; set; } = string.Empty;
    }

    public sealed class InsertDependencyModalResult
    {
        public InsertDependencyModalResult(string typeName, string propertyName)
        {
            TypeName = typeName;
            PropertyName = propertyName;
        }

        public string TypeName { get; }
        public string PropertyName { get; }
    }

    public partial class InsertDependencyModal : DialogWindow
    {
        public InsertDependencyModalResult Result { get; private set; } = null;

        private bool _dropDownClosing = false;

        public InsertDependencyModal(IEnumerable<TypeName> types)
        {
            DataContext = new InsertDependencyModalViewModel(types);
            InitializeComponent();
            Loaded += (o, e) =>
            {
                var textProp = TypeNameBox.GetType().GetProperty("TextBox", BindingFlags.NonPublic | BindingFlags.Instance);
                var textBox = (TextBox)textProp.GetValue(TypeNameBox);
                textBox.Focus();
                Keyboard.Focus(textBox);
            };
        }
        
        public void TypeNameBox_DropDownClosed(object sender, RoutedEventArgs args)
        {
            if (_dropDownClosing)
            {
                _dropDownClosing = false;
                return;
            }

            _dropDownClosing = true;

            var viewProp = TypeNameBox.GetType().GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance);
            var view = (ObservableCollection<object>)viewProp.GetValue(TypeNameBox);
            var viewCount = view.Count;
            if (viewCount > 0)
            {
                TypeNameBox.Text = (TypeNameBox.SelectedItem ?? view[0]).ToString();
                _dropDownClosing = true;

                if (viewCount == 1)
                {
                    OkButton_Click(this, new RoutedEventArgs());
                }
            }
        }

        public void TypeNameBox_TextChanged(object sender, RoutedEventArgs args)
        {
            PropertyNameBox.Text = InsertDependencyUtility.GetDefaultPropertyNameForType(TypeNameBox.Text.Split('.').Last());
            OkButton.IsEnabled = !string.IsNullOrWhiteSpace(TypeNameBox.Text) && !string.IsNullOrWhiteSpace(PropertyNameBox.Text);
        }

        private void TypeNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OkButton_Click(this, new RoutedEventArgs());
            }
            else if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        public void OkButton_Click(object sender, RoutedEventArgs args)
        {
            if (DialogResult != null)
                return;

            DialogResult = true;
            Result = new InsertDependencyModalResult(TypeNameBox.Text, PropertyNameBox.Text);
            Close();
        }

        public void CancelButton_Click(object sender, RoutedEventArgs args)
        {
            Close();
        }
    }
}
