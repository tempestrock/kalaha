//
// Gameboard
//
// As opposed to the Kalaha.Model.Gameboard, this Kalaha.View.Model.GameBoard holds all information about the
// displayed game board.
// This class is implemented in the design pattern of a decorator to the class Kalaha.Model.GameBoard. It enhances
// the Kalaha.Model.GameBoard by adding the capabilities to display the GameBoard and to handle something like
// coordinates and screen sizes.
//

using Kalaha.Model;
using PST_Common;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;

namespace Kalaha.View.Model
{

    public class GameBoard
    {
        // --- Enums --------
        private enum KalahPosition { Middle, Outside };
        
        // --- Attributes of the class ---

         /// <summary>The zIndexes define the visibility layers for the different images of pits.</summary>
        const int _zIndexOfHouse = 1;
        const int _zIndexOfKalah = 1;
        int _zIndexOfTouchFields = Basics.Infinity();        // The touch fields always have to stay on top

        /// <summary>The GameBoard object that is encapsulated by this decorator class</summary>
        private Kalaha.Model.GameBoard _gameBoard;

        /// <summary>The view representation of the pits</summary>
        private Pit[] _pit;

        /// <summary>The canvas that all pits and seeds are painted on</summary>
        private Canvas _canvasToPaintOn;

        // --- Methods of the class ---

        /// <summary>The constructor</summary>
        /// <param name="gameBoard">The GameBoard to consider as the basis</param>
        public GameBoard(Kalaha.Model.GameBoard gameBoard,
                            Canvas gameBoardCanvas,
                            PointerEventHandler PointerEnteredHouse,
                            PointerEventHandler PointerExitedHouse,
                            RoutedEventHandler TouchFieldSelected)
        {
            _gameBoard = gameBoard;
            _canvasToPaintOn = gameBoardCanvas;

            KalahPosition kalahPosition;
            kalahPosition = GetKalahPosition();
            CreatePitsAndSetCoordinatesAndSizes(kalahPosition);
            CreateImagesAndTouchFieldsAndNumFieldsOfHouses(PointerEnteredHouse, PointerExitedHouse, TouchFieldSelected);
            CreateImagesAndNumFieldsOfKalahs();
        }

        /// <summary>
        /// Returns whether the Kalah images are to be positioned in the middle of the picture or at the outer edges.
        /// </summary>
        private KalahPosition GetKalahPosition()
        {
            const string KalahPositionMiddle = "middle";
            const string KalahPositionOutside = "outside";

            // The definition whether the Kalah image is to be positioned in the middle ("middle") of the picture or at the outer edges ("outside"):
            string kalahPositionStr = KalahaResources.I.GetLayoutRes("KalahPosition");
            if (kalahPositionStr == KalahPositionMiddle)
            {
                return KalahPosition.Middle;
            }
            else
            {
                if (kalahPositionStr == KalahPositionOutside)
                {
                    return KalahPosition.Outside;
                }
                else
                {
                    throw new PSTException("GameBoard.GetKalahPosition: Unknown kalahPosition \"" + kalahPositionStr + "\". May be \"" +
                                            KalahPositionMiddle + "\" or \"" + KalahPositionOutside + "\".");
                }
            }
        }

