//
// Presenter
//
// Defines the class of the presenter in the model-view-presenter pattern.
// Similar to the controller in the MVC pattern, the presenter acts upon the model and the view.
// It retrieves data from repositories (the model) and formats it for display in the view.
// The presenter is implemented as a singleton that is available throughout the source "space".
//

#region Using

using Kalaha.Model;
using Kalaha.View;
using PST_Common;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#endregion Using


namespace Kalaha
{

    public sealed class Presenter
    {
        // --- Enums ---

        #region Enums

        /// <summary>The status the current game is in.</summary>
        public enum GameStatus
        {
            Ended,              // when a game has ended (or right after the app was started)
            StartedNew,         // when the user presses the "Start" button on the HubPage
            Continued,          // when the user presses the "Continue" button on the HubPage or shortly before a reload of the GamePage
            Running,            // while a game is actively running and the GamePage is shown
            Interrupted         // when a game has been left but there is still the possibility to continue this game
        }

        #endregion Enums


        // --- Attributes of the class ---

        #region Attributes

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile Presenter _inst;
        private static object _syncRoot = new Object();

       /// <summary>The private attribute _view is the 'V' in MVP.</summary>
        private GamePage _view;

        /// <summary>The _gameBoard is part of the 'M' in MVP.</summary>
        private Kalaha.Model.GameBoard _modelGameBoard;

        /// <summary>
        /// The "View.Model.GameBoard" which handles all necessary information about
        /// the gameboard to display
        /// </summary>
        private Kalaha.View.Model.GameBoard _viewModelGameBoard;

        /// <summary>The southern player</summary>
        private Player _southPlayer;

        /// <summary>The northern player</summary>
        private Player _northPlayer;

        /// <summary>The player whose turn it is (respectively; changing during the game)</summary>
        private Player _currentPlayer;

        /// <summary>The player who may play the first move in the game</summary>
        private Player _playerWhoIsFirst;

        /// <summary>The object that manages the undo activities</summary>
        private UndoManager _undoManager;

        /// <summary>Flag whether or not the last seed of the last move fell into the player's Kalah</summary>
        bool _lastSeedlFellIntoOwnKalah;

        /// <summary>Flag whether or not the last seed fell into an empty house of the player</summary>
        bool _lastSeedFellIntoEmptyOwnHouse;

        /// <summary>Flag whether or not it is the player's move again (due to respective rules)</summary>
        bool _playerMayMoveAgain;

        /// <summary>The pit that the last seed fell into (only valid if __lastSeedFellIntoEmptyOwnHouse is true)</summary>
        int _pitOfLastSeed;

        /// <summary>An array that stores the maximum value per house when the computer calculates the move</summary>
        int[] _maxValue;

        /// <summary>
        /// A little workaround for the explanations of themes on the HubPage. Due to the fact that the HubPage is reloaded when
        /// a theme button is pressed, it needs to remember somewhere whether a theme explanation shall be shown after the reload.
        /// That is done in the Presenter via this member variable.
        /// </summary>
        bool _tellHubPageToShowThemeExplanation;

        /// <summary>
        /// The status that the current game is in.
        /// </summary>
        private GameStatus _gameStatus;

        #endregion Attributes


        // --- Methods of the class ---

        #region Constructor and Initializers

        /// <summary>
        /// The constructor takes a View_Base object (or an instance of any derived class) in order
        /// to store it for later use.
        /// </summary>
        private Presenter()
        {
//DEBUG     Logging.I.LogMessage("Calling constructor of Presenter.\n");

            _view = null;

            // Initially, there is no game running:
            _gameStatus = GameStatus.Ended;

            // Create the game board, defining the number of pits per player:
            _modelGameBoard = new Kalaha.Model.GameBoard();

            // Create the two players:
            _southPlayer = new Player(Player.Position.South);
            _northPlayer = new Player(Player.Position.North);

            // Start with an empty undo list:
            _undoManager = new UndoManager();

            // Restore previously saved values for all kinds of settings:
            string restoredThemeName = "";
            GameStorage.I.RestoreSavedValues(ref _southPlayer, ref _northPlayer, ref restoredThemeName);
            SwitchToRestoredTheme(restoredThemeName);

            // Define the southern player to start first:
            _playerWhoIsFirst = _southPlayer;          //TODO: Restore the first player from the saved values.

            // Set the HubPage workaround to "don't show explanation":
            _tellHubPageToShowThemeExplanation = false;
        }

        /// <summary>
        /// Switches the current theme to the theme that is given in the parameter.
        /// If no match can be found, the default theme is taken.
        /// </summary>
        /// <param name="restoredThemeName">The restored theme name which indicates which theme to switch to.</param>
        private void SwitchToRestoredTheme(string restoredThemeName)
        {
//DEBUG     Logging.I.LogMessage("Presenter.SwitchToRestoredTheme(): Trying to switch theme to \"" + restoredThemeName + "\".\n");

            // Restore the theme that was selected in the last session of the app:
            bool foundTheme = false;
            if (restoredThemeName != "")
            {
                // Loop over all known themes in order to find the one that was restored:
                foreach (int themeIndex in Enum.GetValues(typeof(ThemeHandler.KalahaTheme)))
                {
                    // Get the string to the currently looped enum:
                    string themeName = ((ThemeHandler.KalahaTheme)themeIndex).ToString();

                    // Compare the restored string to the currently looped enum string:
                    if (restoredThemeName == themeName)
                    {
                        // We were successful!
                        foundTheme = true;

                        // Change the theme accordingly:
                        ThemeHandler.I.ChangeTheme((ThemeHandler.KalahaTheme)themeIndex, false);
//DEBUG                 Logging.I.LogMessage("Presenter.SwitchToRestoredTheme(): Successful.\n");
                        
                        // Leave the loop:
                        break;
                    }
                }
            }

            if (!foundTheme)
            {
//DEBUG         Logging.I.LogMessage("Presenter.SwitchToRestoredTheme(): Unsuccessful -> Switching to default theme.\n");

                // We could not find the theme that was supposed to be restored -> Switch to the default theme:
                ThemeHandler.I.ChangeToDefaultTheme();
            }
        }

        /// <summary>
        /// Takes a View_Base object (or an instance of any derived class) in order
        /// to store it for later use.
        /// </summary>
        public void SetView(GamePage view)
        {
            _view = view;
        }

