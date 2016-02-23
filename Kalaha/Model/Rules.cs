//
// Rules
//
// Represents the rules in the game.
// The Rules class is implemented as a thread-safe singleton. Only one instance of the class can be used.
//


namespace Kalaha.Model
{
    using System;

    public sealed class Rules
    {
        // --- Enums ---

        /// <summary>The direction of sowing defines in which direction the moves go.
        /// CrossKalah is a special rule that was invented by W. Dan Troyka in 2001:
        /// If a pit contains an odd number of seeds then these are distributed clockwise while pits with an even
        /// number continue to be sown counterclockwise.
        /// </summary>
        public enum DirectionOfSowing
        {
            Counterclockwise = 0,
            Clockwise = 1,
            CrossKalah = 2
        }
        private const int _numberOfSowingDirections = 3;

        /// <summary>
        /// The way a capture move is done.
        /// </summary>
        public enum CaptureType
        {
            StandardCapture = 0,   // When a player ends his move in an empty house and there are seeds in the opposite house, all seeds are captured (incl. the own seed)
            EmptyCapture = 1,      // Only the player's own seed is captured
            NoCapture = 2,         // Seeds are never captured
            AlwaysCapture = 3      // Like standard capture, but the player's own seed is captured also when there are no seeds in the opposite house
        }
        private const int _numberOfCaptureTypes = 4;

        // --- Attributes of the class ---

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile Rules _inst;
        private static object _syncRoot = new Object();

        /// <summary>Flag of the rule that a player may play again if the last seed falls into their own Kalah.</summary>
        private bool _playAgainWhenLastSeedInOwnKalah;

        /// <summary>When the last move has been done, do the seeds in the pits of the opponent count or not?</summary>
        private bool _captureSeedsAtEndOfGame;

        /// <summary>The type of capture that is currently selected.</summary>
        private CaptureType _captureType;

        /// <summary>
        /// Flag saying whether or not the "Pie Rule" is enabled. If it is enabled,
        /// after the first player makes their first move, the second player has the option of either:
        ///   - Letting the move stand, in which case they are the second player and move immediately, or
        ///   - Switching places, in which case they are now the first player, and the "new" second player
        ///     now makes their "first" move. Effectively, the second player becomes the first player,
        ///     and it is as if that move was theirs; the game proceeds from that opening move with the
        ///     newly reversed roles.
        /// </summary>
        private bool _pieRuleIsEnabled;

        /// <summary>Flag that defines the direction of sowing</summary>
        private DirectionOfSowing _directionOfSowing;

        // --- Methods of the class ---

        /// <summary>The constructor of the Rules class. This is declared private because only
        /// the Instance property method may call it.</summary>
        private Rules()
        {
            SetDefaults();
        }

        /// <summary>Sets the default values for all rules.</summary>
        public void SetDefaults()
        {
            _playAgainWhenLastSeedInOwnKalah = true;
            _captureSeedsAtEndOfGame = true;
            _captureType = CaptureType.StandardCapture;
            _pieRuleIsEnabled = false;
            _directionOfSowing = DirectionOfSowing.Counterclockwise;
        }

/*
        public void SetPlayAgainWhenLastSeedInOwnKalah(bool playAgainWhenLastSeedInOwnKalah)
        {
            _playAgainWhenLastSeedInOwnKalah = playAgainWhenLastSeedInOwnKalah;
        }
*/

        /// <summary>Currently always true.</summary>
        /// <returns>True</returns>
        public bool PlayAgainWhenLastSeedInOwnKalah()
        {
            return _playAgainWhenLastSeedInOwnKalah;
        }

        public void SetCaptureSeedsAtEndOfGame(bool isEnabled)
        {
            _captureSeedsAtEndOfGame = isEnabled;
        }

        public bool CaptureSeedsAtEndOfGame()
        {
            return _captureSeedsAtEndOfGame;
        }

        public void SetCaptureType(CaptureType captureType)
        {
            _captureType = captureType;
        }

        public CaptureType GetCaptureType()
        {
            return _captureType;
        }

        public int GetNumberOfCaptureTypes()
        {
            return _numberOfCaptureTypes;
        }

        /// <summary>
        /// Returns in the reference parameters whether or not a capture move may be executed and (if that's true) whether the opponent's house may
        /// be captured, too.
        /// </summary>
        /// <param name="numOfSeedsOfOpponentsHouse">The number of seeds in the opponent's house directly opposite</param>
        /// <param name="captureMoveMayBeExecuted">True if the capture move may be exectured</param>
        /// <param name="captureOpponentsHouse">True if the opponent's house may be captured, too</param>
        public void GetCapturePermissions(int numOfSeedsOfOpponentsHouse,
                                           ref bool captureMoveMayBeExecuted,
                                           ref bool captureOpponentsHouse)
        {
            switch (_captureType)
            {
                case Rules.CaptureType.StandardCapture:
                    if (numOfSeedsOfOpponentsHouse > 0)
                    {
                        captureMoveMayBeExecuted = true;
                        captureOpponentsHouse = true;
                    }
                    else
                    {
                        captureMoveMayBeExecuted = false;
                        captureOpponentsHouse = false;
                    }
                    break;

                case Rules.CaptureType.NoCapture:
                    captureMoveMayBeExecuted = false;
                    captureOpponentsHouse = false;
                    break;

                case Rules.CaptureType.AlwaysCapture:
                    captureMoveMayBeExecuted = true;
                    captureOpponentsHouse = (numOfSeedsOfOpponentsHouse > 0);
                    break;

                case Rules.CaptureType.EmptyCapture:
                    captureMoveMayBeExecuted = true;
                    captureOpponentsHouse = false;
                    break;
            }
        }

        public void SetPieRuleIsEnabled(bool isEnabled)
        {
            _pieRuleIsEnabled = isEnabled;
        }

        public bool PieRuleIsEnabled()
        {
            return _pieRuleIsEnabled;
        }

        public void SetDirectionOfSowing(DirectionOfSowing directionOfSowing)
        {
            _directionOfSowing = directionOfSowing;
        }

        public DirectionOfSowing GetDirectionOfSowing()
        {
            return _directionOfSowing;
        }

        public int GetNumberOfSowingDirections()
        {
            return _numberOfSowingDirections;
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
        public static Rules I
        {
            get
            {
                if (_inst == null)
                {
                    lock (_syncRoot)
                    {
                        if (_inst == null)
                            _inst = new Rules();
                    }
                }

                return _inst;
            }
        }
    }
}