        /// <summary>Calculates the center of each pit.</summary>
        private void CreatePitsAndSetCoordinatesAndSizes(KalahPosition kalahPosition)
        {
            // Take over the number of houses in order to not call the method each time:
            int numHouses = _gameBoard.GetNumOfHousesPerPlayer();

            // --- Get some resource values first. ---
            
            // The distance between two pits on the x-axis (on the basis of the "1000-page"):
            int distanceBetweenPits = ((numHouses < Settings.I.GetMaxNumberOfHouses()) ? KalahaResources.I.GetLayoutValue("DistBetweenPits") : 2);

            // The house image's height (on the basis of the "1000-page"):
            int houseImageHeight = KalahaResources.I.GetLayoutValue("HouseImageHeight");

            // The house image's width (on the basis of the "1000-page"):
            int houseImageWidth = KalahaResources.I.GetLayoutValue("HouseImageWidth");

            // The thickness of the frame of the house image in x direction (on the basis of the "1000-page"):
            int HouseFrameThicknessX = KalahaResources.I.GetLayoutValue("HouseFrameThicknessX");

            // The thickness of the frame of the house image in x direction (on the basis of the "1000-page"):
            int HouseFrameThicknessY = KalahaResources.I.GetLayoutValue("HouseFrameThicknessY");

            // The Kalah image's height (on the basis of the "1000-page"):
            int kalahImageHeight = KalahaResources.I.GetLayoutValue("KalahImageHeight");

            // The Kalaha image's width (on the basis of the "1000-page"):
            int kalahaImageWidth = KalahaResources.I.GetLayoutValue("KalahImageWidth");

            // The y-coordinate of the centers of the southern houses:
            int CenterCoordOfSouthernHousesY = KalahaResources.I.GetLayoutValue("CoordinateOfSouthernHousesY");

            // The y-coordinate of the centers of the northern houses:
            int CenterCoordOfNorthernHousesY = KalahaResources.I.GetLayoutValue("CoordinateOfNorthernHousesY");

            // Create the arrays for the images and buttons:
            _pit = new Pit[2 * (numHouses + 1)];

            // Calculate the first x-coordinate:
            int widthOfAllHouses = ((numHouses * houseImageWidth) + ((numHouses-1) * distanceBetweenPits));
            int firstXCoord = ((1000 - widthOfAllHouses) / 2);

            // Calculate the Kalah's vertical center:
            int yCoordOfKalahs = ((CenterCoordOfSouthernHousesY + CenterCoordOfNorthernHousesY) / 2) - (kalahImageHeight / 2);

            // Counter for the following loop:
            int currentXCoord = firstXCoord;

            // Assign the coordinates for the southern houses:
            for (int index = 0; index < numHouses; ++index)
            {
                _pit[index] = new Pit(_canvasToPaintOn, Pit.PitType.House, currentXCoord, CenterCoordOfSouthernHousesY - (houseImageHeight / 2),
                                            houseImageWidth, houseImageHeight, HouseFrameThicknessX, HouseFrameThicknessY, Pit.PitPosition.South);
 
                // Increase the counter by the width of a house and the distance between the pits:
                currentXCoord += (houseImageWidth + distanceBetweenPits);
            }

            // Set the coordinates of the southern/right Kalah:
            if (kalahPosition == KalahPosition.Outside)
            {
                // Position the Kalah at the right hand side of the game board:
                _pit[numHouses] = new Pit(_canvasToPaintOn, Pit.PitType.Kalah, currentXCoord,
                                            yCoordOfKalahs, kalahaImageWidth, kalahImageHeight, HouseFrameThicknessX, HouseFrameThicknessY, Pit.PitPosition.South);
            }
            else
            {
                if (kalahPosition == KalahPosition.Middle)
                {
                    // Position the Kalah in the middle of the game board between the houses:
                    _pit[numHouses] = new Pit(_canvasToPaintOn, Pit.PitType.Kalah, currentXCoord - kalahaImageWidth + (houseImageWidth * 3/4),
                                                yCoordOfKalahs, kalahaImageWidth, kalahImageHeight, HouseFrameThicknessX, HouseFrameThicknessY, Pit.PitPosition.South);
                }
            }

            // (Re-)set currentXCoord to the rightmost house:
            currentXCoord -= (houseImageWidth + distanceBetweenPits);

            // Assign the coordinates for the northern houses:
            for (int index = numHouses+1; index < ((numHouses * 2) + 1); ++index)
            {
                _pit[index] = new Pit(_canvasToPaintOn, Pit.PitType.House, currentXCoord, CenterCoordOfNorthernHousesY - (houseImageHeight / 2),
                                            houseImageWidth, houseImageHeight, HouseFrameThicknessX, HouseFrameThicknessY, Pit.PitPosition.North);

                // Increase the counter by the width of a house and the distance between the pits:
                currentXCoord -= (houseImageWidth + distanceBetweenPits);
            }

            // Set the coordinates of the northern/left Kalah:
            if (kalahPosition == KalahPosition.Outside)
            {
                // Position the Kalah at the left hand side of the game board:
                _pit[numHouses * 2 + 1] = new Pit(_canvasToPaintOn, Pit.PitType.Kalah, firstXCoord - distanceBetweenPits - kalahaImageWidth,
                                                    yCoordOfKalahs, kalahaImageWidth, kalahImageHeight, HouseFrameThicknessX, HouseFrameThicknessY,
                                                    Pit.PitPosition.North);
            }
            else
            {
                if (kalahPosition == KalahPosition.Middle)
                {
                    // Position the Kalah in the middle of the game board between the houses:
                    _pit[numHouses * 2 + 1] = new Pit(_canvasToPaintOn, Pit.PitType.Kalah, firstXCoord - (houseImageWidth * 3/4),
                                                        yCoordOfKalahs, kalahaImageWidth, kalahImageHeight, HouseFrameThicknessX, HouseFrameThicknessY,
                                                        Pit.PitPosition.North);
                }
            }
        }