        /// <summary>
        ///  Initializes the gameboard and other objects when a new game is started.
        /// </summary>
        public void InitializeAtGameStart(Canvas gameBoardCanvas,
                                          PointerEventHandler PointerEnteredHouse,
                                          PointerEventHandler PointerExitedHouse,
                                          RoutedEventHandler TouchFieldSelected)
        {
            GameStatus gameStatus = GetGameStatus();
            if ((gameStatus != GameStatus.StartedNew) && (gameStatus != GameStatus.Continued))
            {
                throw new PSTException("Presenter.InitializeAtGameStart: Unexpected game status: " + GetGameStatus() + ".");
            }

            if (gameStatus == GameStatus.StartedNew)
            {
                // Define the size of the game board:
                _modelGameBoard.SetSize(Settings.I.GetNumberOfHousesPerPlayer(), Settings.I.GetNumberOfSeedsPerHouse());

                // Set the current player:
                _currentPlayer = _playerWhoIsFirst;

                // We set this to true initially in order to avoid an initial toggle in "DecideWhoIsNext()":
                _playerMayMoveAgain = true;

                // Initialize the undo manager:
                _undoManager.Inititalize();
            }
 
            // Create a new "ModelView" GameBoard:
            _viewModelGameBoard = new Kalaha.View.Model.GameBoard(_modelGameBoard, gameBoardCanvas,
                                                                  PointerEnteredHouse,
                                                                  PointerExitedHouse,
                                                                  TouchFieldSelected);

            switch(gameStatus)
            {
                case GameStatus.StartedNew:
                    // In a new game we start with the initial number of seeds:
                    _viewModelGameBoard.InitializeNumbersOfSeeds();
                    break;

                case GameStatus.Continued:
                    // An existing game is to be continued, so we copy the number of seeds per pit from the "Model" GameBoard to the "ViewModel" GameBoard:
                    _viewModelGameBoard.CopySeedNumbersOfGameBoard();
                    break;
            }
        }

        #endregion Constructor and Initializers


        #region Main Game Handling

        /// <summary>
        /// Starts or continues the game by preparing some steps and deciding whose turn it is.
        /// </summary>
        public void StartOrContinueGame()
        {
            GameStatus gameStatus = GetGameStatus();
            if ((gameStatus != GameStatus.StartedNew) && (gameStatus != GameStatus.Continued))
            {
                throw new PSTException("Presenter.InitializeAtGameStart: Unexpected game status: " + GetGameStatus() + ".");
            }
            if (_view == null)
            {
                throw new PSTException("Presenter.StartGame: View is undefined");
            }

            switch(gameStatus)
            {
                case GameStatus.StartedNew:
                    Logging.I.LogMessage("---------- Starting a new game. ------------\n", Logging.LogLevel.Info);
                    break;

                case GameStatus.Continued:
                    Logging.I.LogMessage("---------- Continuing the existing game. ---\n", Logging.LogLevel.Info);
                    if (!_playerMayMoveAgain)
                    {
                        // Players need to be toggled in order to hit the right one:
                        TogglePlayers();
                    }
                    break;
            }

            // Now we are "officially" up and running again: 
            SetGameStatus(GameStatus.Running);

            // Continue with the actual game:
            DecideWhoIsNext();
        }


        /// <summary>
        /// The current user made his choice by selecting a pit with the given index.
        /// If the choice was valid, the according move is performed on the game board.
        /// If specific rules apply, the respective moves are performed as well.
        /// </summary>
        /// <param name="selectedHouse">The user's choice, starting with 0</param>
        public void MoveSeedsOfSelectedHouse(int selectedHouse)
        {
            // "Normalize" the selected house:
            selectedHouse = (selectedHouse % (_modelGameBoard.GetNumOfHousesPerPlayer()+1));

            if (!UsersChoiceIsValid(selectedHouse))
            {
                return;
            }

            // Present the selected pit as selected and reset the display of all other pits:
            _viewModelGameBoard.SetSelected(selectedHouse, _currentPlayer);

            // Disable the touch fields of all houses:
            _viewModelGameBoard.DisableAllHouses();

            // Move the seeds according to the user's choice:
            _lastSeedlFellIntoOwnKalah = false;
            _lastSeedFellIntoEmptyOwnHouse = false;
            _pitOfLastSeed = -1;

            Move move;

            move = _modelGameBoard.MoveHouse(_currentPlayer, selectedHouse,
                                     ref _lastSeedlFellIntoOwnKalah,
                                     ref _lastSeedFellIntoEmptyOwnHouse,
                                     ref _pitOfLastSeed,
                                     true);  // Get back the move in detail

            Logging.I.LogMessage("Normal" + move.ToString(), Logging.LogLevel.DeepDebug);

            _undoManager.RememberNormalNove(move, _currentPlayer);
//DEBUG     Logging.I.LogMessage(_undoManager.ToString());

            _view.DisableButtonsOnPage();

            // Tell the View that the given move shall be visualized and also which method to call next after the visualization:
            _viewModelGameBoard.VisualizeMove(move, CheckRulesAfterMove);
//DEBUG     Logging.Inst.LogMessage("Presenter.MoveSelectedPit(): Back from _view.VisualizeMove().\n"); 
        }

