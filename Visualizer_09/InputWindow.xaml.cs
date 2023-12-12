using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Visualizer_09
{
    public partial class InputWindow : Window
    {
        // Property to retrieve the entered data
        public string IndexInput { get; private set; }
        public string SelectedType { get; private set; }
        public string ContentInput { get; private set; }

        public string TextInput { get; private set; }

        public InputWindow()
        {
            InitializeComponent();
            IndexInput = string.Empty;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the DialogResult to true and close the window
            IndexInput = indexTextBox.Text;
            ContentInput = contentTextBox.Text;
            TextInput = textTextBox.Text;
            SelectedType = (optionsComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            DialogResult = true;


            Close();
        }

    }
}
