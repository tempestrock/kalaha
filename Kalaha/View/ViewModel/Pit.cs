//
// Pit
//
// Represents the pits on the view layer, carrying the Kalaha.View.Model.Seeds and being
// carried by the Kalaha.View.Model.GameBoard.
//

using PST_Common;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Kalaha.View.Model
{
    public class Pit
    {
        // --- Enums ---

        /// <summary>
        /// There are three type of images for a pit: a normally displayed pit, a highlighted pit (when the mouse is over the pit),
        /// and a selected pit (when the user or the computer selected it).
        /// A Kalah can only be "normal".
        /// </summary>
        public enum ImageType { Normal = 0, Highlighted = 1, Selected = 2 };
        private const int _numImageTypes = 3;

        /// <summary>A "House" is the normal pit that a user can select, the Kalah is the main pit to finally collect all seeds.</summary>
        public enum PitType { House, Kalah };

        /// <summary>The position the pit is located</summary>
        public enum PitPosition { South, North };

        // --- Attributes of the class ---

        /// <summary>A counter for the creation of unique IDs (mainly for debugging reasons)</summary>
        static int _staticPitCounter = 0;

        /// <summary>The position of the number field on the z-axis</summary>
        const int _zIndexOfNumberField = 4;

        /// <summary>The canvas that all pits and seeds are painted on</summary>
        private Canvas _canvasToPaintOn;

        /// <summary>The PitType of this pit</summary>
        private PitType _pitType;

        /// <summary>The position of this pit</summary>
        private PitPosition _position;

        /// <summary>The list of seeds currently stored by the pit</summary>
        private List<Seed> _seeds;

        /// <summary>The position of the pit on the 1000-page</summary>
        private Point _topLeftCorner;

        /// <summary>The height of the image (based on the 1000-page)</summary>
        private int _height;

        /// <summary>The width of the image (based on the 1000-page)</summary>
        private int _width;

        // The thickness of the frame of the pit image in x direction (on the basis of the "1000-page"):
        private int _pitFrameThicknessX;

        // The thickness of the frame of the pit image in x direction (on the basis of the "1000-page"):
        private int _pitFrameThicknessY;

        /// <summary>The array of images of a normal, a highlighted, and a selected version</summary>
        private Image[] _image;

        /// <summary>The invisible touch field that a user can tap or click on to select the pit</summary>
        private Button _touchField;

        /// <summary>The textblock to display the number of seeds</summary>
        private TextBlock _numberField;

        /// <summary>A unique id for debugging purposes</summary>
        private int _pitId;


        // --- Methods of the class ---

        /// <summary>Constructor takes as parameters the x and y coordinate on the 1000-page</summary>
        public Pit(Canvas canvasToPaintOn,
                    PitType pitType,
                    int x, int y, int width, int height,
                    int pitFrameThicknessX, int pitFrameThicknessY,
                    PitPosition position)
        {
            _pitId = _staticPitCounter++;
            _canvasToPaintOn = canvasToPaintOn;
            _pitType = pitType;
            _seeds = new List<Seed>();
            _topLeftCorner = new Point(0, 0);
            _height = 0;
            _width = 0;
            _image = new Image[_numImageTypes];
            _touchField = null;
            _numberField = null;
            SetTopLeftCorner(x, y);
            _width = width;
            _height = height;
            _pitFrameThicknessX = pitFrameThicknessX;
            _pitFrameThicknessY = pitFrameThicknessY;
            _position = position;

//DEBUG     Logging.Inst.LogMessage("Created " + this.ToString() + " at " + _topLeftCorner + " with width " + _width + ", height " + _height +
//DEBUG                             ", xPitFrameThickness " + _xPitFrameThickness + ", and yPitFrameThickness " + _yPitFrameThickness + "\n");
        }

        /// <summary>
        /// Repaints the seeds of the pit.
        /// </summary>
        /// <param name="canvasToPaintOn"></param>
        public void RepaintSeeds(Canvas canvasToPaintOn)
        {
            _canvasToPaintOn = canvasToPaintOn;
            int numberOfSeeds = _seeds.Count;

            for (int seedIndex = 0; seedIndex < numberOfSeeds; ++seedIndex)
            {
                _canvasToPaintOn.Children.Add(_seeds[seedIndex].GetImage());
            }

            // PositionNumberFieldCorrectly();
        }

        /// <summary>Initializes the internal structure and sets the number of seeds in the pit.</summary>
        /// <param name="numSeeds">The total number of seeds</param>
        public void SetInitialNumberOfSeeds(int numSeeds)
        {
            _seeds = new List<Seed>();

            // Initialize the number field:
            _numberField.Text = "0";
            PositionNumberFieldCorrectly();

            for (int seedIndex = 0; seedIndex < numSeeds; ++seedIndex)
            {
                AddNewSeed();
            }
        }

        /// <returns>The top left corner of a possible place for a new seed. The used numbers all refer to the 1000-page.</returns>
        public Point FindPlaceForNewSeed(int seedWidth, int seedHeight)
        {
            // We try 100 times to find a place that is not already covered by another seed.
            // If that is not successful, we simply take any place.

            // Known bug: Due to the fact that this method is called asynchronously various times without actually already placing seeds into pits,
            // it may happen that a place for a seed B is told to be free where a different seed A tried this position before (and A was told already
            // that this place is free). Finally, when both seeds A and B are positioned, they overlap although other space would still be empty.
            // This happens only for many seeds in one move, ending in one pit, i.e. usually a Kalah.

            const int numTries = 100;
            bool foundAPlace = false;
            Point pointFound = new Point();

//DEBUG     Logging.Inst.LogMessage("Find a place for seed with width " + seedWidth + " and height " + seedHeight + ".\n");

            for (int tryIndex = 0; ((tryIndex < numTries) && (!foundAPlace)); ++tryIndex)
            {
                // Find a random position inside the pit:
                pointFound.X = Basics.GetRandomNumber(Convert.ToInt32(_topLeftCorner.X) + _pitFrameThicknessX + 1,
                                                      Convert.ToInt32(_topLeftCorner.X) + _width - seedWidth - _pitFrameThicknessX - 1);
                pointFound.Y = Basics.GetRandomNumber(Convert.ToInt32(_topLeftCorner.Y) + _pitFrameThicknessY + 1,
                                                      Convert.ToInt32(_topLeftCorner.Y) + _height - seedHeight - _pitFrameThicknessY - 1);

//DEBUG         Logging.Inst.LogMessage("Trying (" + pointFound + ") -> (" + (pointFound.X + seedWidth) + ";" +
//DEBUG                                 (pointFound.Y + seedHeight) + ")...\n");

                bool isCovered = false;
                // Check whether this place is already covered by another seed:
                for (int seedIndex = 0; ((seedIndex < _seeds.Count) && (!isCovered)); ++seedIndex)
                {
                    // Get coordinates and sizes of the current seed:
                    Point topLeftCornerOfCurSeed = _seeds[seedIndex].GetTopLeftCorner();
                    int widthOfCurSeed = _seeds[seedIndex].GetWidth();
                    int heightOfCurSeed = _seeds[seedIndex].GetHeight();

//DEBUG             Logging.Inst.LogMessage("   -> Comparing to " + _seeds[seedIndex] + "(" + seedIndex + "): (" + topLeftCornerOfCurSeed + ") -> (" +
//DEBUG                                     (topLeftCornerOfCurSeed.X + widthOfCurSeed) + ";" +
//DEBUG                                     (topLeftCornerOfCurSeed.Y + heightOfCurSeed) + ")...\n");

                    if ((((topLeftCornerOfCurSeed.X <= pointFound.X) && (topLeftCornerOfCurSeed.X + widthOfCurSeed >= pointFound.X)) ||
                         ((pointFound.X <= topLeftCornerOfCurSeed.X) && (pointFound.X + seedWidth >= topLeftCornerOfCurSeed.X))) &&
                        (((topLeftCornerOfCurSeed.Y <= pointFound.Y) && (topLeftCornerOfCurSeed.Y + heightOfCurSeed >= pointFound.Y)) ||
                         ((pointFound.Y <= topLeftCornerOfCurSeed.Y) && (pointFound.Y + seedHeight >= topLeftCornerOfCurSeed.Y))))
                    {
                        isCovered = true;
//DEBUG                 Logging.Inst.LogMessage("      -> Is covered.\n");
                    }
//DEBUG             else
//DEBUG             {
//DEBUG                 Logging.Inst.LogMessage("      -> Is not covered.\n");
//DEBUG             }
                }

                foundAPlace = (!isCovered);
            }

//DEBUG     Logging.Inst.LogMessage("Found a place: " + foundAPlace + ".\n");

            return pointFound;
        }

        /// <summary>Adds one seed to the pit.</summary>
        public void AddNewSeed()
        {
            // Create a new seed and add it to the list:
            Seed newSeed = new Seed();
            _seeds.Add(newSeed);
            _numberField.Text = _seeds.Count.ToString();
            PositionNumberFieldCorrectly();

            Point newTopLeftCorner = FindPlaceForNewSeed(newSeed.GetWidth(), newSeed.GetHeight());
            newSeed.SetTopLeftCorner(newTopLeftCorner);

//DEBUG     Logging.Inst.LogMessage(this.ToString() + ": Added " + newSeed + " at (" + newTopLeftCorner + "). Image.coords: (" +
//DEBUG                             Canvas.GetLeft(newSeed.GetImage()).ToString() + ";" + Canvas.GetTop(newSeed.GetImage()).ToString() + ")\n");
            _canvasToPaintOn.Children.Add(newSeed.GetImage());
        }

        /// <summary>Moves an existing seed into this pit.</summary>
        /// <param name="seed">The seed to be moved here.</param>
        public void MoveSeedHere(Seed seed, Point newTopLeftCorner)
        {
            Logging.I.LogMessage("Pit.MoveSeedHere (" + seed + ", (" + newTopLeftCorner + "))\n", Logging.LogLevel.DeepDebug);
            // Add the seed to the list of seeds in this pit:
            _seeds.Add(seed);

            // Set the coordinates of the seed to the new position:
            seed.SetTopLeftCorner(newTopLeftCorner);

            // Modify the number that is shown for this pit:
            _numberField.Text = _seeds.Count.ToString();
            PositionNumberFieldCorrectly();
        }

        /// <returns>The seed in this pit that was stored here lately</returns>
        public Seed GetSomeSeedAndRemoveIt()
        {
            if (_seeds.Count == 0)
            {
                throw new PSTException("VMPit.GetSomeSeedAndRemoveIt: No seed left in list.");
            }

            // Simply take the last seed that we added:
            Seed seedTaken = _seeds[_seeds.Count - 1];

            // Remove the given seed from the list of seeds:
            _seeds.Remove(seedTaken);

            // Modify the number that is shown for this pit:
            _numberField.Text = _seeds.Count.ToString();
            PositionNumberFieldCorrectly();

            return seedTaken;
        }

        /// <summary>Removes one seed from the pit.</summary>
        public void RemoveSeed(Seed seedToRemove)
        {
            if (_seeds.Count == 0)
            {
                throw new PSTException("VMPit.RemoveSeed: No more seed available for removal.");
            }
        }


        /// <summary>Centers the number field according to what has been defined in the resource file.</summary>
        private void PositionNumberFieldCorrectly()
        {
            // Re-calculate the current number field's size.
            _numberField.Measure(new Size(500, 500));          // some large numbers for the available size
            _numberField.Arrange(new Rect(0, 0, 500, 500));

            int DeviationOfNumberFieldX = 0;
            int DeviationOfNumberFieldY = 0;

            if (_pitType == PitType.House)
            {
                if (_position == PitPosition.South)
                {
                    DeviationOfNumberFieldX = KalahaResources.I.GetLayoutValue("DeviationOfNumberFieldHouseSouthX");
                    DeviationOfNumberFieldY = KalahaResources.I.GetLayoutValue("DeviationOfNumberFieldHouseSouthY");
                }
                else
                {
                    DeviationOfNumberFieldX = KalahaResources.I.GetLayoutValue("DeviationOfNumberFieldHouseNorthX");
                    DeviationOfNumberFieldY = KalahaResources.I.GetLayoutValue("DeviationOfNumberFieldHouseNorthY");
                }
            }
            else
            {
                if (_position == PitPosition.South)
                {
                    DeviationOfNumberFieldX = KalahaResources.I.GetLayoutValue("DeviationOfNumberFieldKalahSouthX");
                    DeviationOfNumberFieldY = KalahaResources.I.GetLayoutValue("DeviationOfNumberFieldKalahSouthY");
                }
                else
                {
                    DeviationOfNumberFieldX = KalahaResources.I.GetLayoutValue("DeviationOfNumberFieldKalahNorthX");
                    DeviationOfNumberFieldY = KalahaResources.I.GetLayoutValue("DeviationOfNumberFieldKalahNorthY");
                }
            }

            // Define the x and y coordinates of the top left corner of the number field, taking into account what has been defined as deviation:
            double xCoord = PSTScreen.I.ToScreenX(_topLeftCorner.X + _width / 2 + DeviationOfNumberFieldX) - _numberField.ActualWidth / 2;
            double yCoord = PSTScreen.I.ToScreenY(_topLeftCorner.Y + _height / 2 + DeviationOfNumberFieldY) - _numberField.ActualHeight / 2;

            // Set the coordinates in all three dimensions:
            Canvas.SetLeft(_numberField, xCoord);
            Canvas.SetTop(_numberField, yCoord);
            Canvas.SetZIndex(_numberField, _zIndexOfNumberField);
        }

        /// <returns>The PitType of the pit</returns>
        public PitType GetPitType()
        {
            return _pitType;
        }

        /// <returns>The number of seeds in the pit</returns>
        public int GetNumberOfSeeds()
        {
            return _seeds.Count;
        }

        /// <summary>Sets the top left corner of the pit on the 1000-page.<summary>
        /// <param name="x">The coordinate on the x axis</param>
        /// <param name="y">The coordinate on the y axis</param>
        public void SetTopLeftCorner(int x, int y)
        {
            _topLeftCorner.X = x;
            _topLeftCorner.Y = y;
        }

        /// <returns>The top left corner of the pit on the 1000-page.</returns>
        public Point GetTopLeftCorner()
        {
            return _topLeftCorner;
        }

        public void SetWidth(int width)
        {
            _width = width;
        }

        public int GetWidth()
        {
            return _width;
        }

        public void SetHeight(int height)
        {
            _height = height;
        }

        public int GetHeight()
        {
            return _height;
        }

        public void SetImage(Image image, ImageType imageType)
        {
            _image[(int)imageType] = image;
        }

        public Image GetImage(ImageType imageType)
        {
            return _image[(int)imageType];
        }

        /// <summary>Highlights the pit by setting the "highlight" image to visible.</summary>
        public void Highlight()
        {
            _image[(int)ImageType.Highlighted].Visibility = Visibility.Visible;
        }

        /// <summary>"Unhighlights" the pit by setting the "highlight" image to invisible.</summary>
        public void UnHighlight()
        {
            _image[(int)ImageType.Highlighted].Visibility = Visibility.Collapsed;
        }

        /// <summary>Set the pit to selected by setting the "selected" image to visible.</summary>
        public void SetSelected()
        {
            _image[(int)ImageType.Selected].Visibility = Visibility.Visible;
            UnHighlight();
        }

        /// <summary>Set the pit to unselected by setting the "selected" image to invisible.</summary>
        public void SetUnSelected()
        {
            _image[(int)ImageType.Selected].Visibility = Visibility.Collapsed;
        }

        public void SetTouchField(Button touchField)
        {
            _touchField = touchField;
        }

        /// <summary>Enables the touch field to be pressed.</summary>
        public void EnableTouchField()
        {
            _touchField.Visibility = Visibility.Visible;
        }

        /// <summary>Disables the touch field to be pressed.</summary>
        public void DisableTouchField()
        {
            _touchField.Visibility = Visibility.Collapsed;
        }

        public void SetNumberField(TextBlock numberField)
        {
            _numberField = numberField;
        }

        public override string ToString()
        {
            return "Pit #" + _pitId;
        }

        public override int GetHashCode()
        {
            return _pitId;
        }
    }
}
