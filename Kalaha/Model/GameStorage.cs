//
// GameStorage
// 
// Class GameStorage handles the storage and reading of the storage of all kinds of data (settings, players, etc.).
// It is implemented as a thread-safe singleton.
//

using PST_Common;
using System;
using Windows.Storage;

namespace Kalaha.Model
{
    public sealed class GameStorage
    {
        // --- Attributes of the class ---

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile GameStorage _inst;
        private static object _syncRoot = new Object();

        // For each group of stuff to store, a specific name is defined:

        /// <summary>The storage for the currently selected theme</summary>
        private const string _themeStorageName = "KalahaStorage_Theme";

        /// <summary>The storage for the stuff that is kept in the settings</summary>
        private const string _settingsStorageName = "KalahaStorage_Settings";

        /// <summary>The storage for the stuff that is kept in the players</summary>
        private const string _playersStorageName = "KalahaStorage_Players";

        /// <summary>The storage for the stuff that is kept in the rules</summary>
        private const string _rulesStorageName = "KalahaStorage_Rules";

        /// <summary>Keys for storing the theme</summary>
        const string _themesKey_themeName = "themeName";

        /// <summary>Keys for storing the settings</summary>
        const string _settingsKey_numOfHouses = "numOfHouses";
        const string _settingsKey_numOfInitialSeeds = "numOfInitialKeys";
        const string _settingsKey_gameDevice = "gameDevice";
        const string _settingsKey_numberFieldsOnOff = "numberFieldsOnOff";

        /// <summary>Keys for storing the players</summary>
        const string _playersKey_humanNameSouth = "humanNameSouth";
        const string _playersKey_humanNameNorth = "humanNameNorth";
        const string _playersKey_speciesSouth = "speciesSouth";
        const string _playersKey_speciesNorth = "speciesNorth";
        const string _playersKey_computerStrengthSouth = "computerStrengthSouth";
        const string _playersKey_computerStrengthNorth = "computerStrengthNorth";

        /// <summary>Keys for storing the rules</summary>
        const string _rulesKey_captureSeedsAtEndOfGame = "captureSeedsAtEndOfGame";
        const string _rulesKey_captureType = "captureType";
        const string _rulesKey_directionOfSowing = "directionOfSowing";

        /// <summary>The place to save the settings</summary>
        private ApplicationDataContainer _roamingSettings = null;


        // --- Methods of the class ---

        /// <summary>The constructor of the class. This is declared private because only
        /// the Instance property method may call it.</summary>
        private GameStorage()
        {
            _roamingSettings = ApplicationData.Current.RoamingSettings;
        }

        /// <summary>
        /// Saves all settings.
        /// </summary>
        public void Save(Player southPlayer, Player northPlayer, string currentThemeName)
        {
            SaveThemesValues(currentThemeName);
            SaveSettingsValues();
            SavePlayersValues(southPlayer, northPlayer);
            SaveRulesValues();
        }

        /// <summary>
        /// Restores previously saved values in the respective objects. If no saved values could be found, the defaults are set.
        /// </summary>
        public void RestoreSavedValues(ref Player southPlayer, ref Player northPlayer, ref string themeName)
        {
            RestoreThemesValues(ref themeName);
            RestoreSettingsValues();
            RestorePlayersValues(ref southPlayer, ref northPlayer);
            RestoreRulesValues();
        }

        /// <summary>
        /// Saves the stuff that is stored for the current theme.
        /// </summary>
        private void SaveThemesValues(string currentThemeName)
        {
            ApplicationDataCompositeValue themeGroup_GameStorage = new ApplicationDataCompositeValue();
            themeGroup_GameStorage[_themesKey_themeName] = currentThemeName;

            _roamingSettings.Values[_themeStorageName] = themeGroup_GameStorage;
        }

        /// <summary>
        /// Restores the stuff that is stored for the theme.
        /// If no saved data can be found, "" is returned in the reference parameter.
        /// </summary>
        private void RestoreThemesValues(ref string themeName)
        {
            // Set the reference parameter to default:
            themeName = "";

            // Retrieve the settings from the ApplicationData.Current.RoamingGameStorage:
            ApplicationDataCompositeValue themeGroup_GameStorage = (ApplicationDataCompositeValue)_roamingSettings.Values[_themeStorageName];
            if (themeGroup_GameStorage != null)
            {
                try
                {
                    // We found some settings!
//DEBUG             Logging.I.LogMessage("GameStorage.RestoreThemesValues: Found saved data.\n");

                    // Save back the found settings:
                    themeName = (string)themeGroup_GameStorage[_themesKey_themeName];
                }
                catch (Exception ex)
                {
                    // In the case of not reading the settings correctly the last resort is to set the values to the defaults:
                    Logging.I.LogMessage("GameStorage.RestoreThemesValues: Exception: " + ex.Message + "\n", Logging.LogLevel.Error);
                    themeName = "";
                }
            }
            else
            {
                // No saved settings could be found -> Take the defaults:
//DEBUG         Logging.I.LogMessage("GameStorage.RestoreThemesValues: No saved data found.\n");
                themeName = "";
            }
        }


