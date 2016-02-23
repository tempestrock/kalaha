//
// GamePage
// 
// This class represents the page that is called and displayed whenever a game is started.
// The class has the actual "View.Model.GameBoard" as an attribute which stores all the information about where to put
// what on the page.
//

#region Using

using Kalaha.Model;
using Kalaha.View.Model;
using PST_Common;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

#endregion Using


namespace Kalaha.View
{
    public delegate void NextMethodToCall();

    public sealed partial class GamePage : Kalaha.Common.LayoutAwarePage
    {
        // --- Attributes of the class ---

        #region Attributes of the Class

        /// <summary>Visual indicators that show whose turn it is</summary>
        private VisualIndicator _visualIndicatorSouth;
        private VisualIndicator _visualIndicatorNorth;

        #endregion Attributes of the Class


        // --- Methods of the class ---

        #region Constructor

        /// <summary>The constructor</summary>
        public GamePage()
            : base()
        {
//DEBUG     Logging.I.LogMessage("Calling constructor of GamePage (" + Presenter.I.GetGameStatus().ToString() + ").\n");

            // Just for the case that someone changed the screen resolution, we reinit the screen sizes here:
            //DEBUG     Logging.I.LogMessage("Screen size before re-init: " + PSTScreen.I.ToString() + ".\n");
            PSTScreen.I.ReInit();
            //DEBUG     Logging.I.LogMessage("Screen size after re-init: " + PSTScreen.I.ToString() + ".\n");

            // Initialize the Windows stuff:
            this.InitializeComponent();

            // Do not cache this page:
            NavigationCacheMode = NavigationCacheMode.Disabled;

            // Set the background image:
            SetBackgroundImage();

            // Set the image of the "Player Position" button:
            SetImagesOfPlayerPositionButton();

            // Set the image of the "Number Field On/Off" button:
            SetImagesOfNumberFieldOnOffButton();

            // Tell the presenter who we are:
            Presenter.I.SetView(this);

            // Initialize the presenter as we are now about to start a new game:
            Presenter.I.InitializeAtGameStart(GameBoardCanvas,
                                              this.PointerEnteredHouse,
                                              this.PointerExitedHouse,
                                              this.TouchFieldSelected);

            FixedMessageFieldCenter.Text = "";
            SetPlayerNames();
            InitializeVisualIndicatorsAndProgressRings();
            InitializeTooltips();

            // Disable buttons that are not allowed to be pressed, yet:
            DisablePlayAgainButton();
            DisableUndoButton();

            Presenter.I.StartOrContinueGame();
        }

        #endregion Constructor


        #region Progress Ring

        /// <summary>
        /// Shows that some progress is being computed for the player with the given player number.
        /// </summary>
        /// <param name="playerNumber">0 for south, 1 for north</param>
        public void ShowProgress(int playerNumber)
        {
            switch (playerNumber)
            {
                case 0:
                    progressRingSouth.IsActive = true;
                    break;
                case 1:
                    progressRingNorth.IsActive = true;
                    break;
            }
        }

        /// <summary>
        /// Shows that some computation ended for the player with the given player number.
        /// </summary>
        /// <param name="playerNumber">0 for south, 1 for north</param>
        public void ProgressEnded(int playerNumber)
        {
            switch (playerNumber)
            {
                case 0:
                    progressRingSouth.IsActive = false;
                    break;
                case 1:
                    progressRingNorth.IsActive = false;
                    break;
            }
        }

        #endregion Progress Ring


        #region Event Handling of Houses

        /// <summary>This method is called when the user clicks or taps on a pit (which is actually an invisible button).</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TouchFieldSelected(object sender, RoutedEventArgs e)
        {
            Button buttonClicked = (Button)sender;
            int pitSelected = Convert.ToInt32(buttonClicked.Content);

            // Tell the presenter to move the selected pit:
            Presenter.I.MoveSeedsOfSelectedHouse(pitSelected);
        }

        /// <summary>This method is called when the mouse pointer (or a tapping finger) enters a button's area.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerEnteredHouse(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Button buttonEntered = (Button)sender;

            Presenter.I.HighlightHouse(Convert.ToInt32(buttonEntered.Content));
        }

        /// <summary>This method is called when the mouse pointer (or a tapping finger) exits a button's area.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerExitedHouse(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Button buttonExited = (Button)sender;
//DEBUG     TextBlock_TappedInfo.Text = "Pointer exited pit #" + buttonExited.Content + ".";

