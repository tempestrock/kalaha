//
// HubPage
// 
// This class represents the main page that is first seen when starting the app.
// All main settings are displayed here.
//

#region Using

using Kalaha.Model;
using PST_Common;
using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

#endregion Using


namespace Kalaha.View
{
    /// <summary>
    /// Eine Standardseite mit Eigenschaften, die die meisten Anwendungen aufweisen.
    /// </summary>
    public sealed partial class HubPage : Kalaha.Common.LayoutAwarePage
    {
        // --- Attributes of the class ---

        #region Attributes of the class

        /// <summary>The popup window that appears when a name may be typed in:</summary>
        Popup _popup_NameInput;

        #endregion Attributes of the class

        // --- Methods of the class ---

        #region Constructor

        /// <summary>The default constructor creates all objects of the page, i.e. all the settings and the start button.</summary>
        public HubPage()
            : base()
        {
//DEBUG     Logging.I.LogMessage("Calling constructor of HubPage.\n");

            this.InitializeComponent();

            // Do not cache this page:
            NavigationCacheMode = NavigationCacheMode.Disabled;

            _popup_NameInput = null;

//          SetBackgroundImage();
            CreateThemesColumn();
            CreatePlayerColumns();
            CreateGameBoardColumn();
            CreateRulesColumn();
            CheckAdColumn();
            InitializeTooltips();

            if ((Presenter.I.GetGameStatus() == Presenter.GameStatus.Ended) ||
                Presenter.I.NoPlayerIsHuman())
            {
                DisableContinueButton();
            }
            else
            {
                EnableContinueButton();
            }
        }

        /// <summary>Sets the background image dependent on the current theme.</summary>
        private void SetBackgroundImage()
        {
            xaml_BackgroundImage.ImageSource = new BitmapImage(new Uri("ms-appx:/" + ThemeHandler.I.GetCurrentThemesDir() +
                                                                       "/" + KalahaResources.I.GetGamePageBackgroundFileName()));
        }

        #endregion Constructor


        #region ThemesColumn

        /// <summary>
        /// Creates the column that shows the themes selections.
        /// </summary>
        private void CreateThemesColumn()
        {
            string prefix = "../";

            // --- GridView for themes ---

            // Create contents for each button:
            ThemeButton themeButton_Wood = new ThemeButton
            {
                Title = KalahaResources.I.GetRes("Label_ThemeNameWood"),
                Subtitle = KalahaResources.I.GetRes("Label_ThemeNameWoodSubtitle"),
                ImagePath = prefix + KalahaResources.I.GetThemesRootDirName() + "/Wood/Icon.png",
                Theme = ThemeHandler.KalahaTheme.Wood,
                Id = 1
            };

            ThemeButton themeButton_Spring = new ThemeButton
            {
                Title = KalahaResources.I.GetRes("Label_ThemeNameSpring"),
                Subtitle = KalahaResources.I.GetRes("Label_ThemeNameSpringSubtitle"),
                ImagePath = prefix + KalahaResources.I.GetThemesRootDirName() + "/Spring/Icon.png",
                Theme = ThemeHandler.KalahaTheme.Spring,
                Id = 2
            };

            ThemeButton themeButton_SummerOnTheBeach = new ThemeButton
            {
                Title = KalahaResources.I.GetRes("Label_ThemeNameSummerOnTheBeach"),
                Subtitle = KalahaResources.I.GetRes("Label_ThemeNameSummerOnTheBeachSubtitle"),
                ImagePath = prefix + KalahaResources.I.GetThemesRootDirName() + "/SummerOnTheBeach/Icon.png",
                Theme = ThemeHandler.KalahaTheme.SummerOnTheBeach,
                Id = 2
            };

            ThemeButton themeButton_HighContrast = new ThemeButton
            {
                Title = KalahaResources.I.GetRes("Label_ThemeNameHighContrast"),
                Subtitle = KalahaResources.I.GetRes("Label_ThemeNameHighContrastSubtitle"),
                ImagePath = prefix + KalahaResources.I.GetThemesRootDirName() + "/HighContrast/Icon.png",
                Theme = ThemeHandler.KalahaTheme.HighContrast,
                Id = 3
            };

            // Add the buttons to the theme gridview:
            ColumnHeaderThemes.Text = KalahaResources.I.GetRes("Label_ColumnHeaderThemes");
            gridView_ThemeSelection.Items.Add(themeButton_Wood);
            gridView_ThemeSelection.Items.Add(themeButton_Spring);
            gridView_ThemeSelection.Items.Add(themeButton_SummerOnTheBeach);
            gridView_ThemeSelection.Items.Add(themeButton_HighContrast);
            gridView_ThemeSelection.SelectionMode = ListViewSelectionMode.Single;

            switch(ThemeHandler.I.GetCurrentTheme())
            {
                case ThemeHandler.KalahaTheme.Wood:
                    gridView_ThemeSelection.SelectedItem = themeButton_Wood;
                    break;
                case ThemeHandler.KalahaTheme.Spring:
                    gridView_ThemeSelection.SelectedItem = themeButton_Spring;
                    break;
                case ThemeHandler.KalahaTheme.SummerOnTheBeach:
                    gridView_ThemeSelection.SelectedItem = themeButton_SummerOnTheBeach;
                    break;
                case ThemeHandler.KalahaTheme.HighContrast:
                    gridView_ThemeSelection.SelectedItem = themeButton_HighContrast;
                    break;
            }
            if (Presenter.I.FlagToShowThemeExplanationIsSet())
            {
                // We stored in the Presenter the flag whether to now show an explanation for the theme button.
                Explain("Explain_Theme",
                        ((ThemeButton)gridView_ThemeSelection.SelectedItem).Title, ((ThemeButton)gridView_ThemeSelection.SelectedItem).Subtitle);

                // Re-set the flag:
                Presenter.I.SetFlagToShowThemeExplanation(false);
            }
            
            gridView_ThemeSelection.SelectionChanged += Theme_SelectionChanged;

        }

