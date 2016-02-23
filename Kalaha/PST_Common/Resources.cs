//
// Helper_Resources
//
// Class Resources handles the possibility of globalization, implemented as a thread-safe singleton.
// We distinguish between resources (i.e., parts of the UI that get different tags in different languages)
// and messages (i.e. the stuff that pops up during the execution of the app).
//
// Additionally, the Resources handle constants of directory and file names.

using System;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;

namespace PST_Common
{

    public sealed class KalahaResources
    {
        // --- Attributes of the class ---

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile KalahaResources _inst;
        private static object _syncRoot = new Object();

        /// <summary>The resource loader, refering to the UI parts of the resources</summary>
        private ResourceLoader _resLoader;

        /// <summary>The resource loader for messages, refering to the message parts of the resources</summary>
        private ResourceLoader _msgLoader;

        /// <summary>
        /// The resource loader for the specifics of layouts, especially the field of the Kalaha gameboards
        /// for the various themes.
        /// </summary>
        private ResourceLoader _layoutResLoader;

        /// <summary>The root directory of all themes sub-directories</summary>
        private const string _themesRootDirName = "Themes";

        /// Some constants of image file names
        private const string _normalHouseFileName = "HouseNormal.png";
        private const string _selectedHouseFileName = "HouseSelected.png";
        private const string _highlightedHouseFileName = "HouseHighlighted.png";
        private const string _normalKalahFileName = "KalahNormal.png";
        private const string _seedFileName = "Seed_{0}.png";
        private const string _indicatorOnFileName = "IndicatorOn.png";
        private const string _indicatorOffFileName = "IndicatorOff.png";
        private const string _gamePageBackgroundFileName = "Background_GamePage.png";
        private const string _seedMoveSoundFileName = "SeedMoveSound.wav";


        // --- Methods of the class ---

        /// <summary>The constructor of the class. This is declared private because only
        /// the Instance property method may call it.</summary>
        private KalahaResources()
        {
            _resLoader = new ResourceLoader("Resources");
            _msgLoader = new ResourceLoader("Messages");

            // Define the standard theme to be the initial theme:
            ChangeLayoutResources(GetRes("Theme_HighContrast"));
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
        public static KalahaResources I
        {
            get
            {
                if (_inst == null)
                {
                    lock (_syncRoot)
                    {
                        if (_inst == null)
                            _inst = new KalahaResources();
                    }
                }

                return _inst;
            }
        }

        /// <summary>Finds a string in the resource table for the given key.</summary>
        /// The message that is found "behind" the key may contain parameter arguments of the form "{0}, {1}, etc.".
        /// <param name="messageKey">The key for which the entry in the resources shall be found</param>
        /// <param name="args">Zero or more arguments that are to be added in the message</param>
        /// <returns>The string that can be found "behind" the given key, including the given additional arguments</returns>
        public String GetMsg(String messageKey, params object[] args)
        {
            return String.Format(_msgLoader.GetString(messageKey), args);
        }

        /// <summary>Finds a string in the resource table for the given key.</summary>
        /// The resource that is found "behind" the key may contain parameter arguments of the form "{0}, {1}, etc.".
        /// <param name="resourceKey">The key for which the entry in the resources shall be found</param>
        /// <param name="args">Zero or more arguments that are to be added in the message</param>
        /// <returns>The string that can be found "behind" the given key, including the given additional arguments</returns>
        public String GetRes(String resourceKey, params object[] args)
        {
            return String.Format(_resLoader.GetString(resourceKey), args);
        }

        /// <summary>Finds a string in the resource table for the given key.</summary>
        /// The resource that is found "behind" the key may contain parameter arguments of the form "{0}, {1}, etc.".
        /// <param name="resourceKey">The key for which the entry in the resources shall be found</param>
        /// <param name="args">Zero or more arguments that are to be added in the message</param>
        /// <returns>The string that can be found "behind" the given key, including the given additional arguments</returns>
        public String GetLayoutRes(String resourceKey, params object[] args)
        {
            return String.Format(_layoutResLoader.GetString(resourceKey), args);
        }

/*
        /// <summary>
        /// Creates an (X, Y)-point from the given entry of the resource file.
        /// </summary>
        /// <param name="resourceKey">The resource key which represents the point in the resource file</param>
        /// <returns>The corresponting point that was found "behind" the given resource key</returns>
        public Point GetLayoutPoint(String resourceKey)
        {
            // Get the point from the resource file in the form "x,y":
            string pointAsString = GetLayoutRes(resourceKey);
            if (pointAsString == "")
            {
                throw new PSTException("GetLayoutPoint: Point \"" + resourceKey + "\" not found.");
            }

            int indexOfComma = pointAsString.IndexOf(",");
            if(indexOfComma == -1)
            {
                throw new PSTException("Illegal format of point in layout resource file: " + pointAsString +
                                            ". Please use form \"x,y\" without any other characters.");
            }
            string x = pointAsString.Substring(0, indexOfComma);
            string y = pointAsString.Substring(indexOfComma+1, pointAsString.Length-x.Length-1);
            Point point = new Point(Convert.ToInt32(x), Convert.ToInt32(y));

            return point;
        }
*/

        /// <param name="resourceKey">The resource key which represents the value in the resource file</param>
        /// <returns>The corresponting value that was found "behind" the given resource key</returns>
        public int GetLayoutValue(String resourceKey)
        {
            // Get the point from the resource file in the form "x,y":
            string valAsString = GetLayoutRes(resourceKey);
            if (valAsString == "")
            {
                throw new PSTException("GetLayoutValue: Value \"" + resourceKey + "\" not found.");
            }
            return Convert.ToInt32(valAsString);
        }

        /// <returns>The name of the root directory of all themes</returns>
        public string GetThemesRootDirName()
        {
            return _themesRootDirName;
        }

        /// <summary>
        /// Switches to the given resource file in order to handle a different theme.
        /// </summary>
        /// <param name="resFileName">The file name of the resource to switch to</param>
        public void ChangeLayoutResources(String resFileName)
        {
            // TODO: Would be nice to have a "ThemeData.resw" file for each theme without the theme's name in the file name.
            string resourceFile = "ThemeData_" + resFileName;
            _layoutResLoader = new ResourceLoader(resourceFile);
        }

        public string GetNormalHouseFileName()
        {
            return _normalHouseFileName;
        }

        public string GetSelectedHouseFileName()
        {
            return _selectedHouseFileName;
        }

        public string GetHighlightedHouseFileName()
        {
            return _highlightedHouseFileName;
        }

        public string GetNormalKalahFileName()
        {
            return _normalKalahFileName;
        }

        public string GetSeedFileName(int seedImageId)
        {
            return String.Format(_seedFileName, seedImageId);
        }

        public string GetIndicatorOnFileName()
        {
            return _indicatorOnFileName;
        }

        public string GetIndicatorOffFileName()
        {
            return _indicatorOffFileName;
        }

        public string GetGamePageBackgroundFileName()
        {
            return _gamePageBackgroundFileName;
        }

        public string GetSeedMoveSoundFileName()
        {
            return _seedMoveSoundFileName;
        }
    }
}