        /// <summary>Checks the rules after a move has beed executed and possibly executes more moves.</summary>
        public void CheckRulesAfterMove()
        {
//DEBUG     Logging.Inst.LogMessage("Calling Presenter.CheckRulesAfterMove().\n");

            _view.EnableButtonsOnPage();

            // Now check if some rules apply:
            if (Rules.I.PlayAgainWhenLastSeedInOwnKalah() &&
                _modelGameBoard.PlayerStillHasFullHouses(_currentPlayer) &&
                _lastSeedlFellIntoOwnKalah)
            {
                // It is the same player's turn again.
                _view.PrintFadingMsg("PlayersTurnAgain", _currentPlayer);
                _playerMayMoveAgain = true;

                // Decide whose turn it is:
                DecideWhoIsNext();
            }
            else
            {
                _playerMayMoveAgain = false;

                if (_lastSeedFellIntoEmptyOwnHouse)
                {
                    int numOfSeedsOfOpponentsHouse = _modelGameBoard.GetOppositeHouse(_currentPlayer, _pitOfLastSeed).GetNumberofSeeds();
                    bool captureMoveMayBeExecuted = false;
                    bool captureOpponentsHouse = false;

                    // Find out if under the current rule setting a capture move may be executed and whether the opponent's house may be captured:
                    Rules.I.GetCapturePermissions(numOfSeedsOfOpponentsHouse, ref captureMoveMayBeExecuted, ref captureOpponentsHouse);

                    if (captureMoveMayBeExecuted)
                    {
                        // The rule of a capture move applies.

                        _view.PrintFadingMsg("LastSeedInEmptyPit", _currentPlayer, (_pitOfLastSeed + 1));
                        Move captureMove = _modelGameBoard.PlayerWinsHouses(_currentPlayer, _pitOfLastSeed, captureOpponentsHouse, true);
                        Logging.I.LogMessage("Capture" + captureMove.ToString(), Logging.LogLevel.DeepDebug);

                        _undoManager.RememberFollowUpMove(captureMove, _currentPlayer);
//DEBUG                   Logging.I.LogMessage(_undoManager.ToString());

                        _view.DisableButtonsOnPage();

                        // Visualize the capture move and decide whose turn it is only after the move is completely visualized:
                        _viewModelGameBoard.VisualizeMove(captureMove, DecideWhoIsNext, true);
// DEBUG                Logging.Inst.LogMessage("Presenter.CheckRulesAfterMove(): Back from _view.VisualizeMove(captureMove).\n");
                    }
                    else
                    {
                        // Despite the fact that the last seed fell into an empty own house, we do not perform a capture move because
                        // the opposite house is empty and the rule "empy capture" is not enabled.
                        // -> Go on with the normal game:
                        DecideWhoIsNext();
                    }
                }
                else
                {
                    // No special rule applies. -> Go on with the normal game:
                    DecideWhoIsNext();
                }
            }
        }

        /// <summary>
        /// Decides whether the computer makes the next move or the human player needs to enter their move.
        /// </summary>
        public void DecideWhoIsNext()
        {
            //DEBUG     Logging.Inst.LogMessage("Calling DecideWhoIsNext().\n");

            if (_view == null)
            {
                throw new PSTException("Presenter.DecideWhoIsNext: View is undefined");
            }

            if ((Presenter.I.GetGameStatus() == GameStatus.Interrupted) ||
                (Presenter.I.GetGameStatus() == GameStatus.Ended))
            {
                // The game has been interrupted or ended. If both players are the computer, it may be that we have not really realized this interruption
                // for the computation of moves. Then we stop here immediately.
//DEBUG         Logging.I.LogMessage("During move computation the game has been " +
//DEBUG                              ((Presenter.I.GetGameStatus() == GameStatus.Interrupted) ? "interrupted" : "ended") + " -> Stopping immediately.\n");
                return;
            }

            _view.EnableButtonsOnPage();

            if (GameIsOver())
            {
                // The game is over.
                // Show final result (winners etc.):
                FinalizeResultAndShowWinner();
            }
            else
            {
                // The game is not yet over. It it is not the same player's turn, switch to the respective other player:
                if (!_playerMayMoveAgain)
                {
                    TogglePlayers();
                }

                if (_currentPlayer.GetSpecies() == Player.Species.Human)
                {
                    // Show which houses can be selected:
                    _viewModelGameBoard.EnableHousesOfPlayer(_currentPlayer);
                }
                else
                {
                    // The current player is the computer, so let the computer decide which move to take.

                    _view.ShowProgress((_currentPlayer.GetPosition() == Player.Position.South) ? 0 : 1);
                    _view.DisableButtonsOnPage();

                    // In order to let the UI work smoothly, we start the computation via a timer:
                    DispatcherTimer timer = new DispatcherTimer();

                    // Very short time span as we actually do not want to wait but want to run this independently.
                    timer.Interval = new TimeSpan(1);

                    // Assign the method to call when the timer runs out:
                    timer.Tick += StartMoveComputation;

                    // Start the timer:
                    timer.Start();
                }
            }
        }

        #endregion Main Game Handling


        #region Computer calculates moves

        /// <summary>
        /// This method is called as soon as a timer runs out. It starts the computation of a move by the computer.
        /// </summary>
        /// <param name="sender">The timer that started this method</param>
        private void StartMoveComputation(object sender, object eventArgs)
        {
            // First of all, stop the timer that started us:
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();

            // Start a stopwatch to see how long the computation takes:
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            // Now do the computation:
            int houseToMove = LetComputerCalculateTheMove();

            // See how long the computation took:
            stopwatch.Stop();
            long millisecs = stopwatch.ElapsedMilliseconds;
            if (millisecs < 1000)
            {
                // Wait for a while to make it more "thinking":
                int timeToReachOneSecond = 1000 - (int)millisecs;
                Basics.Wait(timeToReachOneSecond);
                // Basics.Wait(Settings.I.GetComputerPause(_currentPlayer.GetComputerStrength()));
                Logging.I.LogMessage("Computation took " + millisecs + " millisecs -> Waited " + timeToReachOneSecond + " millisecs.\n", Logging.LogLevel.DeepDebug);
            }
            else
            {
                Logging.I.LogMessage("Computation took " + millisecs + " millisecs -> No waiting.\n", Logging.LogLevel.DeepDebug);
            }

            _view.ProgressEnded((_currentPlayer.GetPosition() == Player.Position.South) ? 0 : 1);

            _view.EnableButtonsOnPage();
            // Perform the move that the computer selected:
            MoveSeedsOfSelectedHouse(houseToMove);
        }

        /// <summary>The move is calculated by the computer and returns the respective house number.</summary>
        public int LetComputerCalculateTheMove()
        {
            // Get the maximum recursion depth depending on computer strength and number of houses:
            int maxRecursionDepth = Settings.I.GetRecursionDepth(_currentPlayer.GetComputerStrength(), _modelGameBoard.GetNumOfHousesPerPlayer());

            //            Logging.I.LogMessage("Calling LetComputerDoTheMove().\n", Logging.LogLevel.DeepDebug);
            _modelGameBoard.LogBoard(_southPlayer, _northPlayer, Logging.LogLevel.DeepDebug); // Logging/Debugging

            // An array that stores the maximum value house:
            _maxValue = new int[_modelGameBoard.GetNumOfHousesPerPlayer()];

            // Initialize the array with "minus infinity":
            for (int index = 0; index < _modelGameBoard.GetNumOfHousesPerPlayer(); ++index)
            {
                _maxValue[index] = -Basics.Infinity();
            }

            Player currentPlayer = _currentPlayer;

            int recursionDepth = 1;

            // Call the recursive game theory algorithm to find out about the maximum gain we can win:
            int max = Maximizer(_modelGameBoard, currentPlayer, recursionDepth, maxRecursionDepth);

            // Logging/Debugging:
            //            string logString = "maxValue: ";
            //            for (int pitIndex = 0; pitIndex < _modelGameBoard.GetNumOfHousesPerPlayer(); ++pitIndex)
            //            {
            //                logString += (_maxValue[pitIndex] + " ");
            //            }
            //            Logging.I.LogMessage(logString + "\n", Logging.LogLevel.DeepDebug);

            int houseToMove = -1;  // The return value;

            // The "maxValue" array now contains the maximum values for each pit.
            // We look randomly for a pit that has the maximum value for the case that more than one
            // pit has the maximum value:
            do
            {
                houseToMove = Basics.GetRandomNumber(0, _modelGameBoard.GetNumOfHousesPerPlayer() - 1);
            }
            while ((_maxValue[houseToMove] != max) ||
                   (_modelGameBoard.GetPitOfPlayer(_currentPlayer, houseToMove).GetNumberofSeeds() == 0));

            //            Logging.I.LogMessage("houseToMove: " + houseToMove + "\n", Logging.LogLevel.DeepDebug);

            return houseToMove;
        }

