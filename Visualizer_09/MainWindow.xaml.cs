using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Text.Json;

using Visualizer_09;
using static System.Net.Mime.MediaTypeNames;
using static Visualizer_09.MainWindow;
using System.Xaml;

namespace Visualizer_09
{
    public class ItemsCollection
    {
        public Dictionary<string, Dictionary<string, object>> NamedItems { get; set; }

        public ItemsCollection()
        {
            NamedItems = new Dictionary<string, Dictionary<string, object>>();
        }
    }

    public partial class MainWindow : Window
    {
        public List<string> StringList { get; set; }

        public ObservableCollection<GraphPoint> pointsCollection = new ObservableCollection<GraphPoint>();
        private List<Thumb> thumbs = new List<Thumb>();
        public List<ThumbObjectLink> ThumbObjectLinks { get; set; }

        public List<string> propertiesForJohn { get; set; }
        private List<string> pointsParameters { get; set; }
        private Canvas canvas;
        private bool isDragging = false;
        private Point lastMousePosition;
        private double canvasWidth;
        private double canvasHeight;
        private Grid grid_canvas;
        private List<Thumb> thumb_to_delete { get; set; }
        private List<Line> line_to_delete { get; set; }


        LinearGradientBrush linearGradientBrush = new LinearGradientBrush();



        public class Thumb : System.Windows.Controls.Primitives.Thumb
        {
            public static readonly DependencyProperty IsSelectedProperty =
                DependencyProperty.Register("IsSelected", typeof(bool), typeof(Thumb), new PropertyMetadata(false));

            public bool IsSelected
            {
                get { return (bool)GetValue(IsSelectedProperty); }
                set { SetValue(IsSelectedProperty, value); }
            }
        }
        public class GraphPoint
        {
            public double x { get; set; }
            public double y { get; set; }
            public string Type { get; set; }
            public string Content { get; set; }
            public List<string> Connections { get; set; }
            public string Index { get; set; }
            public string Text { get; set; }
            public string Answer { get; set; }
            public Brush Color { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }


            public Dictionary<string, object> CustomProperties { get; set; }
            public GraphPoint()
            {
                Connections = new List<string>();
                CustomProperties = new Dictionary<string, object>();
            }

        }

        public class ThumbObjectLink
        {
            public Thumb Thumb { get; set; }
            public GraphPoint AssociatedObject { get; set; }

            // Parameterless constructor
            public static ThumbObjectLink GetLinkByThumb(List<ThumbObjectLink> links, Thumb thumb)
            {
                return links.FirstOrDefault(link => link.Thumb == thumb);
            }

            // Helper method to get the ThumbObjectLink associated with a specific GraphPoint
            public static ThumbObjectLink GetLinkByGraphPoint(List<ThumbObjectLink> links, GraphPoint graphPoint)
            {
                return links.FirstOrDefault(link => link.AssociatedObject == graphPoint);
            }
        }






