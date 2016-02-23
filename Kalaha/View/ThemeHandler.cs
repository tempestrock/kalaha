//
// ThemeHandler
//
// A class that handles the skinning, i.e. the change of themes thoughout the game's pages.
// This class is implemented as a singleton.
//
// Many thanks to Jerry Nixon for his great walkthrough on how to dynamically skin your app!
//


using PST_Common;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Kalaha.View
{
    public sealed class ThemeHandler
    {
       // --- Enums ---

        /// <summary>The game's possible themes</summary>
        public enum KalahaTheme { Wood, Spring, SummerOnTheBeach, HighContrast }


        // --- Attributes of the class ---

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile ThemeHandler _inst;
        private static object _syncRoot = new Object();

        /// <summary>The dictionary that stores the URIs of the "ResourceDictionary"s corresponding to the themes</summary>
        private Dictionary<KalahaTheme, string> _themeDir = null;

        /// <summary>The currently selected theme</summary>
        private KalahaTheme _currentTheme;

        // --- Methods of the class ---

        /// <summary>The constructor of the class. This is declared private because only
        /// the Instance property method may call it.</summary>
        private ThemeHandler()
        {
            // For each possible theme, we add to the dictionary of theme URIs the strings where we find
            // the respective style file.
            // The strings are not added directly but using the resource files so that there is no hard-coded
            // strings used here:
            _themeDir = new Dictionary<KalahaTheme, string>();
            _themeDir.Add(KalahaTheme.Wood, KalahaResources.I.GetRes("Theme_Wood"));
            _themeDir.Add(KalahaTheme.Spring, KalahaResources.I.GetRes("Theme_Spring"));
            _themeDir.Add(KalahaTheme.SummerOnTheBeach, KalahaResources.I.GetRes("Theme_SummerOnTheBeach"));
            _themeDir.Add(KalahaTheme.HighContrast, KalahaResources.I.GetRes("Theme_HighContrast"));

            // Set the default theme:
            ChangeToDefaultTheme();
        }


        // --- Methods of the class ---

        /// <returns>The currently selected theme</returns>
        public KalahaTheme GetCurrentTheme()
        {
            return _currentTheme;
        }

        /// <returns>A string that contains the complete path of the current theme's directory</returns>
        public string GetCurrentThemesDir()
        {
            return KalahaResources.I.GetThemesRootDirName() + "/" + _themeDir[_currentTheme];
        }

        /// <returns>A string that contains the complete path of the current theme's directory</returns>
        public string GetCurrentThemesDirBackslash()
        {
            return KalahaResources.I.GetThemesRootDirName() + "\\" + _themeDir[_currentTheme];
        }

        /// <summary>Changes the theme by taking over the source parameter as the resource dictionary to be used.</summary>
        /// <param name="source">The information where to find the resource dictionary to be used</param>
        /// <param name="reloadOfPageNecessary">Set this to true if you know that in order to change the theme a reload of the current page is necessary.</param>
        public void ChangeTheme(KalahaTheme theme, bool reloadOfPageNecessary = true)
        {
            const string prefix = "ms-appx:/";
            const string standardStyleFileName = prefix + "Common/StandardStyles.xaml";
            const string themeStyleFileName = "Styles.xaml";
         
            // Save the information about the currently selected theme:
            _currentTheme = theme;
                
            // Switch to the correct layout resource file:
            KalahaResources.I.ChangeLayoutResources(_themeDir[theme]);
            
            var stdResourceDict = new ResourceDictionary
            {
                Source = new Uri(standardStyleFileName) 
            };

            var customResourceDict = new ResourceDictionary
            {
                Source = new Uri(prefix + GetCurrentThemesDir() + "/" + themeStyleFileName)
            };

            // Define a new merged resource dictionary as a combination of the standard dictionary plus
            // the source handed over as parameter.
            var mainResourceDict = new ResourceDictionary { MergedDictionaries = { stdResourceDict, customResourceDict } };
            App.Current.Resources = mainResourceDict;

            if (reloadOfPageNecessary)
            {
                // We need to refresh the page at hand, so we "virtually" switch to an empty page and back from it again:
                PSTScreen.I.RedrawPage();
            }
        }

        /// <summary>Changes the current theme to the default (which is "Wood").</summary>
        public void ChangeToDefaultTheme()
        {
            ChangeTheme(KalahaTheme.Wood, false);
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
        public static ThemeHandler I
        {
            get
            {
                if (_inst == null)
                {
                    lock (_syncRoot)
                    {
                        if (_inst == null)
                            _inst = new ThemeHandler();
                    }
                }

                return _inst;
            }
        }
    }
}