        /// <summary>
        /// Game theory algorithm to maximize the possible return value.
        /// If the maximum recursion depth is reached or the current player's pits are all empty, the difference of the
        /// Kalahs is regarded as the gains of the respective recursion path.
        /// Otherwise, in a loop each non-empty pit is moved, and the minimizer for this move is called.
        /// </summary>
        /// <param name="gameBoard">The game board that is currently valid in the recursion path</param>
        /// <param name="currentPlayer">The current player</param>
        /// <param name="currentRecursionDepth">The current recursion depth we are already in</param>
        /// <param name="maxRecursionDepth">The maximum recursion depth (a constant)</param>
        /// <returns></returns>
        private int Maximizer(Kalaha.Model.GameBoard gameBoard, Player currentPlayer,
                              int currentRecursionDepth, int maxRecursionDepth)
        {
            //            Logging.I.LogMessage("Maximizer: entering depth " + currentRecursionDepth + Environment.NewLine,
            //                                    Logging.LogLevel.DeepDebug);
            gameBoard.LogBoard(currentPlayer, Opponent(currentPlayer), Logging.LogLevel.DeepDebug);

            if (gameBoard.PlayerOnlyHasEmptyHouses(currentPlayer) ||
                gameBoard.PlayerOnlyHasEmptyHouses(Opponent(currentPlayer)))
            {
                // One player's pits are all empty.
                // --> Calculate the difference between the two Kalahs:
                int retValue = gameBoard.GetKalahOfPlayer(currentPlayer).GetNumberofSeeds() -
                               gameBoard.GetKalahOfPlayer(Opponent(currentPlayer)).GetNumberofSeeds();

                if (Rules.I.CaptureSeedsAtEndOfGame())
                {
                    // The rule to capture all seeds in the houses at the end is enabled
                    // --> Take these seeds into account, too:
                    retValue = retValue + gameBoard.SumOfAllSeedsOfPlayer(currentPlayer) -
                                          gameBoard.SumOfAllSeedsOfPlayer(Opponent(currentPlayer));
                }

                //                Logging.I.LogMessage("Maximizer on depth " + currentRecursionDepth + ": Pits empty. Returning " +
                //                                        retValue + "." + Environment.NewLine,
                //                                        Logging.LogLevel.DeepDebug);
                return retValue;
            }

            if (currentRecursionDepth == maxRecursionDepth)
            {
                // We have reached the "bottom" of the recursion.
                // --> Return the difference of the two Kalahs the maximum value:
                int retValue = gameBoard.GetKalahOfPlayer(currentPlayer).GetNumberofSeeds() -
                               gameBoard.GetKalahOfPlayer(Opponent(currentPlayer)).GetNumberofSeeds();
                //                Logging.I.LogMessage("Maximizer on depth " + currentRecursionDepth + ": maxRecursionDepth " + maxRecursionDepth +
                //                                        " reached. Returning " + retValue + "." + Environment.NewLine,
                //                                        Logging.LogLevel.DeepDebug);
                return retValue;
            }

            // We are still in the recursion path and drill deeper down.
            int max = -Basics.Infinity();

            for (int pitIndex = 0; pitIndex < gameBoard.GetNumOfHousesPerPlayer(); ++pitIndex)
            {
                // Create a copy of the current gameboard and take the copy to perform the move of one pit:
                Kalaha.Model.GameBoard localGameBoard = gameBoard.GetACopy();

                if (localGameBoard.GetPitOfPlayer(currentPlayer, pitIndex).ContainsASeed())
                {
                    // The pit of the current pitIndex has a seed in it, so perform the move now:
                    bool lastSeedFellIntoOwnKalah = false;
                    bool lastSeedFellIntoEmptyOwnPit = false;
                    int lastSeedsPit = -1;

                    //                    Logging.I.LogMessage("Maximizer on depth " + currentRecursionDepth + ": Moving pit " +
                    //                                            pitIndex + "." + Environment.NewLine,
                    //                                            Logging.LogLevel.DeepDebug);
                    localGameBoard.MoveHouse(currentPlayer, pitIndex,
                                             ref lastSeedFellIntoOwnKalah,
                                             ref lastSeedFellIntoEmptyOwnPit,
                                             ref lastSeedsPit,
                                             false);  // Do not generate the move's intermediate steps on a seed level

                    int opt = -Basics.Infinity();
                    if (Rules.I.PlayAgainWhenLastSeedInOwnKalah() && lastSeedFellIntoOwnKalah)
                    {
                        // It is the same player's turn again.
                        // --> Continue with the maximizer recursion:
                        opt = Maximizer(localGameBoard, currentPlayer,
                                        currentRecursionDepth + 1, maxRecursionDepth);
                        //                        Logging.I.LogMessage("Maximizer on depth " + currentRecursionDepth + ": Got back opt " +
                        //                                                opt + " from Maximizer one level deeper." + Environment.NewLine,
                        //                                                Logging.LogLevel.DeepDebug);
                    }
                    else
                    {
                        // Check if the "winning opposite pit" rule applies:
                        if (lastSeedFellIntoEmptyOwnPit)
                        {
                            // The last seed fell into an own empty house. This may mean that we can execute the capture move.
                            int numOfSeedsOfOpponentsHouse = gameBoard.GetOppositeHouse(currentPlayer, lastSeedsPit).GetNumberofSeeds();
                            bool captureMoveMayBeExecuted = false;
                            bool captureOpponentsHouse = false;

                            // Find out if under the current rule setting a capture move may be executed and whether the opponent's house may be captured:
                            Rules.I.GetCapturePermissions(numOfSeedsOfOpponentsHouse, ref captureMoveMayBeExecuted, ref captureOpponentsHouse);

                            if (captureMoveMayBeExecuted)
                            {
                                //                                Logging.I.LogMessage("Maximizer on depth " + currentRecursionDepth +
                                //                                                        ": The last seed fell into the empty house #" + (lastSeedsPit + 1) + ".\n",
                                //                                                        Logging.LogLevel.DeepDebug);
                                localGameBoard.PlayerWinsHouses(currentPlayer, lastSeedsPit, captureOpponentsHouse, false);
                            }
                            else
                            {
                                //                                Logging.I.LogMessage("Maximizer on depth " + currentRecursionDepth +
                                //                                                        ": The last seed fell into the empty house #" + (lastSeedsPit + 1) +
                                //                                                        " but rules prohibit the capture.\n",
                                //                                                        Logging.LogLevel.DeepDebug);
                            }
                        }
                        // It is the opponent's turn.
                        // --> Continue with the minimizer recursion:
                        opt = Minimizer(localGameBoard, Opponent(currentPlayer),
                                        currentRecursionDepth + 1, maxRecursionDepth);
                        //                        Logging.I.LogMessage("Maximizer on depth " + currentRecursionDepth + ": Got back opt " +
                        //                                                opt + " from Minimizer one level deeper." + Environment.NewLine,
                        //                                                Logging.LogLevel.DeepDebug);
                    }

                    if (currentRecursionDepth == 1)
                    {
                        // We are in the highest layer of the recursion, so we store the optimum we got
                        // in the respective array of maxima:
                        _maxValue[pitIndex] = opt;
                        //                        Logging.I.LogMessage("Maximizer on depth " + currentRecursionDepth + ": maxValue[" + pitIndex +
                        //                                                "] = " + opt + "." + Environment.NewLine,
                        //                                                Logging.LogLevel.DeepDebug);
                    }

                    // In any case we are interested in the best result we get from the deeper levels:
                    if (opt >= max)
                    {
                        // We have found a new maximum.
                        max = opt;
                        //                        Logging.I.LogMessage("Maximizer on depth " + currentRecursionDepth + ": New maximum found: " + max +
                        //                                                "." + Environment.NewLine,
                        //                                                Logging.LogLevel.DeepDebug);

                    }
                }
            }

            // Return the maximum we found across all possible moves:
            return max;
        }