        /// <summary>
        /// Saves the stuff that is stored in the settings.
        /// </summary>
        private void SaveSettingsValues()
        {
            ApplicationDataCompositeValue settingGroup_GameStorage = new ApplicationDataCompositeValue();
            settingGroup_GameStorage[_settingsKey_numOfHouses] = Settings.I.GetNumberOfHousesPerPlayer();
            settingGroup_GameStorage[_settingsKey_numOfInitialSeeds] = Settings.I.GetNumberOfSeedsPerHouse();
            settingGroup_GameStorage[_settingsKey_gameDevice] = (int)Settings.I.GetGameDevice();
            settingGroup_GameStorage[_settingsKey_numberFieldsOnOff] = (int)(Settings.I.NumberFieldsShallBeShown() ? 1 : 0);

            _roamingSettings.Values[_settingsStorageName] = settingGroup_GameStorage;
        }
        
        /// <summary>
        /// Restores the stuff that is stored in the settings.
        /// If no saved data can be found, the defaults are set.
        /// </summary>
        private void RestoreSettingsValues()
        {
            // Retrieve the settings from the ApplicationData.Current.RoamingGameStorage:
            ApplicationDataCompositeValue settingGroup_GameStorage = (ApplicationDataCompositeValue)_roamingSettings.Values[_settingsStorageName];
            if (settingGroup_GameStorage != null)
            {
                try
                {
                    // We found some settings!
//DEBUG             Logging.I.LogMessage("GameStorage.RestoreSettingsValues: Found saved data.\n");

                    // Save back the found settings:
                    Settings.I.SetNumberOfHousesPerPlayer((int)settingGroup_GameStorage[_settingsKey_numOfHouses]);
                    Settings.I.SetNumberOfSeedsPerHouse((int)settingGroup_GameStorage[_settingsKey_numOfInitialSeeds]);
                    Settings.I.SetGameDevice((Settings.GameDevice)settingGroup_GameStorage[_settingsKey_gameDevice]);
                    Settings.I.SetNumberFieldsOnOff(((int)settingGroup_GameStorage[_settingsKey_numberFieldsOnOff]) == 1);
                }
                catch (Exception ex)
                {
                    // In the case of not reading the settings correctly the last resort is to set the values to the defaults:
                    Logging.I.LogMessage("GameStorage.RestoreSettingsValues: Exception: " + ex.Message + "\n", Logging.LogLevel.Error);
                    Settings.I.SetDefaults();
                }
            }
            else
            {
                // No saved settings could be found -> Take the defaults:
//DEBUG         Logging.I.LogMessage("GameStorage.RestoreSettingsValues: No saved data found.\n");
                Settings.I.SetDefaults();
            }
        }

        /// <summary>
        /// Saves the stuff that is stored in the players' settings.
        /// </summary>
        private void SavePlayersValues(Player southPlayer, Player northPlayer)
        {
            ApplicationDataCompositeValue playersGroup_GameStorage = new ApplicationDataCompositeValue();

            // Save the human player's names:
            playersGroup_GameStorage[_playersKey_humanNameSouth] = southPlayer.GetHumansName();
            playersGroup_GameStorage[_playersKey_humanNameNorth] = northPlayer.GetHumansName();

            // Save the currently set species:
            playersGroup_GameStorage[_playersKey_speciesSouth] = (int)southPlayer.GetSpecies();
            playersGroup_GameStorage[_playersKey_speciesNorth] = (int)northPlayer.GetSpecies();

            // Save the currently set computer's strength:
            playersGroup_GameStorage[_playersKey_computerStrengthSouth] = (int)southPlayer.GetComputerStrength();
            playersGroup_GameStorage[_playersKey_computerStrengthNorth] = (int)northPlayer.GetComputerStrength();

            // Save all stuff at once now:
            _roamingSettings.Values[_playersStorageName] = playersGroup_GameStorage;
        }