        /// <summary>
        /// This handler is called whenever one of the theme buttons is tapped.
        /// </summary>
        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            // We use eventArgs.AddedItems to get the items that are selected in the ItemsControl.
            IList<object> addedItems = eventArgs.AddedItems;
            if (addedItems.Count == 0)
            {
                // The user pulled down the button and thereby switched it off (removed it).
                // We simply switch it on again and leave the handler:
                ((GridView)sender).SelectedItem = ((ThemeButton)eventArgs.RemovedItems[0]);
                return;
            }

            if (addedItems.Count > 1)
            {
                throw new PSTException("HubPage.Theme_SelectionChanged: Unexpected number of added items: " + addedItems.Count);
            }

            // The HubPage will be reloaded now. Therefore it cannot display the theme explanation directly here. Instead we tell the Presenter to
            // keep the flag to show the explanation after the reload of the page:
            Presenter.I.SetFlagToShowThemeExplanation(true);

            // Switch to the next page and hand over the clicked item (which in this case is the respective theme button):
            ThemeButton themeButton = (ThemeButton)addedItems[0];

            // Now change the theme and reload the HubPage:
            ThemeHandler.I.ChangeTheme(themeButton.Theme);
        }

        #endregion ThemesColumn


        #region PlayerColumns

        /// <summary>
        /// Creates the two columns that show the player selections.
        /// </summary>
        private void CreatePlayerColumns()
        {
            // --- Define buttons for player selection (both south and north) ---

            SimpleButton humanButtonSouth = new SimpleButton
            {
                Label = Presenter.I.GetSouthPlayersHumanName(),
                Id = 1,
                Position = 0  // indicating that this is a south button (for later use in events)
            };

            SimpleButton humanButtonNorth = new SimpleButton
            {
                Label = Presenter.I.GetNorthPlayersHumanName(),
                Id = 1,
                Position = 1  // indicating that this is a north button (for later use in events)
            };

            SimpleButton computerEasyButton = new SimpleButton
            {
                Label = KalahaResources.I.GetLayoutRes("ComputerEasyName") + " (" + KalahaResources.I.GetRes("Label_ComputerEasy") + ")",
                Id = 2
            };
            
            SimpleButton computerMediumButton = new SimpleButton
            {
                Label = KalahaResources.I.GetLayoutRes("ComputerMediumName") + " (" + KalahaResources.I.GetRes("Label_ComputerMedium") + ")",
                Id = 3
            };
            
            SimpleButton computerHardButton = new SimpleButton
            {
                Label = KalahaResources.I.GetLayoutRes("ComputerHardName") + " (" + KalahaResources.I.GetRes("Label_ComputerHard") + ")",
                Id = 4
            };


            // --- GridView for the southern player ---

            Label_ColumnHeaderPlayerSouth.Text = KalahaResources.I.GetRes("Label_ColumnHeaderPlayerSouth");
            gridView_PlayerSpeciesSouth.Items.Add(humanButtonSouth);
            gridView_PlayerSpeciesSouth.Items.Add(computerEasyButton);
            gridView_PlayerSpeciesSouth.Items.Add(computerMediumButton);
            gridView_PlayerSpeciesSouth.Items.Add(computerHardButton);
            gridView_PlayerSpeciesSouth.IsHoldingEnabled = true;
            gridView_PlayerSpeciesSouth.Holding += gridView_PlayerSpecies_Holding;
            gridView_PlayerSpeciesSouth.SelectionMode = ListViewSelectionMode.Single;

            // Set the selection of the species button according to the currently set species:
            if (Presenter.I.SouthPlayerIsHuman())
            {
                gridView_PlayerSpeciesSouth.SelectedItem = humanButtonSouth;
            }
            else
            {
                switch (Presenter.I.GetSouthernComputerStrength())
                {
                    case Player.ComputerStrength.Easy:
                        gridView_PlayerSpeciesSouth.SelectedItem = computerEasyButton;
                        break;
                    case Player.ComputerStrength.Medium:
                        gridView_PlayerSpeciesSouth.SelectedItem = computerMediumButton;
                        break;
                    case Player.ComputerStrength.Hard:
                        gridView_PlayerSpeciesSouth.SelectedItem = computerHardButton;
                        break;
                }
            }

            gridView_PlayerSpeciesSouth.SelectionChanged += SpeciesSouth_SelectionChanged;


            // --- GridView for the northern player ---

            Label_ColumnHeaderPlayerNorth.Text = KalahaResources.I.GetRes("Label_ColumnHeaderPlayerNorth");
            gridView_PlayerSpeciesNorth.Items.Add(humanButtonNorth);
            gridView_PlayerSpeciesNorth.Items.Add(computerEasyButton);
            gridView_PlayerSpeciesNorth.Items.Add(computerMediumButton);
            gridView_PlayerSpeciesNorth.Items.Add(computerHardButton);
            gridView_PlayerSpeciesNorth.IsHoldingEnabled = true;
            gridView_PlayerSpeciesNorth.Holding += gridView_PlayerSpecies_Holding;
            gridView_PlayerSpeciesNorth.SelectionMode = ListViewSelectionMode.Single;
            // Set the selection of the species button according to the currently set species:
            if (Presenter.I.NorthPlayerIsHuman())
            {
                gridView_PlayerSpeciesNorth.SelectedItem = humanButtonNorth;
            }
            else
            {
                switch (Presenter.I.GetNorthernComputerStrength())
                {
                    case Player.ComputerStrength.Easy:
                        gridView_PlayerSpeciesNorth.SelectedItem = computerEasyButton;
                        break;
                    case Player.ComputerStrength.Medium:
                        gridView_PlayerSpeciesNorth.SelectedItem = computerMediumButton;
                        break;
                    case Player.ComputerStrength.Hard:
                        gridView_PlayerSpeciesNorth.SelectedItem = computerHardButton;
                        break;
                }
            }

            gridView_PlayerSpeciesNorth.SelectionChanged += SpeciesNorth_SelectionChanged;
        }