        /// <summary>
        /// Creates the houses (various versions), (invisible) buttons ("touch fields"), and number fields of the houses
        /// and paints them onto the canvas.
        /// </summary>
        /// <param>Callback methods that are to be called when an according event occurs</param>
        private void CreateImagesAndTouchFieldsAndNumFieldsOfHouses(PointerEventHandler PointerEnteredHouse,
                                                                    PointerEventHandler PointerExitedHouse,
                                                                    RoutedEventHandler TouchFieldSelected)
        {
            int numHouses = GetNumOfHousesPerPlayer();

            // Paint the pits of both players.
            // Loop over the number of pits per player:
            for (int houseIndex = 0; houseIndex < numHouses; ++houseIndex)
            {
                // Loop over south and north
                for (int positionIndex = 0; positionIndex < 2; ++positionIndex)
                {
                    // Select the index to consider for the rest of the loop (either southern or northern hemisphere):
                    int curHouseIndex = houseIndex + (positionIndex * (numHouses + 1));
                    Player curPlayer = (positionIndex == 0 ? Presenter.I.GetSouthernPlayer() : Presenter.I.GetNorthernPlayer());


                    // -------- Images -------- 

                    // Loop over the three image types we have for houses:
                    for (int imageIndex = 0; imageIndex < 3; ++imageIndex)
                    {
                        string houseFileName = "";
                        Pit.ImageType imageType = Pit.ImageType.Normal;
                        Visibility visibility = Visibility.Collapsed;

                        switch (imageIndex)
                        {
                            case (int)Pit.ImageType.Normal: houseFileName = KalahaResources.I.GetNormalHouseFileName();
                                imageType = Pit.ImageType.Normal;
                                visibility = Visibility.Visible;
                                break;
                            case (int)Pit.ImageType.Highlighted: houseFileName = KalahaResources.I.GetHighlightedHouseFileName();
                                imageType = Pit.ImageType.Highlighted;
                                break;
                            case (int)Pit.ImageType.Selected: houseFileName = KalahaResources.I.GetSelectedHouseFileName();
                                imageType = Pit.ImageType.Selected;
                                break;
                        }

                        // Create a new southern normal pit for the given pitIndex:
                        Image houseImage = new Image()
                        {
                            Source = new BitmapImage(new Uri("ms-appx:/" + ThemeHandler.I.GetCurrentThemesDir() + "/" + houseFileName)),
                            HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                            VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch,
                            Width = PSTScreen.I.ToScreenX(_pit[curHouseIndex].GetWidth()),
                            Height = PSTScreen.I.ToScreenY(_pit[curHouseIndex].GetHeight()),
                            Visibility = visibility
                        };

                        Canvas.SetLeft(houseImage, PSTScreen.I.ToScreenX(GetTopLeftCornerOfPit(curPlayer, houseIndex).X));
                        Canvas.SetTop(houseImage, PSTScreen.I.ToScreenY(GetTopLeftCornerOfPit(curPlayer, houseIndex).Y));
                        Canvas.SetZIndex(houseImage, _zIndexOfHouse);

                        // Having created the image, inform the current pit about it:
                        _pit[curHouseIndex].SetImage(houseImage, imageType);

                        // Add the pit image to the canvas:
                        _canvasToPaintOn.Children.Add(houseImage);

                    } // for imageIndex


                    // -------- Touch Fields -------- 

                    // Create an invisible button ("touch field") for the currently considered southern house:
                    Button houseTouchField = new Button()
                    {
                        HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                        VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch,
                        Width = PSTScreen.I.ToScreenX(_pit[curHouseIndex].GetWidth()),
                        Height = PSTScreen.I.ToScreenY(_pit[curHouseIndex].GetHeight()),
                        // Equivalent to setting the Style "{StaticResource FieldButtonStyle}" in XAML:
                        Style = Application.Current.Resources["FieldButtonStyle"] as Style,
                        Content = curHouseIndex.ToString()
                    };
                    // Set event handlers:
                    houseTouchField.Click += TouchFieldSelected;
                    houseTouchField.PointerEntered += PointerEnteredHouse;
                    houseTouchField.PointerExited += PointerExitedHouse;

                    Canvas.SetLeft(houseTouchField, PSTScreen.I.ToScreenX(GetTopLeftCornerOfPit(curPlayer, houseIndex).X));
                    Canvas.SetTop(houseTouchField, PSTScreen.I.ToScreenY(GetTopLeftCornerOfPit(curPlayer, houseIndex).Y));
                    Canvas.SetZIndex(houseTouchField, _zIndexOfTouchFields);

                    // Having created the touch field, inform the current pit about it:
                    _pit[curHouseIndex].SetTouchField(houseTouchField);

                    // Add the touch field to the canvas:
                    _canvasToPaintOn.Children.Add(houseTouchField);


                    // -------- Number Fields -------- 

                    // Create a new number field for the given houseIndex:
                    TextBlock numberField = new TextBlock()
                    {
                        // Equivalent to setting the Style "{StaticResource SeedNumberStyle}" in XAML:
                        Style = Application.Current.Resources["SeedNumberStyle"] as Style,
                        Visibility = (Settings.I.NumberFieldsShallBeShown() ? Visibility.Visible : Visibility.Collapsed)
                    };

                    // Define the angle of the displayed number:
                    numberField.RenderTransformOrigin = new Point(0.5,0.5);         // Rotate around the center
                    RotateTransform myRotateTransform = new RotateTransform();                   
                    // Put the number upside down if we are in Tablet mode and the player is North:
                    myRotateTransform.Angle = ((Presenter.I.TabletModeIsActive() && (curPlayer.GetPosition() == Player.Position.North)) ? 180 : 0);
                    numberField.RenderTransform = myRotateTransform;

                    // Having created the number field, inform the current pit about it:
                    _pit[curHouseIndex].SetNumberField(numberField);

                    // Add the touch field to the canvas:
                    _canvasToPaintOn.Children.Add(numberField);

                } // for positionIndex

            } // for pitIndex
        }

