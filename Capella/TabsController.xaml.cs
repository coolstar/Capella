using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Capella
{
    public partial class TabsController : UserControl
    {
        private List<dynamic> controls;
        public List<Button> buttons;
        private int selectedControl;
        public NavController navController;

        public Grid parentGrid;
        public Window parentWindow;

        public double windowWidth = 0;

        public TabsController()
        {
            InitializeComponent();
            controls = new List<dynamic>();
            buttons = new List<Button>();
            selectedControl = 0;
        }
        public void addControl(dynamic control, Button button)
        {
            control.Width = this.Width;
            control.Height = this.Height;
            control.Margin = new Thickness(0);
            this.mainGrid.Children.Add((UserControl)control);
            controls.Add(control);
            buttons.Add(button);
            button.Click += (object sender, RoutedEventArgs e) =>
            {
                this.setSelectedControl(buttons.IndexOf(button));
            };
            button.MouseRightButtonUp += (object sender, MouseButtonEventArgs e) =>
            {
                ContextMenu menu = new ContextMenu();

                MenuItem open = new MenuItem();
                open.Header = "Open";
                open.Click += (object sender2, RoutedEventArgs e2) =>
                {
                    this.setSelectedControl(buttons.IndexOf(button));
                };
                menu.Items.Add(open);

                MenuItem addCollumn = new MenuItem();
                addCollumn.Header = "Add Collumn";
                addCollumn.Click += (object sender2, RoutedEventArgs e2) =>
                {
                    Console.WriteLine(windowWidth);
                    dynamic controlCopy = control.createCopy();
                    controlCopy.Width = this.Width;
                    controlCopy.Margin = new Thickness(windowWidth + 1, 1, 1, 1);
                    controlCopy.HorizontalAlignment = HorizontalAlignment.Left;
                    controlCopy.ClipToBounds = true;
                    windowWidth += this.Width + 1;

                    this.parentGrid.Children.Add(controlCopy);
                    Console.WriteLine(windowWidth);
                    this.parentWindow.Width = windowWidth;
                    this.parentWindow.MinWidth = windowWidth;
                    this.parentWindow.MaxWidth = windowWidth;
                    controlCopy.WillDisplay();
                };
                menu.Items.Add(addCollumn);

                menu.PlacementTarget = (UIElement)sender;
                menu.IsOpen = true;
            };
            if (controls.Count() != 1)
            {
                control.Opacity = 0;
                control.Visibility = Visibility.Hidden;
                button.Style = Resources["FlatTab"] as Style;
                TabImage buttonImg = (TabImage)button.Content;
                buttonImg.Opacity = buttonImg.DefaultOpacity;
            }
            else
            {
                button.Style = Resources["FlatTab-selected"] as Style;
                TabImage buttonImg = (TabImage)button.Content;
                buttonImg.Opacity = buttonImg.SelectedOpacity;
                control.WillDisplay();
            }
        }
        public void removeControl(dynamic control)
        {
            buttons.RemoveAt(controls.IndexOf(control));
            controls.Remove(control);
            mainGrid.Children.Remove(control);
        }
        public Button getButton(int index)
        {
            return buttons[index];
        }
        public dynamic getControl(int index)
        {
            return controls[index];
        }
        public int getSelectedControl()
        {
            return selectedControl;
        }
        public void setSelectedControl(int newSelected)
        {
            if (newSelected == selectedControl)
                return;

            if (newSelected > (controls.Count() - 1))
                throw new Exception("Index "+newSelected+" is out of range [0.."+(controls.Count-1)+"].");
            dynamic previousControl = controls[selectedControl];
            buttons[selectedControl].Style = Resources["FlatTab"] as Style;
            TabImage buttonImg = (TabImage)buttons[selectedControl].Content;
            buttonImg.Opacity = buttonImg.DefaultOpacity;

            selectedControl = newSelected;
            dynamic newControl = controls[selectedControl];
            buttons[selectedControl].Style = Resources["FlatTab-selected"] as Style;
            buttonImg = (TabImage)buttons[selectedControl].Content;
            buttonImg.Opacity = buttonImg.SelectedOpacity;

            previousControl.Visibility = Visibility.Visible;
            newControl.Visibility = Visibility.Visible;

            DoubleAnimation hideAnim = new DoubleAnimation();
            hideAnim.From = 1;
            hideAnim.To = 0;
            Storyboard.SetTarget(hideAnim, previousControl);
            Storyboard.SetTargetProperty(hideAnim, new PropertyPath(UserControl.OpacityProperty));

            DoubleAnimation showAnim = new DoubleAnimation();
            showAnim.From = 0;
            showAnim.To = 1;
            Storyboard.SetTarget(showAnim, newControl);
            Storyboard.SetTargetProperty(showAnim, new PropertyPath(UserControl.OpacityProperty));
        
            Storyboard storyBoard = new Storyboard();
            storyBoard.SpeedRatio *= 3;
            storyBoard.Children.Add(hideAnim);
            storyBoard.Children.Add(showAnim);

            storyBoard.Completed += (sender, e) =>
                {
                    previousControl.Visibility = Visibility.Hidden;
                };

            storyBoard.Begin();
            newControl.WillDisplay();
        }

        public bool isButtonSelected(Button button){
            return buttons[selectedControl] == button;
        }
    }
}
