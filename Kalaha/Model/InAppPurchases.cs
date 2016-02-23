//
// InAppPurchases
//
// Defines the class that stores information about in-app puchases.
// The InAppPurchasing is implemented as a singleton that is available throughout the source "space".
//

using PST_Common;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


namespace Kalaha.Model
{

    public sealed class InAppPurchases
    {
        // --- Enums ---

        /// <summary>
        /// The purchase options that are available in this app
        /// </summary>
        public enum PurchaseOption
        {
            NoAds = 0           // Purchase option to remove ads from the game
        }

        /// <summary>
        /// The mode in which we are running this app
        /// </summary>
        public enum RunningMode
        {
            Testing = 0,        // We are just developing or testing this app
            Real = 1            // This version is the one to be used in the Windows 8 store
        }

        // --- Attributes of the class ---

        /// The instance is declared to be volatile to ensure that assignment to the instance variable
        /// completes before the instance variable can be accessed.
        private static volatile InAppPurchases _inst;
        private static object _syncRoot = new Object();

        /// <summary>
        /// A flag whether the "Instanciate" method has been called exactly once and before all other calls.
        /// </summary>
        private static bool _instantiated = false;

        /// <summary>
        /// The mode the app is running in
        /// </summary>
        private RunningMode _runningMode;

        /// <summary>
        /// The license information that is received from the Microsoft servers 
        /// </summary>
        LicenseInformation _licenseInformation;

        /// <summary>
        /// The string which is used to ask the licenseInformation class about the NoAds option
        /// </summary>
        private const string _noAdsString = "NoAds";

        /// <summary>
        /// The flag whether or not the "NoAds" option is activated
        /// </summary>
        private bool _noAdsOptionIsActivated;



        // --- Methods of the class ---

        /// <summary>
        /// Instantiates the InAppPurchasing. This must be the first method that is called of this class.
        /// </summary>
        /// <param name="thisIsReal">True if the app is to be run in the "real" Windows 8 shop. False if this app shall run in test mode.</param>
        public void Instantiate(RunningMode runningMode)
        {
            // By calling this function, the constructor is called first.

            if (_instantiated)
            {
                // We are already instantiated. This should not happen.
                Logging.I.LogMessage("InAppPurchase.Instatiate: Leaving early.\n", Logging.LogLevel.Error);
                return;
            }

            // Get the license information, i.e. the object that stores information about already purchased in-app features:
            switch(runningMode)
            {
                case RunningMode.Real:
//DEBUG             Logging.I.LogMessage("InAppPurchases: Instanciating LicenseInformation object in Real mode.\n");

                    // Get the "real" license info:
                    _licenseInformation = CurrentApp.LicenseInformation;
                    break;

                case RunningMode.Testing:
                    Logging.I.LogMessage("InAppPurchases: Instanciating LicenseInformation object in Test mode.\n");

                    // Get the license info for testing purposes:
                    _licenseInformation = CurrentAppSimulator.LicenseInformation;
/*
                    Removed because of issues with asynchronicity:
                    // Load test data:
                    StorageFolder proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
                    StorageFile proxyFile = await proxyDataFolder.GetFileAsync("InAppPurchasing_Test.xml");
                    await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
*/
                    break;

                default:
                    throw new PSTException("InAppPurchases.Instantiate(): Unknown RunningMode: " + runningMode + ".");            
            }

            // Set the private member:
            _runningMode = runningMode;

            // Set the flag that we are instanciated from now on:
            _instantiated = true;

            // Get the latest in-app purchasing information:
            Update();

        }

        /// <summary>
        /// The private constructor
        /// </summary>
        private InAppPurchases()
        {
            // Set some default values:
            _runningMode = RunningMode.Testing;
            _licenseInformation = null;
            _noAdsOptionIsActivated = false;
        }

        /// <summary>
        /// Updates the currently known information about in-app purchases.
        /// </summary>
        public void Update()
        {
            if (!_instantiated)
            {
                // We are not instantiated, yet.
                throw new PSTException("InAppPurchases.Update(): Class is not instantiated.");
            }

            // Get the information about purchased in-app features from the object:
            _noAdsOptionIsActivated = _licenseInformation.ProductLicenses[_noAdsString].IsActive;

//DEBUG     Logging.I.LogMessage("InAppPurchase.Update: Values after update: noAdsOptionIsActivated: " + _noAdsOptionIsActivated + ".\n");
        }

        /// <summary>
        /// Puchases the "NoAds" option. Please call the Update() method after calling this method!
        /// </summary>
        public Windows.Foundation.IAsyncOperation<string> PurchaseNoAdsOption()
        {
            if (!_instantiated)
            {
                // We are not instantiated, yet.
                throw new PSTException("InAppPurchases.PurchaseNoAdsOption(): Class is not instantiated.");
            }

            if (_noAdsOptionIsActivated)
            {
                // The option is already activated.
                Logging.I.LogMessage("InAppPurchases.PurchaseNoAdsOption: Option is already active.\n");
                return null;
            }

            switch(_runningMode)
            {
                case RunningMode.Real:
                    return CurrentApp.RequestProductPurchaseAsync(_noAdsString, false);

                case RunningMode.Testing:
                    return CurrentAppSimulator.RequestProductPurchaseAsync(_noAdsString, false);

                default:
                    return null;
            }
       }

        /// <summary>
        /// Returns true if the "NoAds" option is activated.
        /// </summary>
        public bool NoAdsOptionIsActivated()
        {
            if (!_instantiated)
            {
                // We are not instantiated, yet.
                throw new PSTException("InAppPurchases.NoAdsActivated(): Class is not instantiated.");
            }

            return _noAdsOptionIsActivated;
        }


        /// <summary>The single instance that this class provides</summary>
        /// <returns>The single instance of the class</returns>
        public static InAppPurchases I
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
                            _inst = new InAppPurchases();
                        }
                    }
                }

                return _inst;
            }
        }
    }
}