        /// <summary>
        /// Game theory algorithm to minimize the possible return value.
        /// If the maximum recursion depth is reached or the current player's pits are all empty, the difference of the
        /// Kalahs is regarded as the gains of the respective recursion path.
        /// Otherwise, in a loop each non-empty pit is moved, and the maximizer for this move is called.
        /// </summary>
        /// <param name="gameBoard">The game board that is currently valid in the recursion path</param>
        /// <param name="currentPlayer">The current player</param>
        /// <param name="currentRecursionDepth">The current recursion depth we are already in</param>
        /// <param name="maxRecursionDepth">The maximum recursion depth (a constant)</param>
        /// <returns></returns>
        private int Minimizer(Kalaha.Model.GameBoard gameBoard, Player currentPlayer,
                              int currentRecursionDepth, int maxRecursionDepth)
        {
            //            Logging.I.LogMessage("Minimizer: entering depth " + currentRecursionDepth + Environment.NewLine,
            //                                    Logging.LogLevel.DeepDebug);
            gameBoard.LogBoard(currentPlayer, Opponent(currentPlayer), Logging.LogLevel.DeepDebug);

            if (gameBoard.PlayerOnlyHasEmptyHouses(currentPlayer) ||
                gameBoard.PlayerOnlyHasEmptyHouses(Opponent(currentPlayer)))
            {
                // One player's pits are all empty.
                // --> Calculate the difference between the two Kalahs:
                int retValue = gameBoard.GetKalahOfPlayer(Opponent(currentPlayer)).GetNumberofSeeds() -
                               gameBoard.GetKalahOfPlayer(currentPlayer).GetNumberofSeeds();

                if (Rules.I.CaptureSeedsAtEndOfGame())
                {
                    // The rule to capture all seeds in the houses at the end is enabled
                    // --> Take these seeds into account, too:
                    retValue = retValue + gameBoard.SumOfAllSeedsOfPlayer(Opponent(currentPlayer)) -
                                          gameBoard.SumOfAllSeedsOfPlayer(currentPlayer);
                }

                //                Logging.I.LogMessage("Minimizer on depth " + currentRecursionDepth + ": Pits empty. Returning " +
                //                                        retValue + "." + Environment.NewLine,
                //                                        Logging.LogLevel.DeepDebug);
                return retValue;
            }

            if (currentRecursionDepth == maxRecursionDepth)
            {
                // We have reached the "bottom" of the recursion.
                // --> Return the difference of the two Kalahs as the minimum value:
                int retValue = gameBoard.GetKalahOfPlayer(Opponent(currentPlayer)).GetNumberofSeeds() -
                               gameBoard.GetKalahOfPlayer(currentPlayer).GetNumberofSeeds();
                //                Logging.I.LogMessage("Minimizer on depth " + currentRecursionDepth + ": maxRecursionDepth " + maxRecursionDepth +
                //                                        " reached. Returning " + retValue + "." + Environment.NewLine,
                //                                        Logging.LogLevel.DeepDebug);
                return retValue;
            }

            // We are still in the recursion path and drill deeper down.
            int min = Basics.Infinity();

            for (int pitIndex = 0; pitIndex < gameBoard.GetNumOfHousesPerPlayer(); ++pitIndex)
            {
                // Create a copy of the current gameboard and take the copy to perform the move of one pit:
                Kalaha.Model.GameBoard localGameBoard = gameBoard.GetACopy();

                if (localGameBoard.GetPitOfPlayer(currentPlayer, pitIndex).ContainsASeed())
                {
                    // The pit of the current pitIndex has a seed in it, so perform the move now:
                    bool lastSeedFellIntoOwnKalah = false;
                    bool lastSeedFellIntoEmptyOwnPit = false;
                    int lastSeedsPit = -1;

                    //                    Logging.I.LogMessage("Minimizer on depth " + currentRecursionDepth + ": Moving pit " +
                    //                                            pitIndex + "." + Environment.NewLine,
                    //                                            Logging.LogLevel.DeepDebug);
                    localGameBoard.MoveHouse(currentPlayer, pitIndex,
                                           ref lastSeedFellIntoOwnKalah,
                                           ref lastSeedFellIntoEmptyOwnPit,
                                           ref lastSeedsPit,
                                           false);  // Do not generate the move's intermediate steps on a seed level

                    int opt = Basics.Infinity();
                    if (Rules.I.PlayAgainWhenLastSeedInOwnKalah() && lastSeedFellIntoOwnKalah)
                    {
                        // It is the same player's turn again.
                        // --> Continue with the minimizer recursion:
                        opt = Minimizer(localGameBoard, currentPlayer,
                                        currentRecursionDepth + 1, maxRecursionDepth);
                        //                        Logging.I.LogMessage("Minimizer on depth " + currentRecursionDepth + ": Got back opt " +
                        //                                                 opt + " from Minimizer one level deeper." + Environment.NewLine,
                        //                                                 Logging.LogLevel.DeepDebug);
                    }
                    else
                    {
                        // Check if the "winning opposite pit" rule applies:
                        if (lastSeedFellIntoEmptyOwnPit)
                        {
                            // The last seed fell into an own empty house. This may mean that we can execute the capture move.
                            int numOfSeedsOfOpponentsHouse = gameBoard.GetOppositeHouse(currentPlayer, lastSeedsPit).GetNumberofSeeds();
                            bool captureMoveMayBeExecuted = false;
                            bool captureOpponentsHouse = false;

                            // Find out if under the current rule setting a capture move may be executed and whether the opponent's house may be captured:
                            Rules.I.GetCapturePermissions(numOfSeedsOfOpponentsHouse, ref captureMoveMayBeExecuted, ref captureOpponentsHouse);

                            if (captureMoveMayBeExecuted)
                            {
                                //                                Logging.I.LogMessage("Minimizer on depth " + currentRecursionDepth +
                                //                                                       ": The last seed fell into the empty pit #" + (lastSeedsPit + 1) + ".\n",
                                //                                                       Logging.LogLevel.DeepDebug);
                                localGameBoard.PlayerWinsHouses(currentPlayer, lastSeedsPit, captureOpponentsHouse, false);
                            }
                            else
                            {
                                //                                Logging.I.LogMessage("Minimizer on depth " + currentRecursionDepth +
                                //                                                        ": The last seed fell into the empty house #" + (lastSeedsPit + 1) +
                                //                                                        " but rules prohibit the capture.\n",
                                //                                                        Logging.LogLevel.DeepDebug);
                            }
                        }

                        // It is the opponent's turn.
                        // --> Continue with the maximizer recursion:
                        opt = Maximizer(localGameBoard, Opponent(currentPlayer),
                                        currentRecursionDepth + 1, maxRecursionDepth);
                        //                        Logging.I.LogMessage("Minimizer on depth " + currentRecursionDepth + ": Got back opt " +
                        //                                                opt + " from Maximizer one level deeper." + Environment.NewLine,
                        //                                                Logging.LogLevel.DeepDebug);
                    }

                    if (opt < min)
                    {
                        // We have found a new minimum.
                        min = opt;
                        //                        Logging.I.LogMessage("Minimizer on depth " + currentRecursionDepth + ": New minimum found: " + min +
                        //                                                "." + Environment.NewLine,
                        //                                                Logging.LogLevel.DeepDebug);
                    }
                }
            }

            // Return the minimum we found across all possible moves:
            return min;
        }

