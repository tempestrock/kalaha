//
// VisualIndicator
//
// A rather simple class that handles the visual indicator, a theme-dependent sign that shows whose turn it is.
//

using PST_Common;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


namespace Kalaha.View.Model
{

    public class VisualIndicator
    {
        // --- Attributes of the class ---

        const int _zIndexOfVisualIndicator = 1;


        /// <summary>The position of the visual indicator on the 1000-page</summary>
        private Point _topLeftCorner;

        /// <summary>The height of the image (based on the 1000-page)</summary>
        private int _height;

        /// <summary>The width of the image (based on the 1000-page)</summary>
        private int _width;

        /// <summary>The images for "on" and "off"</summary>
        private Image _visualIndicatorOn;
        private Image _visualIndicatorOff;


        // --- Methods of the class ---

        public VisualIndicator(Canvas canvasToPaintOn, Point topLeftCorner, int width, int height, bool rotateImage)
        {
            _topLeftCorner = topLeftCorner;
            _width = width;
            _height = height;

            _visualIndicatorOn = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:/" + ThemeHandler.I.GetCurrentThemesDir() + "/" +
                                                 KalahaResources.I.GetIndicatorOnFileName())),
                Width = PSTScreen.I.ToScreenX(_width),
                Height = PSTScreen.I.ToScreenY(_height),
            };

            // Define the angle of the visual indicator:
            _visualIndicatorOn.RenderTransformOrigin = new Point(0.5, 0.5);         // Rotate around the center
            RotateTransform myRotateTransformOn = new RotateTransform();
            // Put the name upside down if we are told to do so:
            myRotateTransformOn.Angle = (rotateImage ? 180 : 0);
            _visualIndicatorOn.RenderTransform = myRotateTransformOn;            
            
            Canvas.SetLeft(_visualIndicatorOn, PSTScreen.I.ToScreenX(_topLeftCorner.X));
            Canvas.SetTop(_visualIndicatorOn, PSTScreen.I.ToScreenY(_topLeftCorner.Y));
            Canvas.SetZIndex(_visualIndicatorOn, _zIndexOfVisualIndicator);

            // Add the image to the canvas:
            canvasToPaintOn.Children.Add(_visualIndicatorOn);

            _visualIndicatorOff = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:/" + ThemeHandler.I.GetCurrentThemesDir() + "/" +
                                                 KalahaResources.I.GetIndicatorOffFileName())),
                Width = PSTScreen.I.ToScreenX(_width),
                Height = PSTScreen.I.ToScreenY(_height),
            };

            // Define the angle of the visual indicator:
            _visualIndicatorOff.RenderTransformOrigin = new Point(0.5, 0.5);         // Rotate around the center
            RotateTransform myRotateTransformOff = new RotateTransform();
            // Put the name upside down if we are told to do so:
            myRotateTransformOff.Angle = (rotateImage ? 180 : 0);
            _visualIndicatorOff.RenderTransform = myRotateTransformOff;            
            
            Canvas.SetLeft(_visualIndicatorOff, PSTScreen.I.ToScreenX(_topLeftCorner.X));
            Canvas.SetTop(_visualIndicatorOff, PSTScreen.I.ToScreenY(_topLeftCorner.Y));
            Canvas.SetZIndex(_visualIndicatorOff, _zIndexOfVisualIndicator);

            // Add the image to the canvas:
            canvasToPaintOn.Children.Add(_visualIndicatorOff);

            // Show the indicator to be switched off initially:
            SwitchOff();
        }

        /// <summary>
        /// Switches the indicator to "on".
        /// </summary>
        public void SwitchOn()
        {
            _visualIndicatorOn.Visibility = Visibility.Visible;
            _visualIndicatorOff.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Switches the indicator to "off".
        /// </summary>
        public void SwitchOff()
        {
            _visualIndicatorOn.Visibility = Visibility.Collapsed;
            _visualIndicatorOff.Visibility = Visibility.Visible;
        }
    }
}
