using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Capella
{
    /// <summary>
    /// Interaction logic for NavController.xaml
    /// </summary>
    public partial class NavController : UserControl
    {
        private List<dynamic> controls;
        public NavController navController;
        private bool isAnimating = false;
        public NavController(dynamic rootControl)
        {
            InitializeComponent();
            rootControl.Width = this.Width;
            rootControl.Height = this.Height;
            rootControl.Margin = new Thickness(0);
            rootControl.navController = this;
            mainGrid.Children.Add(rootControl);

            controls = new List<dynamic>();
            controls.Add(rootControl);
        }

        public NavController createCopy()
        {
            dynamic rootControl = controls[0];

            dynamic rootCopy = rootControl.createCopy();

            NavController navCopy = new NavController(rootCopy);
            return navCopy;
        }

        public void pushControl(dynamic control)
        {
            if (isAnimating)
                return;
            control.navController = this;
            control.Width = this.Width;
            control.Height = this.Height;

            dynamic currentControl = controls[controls.Count - 1];

            ThicknessAnimation animOut = new ThicknessAnimation();
            animOut.From = new Thickness(0);
            animOut.To = new Thickness(-this.RenderSize.Width, 0, this.RenderSize.Width, 0);
            Storyboard.SetTarget(animOut, currentControl);
            Storyboard.SetTargetProperty(animOut, new PropertyPath(UserControl.MarginProperty));

            ThicknessAnimation animIn = new ThicknessAnimation();
            animIn.From = new Thickness(this.RenderSize.Width, 0, -this.RenderSize.Width, 0);
            animIn.To = new Thickness(0);
            Storyboard.SetTarget(animIn, control);
            Storyboard.SetTargetProperty(animIn, new PropertyPath(UserControl.MarginProperty));

            DoubleAnimation backAnim = new DoubleAnimation();
            backAnim.From = 0.5;
            backAnim.To = 1.0;
            Storyboard.SetTarget(backAnim, control.backBtn);
            Storyboard.SetTargetProperty(backAnim, new PropertyPath(UserControl.OpacityProperty));

            Storyboard storyBoard = new Storyboard();
            storyBoard.Children.Add(animIn);
            storyBoard.Children.Add(animOut);
            storyBoard.Children.Add(backAnim);
            storyBoard.SpeedRatio *= 3.0;
            storyBoard.Completed += (sender, e) =>
            {
                currentControl.Visibility = Visibility.Hidden;
                currentControl.Margin = new Thickness(0);
                isAnimating = false;
            };
            this.mainGrid.Children.Add(control);
            controls.Add(control);
            control.backBtn.Visibility = Visibility.Visible;
            storyBoard.Begin();
            isAnimating = true;
        }
        public void popControl()
        {
            if (isAnimating)
                return;
            if (controls.Count == 1)
                return;
            dynamic control = controls[controls.Count - 2];
            control.Height = this.Height;
            control.Width = this.Width;

            dynamic currentControl = controls[controls.Count - 1];

            ThicknessAnimation animOut = new ThicknessAnimation();
            animOut.From = new Thickness(0);
            animOut.To = new Thickness(this.RenderSize.Width, 0, -this.RenderSize.Width, 0);
            Storyboard.SetTarget(animOut, currentControl);
            Storyboard.SetTargetProperty(animOut, new PropertyPath(UserControl.MarginProperty));

            ThicknessAnimation animIn = new ThicknessAnimation();
            animIn.From = new Thickness(-this.RenderSize.Width, 0, this.RenderSize.Width, 0);
            animIn.To = new Thickness(0);
            Storyboard.SetTarget(animIn, control);
            Storyboard.SetTargetProperty(animIn, new PropertyPath(UserControl.MarginProperty));

            Storyboard storyBoard = new Storyboard();
            storyBoard.Children.Add(animIn);
            storyBoard.Children.Add(animOut);
            storyBoard.SpeedRatio *= 3.0;
            control.Visibility = Visibility.Visible;
            storyBoard.Completed += (sender, e) =>
                {
                    this.mainGrid.Children.Remove(currentControl);
                    controls.Remove(currentControl);
                    isAnimating = false;
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                };
            storyBoard.Begin();
            isAnimating = true;
        }

        public void WillDisplay()
        {
            dynamic currentControl = controls[controls.Count - 1];
            currentControl.WillDisplay();
        }
    }
}