        /// <summary>
        /// Shows a popup to enter the player's name.
        /// </summary>
        private void ShowPopup_NameInput(object sender, int playerPosition)
        {
            GridView gridView = (GridView)sender;

            SimpleButton buttonClicked = (SimpleButton)gridView.Items[0];

            // if we already have one showing, don't create another one
            if (_popup_NameInput == null)
            {
                // create the Popup in code
                _popup_NameInput = new Popup();

                // Attach a function to the Popup.Closed event to remove this reference when saving the player's name later on:
                _popup_NameInput.Closed += (senderPopup, argsPopup) =>
                {
                    Popup popup = (Popup)senderPopup;
                    string nameEntered = popup.Tag.ToString();

                    if (nameEntered != "")
                    {
                        // A name has been entered, i.e. the "OK" button has been pressed.

                        buttonClicked.Label = nameEntered;
                        gridView.Items.RemoveAt(0);
                        switch (playerPosition)
                        {
                            case 0:
                                Presenter.I.SetSouthPlayersName(buttonClicked.Label);

                                // Remove the event handler temporarily in order not to arrive here immediately when setting the selected item:
                                gridView.SelectionChanged -= SpeciesSouth_SelectionChanged;

                                // Insert the previously remove button at the same position again (now with the new name on it):
                                gridView.Items.Insert(0, buttonClicked);

                                // Set this button selected again:
                                gridView.SelectedItem = buttonClicked;

                                // Add the previously removed event handler again:
                                gridView.SelectionChanged += SpeciesSouth_SelectionChanged;
                                break;
                            case 1:
                                // See above comments.
                                Presenter.I.SetNorthPlayersName(buttonClicked.Label);
                                gridView.SelectionChanged -= SpeciesNorth_SelectionChanged;
                                gridView.Items.Insert(0, buttonClicked);
                                gridView.SelectedItem = buttonClicked;
                                gridView.SelectionChanged += SpeciesNorth_SelectionChanged;
                                break;
                        }
                    }
                    _popup_NameInput = null;

                };

                // Set the content to our UserControl
                _popup_NameInput.Child = new InputPopup();

                // Open the Popup
                _popup_NameInput.IsOpen = true;
            }
        }

