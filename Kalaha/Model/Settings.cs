//
// Settings
// 
// Class Settings handles the settings that is read from a file and set by the users.
// It is implemented as a thread-safe singleton.
//

using PST_Common;
using System;

namespace Kalaha.Model
{
    public sealed class Settings
    {
        // --- Enums ---

        /// <summary>
        /// The device that is used to play the game.
        /// </summary>
        public enum GameDevice
        {
            Tablet = 0,     // The game is played on a tablet computer.
            PC = 1          // The game is played on an old-school PC.
        }


        // --- Attributes of the class ---

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile Settings _inst;
        private static object _syncRoot = new Object();

        /// <summary>Maximum numbers</summary>
        const int _maxNumHouses = 7;
        const int _maxNumInitialSeeds = 7;

        /// <summary>Number of houses per player</summary>
        private int _numHousesPerPlayer;

        /// <summary>Initial number of seeds per house</summary>
        private int _numSeedsPerHouse;

        /// <summary>The device that is used to play this game.</summary>
        private GameDevice _gameDevice;

        /// <summary>
        /// Flag whether or not the number fields on the game page are visible.
        /// </summary>
        private bool _numberFieldsShallBeShown;

        /// <summary>The settings' name in the registry</summary>
        private const string _settingsName = "KalahaSettings_v1.2";

        /// <summary>Keys for storing the settings</summary>
        const string _numOfHousesKey = "numOfHouses";
        const string _numOfInitialSeedsKey = "numOfInitialKeys";


        /// <summary>
        /// The recursion depths for the computer's calculations as a 2-dimensional matrix.
        /// First dimension: Computer strength (0..2)
        /// Second dimension: Number of houses per player (3..7)
        /// </summary>
        private int[,] _recursionDepth;


        // --- Methods of the class ---

        /// <summary>The constructor of the class. This is declared private because only
        /// the Instance property method may call it.</summary>
        private Settings()
        {
            SetDefaults();
        }

        /// <summary>Sets the default values for all setting parameters.</summary>
        public void SetDefaults()
        {
            _numHousesPerPlayer = 6;
            _numSeedsPerHouse = 4;
            _gameDevice = GameDevice.PC;
            _numberFieldsShallBeShown = true;

            // The number of gameboards to evaluate at a maximum is numEvals = m^n where m is the number of houses
            // and n is the recursion depth.
            // Ideally, the chosen recursion depth should be independent from the number of houses. Practically, we
            // need to be careful with a too deep recursion for a high number of houses. Therefore, on the harder levels
            // the computer plays better with a decreasing number of houses:
            _recursionDepth = new int[3, 8]
            {
                { -1, -1, -1,  3,  3, 3, 3, 3 },        // Easy computer opponent
                { -1, -1, -1,  6,  6, 6, 6, 5 },        // Medium computer opponent
                { -1, -1, -1, 15, 11, 10, 9, 8 }        // Hard computer opponent
            };
        }

        public void SetNumberOfHousesPerPlayer(int num)
        {
            _numHousesPerPlayer = num; 
        }

        public int GetNumberOfHousesPerPlayer()
        {
            return _numHousesPerPlayer;
        }

        public void SetNumberOfSeedsPerHouse(int num)
        {
            _numSeedsPerHouse = num;
        }

        public int GetNumberOfSeedsPerHouse()
        {
            return _numSeedsPerHouse;
        }

        public void SetGameDevice(GameDevice gameDevice)
        {
            _gameDevice = gameDevice;
        }

        /// <summary>
        /// Toggles the current game device to the respective other possibility.
        /// </summary>
        public void ToggleGameDevice()
        {
            if (_gameDevice == GameDevice.PC)
            {
                _gameDevice = GameDevice.Tablet;
            }
            else
            {
                _gameDevice = GameDevice.PC;
            }
        }

        public GameDevice GetGameDevice()
        {
            return _gameDevice;
        }

        /// <summary>
        /// Toggles the flag for the number fields from on to off and vice versa.
        /// </summary>
        public void ToggleNumberFieldsOnOff()
        {
            _numberFieldsShallBeShown = (!_numberFieldsShallBeShown);
        }

        public void SetNumberFieldsOnOff(bool onOrOff)
        {
            _numberFieldsShallBeShown = onOrOff;
        }

        public bool NumberFieldsShallBeShown()
        {
            return _numberFieldsShallBeShown;
        }

        public int GetMaxNumberOfHouses()
        {
            return _maxNumHouses;
        }

        /// <summary>
        /// Returns the recursion depth for the calculation of a move, depending on the computer's strength and the number of houses
        /// </summary>
        /// <param name="compStrength">The computer's strength</param>
        /// <param name="numOfHouses">The number of houses per player (may have values 3 through 7)</param>
        /// <returns></returns>
        public int GetRecursionDepth(Player.ComputerStrength compStrength, int numOfHouses)
        {
            if ((numOfHouses < 3) || (numOfHouses > 7))
            {
                throw new PSTException("Settings.RecursionDepth: Unexpected numOfHouses: " + numOfHouses);
            }

            return _recursionDepth[(int)compStrength, numOfHouses];
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
        public static Settings I
        {
            get
            {
                if (_inst == null)
                {
                    lock (_syncRoot)
                    {
                        if (_inst == null)
                            _inst = new Settings();
                    }
                }

                return _inst;
            }
        }
    }
} // namespace Kalaha
