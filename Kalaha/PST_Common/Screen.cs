//
// Screen
//
// Some simple screen size handlings, implemented as a singleton.
//

using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace PST_Common
{
    public sealed class PSTScreen
    {
        // --- Attributes of the class ---

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile PSTScreen _inst;
        private static object _syncRoot = new Object();

        /// <summary>The window size as rectangle</summary>
        private Rect _bounds;

        /// <summary>The factor with which an x coordinate that is based on a 1000-page must be multiplied in order to get the "real" screen x coordinate</summary>
        private double _xFactor;

        /// <summary>The factor with which a y coordinate that is based on a 1000-page must be multiplied in order to get the "real" screen y coordinate</summary>
        private double _yFactor;


        // --- Methods of the class Logging ---

        /// <summary>The constructor of the class. This is declared private because only
        /// the Instance property method may call it.</summary>
        private PSTScreen()
        {
            ReInit();
        }

        /// <summary>Re-initializes the screen size.</summary>
        public void ReInit()
        {
            _bounds = Window.Current.Bounds;
            _xFactor = _bounds.Width / 1000.0;
            _yFactor = _bounds.Height / 1000.0;
        }

        /// <summary>
        /// Converts a given x coordinate on the 1000-page to a "real" x coordinate on the screen, taking into account the current screen size.
        /// </summary>
        /// <param name="xOn1000Page"></param>
        /// <returns>The "real" x value on the screen</returns>
        public double ToScreenX(double xOn1000Page)
        {
            return xOn1000Page * _xFactor;
        }

        /// <summary>
        /// Converts a given y coordinate on the 1000-page to a "real" y coordinate on the screen, taking into account the current screen size.
        /// </summary>
        /// <param name="yOn1000Page"></param>
        /// <returns>The "real" y value on the screen</returns>
        public double ToScreenY(double yOn1000Page)
        {
            return yOn1000Page * _yFactor;
        }

        /// <summary>
        /// Converts a given x coordinate on the "real" screen to a coordinate on the 1000-page, taking into account the current screen size.
        /// </summary>
        /// <param name="xOnScreen"></param>
        /// <returns>The x value on the 1000-page</returns>
        public double To1000PageX(double xOnScreen)
        {
            return xOnScreen / _xFactor;
        }

        public override string ToString()
        {
            return (_bounds.Width.ToString() + "x" + _bounds.Height.ToString());
        }

        /// <summary>
        /// Converts a given y coordinate on the "real" screen to a coordinate on the 1000-page, taking into account the current screen size.
        /// </summary>
        /// <param name="yOnScreen"></param>
        /// <returns>The y value on the 1000-page</returns>
        public double To1000PageY(double yOnScreen)
        {
            return yOnScreen / _yFactor;
        }

        /// <summary>
        /// Redraws the page by actually reloading it completely.
        /// </summary>
        public void RedrawPage()
        {
//DEBUG     Logging.I.LogMessage("Calling PST_Common.RedrawPage(): " + Window.Current.ToString() + ".\n");

            // Reload the page by loading the empty page and jumping back to this page:
            var frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(Kalaha.View.emptyPage));
            frame.GoBack();
        }


        /// <summary>
        /// If the single instance has not yet been created, yet, creates the instance.
        /// This approach ensures that only one instance is created and only when the instance is needed.
        /// This approach uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        /// This double-check locking approach solves the thread concurrency problems while avoiding an exclusive
        /// lock in every call to the Instance property method. It also allows you to delay instantiation
        /// until the object is first accessed.
        /// </summary>
        /// <returns>The single instance of the class</returns>
        public static PSTScreen I
        {
            get
            {
                if (_inst == null)
                {
                    lock (_syncRoot)
                    {
                        if (_inst == null)
                            _inst = new PSTScreen();
                    }
                }

                return _inst;
            }
        }
    }
}