        #endregion Computer calculates moves


        #region End of a Game

        /// <summary>
        /// Finalizes the result by collecting the last seeds (if the respective rule applies)..
        /// </summary>
        private void FinalizeResultAndShowWinner()
        {
            SetGameStatus(GameStatus.Ended);

            // Switch "off" the visual indicators of the player names:
            _view.SwitchOffAllVisualIndicators();

            if (Rules.I.CaptureSeedsAtEndOfGame())
            {
                // Move the rest of the seeds to the Kalah if the respective rule applies:
                Move finalMove = _modelGameBoard.CollectFinalSeeds(_southPlayer, _northPlayer);
                Logging.I.LogMessage("Final" + finalMove.ToString(), Logging.LogLevel.DeepDebug);

                if (finalMove.GetNumberOfSeedMovements() > 0)
                {
                    _undoManager.RememberFollowUpMove(finalMove, _currentPlayer);
//DEBUG             Logging.I.LogMessage(_undoManager.ToString());

                    _view.DisableButtonsOnPage();
                    _viewModelGameBoard.VisualizeMove(finalMove, ShowWinner, true);
                }
                else
                {
                    ShowWinner();
                }
            }
            else
            {
                ShowWinner();
            }
        }

        /// <summary>
        /// Shows the winner or whether the game has ended in a draw.
        /// </summary>
        private void ShowWinner()
        {
//DEBUG     Logging.I.LogMessage("Calling Presenter.ShowWinner().\n");

            _view.EnableButtonsOnPage();

            // Set all pits to "normal":
            _viewModelGameBoard.UnselectAllHouses();

            // Switch off all possibilities to select a house:
            _viewModelGameBoard.DisableAllHouses();

            // Show who has won or whether it's a draw:
            if (_modelGameBoard.GetKalahOfPlayer(_southPlayer).GetNumberofSeeds() > _modelGameBoard.GetKalahOfPlayer(_northPlayer).GetNumberofSeeds())
            {
                // South has won.
                _view.PrintFixedMsg("PlayerWon", _southPlayer, _southPlayer.GetName());
            }
            else
            {
                if (_modelGameBoard.GetKalahOfPlayer(_southPlayer).GetNumberofSeeds() < _modelGameBoard.GetKalahOfPlayer(_northPlayer).GetNumberofSeeds())
                {
                    // North has won.
                    _view.PrintFixedMsg("PlayerWon", _northPlayer, _northPlayer.GetName());
                }
                else
                {
                    // The game ended in a draw.
                    _view.PrintFixedMsg("GameEndedInDraw", _southPlayer);
                }
            }

            _view.EnablePlayAgainButton();
            // Debugging/Logging:
//DEBUG         Logging.Inst.LogMessage("Game over. Final result:\n", Logging.LogLevel.Info);
//DEBUG         _gameBoard.LogBoard(_southPlayer, _northPlayer, Logging.LogLevel.Info);
        }