        /// <summary>
        /// Creates the pits (various versions) and number fields of the Kalahs and paints them onto the canvas.
        /// </summary>
        /// <param>Other params describe the callback methods that are to be called when an according event occurs.</param>
        private void CreateImagesAndNumFieldsOfKalahs()
        {
            int numPits = GetNumOfHousesPerPlayer();

            // -------- Images for Kalahs -------- 

            // Paint the southern/right Kalah:
            int kalahIndex = numPits;
            Image kalahImage = new Image()
            {
                Source = new BitmapImage(new Uri("ms-appx:/" + ThemeHandler.I.GetCurrentThemesDir() + "/" +
                                                 KalahaResources.I.GetNormalKalahFileName())),
//                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
  //              VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch,
                Width = PSTScreen.I.ToScreenX(_pit[kalahIndex].GetWidth()),
                Height = PSTScreen.I.ToScreenY(_pit[kalahIndex].GetHeight()),
            };
            Canvas.SetLeft(kalahImage, PSTScreen.I.ToScreenX(GetTopLeftCornerOfKalah(Presenter.I.GetSouthernPlayer()).X));
            Canvas.SetTop(kalahImage, PSTScreen.I.ToScreenY(GetTopLeftCornerOfKalah(Presenter.I.GetSouthernPlayer()).Y));
            Canvas.SetZIndex(kalahImage, _zIndexOfKalah);

            // Having created the Kalah image, inform the current pit about it:
            _pit[kalahIndex].SetImage(kalahImage, Pit.ImageType.Normal);

            // Add the touch field to the canvas:
            _canvasToPaintOn.Children.Add(kalahImage);


            // Paint the northern/left Kalah:
            kalahIndex = 2 * numPits + 1;
            kalahImage = new Image()
            {
                Source = new BitmapImage(new Uri("ms-appx:/" + ThemeHandler.I.GetCurrentThemesDir() + "/" +
                                                 KalahaResources.I.GetNormalKalahFileName())),
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch,
                Width = PSTScreen.I.ToScreenX(_pit[kalahIndex].GetWidth()),
                Height = PSTScreen.I.ToScreenY(_pit[kalahIndex].GetHeight()),
            };
            Canvas.SetLeft(kalahImage, PSTScreen.I.ToScreenX(GetTopLeftCornerOfKalah(Presenter.I.GetNorthernPlayer()).X));
            Canvas.SetTop(kalahImage, PSTScreen.I.ToScreenY(GetTopLeftCornerOfKalah(Presenter.I.GetNorthernPlayer()).Y));
            Canvas.SetZIndex(kalahImage, _zIndexOfKalah);

            // Having created the Kalah image, inform the current pit about it:
            _pit[kalahIndex].SetImage(kalahImage, Pit.ImageType.Normal);

            // Add the touch field to the canvas:
            _canvasToPaintOn.Children.Add(kalahImage);


            // -------- Number Fields for Kalahs -------- 

            // Create a new number field for the right/southern Kalah:
            kalahIndex = numPits;
            TextBlock numberField = new TextBlock()
            {
                // Equivalent to setting the Style "{StaticResource SeedNumberStyle}" in XAML:
                Style = Application.Current.Resources["SeedNumberStyle"] as Style,
                Visibility = (Settings.I.NumberFieldsShallBeShown() ? Visibility.Visible : Visibility.Collapsed)
            };

            // Having created the number field, inform the current pit about it:
            _pit[kalahIndex].SetNumberField(numberField);

            // Add the new number fields to the canvas:
            _canvasToPaintOn.Children.Add(numberField);


            // Create a new number field for the left/northern Kalah:
            kalahIndex = 2 * numPits + 1;
            numberField = new TextBlock()
            {
                // Equivalent to setting the Style "{StaticResource SeedNumberStyle}" in XAML:
                Style = Application.Current.Resources["SeedNumberStyle"] as Style,
                Visibility = (Settings.I.NumberFieldsShallBeShown() ? Visibility.Visible : Visibility.Collapsed)
            };

            // Define the angle of the displayed number:
            numberField.RenderTransformOrigin = new Point(0.5, 0.5);         // Rotate around the center
            RotateTransform myRotateTransform = new RotateTransform();
            // Put the number upside down if we are in Tablet mode and the player is North:
            myRotateTransform.Angle = (Presenter.I.TabletModeIsActive() ? 180 : 0);
            numberField.RenderTransform = myRotateTransform;

            // Having created the number field, inform the current pit about it:
            _pit[kalahIndex].SetNumberField(numberField);

            // Add the new number fields to the canvas:
            _canvasToPaintOn.Children.Add(numberField);
        }