        public MainWindow()
        {
            thumb_to_delete = new List<Thumb>();
            line_to_delete = new List<Line>();
            //defining the gradient color
            linearGradientBrush.StartPoint = new System.Windows.Point(0.5, 0);
            linearGradientBrush.EndPoint = new System.Windows.Point(0.5, 1);
            linearGradientBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1d2124"), 0.0)); //FF100585 //#6B7A8F
            linearGradientBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1c2124"), 1.0)); //230ff7    //#7897AB

            //collections defining
            pointsCollection = new ObservableCollection<GraphPoint>();
            ThumbObjectLinks = new List<ThumbObjectLink>();

            InitializeComponent();

            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString("#68747a");
            canvas = new Canvas
            {
                Background = brush
            };
            canvas.Width = 5000;
            canvas.Height = 5000;
            WireUpEventHandlers(); // Call this method to wire up event handlers

            WindowState = WindowState.Maximized;

            ExplorerLB.ItemsSource = pointsCollection;
            DrawEditableGraph();
            canvasWidth = canvas.ActualWidth;
            canvasHeight = canvas.ActualHeight;

            CreateGrid();
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /*var itemsCollection = new ItemsCollection();
            itemsCollection.NamedItems["type1"] = new Dictionary<string, object>
            {
                { "color", "colorstring" },
                { "property2", "property2string" }

            };
            itemsCollection.NamedItems["Alice"] = new Dictionary<string, object>
            {
                { "Age", "30" },
                { "Address", "456 Oak St" },
                { "Occupation", "Engineer" }
            };

            string jsonData = JsonSerializer.Serialize(itemsCollection);

            // write to the properties.json file
            File.WriteAllText("properties.json", jsonData);
*/

            // read from the properties.json file
            /*string loadedJsonData = File.ReadAllText("properties.json");

            // convert the json format to the objects
            ItemsCollection loadedItemsCollection = JsonSerializer.Deserialize<ItemsCollection>(loadedJsonData);

            if (loadedItemsCollection.NamedItems.TryGetValue("type1", out var johnParameters))
            {
                string johnAge = johnParameters["color"].ToString();
                string johnAddress = (string)johnParameters["property2"].ToString();

                textblockoutput.Text = johnAge;
            }*/
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public int AddElement(string type, string content, string index, string text, string answer)
        {
            //0 - Subject
            //1 - Topic
            //2 - Example
            //3 - Task
            //4 - Theorem

            GraphPoint element1 = new GraphPoint();
            element1.Type = type;
            element1.Content = content;
            element1.Index = index;
            element1.Text = text;
            element1.Answer = answer;
            pointsCollection.Add(element1);

            int ret_obj = GetIndexByIndex(index);
            return ret_obj;
        }

        public void RemoveElement(GraphPoint object2)
        {
            int object_index = GetIndexByIndex(object2.Index);
            Thumb thumb_to_delete = ThumbObjectLinks[object_index].Thumb;
            thumbs.Remove(thumb_to_delete);
            pointsCollection.Remove(object2 as GraphPoint);
            ThumbObjectLinks.Remove(ThumbObjectLinks[object_index]);
        }

        private int GetIndexByIndex(string targetValue)
        {
            // Use LINQ to find the index based on the property value
            GraphPoint item = pointsCollection.FirstOrDefault(x => x.Index == targetValue);

            if (item != null)
            {
                int index = pointsCollection.IndexOf(item);
                return index;
            }
            else
            {
                return -1;
            }


        }

        private bool CheckInput(string index)
        {
            bool isOK = true;

            for (int i = 0; i < pointsCollection.Count; i++)
            {
                if (pointsCollection[i].Index == index || index.Contains(" "))
                {
                    isOK = false;
                    MessageBox.Show("You've provided the wrong input.");
                    break;
                }
            }

            return isOK;
        }

        public void AddButton_Click(object sender, RoutedEventArgs e)
        {

            InputWindow inputWindow = new InputWindow(this);
            ComboBox comboBoxInInputWindow = inputWindow.typesComboBox;
            string existingJsonData = File.ReadAllText("properties.json");
            var existingItemsCollection = JsonSerializer.Deserialize<ItemsCollection>(existingJsonData);

            foreach (var namedItem in existingItemsCollection.NamedItems)
            {
                ComboBoxItem comboitem = new ComboBoxItem
                {
                    Content = namedItem.Key
                };
                comboBoxInInputWindow.Items.Add(comboitem);
            }

            comboBoxInInputWindow.Items.Add("testthing");
            
            inputWindow.ShowDialog();
            

            if (inputWindow.DialogResult == true && CheckInput(inputWindow.IndexInput))
            {
                string indexInput = inputWindow.IndexInput;
                string selectedOption = inputWindow.SelectedType;
                string contentInput = inputWindow.ContentInput;
                string textInput = inputWindow.TextInput;

                int length = contentInput.Length;

                int width = 100;
                int height = 70;
                if (selectedOption == "Subject" || selectedOption == null)
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#2713a1");
                    SolidColorBrush brush = new SolidColorBrush(color);



                    int ret_obj = AddElement("0", contentInput, indexInput, textInput, null);
                    pointsCollection[ret_obj].Width = 130.0 + 3.4 * length;
                    pointsCollection[ret_obj].Height = 100 + 1.1 * length;
                    pointsCollection[ret_obj].Color = brush;
                    Thumb ret_thumb = CreateEditablePoint(0, 0, pointsCollection[ret_obj].Width, pointsCollection[ret_obj].Height, contentInput, brush, pointsCollection[ret_obj]);
                    canvas.Children.Add(ret_thumb);
                }
                else if (selectedOption == "Topic")
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#9dc183");
                    SolidColorBrush brush = new SolidColorBrush(color);
                    int ret_obj = AddElement("1", contentInput, indexInput, textInput, null);
                    pointsCollection[ret_obj].Width = (width + 10) + 1.5 * length;
                    pointsCollection[ret_obj].Height = height + 5;
                    pointsCollection[ret_obj].Color = brush;
                    Thumb ret_thumb = CreateEditablePoint(0, 0, pointsCollection[ret_obj].Width, pointsCollection[ret_obj].Height, contentInput, brush, pointsCollection[ret_obj]);
                    canvas.Children.Add(ret_thumb);
                }
                else if (selectedOption == "Example")
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#b3ff00");
                    SolidColorBrush brush = new SolidColorBrush(color);
                    int ret_obj = AddElement("2", contentInput, indexInput, textInput, null);
                    pointsCollection[ret_obj].Width = width + 1.5 * length;
                    pointsCollection[ret_obj].Height = height;
                    pointsCollection[ret_obj].Color = brush;
                    Thumb ret_thumb = CreateEditablePoint(0, 0, pointsCollection[ret_obj].Width, pointsCollection[ret_obj].Height, contentInput, brush, pointsCollection[ret_obj]);
                    canvas.Children.Add(ret_thumb);
                }
                else if (selectedOption == "Task")
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#ff8800");
                    SolidColorBrush brush = new SolidColorBrush(color);
                    int ret_obj = AddElement("3", contentInput, indexInput, textInput, null);
                    pointsCollection[ret_obj].Width = (width - 5) + 1.5 * length;
                    pointsCollection[ret_obj].Height = height - 2;
                    pointsCollection[ret_obj].Color = brush;
                    Thumb ret_thumb = CreateEditablePoint(0, 0, pointsCollection[ret_obj].Width, pointsCollection[ret_obj].Height, contentInput, brush, pointsCollection[ret_obj]);
                    canvas.Children.Add(ret_thumb);
                }
                else if (selectedOption == "Theorem")
                {
                    Color color = (Color)ColorConverter.ConvertFromString("#ffd000");
                    SolidColorBrush brush = new SolidColorBrush(color);
                    int ret_obj = AddElement("4", contentInput, indexInput, textInput, null);
                    pointsCollection[ret_obj].Width = width;
                    pointsCollection[ret_obj].Height = height;
                    pointsCollection[ret_obj].Color = brush;
                    Thumb ret_thumb = CreateEditablePoint(0, 0, pointsCollection[ret_obj].Width, pointsCollection[ret_obj].Height, contentInput, brush, pointsCollection[ret_obj]);
                    canvas.Children.Add(ret_thumb);
                }
                else //if (selectedOption != null && existingItemsCollection.NamedItems.ContainsKey(selectedOption))
                {
                    foreach (var item in existingItemsCollection.NamedItems)
                    {
                        if (item.Key == selectedOption)
                        {
                            Color color = (Color)ColorConverter.ConvertFromString("#ffd000");
                            SolidColorBrush brush = new SolidColorBrush(color);
                            int ret_obj = AddElement(item.Key, contentInput, indexInput, textInput, null);
                            
                            pointsCollection[ret_obj].Width = width;
                            pointsCollection[ret_obj].Height = height;
                            pointsCollection[ret_obj].Color = brush;
                            Thumb ret_thumb = CreateEditablePoint(0, 0, pointsCollection[ret_obj].Width, pointsCollection[ret_obj].Height, contentInput, brush, pointsCollection[ret_obj]);
                            canvas.Children.Add(ret_thumb);
                        } else
                        {
                            if(selectedOption == null)
                            {
                                MessageBox.Show("selectedoption is null");
                            }
                        }
                        
                    }
                }


