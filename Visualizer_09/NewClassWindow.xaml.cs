using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Visualizer_09
{
    /// <summary>
    /// Interaction logic for NewClassWindow.xaml
    /// </summary>
    public partial class NewClassWindow : Window
    {
        public string typeName { get; private set; }
        public List<string> customProperties { get;  private set; }
        private List<TextBox> propertiesBoxes { get;  set; }
        
        private Button okbutton = new Button()
        {
            Name = "OkButton",
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 10, 0, 0)
        };
        private Button addbutton1 = new Button()
        {
            Name = "AddButton",
            Content = "+Add Property",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 10, 0, 0)
        };

        public NewClassWindow()
        {
            propertiesBoxes = new List<TextBox>();
            customProperties = new List<string>();
            InitializeComponent();
            stackPanel.Children.Add(addbutton1);
            stackPanel.Children.Add(okbutton);
            addbutton1.Click += AddButton_Click;
            okbutton.Click += OkButton_Click;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock textblock = new TextBlock()
            {
                Text = "Custom Property Name:",
                Margin = new Thickness(0, 10, 0, 0)
            };
            TextBox textbox = new TextBox()
            {
                Width = 200,
                Name = "PropertyTextBox",
                Margin = new Thickness(0, 10, 0, 0)
            };
            Button addbutton = new Button()
            {
                Name = "AddButton",
                Content = "+Add Property",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 10, 0, 0)
            };
            addbutton.Click += AddButton_Click;

            stackPanel.Children.Remove(okbutton);
            stackPanel.Children.Add(textblock);
            stackPanel.Children.Add(textbox);

            customProperties.Add(string.Empty);

            propertiesBoxes.Add(textbox);

            stackPanel.Children.Add(okbutton);
            okbutton.Click += OkButton_Click;
        }



        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            typeName = TypeTextBox.Text;
            for (int i = 0; i < propertiesBoxes.Count; i++)
            {
                customProperties[i] = propertiesBoxes[i].Text;
            }

            Close();
        }
    }
}