        /// <summary>
        /// Restores the stuff that is stored for the players.
        /// If no saved data can be found or an exception occurs, nothing is done and the current settings of the players are kept unchanged.
        /// </summary>
        private void RestorePlayersValues(ref Player southPlayer, ref Player northPlayer)
        {
            // Retrieve the settings from the ApplicationData.Current.RoamingGameStorage:
            ApplicationDataCompositeValue playersGroup_GameStorage = (ApplicationDataCompositeValue)_roamingSettings.Values[_playersStorageName];
            if (playersGroup_GameStorage != null)
            {
                try
                {
                    // We found some settings!
//DEBUG             Logging.I.LogMessage("GameStorage.RestorePlayersValues: Found saved data.\n");

                    // Restore the human player's names:
                    southPlayer.SetName((string)playersGroup_GameStorage[_playersKey_humanNameSouth]);
                    northPlayer.SetName((string)playersGroup_GameStorage[_playersKey_humanNameNorth]);

                    // Restore the species and the computer's strength:
                    southPlayer.SetSpecies((Player.Species)playersGroup_GameStorage[_playersKey_speciesSouth],
                                           (Player.ComputerStrength)playersGroup_GameStorage[_playersKey_computerStrengthSouth]);
                    northPlayer.SetSpecies((Player.Species)playersGroup_GameStorage[_playersKey_speciesNorth],
                                           (Player.ComputerStrength)playersGroup_GameStorage[_playersKey_computerStrengthNorth]);
                }
                catch (Exception ex)
                {
                    // In the case of not reading the player settings correctly we do nothing:
                    Logging.I.LogMessage("GameStorage.RestorePlayersValues: Exception: " + ex.Message + "\n", Logging.LogLevel.Error);
                }
            }
            else
            {
//DEBUG         Logging.I.LogMessage("GameStorage.RestorePlayersValues: No saved data found.\n");
            }
        }

        /// <summary>
        /// Saves the stuff that is stored in the rules' settings.
        /// </summary>
        private void SaveRulesValues()
        {
            ApplicationDataCompositeValue rulesGroup_GameStorage = new ApplicationDataCompositeValue();

            // Save the currently used rules:
            rulesGroup_GameStorage[_rulesKey_captureSeedsAtEndOfGame] = (Rules.I.CaptureSeedsAtEndOfGame() ? 1 : 0);
            rulesGroup_GameStorage[_rulesKey_captureType] = (int)Rules.I.GetCaptureType();
            rulesGroup_GameStorage[_rulesKey_directionOfSowing] = (int)Rules.I.GetDirectionOfSowing();

            // Save all stuff at once now:
            _roamingSettings.Values[_rulesStorageName] = rulesGroup_GameStorage;
        }

        /// <summary>
        /// Restores the stuff that is stored in the rules.
        /// If no saved data can be found or an exception occurs, the defaults are set.
        /// </summary>
        private void RestoreRulesValues()
        {
            // Retrieve the settings from the ApplicationData.Current.RoamingGameStorage:
            ApplicationDataCompositeValue rulesGroup_GameStorage = (ApplicationDataCompositeValue)_roamingSettings.Values[_rulesStorageName];
            if (rulesGroup_GameStorage != null)
            {
                try
                {
                    // We found some settings!
//DEBUG             Logging.I.LogMessage("GameStorage.RestoreRulesValues: Found saved data.\n");

                    // Save back the found settings:
                    Rules.I.SetCaptureSeedsAtEndOfGame((int)rulesGroup_GameStorage[_rulesKey_captureSeedsAtEndOfGame] == 1);
                    Rules.I.SetCaptureType((Rules.CaptureType)rulesGroup_GameStorage[_rulesKey_captureType]);
                    Rules.I.SetDirectionOfSowing((Rules.DirectionOfSowing)rulesGroup_GameStorage[_rulesKey_directionOfSowing]);
                }
                catch (Exception ex)
                {
                    // In the case of not reading the settings correctly the last resort is to set the values to the defaults:
                    Logging.I.LogMessage("GameStorage.RestoreRulesValues: Exception: " + ex.Message + "\n", Logging.LogLevel.Error);
                    Rules.I.SetDefaults();
                }
            }
            else
            {
                // No saved settings could be found -> Take the defaults:
//DEBUG         Logging.I.LogMessage("GameStorage.RestoreRulesValues: No saved data found.\n");
                Rules.I.SetDefaults();
            }
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
        public static GameStorage I
        {
            get
            {
                if (_inst == null)
                {
                    lock (_syncRoot)
                    {
                        if (_inst == null)
                            _inst = new GameStorage();
                    }
                }

                return _inst;
            }
        }
    }
}