                /*                AddElement(1, "sub1", null, null);
                */
                ExplorerLB.ItemsSource = pointsCollection;
                foreach (ThumbObjectLink link in ThumbObjectLinks)
                {
                    Thumb thumb1 = link.Thumb;
                    EventManager.RegisterClassHandler(typeof(Thumb), UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(Thumb_MouseLeftButtonUp));

                }


            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectPointsFromExplorer();
        }

        /*private void ProcessSelectedItems()
        {
            // Retrieve the selected items from the ListBox
            List<string> selectedItems = new List<string>();
            foreach (ListBoxItem item in ExplorerLB.SelectedItems)
            {
                selectedItems.Add(item.Content.ToString());
            }
            if (selectedItems.Count == 2)
            {
                string FirstSelectedItem = selectedItems[0];
                string SecondSelectedItem = selectedItems[1];

            }
        }*/



        private void ConnectPointsFromExplorer()
        {
            var selectedItems = ExplorerLB.SelectedItems.Cast<GraphPoint>().ToList();

            if (selectedItems.Count == 2)
            {
                // also add an if() for the bug situation when the first point is connected to the second,
                // but the second is not connected to the first
                if (!selectedItems[0].Connections.Contains(selectedItems[1].Index) && !selectedItems[1].Connections.Contains(selectedItems[0].Index))
                {
                    selectedItems[0].Connections.Add(selectedItems[1].Index);
                    selectedItems[1].Connections.Add(selectedItems[0].Index);
                    ConnectThumbs();

                    //just output for some information(connections list of selectedItems[0])
                    /*string connectionsText = string.Join(", ", selectedItems[0].Connections);
                    outputTextBlock.Text = connectionsText;*/
                }
                else
                {
                    MessageBox.Show("points are already connected, or a bug occured");
                }
            }
            else
            {
                MessageBox.Show("you can only connect 2 points");
            }
        }

        public void UnconnectPointsFromExplorer()
        {
            var selectedItems = ExplorerLB.SelectedItems.Cast<GraphPoint>().ToList();

            if (selectedItems.Count == 2)
            {
                // also add an if() for the bug situation when the first point is connected to the second,
                // but the second is not connected to the first
                if (selectedItems[0].Connections.Contains(selectedItems[1].Index) && selectedItems[1].Connections.Contains(selectedItems[0].Index))
                {
                    selectedItems[0].Connections.Remove(selectedItems[1].Index);
                    selectedItems[1].Connections.Remove(selectedItems[0].Index);


                }
                else
                {
                    MessageBox.Show("points are not connected, or a bug occured");
                }
            }
            else
            {
                MessageBox.Show("you can only connect 2 points");
            }
        }


        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////'///////////////////////////////////
        /// </summary>




        /*public void LinkThumbsWithObjects()
        {

            for (int i = 0; i < pointsCollection.Count; i++)
            {
                Thumb thumb = CreateEditablePoint(pointsCollection[i].x, pointsCollection[i].y, 70, 50, pointsCollection[i].Content, pointsCollection[i].Color);
                // Associate the Thumb with the corresponding item in the ObservableCollection

                ThumbObjectLinks[i].Thumb = thumb;
                ThumbObjectLinks[0].AssociatedObject = pointsCollection[i];
                *//*ThumbObjectLink link = new ThumbObjectLink(thumb, pointsCollection[i]);
                ThumbObjectLinks.Add(link);*//*


                thumbs.Add(thumb);
            }
        }*/

        private void WireUpEventHandlers()
        {
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
            canvas.MouseWheel += Canvas_MouseWheel;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            textblockoutput.Text = "Canvas_MouseDown got triggered";
            /*isDragging = true;
            lastMousePosition = e.GetPosition(canvas);*/
            isDragging = true;
            canvas.CaptureMouse();
            lastMousePosition = e.GetPosition(canvas);
            canvas.CaptureMouse();
            DeselectAllThumbs();
            DeselectAllItems();
        }


        // Assuming you set canvasWidth and canvasHeight somewhere, such as in the window constructor.


        /*private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentMousePosition = e.GetPosition(canvas);
                double offsetX = currentMousePosition.X - lastMousePosition.X;
                double offsetY = currentMousePosition.Y - lastMousePosition.Y;

                foreach (UIElement element in canvas.Children)
                {
                    if (element is Thumb)
                    {
                        double left = Canvas.GetLeft(element);
                        double top = Canvas.GetTop(element);

                        // Ensure the element stays within the canvas bounds
                        double newLeft = Math.Max(0, Math.Min(canvas.ActualWidth - element.RenderSize.Width, left + offsetX));
                        double newTop = Math.Max(0, Math.Min(canvas.ActualHeight - element.RenderSize.Height, top + offsetY));

                        Canvas.SetLeft(element, newLeft);
                        Canvas.SetTop(element, newTop);
                    }
                    else if (element is Line)
                    {
                        // Handle dragging for lines
                        Line line = element as Line;
                        line.X1 += offsetX;
                        line.Y1 += offsetY;
                        line.X2 += offsetX;
                        line.Y2 += offsetY;
                    }
                }

                lastMousePosition = currentMousePosition;
            }
        }*/


        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                canvas.ReleaseMouseCapture();
            }
        }
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var canvas = sender as Canvas;

            if (canvas != null)
            {
                // Adjust the scaling factor as needed
                double scale = (e.Delta > 0) ? 1.2 : 0.8; // You can adjust the scaling factors

                // Apply scaling
                ScaleTransform scaleTransform = canvas.LayoutTransform as ScaleTransform;

                if (scaleTransform == null)
                {
                    // Create a new ScaleTransform if none exists
                    scaleTransform = new ScaleTransform();
                    canvas.LayoutTransform = scaleTransform;
                }

                // Apply the scaling factor within the specified range
                scaleTransform.ScaleX = Math.Max(0.25, Math.Min(2, scaleTransform.ScaleX * scale));
                scaleTransform.ScaleY = Math.Max(0.25, Math.Min(2, scaleTransform.ScaleY * scale));
            }
        }

        /*private void CreateGrid() 
        {
            for (int i = 1; i < 500; i++)
            {
                for(int k = 1; k < 500; k++)
                {
                    Ellipse ellipse = new Ellipse
                    {
                        Width = 1,
                        Height = 1,
                        Fill = Brushes.Red
                    };
                    canvas.Children.Add(ellipse);
                    Canvas.SetLeft(ellipse, k * 10);
                    Canvas.SetTop(ellipse, i * 10);
                }

            }
        }*/

        private void CreateGrid()
        {
            Color color = (Color)ColorConverter.ConvertFromString("#1a1a1a");
            SolidColorBrush brush = new SolidColorBrush(color);
            for (int i = 0; i <= 98; i++)
            {
                for (int j = 0; j <= 98; j++)
                {
                    Ellipse ellipse = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Fill = brush
                    };

                    // Set the position of the ellipse
                    Canvas.SetLeft(ellipse, i * 50);
                    Canvas.SetTop(ellipse, j * 50);

                    // Add the ellipse to the canvas
                    canvas.Children.Add(ellipse);
                }
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentMousePosition = e.GetPosition(canvas);
                double offsetX = currentMousePosition.X - lastMousePosition.X;
                double offsetY = currentMousePosition.Y - lastMousePosition.Y;

                foreach (UIElement element in canvas.Children)
                {
                    if (element is Thumb)
                    {
                        // Handle dragging for thumbs
                        double left = Canvas.GetLeft(element);
                        double top = Canvas.GetTop(element);
                        Canvas.SetLeft(element, left + offsetX);
                        Canvas.SetTop(element, top + offsetY);
                    }
                    else if (element is Line)
                    {
                        // Handle dragging for lines
                        Line line = element as Line;
                        line.X1 += offsetX;
                        line.Y1 += offsetY;
                        line.X2 += offsetX;
                        line.Y2 += offsetY;
                    }
                }

                lastMousePosition = currentMousePosition;
            }
        }

        /*private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            canvas.ReleaseMouseCapture();
        }
*/

        private void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked ListBoxItem
            var clickedItem = VisualTreeHelper.HitTest(ExplorerLB, e.GetPosition(ExplorerLB)).VisualHit as ListBoxItem;

            // If the clicked area is not over any item, unselect all items
            if (clickedItem == null)
            {
                ExplorerLB.SelectedItems.Clear();
                foreach (Thumb thumb in selectedThumbs)
                {
                    ThumbObjectLink thumbLink = ThumbObjectLink.GetLinkByThumb(ThumbObjectLinks, thumb);
                    GraphPoint obj = thumbLink.AssociatedObject;
                    ControlTemplate thumbTemplate2 = new ControlTemplate(typeof(Thumb));

                    FrameworkElementFactory gridFactory2 = new FrameworkElementFactory(typeof(Grid));
                    FrameworkElementFactory ellipseFactory2 = new FrameworkElementFactory(typeof(Ellipse));
                    //ellipseFactory2.SetValue(Shape.FillProperty, new SolidColorBrush(Color.FromRgb(179, 179, 179)));
                    ellipseFactory2.SetValue(Shape.FillProperty, obj.Color);
                    ellipseFactory2.SetValue(FrameworkElement.WidthProperty, obj.Width);
                    ellipseFactory2.SetValue(FrameworkElement.HeightProperty, obj.Height);
                    //ellipseFactory2.SetValue(Shape.StrokeProperty, new SolidColorBrush(Color.FromRgb(247, 247, 247)));
                    ellipseFactory2.SetValue(Shape.StrokeThicknessProperty, 0.0);

                    FrameworkElementFactory textBlockFactory2 = new FrameworkElementFactory(typeof(TextBlock));
                    textBlockFactory2.SetValue(TextBlock.TextProperty, obj.Content);
                    textBlockFactory2.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    textBlockFactory2.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                    gridFactory2.AppendChild(ellipseFactory2);
                    gridFactory2.AppendChild(textBlockFactory2);
                    thumbTemplate2.VisualTree = gridFactory2;
                    thumb.Template = thumbTemplate2;
                    thumb.IsSelected = false;
                }
            }
        }

        private void DeselectAllItems()
        {
            ExplorerLB.SelectedItems.Clear();
        }


        private void DrawEditableGraph()
        {
            // Create Thumbs for editable points
            for (int i = 0; i < pointsCollection.Count; i++)
            {
                var thumb = CreateEditablePoint(pointsCollection[i].x, pointsCollection[i].y, 100, 70, pointsCollection[i].Content, pointsCollection[i].Color, pointsCollection[i]);
                thumbs.Add(thumb);
                canvas.Children.Add(thumb);
                canvas.UpdateLayout();

            }

            // Connect the thumbs with lines
            ConnectThumbs();
            if (mainWindow.Content is Grid mainGrid)
            {
                SecondSubwindow.Children.Add(canvas);
            }
        }



        private Thumb CreateEditablePoint(double x, double y, double width, double height, string text, Brush fill, GraphPoint obj)
        {

            var thumb = new Thumb
            {
                Width = width,
                Height = height,
                Background = Brushes.Red,
                Cursor = Cursors.Hand
            };
            ControlTemplate thumbTemplate = new ControlTemplate(typeof(Thumb));
            FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

            // Customize the visual appearance of the Thumb here

            FrameworkElementFactory ellipseFactory = new FrameworkElementFactory(typeof(Ellipse));
            ellipseFactory.SetValue(Shape.FillProperty, fill);
            ellipseFactory.SetValue(FrameworkElement.WidthProperty, width);
            ellipseFactory.SetValue(FrameworkElement.HeightProperty, height);
            ellipseFactory.SetValue(FrameworkElement.NameProperty, "PART_Ellipse");


            // Add a TextBlock to display text
            FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.TextProperty, text);
            textBlockFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlockFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Add the Ellipse and TextBlock to the Grid
            gridFactory.AppendChild(ellipseFactory);
            gridFactory.AppendChild(textBlockFactory);

            // Set the Grid as the VisualTree of the ControlTemplate
            thumbTemplate.VisualTree = gridFactory;

            // Set the ControlTemplate to the Thumb
            thumb.Template = thumbTemplate;


            // Custom property example (set as needed)
            //thumb.Tag = customProperty;

            // Register the DragDelta event handler
            thumb.DragDelta += Thumb_DragDelta;
            thumb.MouseDoubleClick += Thumb_MouseDoubleClick;


            ThumbObjectLink link = new ThumbObjectLink
            {
                Thumb = thumb,
                AssociatedObject = obj
            };

            ThumbObjectLinks.Add(link);

            Color color = (Color)ColorConverter.ConvertFromString("#303030");
            SolidColorBrush brush = new SolidColorBrush(color);
            //#dedede
            Color textColor = (Color)ColorConverter.ConvertFromString("#dedede");
            SolidColorBrush textBrush = new SolidColorBrush(textColor);
            Rectangle roundedRectangle = new Rectangle
            {
                Width = 220,
                Height = 350,
                Fill = fill,
                RadiusX = 10,
                RadiusY = 10
            };
            Rectangle contentRectangle = new Rectangle
            {
                Width = 210,
                Height = 50,
                Fill = brush,
                RadiusX = 10,
                RadiusY = 10
            };

            Rectangle textRectangle = new Rectangle
            {
                Width = 210,
                Height = 285,
                Fill = brush,
                RadiusX = 10,
                RadiusY = 10
            };
            
            TextBlock contentText = new TextBlock
            {
                
                Text = obj.Content,
                FontSize = 14,
                Foreground = textBrush,
                TextWrapping = TextWrapping.Wrap,
                Width = 200,
                Height = 50
            };
            TextBlock descriptionText = new TextBlock
            {

                Text = obj.Text,
                FontSize = 14.3,
                Foreground = textBrush,
                TextWrapping = TextWrapping.Wrap,
                Width = 200,
                Height = 285
            };

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500),
            };

            timer.Tick += (sender, e) =>
            {
                roundedRectangle.Fill = obj.Color;
                canvas.Children.Add(roundedRectangle);
                canvas.Children.Add(contentRectangle);
                canvas.Children.Add(textRectangle);
                canvas.Children.Add(descriptionText);
                if (obj.Content.Length <= 47)
                {
                    if (obj.Content.Length <= 20)
                    {
                        contentText.FontSize = 18;
                    }
                    contentText.Text = obj.Content;
                }
                else
                {
                    string truncatedString = obj.Content.Length <= 47
                    ? obj.Content
                    : obj.Content.Substring(0, 47) + "...";
                    contentText.Text = truncatedString;
                }
                if (obj.Text.Length! <= 497)
                {
                    string truncatedString = obj.Text.Length <= 497
                    ? obj.Text
                    : obj.Text.Substring(0, 497) + "...";
                    descriptionText.Text = truncatedString;
                }
                canvas.Children.Add(contentText);
                timer.Stop();
                Canvas.SetLeft(roundedRectangle, Canvas.GetLeft(thumb) + (obj.Width + 10));
                Canvas.SetTop(roundedRectangle, Canvas.GetTop(thumb) - 35);
                Canvas.SetLeft(contentRectangle, Canvas.GetLeft(thumb) + (obj.Width + 15));
                Canvas.SetTop(contentRectangle, Canvas.GetTop(thumb) - 30);
                Canvas.SetLeft(textRectangle, Canvas.GetLeft(thumb) + (obj.Width + 15));
                Canvas.SetTop(textRectangle, Canvas.GetTop(thumb) + 25);
                Canvas.SetLeft(contentText, Canvas.GetLeft(thumb) + (obj.Width + 20));
                Canvas.SetTop(contentText, Canvas.GetTop(thumb) - 25);
                Canvas.SetLeft(descriptionText, Canvas.GetLeft(thumb) + (obj.Width + 20));
                Canvas.SetTop(descriptionText, Canvas.GetTop(thumb) + 25);
            };

            thumb.MouseEnter += (sender, e) =>
            {
                timer.Start();
                e.Handled = true;

            };
            thumb.MouseLeave += (sender, e) =>
            {
                if (timer.IsEnabled)
                {
                    timer.Stop();
                }
                else
                {
                    canvas.Children.Remove(roundedRectangle);
                    canvas.Children.Remove(contentRectangle);
                    canvas.Children.Remove(textRectangle);
                    canvas.Children.Remove(contentText);
                    canvas.Children.Remove(descriptionText);

                }
            }; 



            Canvas.SetLeft(thumb, x);
            Canvas.SetTop(thumb, y);
            canvas.UpdateLayout();

            return thumb;
        }

        public void Thumb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Thumb thumb = (Thumb)sender;
            ThumbObjectLink thumbLink = ThumbObjectLink.GetLinkByThumb(ThumbObjectLinks, thumb);
            GraphPoint obj = thumbLink.AssociatedObject;
            textblockoutput.Text = "thumb mousclick detected";

            DetailsWindow detailsWindow = new DetailsWindow();
            detailsWindow.ShowDialog();
        }

        private List<Thumb> selectedThumbs = new List<Thumb>();

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                DeselectAllThumbs();
            }
        }


        private bool IsThumbWiredWithEvent(Thumb thumb, MouseButtonEventHandler handler)
        {
            // Get the list of event handlers for the MouseLeftButtonDown event
            var eventHandlersStore = typeof(UIElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(thumb, null);

            if (eventHandlersStore != null)
            {
                var handlersField = eventHandlersStore.GetType().GetField("_handlers", BindingFlags.Instance | BindingFlags.NonPublic);
                var handlers = handlersField?.GetValue(eventHandlersStore) as RoutedEventHandlerInfo[];

                if (handlers != null)
                {
                    return handlers.Any(info => info.Handler == handler);
                }
            }

            return false;
        }




        private void Thumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            Thumb thumb = (Thumb)sender;
            ThumbObjectLink thumbLink = ThumbObjectLink.GetLinkByThumb(ThumbObjectLinks, thumb);
            GraphPoint obj = thumbLink.AssociatedObject;
            textblockoutput.Text = "some text";


            thumb.IsSelected = true;
            selectedThumbs.Add(thumb);
            ControlTemplate thumbTemplate2 = new ControlTemplate(typeof(Thumb));
            FrameworkElementFactory gridFactory2 = new FrameworkElementFactory(typeof(Grid));
            FrameworkElementFactory ellipseFactory2 = new FrameworkElementFactory(typeof(Ellipse));
            //ellipseFactory2.SetValue(Shape.FillProperty, new SolidColorBrush(Color.FromRgb(179, 179, 179)));
            ellipseFactory2.SetValue(Shape.FillProperty, obj.Color);
            ellipseFactory2.SetValue(FrameworkElement.WidthProperty, obj.Width);
            ellipseFactory2.SetValue(FrameworkElement.HeightProperty, obj.Height);
            ellipseFactory2.SetValue(Shape.StrokeProperty, new SolidColorBrush(Color.FromRgb(247, 247, 247)));
            ellipseFactory2.SetValue(Shape.StrokeThicknessProperty, 2.0);

            FrameworkElementFactory textBlockFactory2 = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory2.SetValue(TextBlock.TextProperty, obj.Index);
            textBlockFactory2.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlockFactory2.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            gridFactory2.AppendChild(ellipseFactory2);
            gridFactory2.AppendChild(textBlockFactory2);
            thumbTemplate2.VisualTree = gridFactory2;
            thumb.Template = thumbTemplate2;
            textblockoutput.Text = "with shift part works";


            var ItemsInExplorer = ExplorerLB.Items.Cast<GraphPoint>().ToList();
                
            
            int numb = GetIndexByIndex(obj.Index);
            ExplorerLB.SelectedItems.Add(ItemsInExplorer[numb]);

        }

        private void DeselectAllThumbs()
        {
            foreach (Thumb thumb in selectedThumbs)
            {
                ThumbObjectLink thumbLink = ThumbObjectLink.GetLinkByThumb(ThumbObjectLinks, thumb);
                GraphPoint obj = thumbLink.AssociatedObject;
                ControlTemplate thumbTemplate2 = new ControlTemplate(typeof(Thumb));

                FrameworkElementFactory gridFactory2 = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory ellipseFactory2 = new FrameworkElementFactory(typeof(Ellipse));
                //ellipseFactory2.SetValue(Shape.FillProperty, new SolidColorBrush(Color.FromRgb(179, 179, 179)));
                ellipseFactory2.SetValue(Shape.FillProperty, obj.Color);
                ellipseFactory2.SetValue(FrameworkElement.WidthProperty, obj.Width);
                ellipseFactory2.SetValue(FrameworkElement.HeightProperty, obj.Height);
                //ellipseFactory2.SetValue(Shape.StrokeProperty, new SolidColorBrush(Color.FromRgb(247, 247, 247)));
                ellipseFactory2.SetValue(Shape.StrokeThicknessProperty, 0.0);

                FrameworkElementFactory textBlockFactory2 = new FrameworkElementFactory(typeof(TextBlock));
                textBlockFactory2.SetValue(TextBlock.TextProperty, obj.Content);
                textBlockFactory2.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                textBlockFactory2.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                gridFactory2.AppendChild(ellipseFactory2);
                gridFactory2.AppendChild(textBlockFactory2);
                thumbTemplate2.VisualTree = gridFactory2;
                thumb.Template = thumbTemplate2;
                thumb.IsSelected = false;
            }

            selectedThumbs.Clear();
        }


        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;

            if (thumb != null)
            {
                double newX = Canvas.GetLeft(thumb) + e.HorizontalChange;
                double newY = Canvas.GetTop(thumb) + e.VerticalChange;


                newX = Math.Min(Math.Max(newX, 0), canvas.ActualWidth - thumb.ActualWidth);
                newY = Math.Min(Math.Max(newY, 0), canvas.ActualHeight - thumb.ActualHeight);

                // Update the position of the thumb
                Canvas.SetLeft(thumb, newX);
                Canvas.SetTop(thumb, newY);

                // Reconnect the thumbs with lines
                ConnectThumbs();
            }
        }

        private void ConnectThumbs()
        {
            // Clear existing lines
            foreach (UIElement element in canvas.Children)
            {
                if(element is Line line)
                {
                    line_to_delete.Add(line);

                } else if (element is Thumb thumb)
                {
                    thumb_to_delete.Add(thumb);
                }
            }
            foreach (Thumb thumb in thumb_to_delete)
            {
                canvas.Children.Remove(thumb);
            }
            foreach (Line line in line_to_delete)
            {
                canvas.Children.Remove(line);
            }
            thumb_to_delete.Clear();
            line_to_delete.Clear();

            // Draw lines between thumbs
            for (int i = 0; i < ThumbObjectLinks.Count; i++)
            {
                GraphPoint graph_obj = ThumbObjectLinks[i].AssociatedObject;
                Thumb thumb1 = ThumbObjectLinks[i].Thumb;
                List<GraphPoint> connected_thumbs = new List<GraphPoint>();

                List<ThumbObjectLink> matchingThumbLinks = ThumbObjectLinks
                    .Where(link => graph_obj.Connections.Contains(link.AssociatedObject.Index))
                    .ToList();
                for (int k = 0; k < matchingThumbLinks.Count; k++)
                {
                    ConnectThumbs(canvas, thumb1, matchingThumbLinks[k].Thumb);
                }

            }

            // Add thumbs to the canvas after lines (to ensure they are on top)
            for (int i = 0; i < ThumbObjectLinks.Count; i++)
            {
                canvas.Children.Add(ThumbObjectLinks[i].Thumb);
            }
        }



        private void ConnectThumbs(Canvas canvas, Thumb thumb1, Thumb thumb2)
        {
            // Create a Line to connect two thumbs
            var line = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                X1 = Canvas.GetLeft(thumb1) + thumb1.Width / 2,
                Y1 = Canvas.GetTop(thumb1) + thumb1.Height / 2,
                X2 = Canvas.GetLeft(thumb2) + thumb2.Width / 2,
                Y2 = Canvas.GetTop(thumb2) + thumb2.Height / 2
            };

            // Add the Line to the Canvas
            canvas.Children.Add(line);
        }

        /*public void AddPointToGraph(double x, double y)
        {
            var thumb = CreateEditablePoint(x, y, 70, 50, "Topic1", );
            thumbs.Add(thumb);
            canvas.Children.Add(thumb);
            ConnectThumbs();
        }*/

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems2 = ExplorerLB.SelectedItems.Cast<GraphPoint>().ToList();

            for (int i = 0; i < selectedItems2.Count; i++)
            {
                RemoveElement(selectedItems2[i]);
            }
            ConnectThumbs();
        }

        private void UnconnectButton_Click(object sender, RoutedEventArgs e)
        {
            UnconnectPointsFromExplorer();
            ConnectThumbs();
        }

        private void ExplorerLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeselectAllThumbs();
            var selectedItems = ExplorerLB.SelectedItems.Cast<GraphPoint>().ToList();
            foreach (GraphPoint point in selectedItems)
            {
                
                Thumb thumb = ThumbObjectLinks[GetIndexByIndex(point.Index)].Thumb;
                thumb.IsSelected = true;
                selectedThumbs.Add(thumb);
                ControlTemplate thumbTemplate2 = new ControlTemplate(typeof(Thumb));
                FrameworkElementFactory gridFactory2 = new FrameworkElementFactory(typeof(Grid));
                FrameworkElementFactory ellipseFactory2 = new FrameworkElementFactory(typeof(Ellipse));
                //ellipseFactory2.SetValue(Shape.FillProperty, new SolidColorBrush(Color.FromRgb(179, 179, 179)));
                ellipseFactory2.SetValue(Shape.FillProperty, point.Color);
                ellipseFactory2.SetValue(FrameworkElement.WidthProperty, point.Width);
                ellipseFactory2.SetValue(FrameworkElement.HeightProperty, point.Height);
                ellipseFactory2.SetValue(Shape.StrokeProperty, new SolidColorBrush(Color.FromRgb(247, 247, 247)));
                ellipseFactory2.SetValue(Shape.StrokeThicknessProperty, 2.0);

                FrameworkElementFactory textBlockFactory2 = new FrameworkElementFactory(typeof(TextBlock));
                textBlockFactory2.SetValue(TextBlock.TextProperty, point.Index);
                textBlockFactory2.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                textBlockFactory2.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                gridFactory2.AppendChild(ellipseFactory2);
                gridFactory2.AppendChild(textBlockFactory2);
                thumbTemplate2.VisualTree = gridFactory2;
                thumb.Template = thumbTemplate2;
            }
        }

        private void NewClassButton_Click(object sender, RoutedEventArgs e)
        {
            NewClassWindow classWindow = new NewClassWindow();
            classWindow.ShowDialog();

            List<string> propertiesForJohn = classWindow.customProperties;

            string TypeName = classWindow.typeName;
            ComboBoxItem newType = new ComboBoxItem();
            newType.Content = TypeName;



            string existingJsonData = File.ReadAllText("properties.json");
            var existingItemsCollection = JsonSerializer.Deserialize<ItemsCollection>(existingJsonData);

            if (existingItemsCollection.NamedItems.ContainsKey(TypeName))
            {
                foreach (string property in propertiesForJohn)
                {
                    if (!existingItemsCollection.NamedItems[TypeName].ContainsKey(property))
                    {
                        existingItemsCollection.NamedItems[TypeName][property] = "Space";
                    }
                }
            }
            else
            {
                // If the key "type1" doesn't exist, create a new entry
                existingItemsCollection.NamedItems[TypeName] = new Dictionary<string, object>();
                foreach (string property in propertiesForJohn)
                {
                    existingItemsCollection.NamedItems[TypeName][property] = "Space";
                }
            }

            string updatedJsonData = JsonSerializer.Serialize(existingItemsCollection);
            File.WriteAllText("properties.json", updatedJsonData);

            /*var itemsCollection = new ItemsCollection();
            itemsCollection.NamedItems[TypeName] = new Dictionary<string, object>();
            foreach (string property in propertiesForJohn)
            {
                itemsCollection.NamedItems[TypeName].Add(property, "string");
            }
            string jsonData = JsonSerializer.Serialize(itemsCollection);
            File.WriteAllText("properties.json", jsonData);
*/

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ThumbObjectLink link in ThumbObjectLinks)
            {
                Thumb thumb1 = link.Thumb;
                GraphPoint obj = link.AssociatedObject as GraphPoint;
                obj.x = Canvas.GetLeft(thumb1);
                obj.y = Canvas.GetTop(thumb1);
            }
            List<string> pointsParameters = new List<string>();
            List<GraphPoint> objectList = new List<GraphPoint>();
            string existingJsonData = File.ReadAllText("PointsCollection.json");
            var existingItemsCollection = JsonSerializer.Deserialize<ItemsCollection>(existingJsonData);
            foreach (GraphPoint point in pointsCollection)
            {
                objectList.Add(point);
            }
            string jsonString = JsonSerializer.Serialize(objectList);
            File.WriteAllText("PointsCollection.json", jsonString);

            
        }
    }

}