        /// <summary>Initializes the number of seeds in all pits</summary>
        public void InitializeNumbersOfSeeds()
        {
            int numPits = GetNumOfHousesPerPlayer();

            // Loop over the complete number of pits:
            for (int pitIndex = 0; pitIndex < 2 * (numPits + 1); ++pitIndex)
            {
                if (_pit[pitIndex].GetPitType() == Pit.PitType.Kalah)
                {
                    // The pit of the current index is a Kalah -> Number of seeds is 0:
                    _pit[pitIndex].SetInitialNumberOfSeeds(0);
                }
                else
                {
                    // This is a "standard" pit -> Initialize with inital number of seeds:
                    _pit[pitIndex].SetInitialNumberOfSeeds(_gameBoard.GetInitialNumOfSeeds());
                }
            }
        }

        /// <summary>
        /// Copies the seed numbers for each pit from the "Model" GameBoard.
        /// </summary>
        public void CopySeedNumbersOfGameBoard()
        {
            int numPits = GetNumOfHousesPerPlayer();

            // Loop over the complete number of pits:
            for (int pitIndex = 0; pitIndex < 2 * (numPits + 1); ++pitIndex)
            {
                _pit[pitIndex].SetInitialNumberOfSeeds(_gameBoard.GetPit(pitIndex).GetNumberofSeeds());
            }

        }

        /// <summary>Shows the given move in detail.</summary>
        /// <param name="move">The move to be shown</param>
        /// <param name="nextMethodToCall">The method that shall be called after the visualization has finished</param>
        /// <param name="moveFast">Flag whether or not to move the seeds very fast</param>
        public void VisualizeMove(Move move, NextMethodToCall nextMethodToCall, bool moveFast = false)
        {
            MoveExecutor.ExecuteMove(move, _pit, nextMethodToCall, moveFast);
        }

        /// <summary>Hightlights the pit with the given index by making visible the according image</summary>
        /// <param name="pitIndex"></param>
        public void Highlight(int pitIndex)
        {
            _pit[pitIndex].Highlight();
        }

        /// <summary>Hightlights the pit with the given index by making visible the according image</summary>
        /// <param name="pitIndex"></param>
        public void UnHighlight(int pitIndex)
        {
            _pit[pitIndex].UnHighlight();
        }