        /// <summary>Calls the actual event handler.</summary>
        private void SpeciesSouth_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            Species_SelectionChanged(sender, eventArgs, 0);
        }

        /// <summary>Calls the actual event handler.</summary>
        private void SpeciesNorth_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            Species_SelectionChanged(sender, eventArgs, 1);
        }

        /// <summary>
        /// This method is called on the event of the user clicking or tapping on one of the first player's
        /// species buttons. The settings are adjusted accordingly.
        /// </summary>
        /// <param name="playerNumber">0 for south, 1 for north</param>
        private void Species_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs, int playerNumber)
        {
            if ((playerNumber < 0) || (playerNumber > 1))
            {
                throw new PSTException("HubPage.Species_SelectionChanged: Unexpected player number: " + playerNumber);
            }

//DEBUG     Logging.I.LogMessage("Calling Species_SelectionChanged(" + playerNumber + ").\n");

            // We use eventArgs.AddedItems to get the items that are selected in the ItemsControl.
            IList<object> addedItems = eventArgs.AddedItems;
            if (addedItems.Count == 0)
            {
                // The user pulled down the button and thereby switched it off (removed it).
                // We simply switch it on again and leave the handler:
                ((GridView)sender).SelectedItem = ((SimpleButton)eventArgs.RemovedItems[0]);
                return;
            }

            if (addedItems.Count > 1)
            {
                throw new PSTException("HubPage.Species_SelectionChanged: Unexpected number of added items: " + addedItems.Count);
            }

            // Some item has been enabled.
            SimpleButton buttonPressed = (SimpleButton)addedItems[0];

            switch (buttonPressed.Id)
            {
                case 1:
                    if (Presenter.I.PlayerIsHuman(playerNumber))
                    {
                        // The user pressed the already enabled human player button.
                        // --> Jump to the input of the player's name:
                        ShowPopup_NameInput(sender, playerNumber);
                    }
                    else
                    {
                        // The user just switched from a computer player to human.
                        Presenter.I.SetPlayerToHuman(playerNumber);
                        Explain("Explain_HoldFingerToChangeName");
                    }
                    break;
                case 2:
                case 3:
                case 4:
                    Presenter.I.SetPlayerToComputer(playerNumber, buttonPressed.Id - 2);
                    Explain("Explain_ComputerOpponent_" + (buttonPressed.Id - 2).ToString());
                    break;
                default:
                    throw new PSTException("HubPage.Species_SelectionChanged: Unknown value for Id: " + buttonPressed.Id);
            }
        }

        /// <summary>
        /// Event that is called when the user holds down a player's button with his finger for a few seconds.
        /// </summary>
        private void gridView_PlayerSpecies_Holding(object sender, HoldingRoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            SimpleButton buttonClicked = (SimpleButton)gridView.Items[0];

            if (buttonClicked.Id == 1)
            {
                // The user held down the human player button.
                // --> Jump to the input of the player's name:
                ShowPopup_NameInput(sender, buttonClicked.Position);
            }
        }

        #endregion PlayerColumns


        #region GameBoardColumn

        /// <summary>
        /// Creates the column that shows the GameBoard selections.
        /// </summary>
        private void CreateGameBoardColumn()
        {
            // Set the column header:
            Label_ColumnHeaderGameBoard.Text = KalahaResources.I.GetRes("Label_ColumnHeaderGameBoard");

            // Create the "num houses" slider:
            Label_SelectNumHouses.Text = KalahaResources.I.GetRes("Label_SelectNumHouses");
            Slider_NumHouses.Value = Settings.I.GetNumberOfHousesPerPlayer();
            TextBlock_NumHouses.Text = Slider_NumHouses.Value.ToString();
            Slider_NumHouses.ValueChanged += Slider_NumHouses_ValueChanged;

            // Create the "num seeds" slider:
            Label_SelectNumSeeds.Text = KalahaResources.I.GetRes("Label_SelectNumSeeds");
            Slider_NumSeeds.Value = Settings.I.GetNumberOfSeedsPerHouse();
            TextBlock_NumSeeds.Text = Slider_NumSeeds.Value.ToString();
            Slider_NumSeeds.ValueChanged += Slider_NumSeeds_ValueChanged;
        }

        /// <summary>
        /// Event handler for a changed slider of the number of houses.
        /// </summary>
        private void Slider_NumHouses_ValueChanged(object sender, RangeBaseValueChangedEventArgs eventArgs)
        {
            double newSliderValue = eventArgs.NewValue;
            if (TextBlock_NumHouses != null)
            {
                TextBlock_NumHouses.Text = newSliderValue.ToString();
                Settings.I.SetNumberOfHousesPerPlayer(Convert.ToInt32(newSliderValue));
                Explain("Explain_NumHousesChanged");
                DisableContinueButton();
            }
        }

        /// <summary>
        /// Event handler for a changed slider of the initial number of seeds.
        /// </summary>
        private void Slider_NumSeeds_ValueChanged(object sender, RangeBaseValueChangedEventArgs eventArgs)
        {
            double newSliderValue = eventArgs.NewValue;
            if (TextBlock_NumSeeds != null)
            {
                TextBlock_NumSeeds.Text = newSliderValue.ToString();
                Settings.I.SetNumberOfSeedsPerHouse(Convert.ToInt32(newSliderValue));
                Explain("Explain_NumSeedsChanged");
                DisableContinueButton();
            }
        }

        #endregion GameBoardColumn


        #region RulesColumn

        /// <summary>
        /// Creates the column that shows the rules selections.
        /// </summary>
        private void CreateRulesColumn()
        {
            // --- Rules Gridview ---

            Label_ColumnHeaderRules.Text = KalahaResources.I.GetRes("Label_ColumnHeaderRules");

            SimpleButton captureSeedsAtEndButton = new SimpleButton
            {
                Label = KalahaResources.I.GetRes("Label_Rule_CaptureSeedsAtEnd"),
                Id = 1
            };

            SimpleButton pieRuleButton = new SimpleButton
            {
                Label = KalahaResources.I.GetRes("Label_Rule_PieRule"),
                Id = 2
            };

            gridView_Rules.Items.Add(captureSeedsAtEndButton);
       //     gridView_Rules.Items.Add(pieRuleButton);

            gridView_Rules.IsItemClickEnabled = false;
            gridView_Rules.SelectionMode = ListViewSelectionMode.Multiple;

            if (Rules.I.CaptureSeedsAtEndOfGame())
            {
                gridView_Rules.SelectedItems.Add(captureSeedsAtEndButton);
            }

            if (Rules.I.PieRuleIsEnabled())
            {
                gridView_Rules.SelectedItems.Add(pieRuleButton);
            }

            gridView_Rules.SelectionChanged += Rules_SelectionChanged;


            // --- Capture type ---

            // Create the "toggle" button:
            SimpleButton captureTypeButton = new SimpleButton
            {
                Id = (int)Rules.I.GetCaptureType(),
                Label = KalahaResources.I.GetRes("Label_CaptureType_" + ((int)Rules.I.GetCaptureType()).ToString())
            };

            // Create the gridview with the one "toggle" button for the capture type:
            Label_CaptureType.Text = KalahaResources.I.GetRes("Label_CaptureType");
            gridView_CaptureType.Items.Add(captureTypeButton);
            gridView_CaptureType.SelectionMode = ListViewSelectionMode.Multiple;
            gridView_CaptureType.SelectedItem = captureTypeButton;
            gridView_CaptureType.SelectionChanged += CaptureType_Clicked;


            // --- Sowing direction in the settings ---

            // Create the "toggle" button:
            SimpleButton directionSowingButton = new SimpleButton
            {
                Id = (int)Rules.I.GetDirectionOfSowing(),
                Label = KalahaResources.I.GetRes("Label_DirOfSowing_" + ((int)Rules.I.GetDirectionOfSowing()).ToString())
            };

            // Create the gridview with the one "toggle" button for the direction of sowing:
            Label_SelectSowingDirection.Text = KalahaResources.I.GetRes("Label_SelectSowingDirection");
            gridView_SowingDirection.Items.Add(directionSowingButton);
            gridView_SowingDirection.SelectionMode = ListViewSelectionMode.Multiple;
            gridView_SowingDirection.SelectedItem = directionSowingButton;
            gridView_SowingDirection.SelectionChanged += DirectionSowing_Clicked;
        }

        /// <summary>
        /// Handler for the events of clicking or tapping the Rules section.
        /// </summary>
        private void Rules_SelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            // We use eventArgs.AddedItems to get the items that are selected in the ItemsControl.
            IList<object> addedItems = eventArgs.AddedItems;
            if (addedItems.Count > 1)
            {
                throw new PSTException("HubPage.Rules_SelectionChanged: Unexpected addedItems.Count: " + addedItems.Count);
            }

            if (addedItems.Count == 1)
            {
                // Some item has been enabled.
                SimpleButton buttonPressed = (SimpleButton)addedItems[0];
                switch (buttonPressed.Id)
                {
                    case 1:
                        Rules.I.SetCaptureSeedsAtEndOfGame(true);
                        Explain("Explain_CaptureSeedsAtEndOn");
                        break;
                    case 2:
                        Rules.I.SetPieRuleIsEnabled(true);
                        break;
                    default:
                        throw new PSTException("HubPage.Rules_SelectionChanged: Unknown value for Id: " + buttonPressed.Id + "(adding)");
                }
            }
            else
            {
                IList<object> removedItems = eventArgs.RemovedItems;
                if (removedItems.Count > 1)
                {
                    throw new PSTException("HubPage.Rules_SelectionChanged: Unexpected removedItems.Count: " + removedItems.Count);
                }

                if (removedItems.Count == 1)
                {
                    // Some item has been disabled.
                    SimpleButton buttonPressed = (SimpleButton)removedItems[0];
                    switch (buttonPressed.Id)
                    {
                        case 1:
                            Rules.I.SetCaptureSeedsAtEndOfGame(false);
                            Explain("Explain_CaptureSeedsAtEndOff");
                            break;
                        case 2:
                            Rules.I.SetPieRuleIsEnabled(false);
                            break;
                        default:
                            throw new PSTException("HubPage.Rules_SelectionChanged: Unknown value for Id: " + buttonPressed.Id + "(removing)");
                    }
                }
                else
                {
                    // Nothing switched off.
                }
            }
        }

        /// <summary>
        /// Event handler that is called when the user clicks on the capture type button.
        /// </summary>
        private void CaptureType_Clicked(object sender, SelectionChangedEventArgs eventArgs)
        {
            // For the "toggle button", we always click on a selected button and thereby always find the button in the list of removed items:
            IList<object> removedItems = eventArgs.RemovedItems;
            if (removedItems.Count != 1)
            {
                throw new PSTException("HubPage.CaptureType_Clicked: Unexpected removedItems.Count: " + removedItems.Count);
            }

            SimpleButton buttonClicked = (SimpleButton)removedItems[0];

            // Switch the selection one "forward":
            int newId = (buttonClicked.Id + 1) % Rules.I.GetNumberOfCaptureTypes();
            buttonClicked.Id = newId;
            buttonClicked.Label = KalahaResources.I.GetRes("Label_CaptureType_" + newId.ToString());

            // In order to update the button, we remove the "old" button from the gridview and add the "new" one:
            GridView gridView = (GridView)sender;

            // First of all, temporarily remove this event handler from the gridview in order not to be called endlessly:
            gridView.SelectionChanged -= CaptureType_Clicked;

            // Now remove the "old" button:
            gridView.Items.RemoveAt(0);

            // Add the "new" button:
            gridView.Items.Add(buttonClicked);
            gridView.SelectedItem = buttonClicked;

            // Now add the event handler again:
            gridView.SelectionChanged += CaptureType_Clicked;

            // Set the new value in the rules:
            Rules.I.SetCaptureType((Rules.CaptureType)newId);

            // Explain the new capture rule:
            Explain("Explain_CaptureType_" + newId.ToString());
        }

        /// <summary>
        /// Event handler that is called when the user changes the direction of sowing.
        /// </summary>
        private void DirectionSowing_Clicked(object sender, SelectionChangedEventArgs eventArgs)
        {
            // For the "toggle button", we always click on a selected button and thereby always find the button in the list of removed items:
            IList<object> removedItems = eventArgs.RemovedItems;
            if (removedItems.Count != 1)
            {
                throw new PSTException("HubPage.DirectionSowing_Clicked: Unexpected removedItems.Count: " + removedItems.Count);
            }

            SimpleButton buttonClicked = (SimpleButton)removedItems[0];

            // Switch the selection one "forward":
            int newId = (buttonClicked.Id + 1) % Rules.I.GetNumberOfSowingDirections();
            buttonClicked.Id = newId;
            buttonClicked.Label = KalahaResources.I.GetRes("Label_DirOfSowing_" + newId.ToString());

            // In order to update the button, we remove the "old" button from the gridview and add the "new" one:
            GridView gridView = (GridView)sender;

            // First of all, temporarily remove this event handler from the gridview in order not to be called endlessly:
            gridView.SelectionChanged -= DirectionSowing_Clicked;

            // Now remove the "old" button:
            gridView.Items.RemoveAt(0);

            // Add the "new" button:
            gridView.Items.Add(buttonClicked);
            gridView.SelectedItem = buttonClicked;

            // Now add the event handler again:
            gridView.SelectionChanged += DirectionSowing_Clicked;

            // Set the new value in the rules:
            Rules.I.SetDirectionOfSowing((Rules.DirectionOfSowing)newId);

            // Explain the new capture rule:
            Explain("Explain_SowingDir_" + newId.ToString());
        }

        #endregion RulesColumn


        #region AdColumn

        /// <summary>
        /// Checks whether or not to show the ad column. This depends on the fact whether the user purchased the "NoAd" option.
        /// </summary>
        private void CheckAdColumn()
        {
            adColumn.Visibility = (InAppPurchases.I.NoAdsOptionIsActivated() ? Visibility.Collapsed : Visibility.Visible);
//DEBUG     Logging.I.LogMessage("HubPage.CheckAdColumn: Visibility of the ad column is \"" + adColumn.Visibility + "\".\n");
        }

        /// <summary>
        /// Calls the in-app purchase of the "NoAds" option.
        /// </summary>
        private async void RemoveAdsButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                // Start the purchasing process:
                Windows.Foundation.IAsyncOperation<string> operation = InAppPurchases.I.PurchaseNoAdsOption();
                await operation;
