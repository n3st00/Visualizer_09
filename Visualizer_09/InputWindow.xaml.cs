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

        public MainWindow MainWindowReference { get; set; }


        private Button okbutton = new Button()
        {
            Name = "OkButton",
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 10, 0, 0)
        };
        public InputWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            stackPanel.Children.Add(okbutton);
            okbutton.Click += OkButton_Click;
            IndexInput = string.Empty;


            MainWindowReference = mainWindow;
        }

        public ComboBox typesComboBox
        {
            get { return optionsComboBox; }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            IndexInput = indexTextBox.Text;
            ContentInput = contentTextBox.Text;
            TextInput = textTextBox.Text;
            SelectedType = (optionsComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            
            DialogResult = true;
            Close();
        }

        
    }
}