        /// <returns>True if the game is over because one of the players has no pits left</returns>
        public bool GameIsOver()
        {
            return (_modelGameBoard.PlayerOnlyHasEmptyHouses(_southPlayer) ||
                    _modelGameBoard.PlayerOnlyHasEmptyHouses(_northPlayer));
        }

        #endregion End of a Game


        #region Undo

        /// <summary>
        /// "Undoes" the last move by revoking the gameboard of the previous status.
        /// This is only possible for the last move. A multiple "undo" is not implemented.
        /// </summary>
        public void UndoLastMove()
        {
            if (_view == null)
            {
                throw new PSTException("Presenter.UndoLastMove: View is undefined");
            }

            if (_undoManager.HasAnUndoMoveLeft())
            {
                // There is at least one "undo" move left.

                if (GetGameStatus() == GameStatus.Ended)
                {
                    // The game was completely over when the undo button was pressed. We need to revive it a bit.

//DEBUG             Logging.I.LogMessage("Presenter.UndoLastMove: Game had ended. Need to revive it.\n");

                    // Set the game status back to running:
                    SetGameStatus(GameStatus.Running);

                    // Empty the fixed message area on the game page:
                    _view.ClearFixedMsg();

                    // No "play again" button available anymore:
                    _view.DisablePlayAgainButton();
                }

                // Do not allow any button clicking during the undo moves:
                _view.DisableButtonsOnPage();

                // Get the next undo move from the UndoManager. Also get the player whose turn it is after the move:
                Move undoMove = null;
                Player playerAfterUndoMove = null;
                _undoManager.GetNextUndoMove(ref undoMove, ref playerAfterUndoMove);

                // Perform the undo move on the Model's game board:
                _modelGameBoard.PerformMove(undoMove);

                // Set the current player according to what the UndoManager told us:
                _currentPlayer = playerAfterUndoMove;
                _playerMayMoveAgain = true;  // This is because in "DecideWhoIsNext()" the players would be toggled if the value was false.

                // Set all houses to "unselected" as a selection does not make sense anymore:
                _viewModelGameBoard.UnselectAllHouses();

                // Set visual indicators correctly:
                _view.SwitchVisualIndicators(_currentPlayer);

                if (_currentPlayer.GetSpecies() == Player.Species.Computer)
                {
                    // The player whose turn it is after the undo move is a computer. Therefore, we have to call the undo method
                    // recursively to undo the next move after the visualization of the current undo move:
                    _viewModelGameBoard.VisualizeMove(undoMove, UndoLastMove, true);

                }
                else
                {
                    // Visualize the undo move on the ViewModel's game board and stop undoing by deciding who is next:
                    _viewModelGameBoard.VisualizeMove(undoMove, DecideWhoIsNext, true);
                }
            }
            else
            {
                // There is no undo move left (we may end up here because of a recursive call for this method before).

                // Decide which player has to continue. It may be the computer who immediately starts in this special case.yy
                DecideWhoIsNext();
            }

            _view.PrintFadingMsg("LastMoveUndone", _currentPlayer);
        }

        /// <summary>
        /// Returns true if there is still an undo move possible.
        /// </summary>
        /// <returns></returns>
        public bool UndoMoveIsLeft()
        {
            return _undoManager.HasAnUndoMoveLeft();
        }

        #endregion Undo


        #region Small helper methods

        /// <param name="userChoice">The user's choice, starting with 0.</param>
        /// <returns>True if the user's choice is semantically OK</returns>
        private bool UsersChoiceIsValid(int userChoice)
        {
            if (_view == null)
            {
                throw new PSTException("Presenter.UsersChoiceIsValid: View is undefined");
            }

            if ((userChoice < 0) || (userChoice >= _modelGameBoard.GetNumOfHousesPerPlayer()))
            {
                // How on earth this may be possible.
                return false;
            }

            if (_modelGameBoard.GetPitOfPlayer(_currentPlayer, userChoice).IsEmpty())
            {
                // The selected pit is empty.
                _view.PrintFadingMsg("EmptyPit", _currentPlayer);
                return false;
            }

            return true;
        }

        /// <returns>The opponent of the given player</returns>
        /// <param name="player">The player whose opponent is to be returned</param>
        /// <returns></returns>
        private Player Opponent(Player player)
        {
            if (player.GetPosition() == Player.Position.South)
            {
                return _northPlayer;
            }
            else
            {
                return _southPlayer;
            }
        }

        /// <summary>Toggles the current player to now be the other one.</summary>
        private void TogglePlayers()
        {
            if (_view == null)
            {
                throw new PSTException("Presenter.TogglePlayer: View is undefined");
            }

            _currentPlayer = Opponent(_currentPlayer);

            _view.SwitchVisualIndicators(_currentPlayer);
        }

        /// <summary>Returns the player who may start the game.</summary>
        public Player PlayerWhoIsFirst()
        {
            return _playerWhoIsFirst;
        }

        /// <summary>Toggles the player who may begin the game.</summary>
        public void TogglePlayerWhoIsFirst()
        {
            _playerWhoIsFirst = Opponent(_playerWhoIsFirst);
        }

        /// <summary>Clears everything up because the game shall be ended or interrupted.</summary>
        public void EndTheGame()
        {
            // Set the status of the game:
            if (!GameIsOver())
            {
                SetGameStatus(GameStatus.Interrupted);
            }
        }

        /// <summary>
        /// Highlights the given house.
        /// </summary>
        /// <param name="houseId">The house to highlight</param>
        public void HighlightHouse(int houseId)
        {
            _viewModelGameBoard.Highlight(houseId);
        }

        /// <summary>
        /// Unhighlights the given house.
        /// </summary>
        /// <param name="houseId">The house to unhighlight</param>
        public void UnHighlightHouse(int houseId)
        {
            _viewModelGameBoard.UnHighlight(houseId);
        }

        #endregion Small helper methods


        #region Getters and Setters

        /// <summary>
        /// Returns the status that the game is currently in.
        /// </summary>
        public GameStatus GetGameStatus()
        {
            return _gameStatus;
        }

        /// <summary>
        /// Sets the status that the current game is in.
        /// </summary>
        /// <param name="gameStatus">The new game status</param>
        public void SetGameStatus(GameStatus gameStatus)
        {
            _gameStatus = gameStatus;
            Logging.I.LogMessage("Switching to game status " + _gameStatus + ".\n", Logging.LogLevel.DeepDebug);
        }

        /// <returns>The GameBoard that is currently stored inside the presenter</returns>
        public Kalaha.Model.GameBoard GetGameBoard()
        {
            return _modelGameBoard;
        }