//DEBUG         Logging.I.LogMessage("HubPage.RemoveAdsButtonClicked: After call of InAppPurchases.PurchaseNoAdsOption().\n");

                InAppPurchases.I.Update();
//DEBUG         Logging.I.LogMessage("HubPage.RemoveAdsButtonClicked: After call of InAppPurchases.Update().\n");

                // No exception means everything went fine --> Repaint the whole page:
                PSTScreen.I.RedrawPage();
//DEBUG         Logging.I.LogMessage("HubPage.RemoveAdsButtonClicked: After call of PSTScreen.RedrawPage().\n");
            }
            catch (Exception ex)
            {
                Logging.I.LogMessage("HubPage.RemoveAdsButtonClicked: Purchase was unsuccessful.\n          \"" + ex.Message + "\"\n", Logging.LogLevel.Error);
            }
        }

        #endregion AdColumn


        #region ExplanationLine

        /// <summary>Prints an explanation on the bottom line. Fades out after some time.</summary>
        /// <param name="messageKey">The resource key to be used</param>
        /// <param name="args">0 or more parameters for the resource key</param>
        public /* async */ void Explain(String messageKey, params object[] args)
        {
            string message = KalahaResources.I.GetMsg(messageKey, args);
            xaml_ExplanationLine.Text = message;

/*
            int millisecsToShow = 5000;
            if (message.Length > 60)
            {
                // Long message -> Show the message longer;
                millisecsToShow += 1000;
            }

            // Print the actual message to the screen by fading in:
            await xaml_ExplanationLine.FadeIn(TimeSpan.FromMilliseconds(millisecsToShow));   // The parameter defines the time that the message is visible

            // ... and out:
            await xaml_ExplanationLine.FadeOut(TimeSpan.FromMilliseconds(200));
 */ 
        }

        #endregion ExplanationLine


        #region Start and Continue Button

        /// <summary>
        /// Initializes all tooltips for this page.
        /// </summary>
        private void InitializeTooltips()
        {
            const int tooltipFontsize = 20;

            ToolTip toolTip_ContinueButton = new ToolTip();
            toolTip_ContinueButton.FontSize = tooltipFontsize;
            toolTip_ContinueButton.Content = KalahaResources.I.GetRes("Tooltip_HubPage_ContinueButton");
            ToolTipService.SetToolTip(continueButton, toolTip_ContinueButton);

            ToolTip toolTip_StartButton = new ToolTip();
            toolTip_StartButton.FontSize = tooltipFontsize;
            toolTip_StartButton.Content = KalahaResources.I.GetRes("Tooltip_HubPage_StartButton");
            ToolTipService.SetToolTip(startButton, toolTip_StartButton);
        }

        /// <summary>
        /// Handles the event of having clicked the "Start" button.
        /// </summary>
        private void StartButton_Clicked(object sender, RoutedEventArgs eventArgs)
        {
            Presenter.I.SetGameStatus(Presenter.GameStatus.StartedNew);
            this.Frame.Navigate(typeof(GamePage));
        }

        /// <summary>
        /// Handles the event of having clicked the "Continue Game" button.
        /// </summary>
        private void ContinueButton_Clicked(object sender, RoutedEventArgs eventArgs)
        {
            Presenter.I.SetGameStatus(Presenter.GameStatus.Continued);
            this.Frame.Navigate(typeof(GamePage));
        }

        /// <summary>
        /// Shows the "continue" button
        /// </summary>
        private void EnableContinueButton()
        {
            continueButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the "continue" button
        /// </summary>
        private void DisableContinueButton()
        {
            continueButton.Visibility = Visibility.Collapsed;

            // Set the status of the game to "Ended" in order to make clear that the continue button cannot be enabled anymore:
            Presenter.I.SetGameStatus(Presenter.GameStatus.Ended);
        }

        #endregion Start and Continue Button


        #region Handling of window sizes

        /// <summary>
        /// Handles the event that the window size is changed (e.g. to "snapped" or "filled").
        /// </summary>
        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs eventArgs)
        {
            //TODO: Somehow unify with the same function of the GamePage class.
            // Obtain view state by explicitly querying for it
            ApplicationViewState myViewState = ApplicationView.Value;
//DEBUG     Logging.I.LogMessage("HubPage: Window size changed to " + myViewState.ToString() + ".\n");

            switch (myViewState)
            {
                case Windows.UI.ViewManagement.ApplicationViewState.Filled:
                case Windows.UI.ViewManagement.ApplicationViewState.FullScreenLandscape:
                case Windows.UI.ViewManagement.ApplicationViewState.FullScreenPortrait:

                    // The "large" view has been enabled.

                    landScapeView.Visibility = Visibility.Visible;
                    snappedView.Visibility = Visibility.Collapsed;
                    // Re-init window sizes:
//DEBUG             Logging.I.LogMessage("Screen size before re-init: " + PSTScreen.I.ToString() + ".\n");
                    PSTScreen.I.ReInit();
//DEBUG             Logging.I.LogMessage("Screen size after re-init: " + PSTScreen.I.ToString() + ".\n");

                    // Redraw the page:
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            //DEBUG     Logging.I.LogMessage("Calling HubPage.LoadState().\n");

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
            //DEBUG     Logging.I.LogMessage("Calling HubPage.SaveState().\n");

            // Remove this handler from the "size changed" event list because it will be added anyway when loading the page again:
            Window.Current.SizeChanged -= WindowSizeChanged;
        }

        #endregion Loading and Saving State

    }

    #region Button classes

    class ThemeButton
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ImagePath { get; set; }
        public ThemeHandler.KalahaTheme Theme { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            return Title;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }


    class SimpleButton
    {
        public string Label { get; set; }
        public int Id { get; set; }
        public int Position { get; set; }

        public override string ToString()
        {
            return Label;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    #endregion Button classes
}