        /// <summary>
        /// Sets the pit with the given index to state "selected" by making visible the according image.
        /// Sets all other pits unselected. Sets all pits unhighlighted.
        /// </summary>
        /// <param name="pitIndexSelected"></param>
        public void SetSelected(int pitIndexSelected, Player currentPlayer)
        {
            int numHouses = GetNumOfHousesPerPlayer();

            if (currentPlayer.GetPosition() == Player.Position.North)
            {
                pitIndexSelected += numHouses + 1;
            }

            for (int pitIndex = 0; pitIndex < numHouses*2+1; ++pitIndex)
            {
                if (pitIndex == numHouses)
                {
                    // Skip the Kalah:
                    continue;
                }

                // Unhighlight the pits in any case:
                UnHighlight(pitIndex);

                if (pitIndex == pitIndexSelected)
                {
                    _pit[pitIndex].SetSelected();
                }
                else
                {
                    _pit[pitIndex].SetUnSelected();
                }
            }
        }

        /// <summary>
        /// Sets all houses on the game board to "unselected".
        /// </summary>
        public void UnselectAllHouses()
        {
            int numHouses = GetNumOfHousesPerPlayer();

            for (int pitIndex = 0; pitIndex < numHouses*2+1; ++pitIndex)
            {
                if (pitIndex == numHouses)
                {
                    // Skip the Kalah:
                    continue;
                }

                // Unselect the house:
                _pit[pitIndex].SetUnSelected();
            }
        }

        /// <summary>Enables the non-empty houses of the given player and disables all of his opponent.</summary>
        /// <param name="player">The player whose turn it is</param>
        public void EnableHousesOfPlayer(Player player)
        {
            int numPits = GetNumOfHousesPerPlayer();
            int startIndexOfPlayer = ((player.GetPosition() == Player.Position.South) ? 0 : numPits + 1);
            int startIndexOfOpponent = ((player.GetPosition() == Player.Position.South) ? numPits + 1 : 0);

            for (int pitIndex = 0; pitIndex < numPits; ++pitIndex)
            {
                if (_pit[pitIndex + startIndexOfPlayer].GetNumberOfSeeds() > 0)
                {
                    _pit[pitIndex + startIndexOfPlayer].EnableTouchField();
                }
                else
                {
                    _pit[pitIndex + startIndexOfPlayer].DisableTouchField();
                }
                _pit[pitIndex + startIndexOfOpponent].DisableTouchField();
            }
        }

        /// <summary>
        /// Disables the touch fields of all houses.
        /// </summary>
        public void DisableAllHouses()
        {
            int numPits = GetNumOfHousesPerPlayer();

            // Loop over the complete number of pits:
            for (int pitIndex = 0; pitIndex < 2 * (numPits + 1); ++pitIndex)
            {
                if (_pit[pitIndex].GetPitType() == Pit.PitType.House)
                {
                    // Disable the touch field of this house:
                    _pit[pitIndex].DisableTouchField();
                }
            }
        }

        /// <param name="player">The player whose pit coordinates are requested</param>
        /// <param name="pitIndex">The index of the pit. May be between 0 and the number of pits-1.</param>
        /// <returns>The center coordinate on the "1000-page" of the given pit</returns>
        public Point GetTopLeftCornerOfPit(Player player, int pitIndex)
        {
            // Take over the numer of pits in order not to call the method each time:
            int numPits = GetNumOfHousesPerPlayer();

            if (pitIndex >= numPits)
            {
                throw new PSTException("GetCenterOfPit: Unknown pitIndex: " + pitIndex);
            }

            // Set the internal index of the first pit in the row:
            int internalStartIndex = ((player.GetPosition() == Player.Position.South) ? 0 : (numPits + 1));

            return (_pit[internalStartIndex + pitIndex].GetTopLeftCorner());
        }

        /// <param name="player">The player whose Kalah coordinates are requested</param>
        /// <returns>The center coordinate on the "1000-page" of the given player</returns>
        public Point GetTopLeftCornerOfKalah(Player player)
        {
            // Take over the numer of pits in order not to call the method each time:
            int numPits = _gameBoard.GetNumOfHousesPerPlayer();

            // Set the internal index of the first pit in the row:
            int internalStartIndex = ((player.GetPosition() == Player.Position.South) ? 0 : (numPits + 1));

            return (_pit[internalStartIndex + numPits].GetTopLeftCorner());
        }

        /// <returns>The number of pits per player</returns>
        public int GetNumOfHousesPerPlayer()
        {
            return _gameBoard.GetNumOfHousesPerPlayer();
        }
    }
}