            Presenter.I.UnHighlightHouse(Convert.ToInt32(buttonExited.Content));
        }

        #endregion Event Handling of Houses


        #region Buttons and Their Tooltips on the Page

        /// <summary>
        /// Shows the relevant buttons on the GamePage.
        /// </summary>
        public void EnableButtonsOnPage()
        {
            backButton.Visibility = Visibility.Visible;

            if (Presenter.I.UndoMoveIsLeft())
            {
                undoButton.Visibility = Visibility.Visible;
            }
            SetImagesOfNumberFieldOnOffButton();
            SetImagesOfPlayerPositionButton();
        }

        /// <summary>
        /// Hides all buttons on the GamePage.
        /// The only exception is to show the "back" button if both players are computers.
        /// </summary>
        public void DisableButtonsOnPage()
        {
            if (!Presenter.I.NoPlayerIsHuman())
            {
                // At least one player is human.
                backButton.Visibility = Visibility.Collapsed;
            }

            undoButton.Visibility = Visibility.Collapsed;
            numberFieldOnButton.Visibility = Visibility.Collapsed;
            numberFieldOffButton.Visibility = Visibility.Collapsed;
            playerPositionSameSideButton.Visibility = Visibility.Collapsed;
            playerPositionOppositeSidesButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// This method is called when the back button is tapped or clicked.
        /// <summary>
        private void BackButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            // Tell the Presenter to end the game correctly:
            Presenter.I.EndTheGame();

            // Navigate to the HubPage:
            var frame = Window.Current.Content as Frame;
            frame.Navigate(typeof(HubPage));
        }

        /// <summary>
        /// Makes the undo button visible.
        /// </summary>
        public void EnableUndoButton()
        {
            undoButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Makes the undo button invisible.
        /// </summary>
        public void DisableUndoButton()
        {
            undoButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// This method is called when the undo button is tapped or clicked.
        /// </summary>
        private void UndoButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            Presenter.I.UndoLastMove();
        }

        /// <summary>
        /// This method is called when the "toggle number fields on/off" button is tapped or clicked.
        /// </summary>
        private void NumberFieldOnOffButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            // Toggle the flag for the number field button:
            Settings.I.ToggleNumberFieldsOnOff();

            // Show the respective button:
            SetImagesOfNumberFieldOnOffButton();

            // Tell the presenter that we want to continue the same game after reloading the page:
            Presenter.I.SetGameStatus(Presenter.GameStatus.Continued);

            // Re-draw the whole page:
            PSTScreen.I.RedrawPage();
        }

        /// <summary>
        /// Sets the image of the number field on/off button according to the current settings.
        /// </summary>
        private void SetImagesOfNumberFieldOnOffButton()
        {
            // Set the images of the buttons according to the settings:
            switch (Settings.I.NumberFieldsShallBeShown())
            {
                case true:
                    numberFieldOnButton.Visibility = Visibility.Visible;
                    numberFieldOffButton.Visibility = Visibility.Collapsed;
                    break;

                case false:
                    numberFieldOnButton.Visibility = Visibility.Collapsed;
                    numberFieldOffButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// This method is called when the "toggle player position" button is tapped or clicked.
        /// </summary>
        private void PlayerPositionButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            // Toggle the game device:
            Settings.I.ToggleGameDevice();

            // Show the respective button:
            SetImagesOfPlayerPositionButton();

            // Tell the presenter that we want to continue the same game after reloading the page:
            Presenter.I.SetGameStatus(Presenter.GameStatus.Continued);

            // Re-draw the whole page:
            PSTScreen.I.RedrawPage();
        }

        /// <summary>
        /// Sets the image of the player position button according to the currently used device.
        /// </summary>
        private void SetImagesOfPlayerPositionButton()
        {
            if (Presenter.I.BothPlayersAreHuman())
            {
                // Both players are human -> Show one of the toggle buttons:
                switch (Settings.I.GetGameDevice())
                {
                    case Settings.GameDevice.PC:
                        // For the PC, both players sit on the same side:
                        playerPositionSameSideButton.Visibility = Visibility.Visible;
                        playerPositionOppositeSidesButton.Visibility = Visibility.Collapsed;
                        break;

                    case Settings.GameDevice.Tablet:
                        // For the Table, the players sit on opposite sides:
                        playerPositionSameSideButton.Visibility = Visibility.Collapsed;
                        playerPositionOppositeSidesButton.Visibility = Visibility.Visible;
                        break;
                }
            }
            else
            {
                // If not both players are human, do not show this functionality at all:
                playerPositionSameSideButton.Visibility = Visibility.Collapsed;
                playerPositionOppositeSidesButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Makes the "play again" button visible.
        /// </summary>
        public void EnablePlayAgainButton()
        {
            playAgainButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Makes the "play again" button invisible.
        /// </summary>
        public void DisablePlayAgainButton()
        {
            playAgainButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// This method is called when the "play again" button is tapped or clicked.
        /// </summary>
        private void PlayAgainButtonClicked(object sender, RoutedEventArgs e)
        {
            // Tell the presenter that a new game is about to come:
            Presenter.I.SetGameStatus(Presenter.GameStatus.StartedNew);

            // Let the presenter toggle the player who is first:
            Presenter.I.TogglePlayerWhoIsFirst();

            // Reload the page by loading the empty page and jumping back to this page:
            PSTScreen.I.RedrawPage();
        }

        /// <summary>
        /// Initializes all tooltips for this page.
        /// </summary>
        private void InitializeTooltips()
        {
            const int tooltipFontsize = 20;

            ToolTip toolTip_BackButton = new ToolTip();
            toolTip_BackButton.FontSize = tooltipFontsize;
            toolTip_BackButton.Content = KalahaResources.I.GetRes("Tooltip_GamePage_BackButton");
            ToolTipService.SetToolTip(backButton, toolTip_BackButton);

            ToolTip toolTip_UndoButton = new ToolTip();
            toolTip_UndoButton.FontSize = tooltipFontsize;
            toolTip_UndoButton.Content = KalahaResources.I.GetRes("Tooltip_GamePage_UndoButton");
            ToolTipService.SetToolTip(undoButton, toolTip_UndoButton);

            ToolTip toolTip_PlayAgainButton = new ToolTip();
            toolTip_PlayAgainButton.FontSize = tooltipFontsize;
            toolTip_PlayAgainButton.Content = KalahaResources.I.GetRes("Tooltip_GamePage_PlayAgainButton");
            ToolTipService.SetToolTip(playAgainButton, toolTip_PlayAgainButton);

            ToolTip toolTip_NumberFieldOnButton = new ToolTip();
            toolTip_NumberFieldOnButton.FontSize = tooltipFontsize;
            toolTip_NumberFieldOnButton.Content = KalahaResources.I.GetRes("Tooltip_GamePage_NumberFieldOnButton");
            ToolTipService.SetToolTip(numberFieldOnButton, toolTip_NumberFieldOnButton);

            ToolTip toolTip_NumberFieldOffButton = new ToolTip();
            toolTip_NumberFieldOffButton.FontSize = tooltipFontsize;
            toolTip_NumberFieldOffButton.Content = KalahaResources.I.GetRes("Tooltip_GamePage_NumberFieldOffButton");
            ToolTipService.SetToolTip(numberFieldOffButton, toolTip_NumberFieldOffButton);

            ToolTip toolTip_PlayerPositionSameSideButton = new ToolTip();
            toolTip_PlayerPositionSameSideButton.FontSize = tooltipFontsize;
            toolTip_PlayerPositionSameSideButton.Content = KalahaResources.I.GetRes("Tooltip_GamePage_PlayerPositionSameSideButton");
            ToolTipService.SetToolTip(playerPositionSameSideButton, toolTip_PlayerPositionSameSideButton);

            ToolTip toolTip_PlayerPositionOppositeSidesButton = new ToolTip();
            toolTip_PlayerPositionOppositeSidesButton.FontSize = tooltipFontsize;
            toolTip_PlayerPositionOppositeSidesButton.Content = KalahaResources.I.GetRes("Tooltip_GamePage_PlayerPositionOppositeSidesButton");
            ToolTipService.SetToolTip(playerPositionOppositeSidesButton, toolTip_PlayerPositionOppositeSidesButton);
        }

        #endregion Buttons and Their Tooltips on the Page


        #region Background Image

        /// <summary>Sets the background image dependent on the current theme.</summary>
        private void SetBackgroundImage()
        {
            xaml_BackgroundImage.ImageSource = new BitmapImage(new Uri("ms-appx:/" + ThemeHandler.I.GetCurrentThemesDir() +
                                                                       "/" + KalahaResources.I.GetGamePageBackgroundFileName()));
        }

        #endregion Background Image


        #region Printing Messages

        /// <summary>
        /// Prints a message to the screen at the center position, using a globalized string taken from the corresponding resource file.
        /// This message automatically vanishes after a few seconds.
        /// </summary>
        /// <param name="messageKey">The resource key to be used</param>
        /// <param name="relevantPlayer">The player whose turn it is currently</param>
        /// <param name="args">0 or more parameters for the resource key</param>
        public async void PrintFadingMsg(String messageKey, Player relevantPlayer, params object[] args)
        {
            string message = KalahaResources.I.GetMsg(messageKey, args);
            MessageFieldCenter.Text = message;

            // Define the angle of the visual indicator:
            MessageFieldCenter.RenderTransformOrigin = new Point(0.5, 0.5);         // Rotate around the center
            RotateTransform myRotateTransform = new RotateTransform();

            if ((relevantPlayer.GetPosition() == Player.Position.North) && Presenter.I.TabletModeIsActive())
            {
                // Put the name upside down if we are told to do so:
                myRotateTransform.Angle = 180;
            }
            else
            {
                // Leave the name in the "usual" angle:
                myRotateTransform.Angle = 0;
            }

            MessageFieldCenter.RenderTransform = myRotateTransform;

            // Print the actual message to the screen by fading in:
            await MessageFieldCenter.FadeIn(TimeSpan.FromMilliseconds(1500));   // The parameter defines the time that the message is visible

            // ... and out:
            await MessageFieldCenter.FadeOut(TimeSpan.FromMilliseconds(200));
        }

        /// <summary>
        /// Prints a message to the screen at the center position, using a globalized string taken from the corresponding resource file.
        /// This message does not vanish.
        /// </summary>
        /// <param name="messageKey">The resource key to be used</param>
        /// <param name="relevantPlayer">The player whose turn it is currently</param>
        /// <param name="args">0 or more parameters for the resource key</param>
        public void PrintFixedMsg(String messageKey, Player relevantPlayer, params object[] args)
        {
            string message = KalahaResources.I.GetMsg(messageKey, args);
            FixedMessageFieldCenter.Text = message;

            if ((relevantPlayer.GetPosition() == Player.Position.North) && Presenter.I.TabletModeIsActive())
            {
                // Put the words upside down in order for the current player to be easily readable:

                // Define the angle of the visual indicator:
                FixedMessageFieldCenter.RenderTransformOrigin = new Point(0.5, 0.5);         // Rotate around the center
                RotateTransform myRotateTransform = new RotateTransform();
                // Put the name upside down if we are told to do so:
                myRotateTransform.Angle = 180;
                FixedMessageFieldCenter.RenderTransform = myRotateTransform;
            }
        }

        /// <summary>
        /// Empties the fixed message area on the game page.
        /// </summary>
        public void ClearFixedMsg()
        {
            FixedMessageFieldCenter.Text = "";
        }

        #endregion Printing Messages


        #region Player positions and visual indicators
        /// <summary>Displays the player's name on the page.</summary>
        public void SetPlayerNames()
        {
            var bounds = Window.Current.Bounds;

            PlayerNameFieldSouth.Text = Presenter.I.GetSouthernPlayer().GetName();
            Canvas.SetLeft(PlayerNameFieldSouth, PSTScreen.I.ToScreenX(KalahaResources.I.GetLayoutValue("PlayerNamePosSouthX")));
            Canvas.SetTop(PlayerNameFieldSouth, PSTScreen.I.ToScreenY(KalahaResources.I.GetLayoutValue("PlayerNamePosSouthY")));
            
            PlayerNameFieldNorth.Text = Presenter.I.GetNorthernPlayer().GetName();
            Canvas.SetLeft(PlayerNameFieldNorth, PSTScreen.I.ToScreenX(KalahaResources.I.GetLayoutValue("PlayerNamePosNorthX")));
            Canvas.SetTop(PlayerNameFieldNorth, PSTScreen.I.ToScreenY(KalahaResources.I.GetLayoutValue("PlayerNamePosNorthY")));

            // Define the angle of the northern player name:
            PlayerNameFieldNorth.RenderTransformOrigin = new Point(1, 0.5);         // Rotate around the x-axis
            RotateTransform myRotateTransform = new RotateTransform();
            // Put the name upside down if we are in Tablet mode:
            myRotateTransform.Angle = (Presenter.I.TabletModeIsActive() ? 180 : 0);
            PlayerNameFieldNorth.RenderTransform = myRotateTransform;
        }

        /// <summary>Inizializes the indicators that show whose turn it is.</summary>
        void InitializeVisualIndicatorsAndProgressRings()
        {
            // Get sizes from the theme-specific resource file:
            int visualIndicatorWidth = KalahaResources.I.GetLayoutValue("VisualIndicatorWidth");
            int visualIndicatorHeight = KalahaResources.I.GetLayoutValue("VisualIndicatorHeight");

            // The position is taken from the resources:
            Point southPosition = new Point();
            southPosition.X = KalahaResources.I.GetLayoutValue("PlayerNamePosSouthX") - visualIndicatorWidth - 25;
            southPosition.Y = KalahaResources.I.GetLayoutValue("VisualIndicatorPosSouthY");

            Point northPosition = new Point();
            if (Presenter.I.TabletModeIsActive())
            {
                northPosition.X = 1000 - KalahaResources.I.GetLayoutValue("PlayerNamePosNorthX") + 25;
            }
            else
            {
                northPosition.X = KalahaResources.I.GetLayoutValue("PlayerNamePosNorthX") - visualIndicatorWidth - 25;
            }
            northPosition.Y = KalahaResources.I.GetLayoutValue("VisualIndicatorPosNorthY");

            // Create the actual objects:
            _visualIndicatorSouth = new VisualIndicator(GameBoardCanvas, southPosition, visualIndicatorWidth, visualIndicatorHeight, false);
            _visualIndicatorNorth = new VisualIndicator(GameBoardCanvas, northPosition, visualIndicatorWidth, visualIndicatorHeight, Presenter.I.TabletModeIsActive());

            // Start with the player whose turn it is:
            SwitchVisualIndicators(Presenter.I.GetCurrentPlayer());

            // Position the progress rings for north and south:
            Canvas.SetLeft(progressRingSouth, PSTScreen.I.ToScreenX(southPosition.X) - visualIndicatorWidth - 30);
            Canvas.SetTop(progressRingSouth, PSTScreen.I.ToScreenY(southPosition.Y));
            Canvas.SetLeft(progressRingNorth, PSTScreen.I.ToScreenX(northPosition.X) - visualIndicatorWidth - 30);
            Canvas.SetTop(progressRingNorth, PSTScreen.I.ToScreenY(northPosition.Y));
        }

        /// <summary>
        /// Switches the indicator to the player whose turn it is.
        /// </summary>
        /// <param name="currentPlayer">The player whose turn it is</param>
        public void SwitchVisualIndicators(Player currentPlayer)
        {
            if (currentPlayer.GetPosition() == Player.Position.South)
            {
                _visualIndicatorSouth.SwitchOn();
                _visualIndicatorNorth.SwitchOff();
                PlayerNameFieldSouth.Foreground = Application.Current.Resources["GamePageTextOn"] as SolidColorBrush;
                PlayerNameFieldNorth.Foreground = Application.Current.Resources["GamePageTextOff"] as SolidColorBrush;
            }
            else
            {
                _visualIndicatorSouth.SwitchOff();
                _visualIndicatorNorth.SwitchOn();
                PlayerNameFieldSouth.Foreground = Application.Current.Resources["GamePageTextOff"] as SolidColorBrush;
                PlayerNameFieldNorth.Foreground = Application.Current.Resources["GamePageTextOn"] as SolidColorBrush;
            }
        }

        /// <summary>
        /// Switches all visual indicators and the player names to "off".
        /// </summary>
        public void SwitchOffAllVisualIndicators()
        {
            _visualIndicatorSouth.SwitchOff();
            _visualIndicatorNorth.SwitchOff();
            PlayerNameFieldSouth.Foreground = Application.Current.Resources["GamePageTextOff"] as SolidColorBrush;
            PlayerNameFieldNorth.Foreground = Application.Current.Resources["GamePageTextOff"] as SolidColorBrush;
        }

        #endregion Player positions and visual indicators


        #region Handling of window sizes

        /// <summary>
        /// Handles the event that the window size is changed (e.g. to "snapped" or "filled").
        /// </summary>
        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs eventArgs)
        {
            //TODO: Somehow unify with the same function of the HubPage class.
            // Obtain view state by explicitly querying for it
            ApplicationViewState myViewState = ApplicationView.Value;
//DEBUG     Logging.I.LogMessage("GamePage: Window size changed to " + myViewState.ToString() + ".\n");

            Presenter.I.SetGameStatus(Presenter.GameStatus.Continued);

            switch (myViewState)
            {
                case Windows.UI.ViewManagement.ApplicationViewState.Filled:
                case Windows.UI.ViewManagement.ApplicationViewState.FullScreenLandscape:

                    // The "large" view has been enabled.

                    landScapeView.Visibility = Visibility.Visible;
                    snappedView.Visibility = Visibility.Collapsed;
                    // Re-init window sizes:
//DEBUG             Logging.I.LogMessage("Screen size before re-init: " + PSTScreen.I.ToString() + ".\n");
                    PSTScreen.I.ReInit();
//DEBUG             Logging.I.LogMessage("Screen size after re-init: " + PSTScreen.I.ToString() + ".\n");

                    // We need to refresh the page at hand, so we "virtually" switch to an empty page and back from it again:
                    PSTScreen.I.RedrawPage();
                    break;

                case Windows.UI.ViewManagement.ApplicationViewState.Snapped:

                    // The "snapped" view has been enabled.

                    landScapeView.Visibility = Visibility.Collapsed;
                    snappedView.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Handles the event of the button in the snapped view clicked in order to grow the window to filled or full screen size again.
        /// </summary>
        private void ButtonBackFromSnappedViewClicked(object sender, RoutedEventArgs e)
        {
            bool unsnapWorked = ApplicationView.TryUnsnap();
        }

        #endregion Handling of window sizes


        #region Loading and Saving State

        /// <summary>
        /// Füllt die Seite mit Inhalt auf, der bei der Navigation übergeben wird. Gespeicherte Zustände werden ebenfalls
        /// bereitgestellt, wenn eine Seite aus einer vorherigen Sitzung neu erstellt wird.
        /// </summary>
        /// <param name="navigationParameter">Der Parameterwert, der an
        /// <see cref="Frame.Navigate(Type, Object)"/> übergeben wurde, als diese Seite ursprünglich angefordert wurde.
        /// </param>
        /// <param name="pageState">Ein Wörterbuch des Zustands, der von dieser Seite während einer früheren Sitzung
        /// beibehalten wurde. Beim ersten Aufrufen einer Seite ist dieser Wert NULL.</param>
        protected override void LoadState(object navigationParameter, Dictionary<String, Object> pageState)
        {
//DEBUG     Logging.I.LogMessage("Calling GamePage.LoadState().\n");

            // Register for the window resize event:
            Window.Current.SizeChanged += WindowSizeChanged;
        }


        /// <summary>
        /// Behält den dieser Seite zugeordneten Zustand bei, wenn die Anwendung angehalten oder
        /// die Seite im Navigationscache verworfen wird. Die Werte müssen den Serialisierungsanforderungen
        /// von <see cref="SuspensionManager.SessionState"/> entsprechen.
        /// </summary>
        /// <param name="pageState">Ein leeres Wörterbuch, das mit dem serialisierbaren Zustand aufgefüllt wird.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
//DEBUG     Logging.I.LogMessage("Calling GamePage.SaveState().\n");

            // Remove this handler from the "size changed" event list because it will be added anyway when loading the page again:
            Window.Current.SizeChanged -= WindowSizeChanged;
        }

        #endregion Loading and Saving State


        #region Old Stuff
        /*
        /// <summary>
        /// Shows the current screen size.
        /// </summary>
        private void ShowScreenSize()
        {
            double dpi = DisplayProperties.LogicalDpi;
            var bounds = Window.Current.Bounds;
            double h;
            switch (ApplicationView.Value)
            {
                case ApplicationViewState.Filled:
                    h = bounds.Height;
                    break;

                case ApplicationViewState.FullScreenLandscape:
                    h = bounds.Height;
                    break;

                case ApplicationViewState.Snapped:
                    h = bounds.Height;
                    break;

                case ApplicationViewState.FullScreenPortrait:
                    h = bounds.Width;
                    break;

                default:
                    return;
            }
            double inches = h / dpi;
            string screenType = "";
            if (inches < 10)
            {
                screenType = "Slate";
            }
            else if (inches < 14)
            {
                screenType = "WorkHorsePC";
            }
            else
            {
                screenType = "FamilyHub";
            }

            MessageFieldCenter.Text = screenType + " (" + bounds.Width + "x" + bounds.Height + " at " + dpi + " dpi)";
        }
*/


        /// <summary>Handles areas where a user may tap that are not covered by other screen elements.</summary>
        /// <param name="eventArgs"></param>      
        /*
        protected override void OnTapped(Windows.UI.Xaml.Input.TappedRoutedEventArgs eventArgs)
        {
            base.OnTapped(eventArgs);

            Point pointTapped = eventArgs.GetPosition(null);
            TextBlock_TappedInfo.Text = "You tapped position [" + pointTapped.X + ", " + pointTapped.Y + "].";
        }
        */
        #endregion Old Stuff

    }
}