        /// <returns>The southern player</returns>
        public Player GetSouthernPlayer()
        {
            return _southPlayer;
        }

        /// <returns>The northern player</returns>
        public Player GetNorthernPlayer()
        {
            return _northPlayer;
        }

        /// <returns>True if south may play the first move</returns>
        public bool SouthPlaysFirstMove()
        {
            return (_playerWhoIsFirst == _southPlayer);
        }

        /// <returns>The name of the southern player</returns>
        public string GetSouthPlayersHumanName()
        {
            return _southPlayer.GetHumansName();
        }

        /// <summary>
        /// Sets the southern player's name.
        /// </summary>
        /// <param name="name">The name to be set</param>
        public void SetSouthPlayersName(string name)
        {
            _southPlayer.SetName(name);
        }

        /// <returns>The name of the northern player</returns>
        public string GetNorthPlayersHumanName()
        {
            return _northPlayer.GetHumansName();
        }

        /// <summary>
        /// Sets the northern player's name.
        /// </summary>
        /// <param name="name">The name to be set</param>
        public void SetNorthPlayersName(string name)
        {
            _northPlayer.SetName(name);
        }

        /// <summary>Sets a southern player's species to human.</summary>
        public void SetPlayerToHuman(int playerNumber)
        {
            Player player = null;
            switch (playerNumber)
            {
                case 0:
                    player = _southPlayer;
                    break;
                case 1:
                    player = _northPlayer;
                    break;
                default:
                    throw new PSTException("Presenter.SetPlayerToHuman: Unexpected player number: " + playerNumber);
            }
 
            player.SetSpecies(Player.Species.Human);
        }

        /// <summary>Sets a player's species to computer and sets the strength.</summary>
        /// <param name="playerNumber">0 for south, 1 for north</param>
        /// <param name="strength">1 for easy, 2 for medium, 3 for hard</param>
        public void SetPlayerToComputer(int playerNumber, int strength)
        {
            Player player = null;
            switch (playerNumber)
            {
                case 0:
                    player = _southPlayer;
                    break;
                case 1:
                    player = _northPlayer;
                    break;
                default:
                    throw new PSTException("Presenter.SetPlayerToComputer: Unexpected player number: " + playerNumber);
            }
            
            switch (strength)
            {
                case 0:
                    player.SetSpecies(Player.Species.Computer, Player.ComputerStrength.Easy);
                    break;
                case 1:
                    player.SetSpecies(Player.Species.Computer, Player.ComputerStrength.Medium);
                    break;
                case 2:
                    player.SetSpecies(Player.Species.Computer, Player.ComputerStrength.Hard);
                    break;
                default:
                    throw new PSTException("Presenter.SetPlayerToComputer: Unexpected value for strength: " + strength);
            }
        }

        /// <returns>True is the southern player is human</returns>
        public bool SouthPlayerIsHuman()
        {
            return (_southPlayer.GetSpecies() == Player.Species.Human);
        }

        /// <returns>True is the northern player is human</returns>
        public bool NorthPlayerIsHuman()
        {
            return (_northPlayer.GetSpecies() == Player.Species.Human);
        }

        /// <summary>
        /// Returns true if none of the two players is human.
        /// </summary>
        public bool NoPlayerIsHuman()
        {
            return ((_southPlayer.GetSpecies() == Player.Species.Computer) &&
                    (_northPlayer.GetSpecies() == Player.Species.Computer));
        }

        /// <summary>
        /// Returns true if both players are human.
        /// </summary>
        public bool BothPlayersAreHuman()
        {
            return ((_southPlayer.GetSpecies() == Player.Species.Human) &&
                    (_northPlayer.GetSpecies() == Player.Species.Human));
        }

        /// <summary>
        /// Returns true if the tablet mode is active and both players are human.
        /// </summary>
        public bool TabletModeIsActive()
        {
            return ((Settings.I.GetGameDevice() == Settings.GameDevice.Tablet) && BothPlayersAreHuman());
        }

        /// <summary>
        /// Returns the player whose turn it is.
        /// </summary>
        public Player GetCurrentPlayer()
        {
            return _currentPlayer;
        }

        /// <summary>
        /// Returns true if the given player is human.
        /// </summary>
        /// <param name="playerNumber">0 for south, 1 for north</param>
        /// <returns></returns>
        public bool PlayerIsHuman(int playerNumber)
        {
            switch (playerNumber)
            {
                case 0:
                    return SouthPlayerIsHuman();
                case 1:
                    return NorthPlayerIsHuman();
                default:
                    throw new PSTException("Presenter.PlayerIsHuman: Unknown playerNumber " + playerNumber);
            }
        }

        /// <returns>The strength of the southern computer</returns>
        public Player.ComputerStrength GetSouthernComputerStrength()
        {
            return (_southPlayer.GetComputerStrength());
        }

        /// <returns>The strength of the southern computer</returns>
        public Player.ComputerStrength GetNorthernComputerStrength()
        {
            return (_northPlayer.GetComputerStrength());
        }

        /// <summary>Instantiates the Presenter.</summary>
        public void Instantiate()
        {
            // ... actually by doing nothing special.
            // By calling this function, the Presenter's constructor is called.
        }

        /// <summary>
        /// Set the flag whether or not the HubPage shall explain the theme button click after reload.
        /// </summary>
        public void SetFlagToShowThemeExplanation(bool tell)
        {
            _tellHubPageToShowThemeExplanation = tell;
            Kalaha.View.Model.Seed.Initialize();
        }

        /// <summary>
        /// Returns the flag whether or not the HubPage shall explain the theme button click after reload.
        /// </summary>
        public bool FlagToShowThemeExplanationIsSet()
        {
            return _tellHubPageToShowThemeExplanation;
        }
        
        /// <summary>The single instance that this class provides</summary>
        /// <returns>The single instance of the class</returns>
        public static Presenter I
        {
            // If the single instance has not yet been created, yet, creates the instance.
            // This approach ensures that only one instance is created and only when the instance is needed.
            // This approach uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
            // This double-check locking approach solves the thread concurrency problems while avoiding an exclusive
            // lock in every call to the Instance property method. It also allows you to delay instantiation
            // until the object is first accessed.
            get
            {
                if (_inst == null)
                {
                    lock (_syncRoot)
                    {
                        if (_inst == null)
                        {
                            _inst = new Presenter();
                        }
                    }
                }

                return _inst;
            }
        }

        #endregion Getters and Setters
    }
}
