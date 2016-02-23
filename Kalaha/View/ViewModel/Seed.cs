//
// Seed
//
// A seed on the game board, carried by one of the pits (including the Kalahs) and knowing about its
// (x,y) position (top left corner), height, and width on the 1000-page and about its image to display.
//

using PST_Common;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;


namespace Kalaha.View.Model
{

    public class Seed
    {
        // --- Attributes of the class ---
        const int _zIndexOfSeed = 3;           // This is just the initial zIndex of a seed. Later it is increased during each move.

        /// <summary>A counter for the creation of unique IDs (mainly for debugging reasons)</summary>
        static int _staticSeedCounter = 0;

        /// <summaryThe position of the seed on the 1000-page</summary>
        private Point _topLeftCorner;

        /// <summary>The height of the image (based on the 1000-page)</summary>
        private int _height;

        /// <summary>The width of the image (based on the 1000-page)</summary>
        private int _width;

        /// <summary>
        /// The angle in which the image of the seed is rotated. May be 0 for "no rotation".
        /// </summary>
        private int _rotationAngle;

        /// <summary>The image to be displayed when showing the seed</summary>
        private Image _image;

        /// <summary>
        /// The id (between 1 and the number of different seed images) that refers to the seed image of this seed
        /// </summary>
        private int _seedImageId;

        /// <summary>A unique id for debugging purposes</summary>
        private int _seedId;

        /// <summary>
        /// The sound that is made when the seed arrives in a pit. This is a static sound that is loaded only once and used by all instances.
        /// </summary>
        static MediaElement _moveSound = null;

        // --- Methods of the class ---

        /// <summary>The constructor</summary>
        public Seed()
        {
            _seedId = _staticSeedCounter++;

            // Default value for the initial position:
            _topLeftCorner = new Point(0, 0);

            // Get the number of different seed images:
            int numImages = KalahaResources.I.GetLayoutValue("SeedImageAmount");

            // Select seed image id:
            _seedImageId = Basics.GetRandomNumber(1, numImages);

            // The seed image's width (on the basis of the "1000-page"):
            _width = KalahaResources.I.GetLayoutValue("SeedImageWidth_" + _seedImageId);

            // The seed image's height (on the basis of the "1000-page"):
            _height = KalahaResources.I.GetLayoutValue("SeedImageHeight_" + _seedImageId);

            // Get the information whether to rotate the image arbitrarly:
            bool rotateImagesAtStart = (KalahaResources.I.GetLayoutRes("RotateSeedImagesAtStart") == "true");

            // Get the image from the according image file:
            _image = new Image()
            {
                Source = new BitmapImage(new Uri("ms-appx:/" + ThemeHandler.I.GetCurrentThemesDir() + "/" +
                                                 KalahaResources.I.GetSeedFileName(_seedImageId))),
                Width = PSTScreen.I.ToScreenX(_width),
                Height = PSTScreen.I.ToScreenY(_height),
                Visibility = Visibility.Visible
            };
            Canvas.SetZIndex(_image, _zIndexOfSeed);

            // Rotate the seed image if this is requested:
            _rotationAngle = (rotateImagesAtStart ? Basics.GetRandomNumber(1, 360) : 0);
            _image.RenderTransformOrigin = new Point(0.5, 0.5);         // Rotate around the center
            RotateTransform myRotateTransform = new RotateTransform();
            myRotateTransform.Angle = _rotationAngle;
            _image.RenderTransform = myRotateTransform;

            LoadSounds();
        }

        /// <summary>Sets the position of the top left corner of the seed and positions the image of the seed accordingly.</summary>
        /// <param name="topLeftCorner">The coordinate on the 1000-page</param>
        public void SetTopLeftCorner(Point topLeftCorner)
        {
            // Assign values to attributes:
            _topLeftCorner = topLeftCorner;

//DEBUG     Logging.Inst.LogMessage(this.ToString() + " Canvas.Left: " + Canvas.GetLeft(_image).ToString() + ", Canvas.Top: " + Canvas.GetTop(_image) + "\n");

            Canvas.SetLeft(_image, PSTScreen.I.ToScreenX(_topLeftCorner.X));
            Canvas.SetTop(_image, PSTScreen.I.ToScreenY(_topLeftCorner.Y));
//DEBUG     Logging.Inst.LogMessage(this.ToString() + " Canvas.Left: " + Canvas.GetLeft(_image).ToString() + ", Canvas.Top: " + Canvas.GetTop(_image) + "\n");
        }

        /// <summary>Moves an already existing seed and does not position the image of the seed again.</summary>
        /// <param name="topLeftCorner">The coordinate on the 1000-page</param>
        public void MoveTopLeftCorner(Point topLeftCorner)
        {
            // Assign values to attributes:
            _topLeftCorner = topLeftCorner;
        }

        /// <summary>
        /// Loads the sounds that are associated to this seed.
        /// </summary>
        private async void LoadSounds()
        {
            if (_moveSound == null)
            {
                // No sound has been loaded before.
                try
                {
                    _moveSound = new MediaElement();
                    StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync(ThemeHandler.I.GetCurrentThemesDirBackslash());
                    StorageFile soundFile = await folder.GetFileAsync(KalahaResources.I.GetSeedMoveSoundFileName());
                    var stream = await soundFile.OpenAsync(FileAccessMode.Read);
                    _moveSound.SetSource(stream, soundFile.ContentType);
                }
                catch (Exception ex)
                {
                    // Some error occurred.

                    Logging.I.LogMessage("Seed.LoadSounds: Exception when trying to open sound file: " + ex.ToString(), Logging.LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// Initializes the overall Seed class.
        /// </summary>
        public static void Initialize()
        {
            // Reset the seed movement sound in order to reload it at the next constructor call:
            _moveSound = null;
        }

        /// <summary>
        /// Plays the sound when moving the seed.
        /// </summary>
        public void PlayMovingSound()
        {
            if (_moveSound != null)
            {
                _moveSound.Play();
            }
        }


        /// <summary>
        /// Rotates the image of the seed to the angle that was defined when the seed was created.
        /// </summary>
        public void RotateImage()
        {
            _image.RenderTransformOrigin = new Point(0.5, 0.5);         // Rotate around the center
            RotateTransform myRotateTransform = new RotateTransform();
            myRotateTransform.Angle = _rotationAngle;
            _image.RenderTransform = myRotateTransform;
        }

        /// <summary>
        /// Sets the zIndex of the seed's image, thereby defining its visibility on the stack of other seeds in a pit.
        /// </summary>
        /// <param name="zIndex"></param>
        public void SetZIndex(int zIndex)
        {
            Canvas.SetZIndex(_image, zIndex);
        }

        /// <returns>The top left corner of the seed on the base of the 1000-page</returns>
        public Point GetTopLeftCorner()
        {
            return _topLeftCorner;
        }

        public int GetWidth()
        {
            return _width;
        }

        public int GetHeight()
        {
            return _height;
        }

        public Image GetImage()
        {
            return _image;
        }

        public override string ToString()
        {
            return "Seed #" + _seedId;
        }
    }
}
