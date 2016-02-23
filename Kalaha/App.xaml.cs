//
// App.xaml.cs
//
// The initial class that is starting the app.
//

using Kalaha.Model;
using Kalaha.View;
using PST_Common;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Globalization;

namespace Kalaha
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Initialize the logging, setting the corresponding log level.
            // CAUTION: This needs to be set carefully before submitting the app to the Windows store!!
            Logging.I.Initialize(Logging.LogLevel.Error);
//          Logging.I.Initialize(Logging.LogLevel.Error | Logging.LogLevel.Info | Logging.LogLevel.Debug);
//          Logging.I.Initialize(Logging.LogLevel.Error | Logging.LogLevel.Debug);

//DEBUG     Logging.I.LogMessage("Current culture: " + CultureInfo.CurrentCulture.Name + " -> taking into account \"" +
//DEBUG                          CultureInfo.CurrentCulture.Name.Substring(0,2) + "\".\n");
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden verwendet, wenn die Anwendung zum Öffnen einer bestimmten Datei, zum Anzeigen
        /// von Suchergebnissen usw. gestartet wird.
        /// </summary>
        /// <param name="args">Details über Startanforderung und -prozess.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Einen Rahmen erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Rahmen im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            // CAUTION: This must be set to "Real" before it is submitted to the Windows store!!
            InAppPurchases.I.Instantiate(InAppPurchases.RunningMode.Real);

            // Instantiate the Presenter in order to call its constructor as early as possible:
            Presenter.I.Instantiate();

            if (rootFrame.Content == null)
            {
                // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                // übergeben werden

                // Navigate to the hub page:
                Type selectedPageType = typeof(HubPage);

                if (!rootFrame.Navigate(selectedPageType, args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Sicherstellen, dass das aktuelle Fenster aktiv ist
            Window.Current.Activate();
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird. Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // Permanently save the configuration made by the user:
            GameStorage.I.Save(Presenter.I.GetSouthernPlayer(), Presenter.I.GetNorthernPlayer(),
                               ThemeHandler.I.GetCurrentTheme().ToString());

            deferral.Complete();
        }

        // Handle einfügen, der den Aufruf der Datenschutzrichtlinie auslöst
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            if (CultureInfo.CurrentCulture.Name.Substring(0, 2) == "de")
            {
                SettingsPane.GetForCurrentView().CommandsRequested += (s, e) =>
                  e.Request.ApplicationCommands.Add(
                     new SettingsCommand("privacypolicy", "Datenschutz", ShowPrivacyPolicy)
                  );
            }
            else
            {
                SettingsPane.GetForCurrentView().CommandsRequested += (s, e) =>
                  e.Request.ApplicationCommands.Add(
                     new SettingsCommand("privacypolicy", "Privacy", ShowPrivacyPolicy)
                  );
            }
        }

        // OpenPrivacyPolicy Methode hinzufügen, Webseite mit Datenschutzrichtlinie aufrufen
        private async void ShowPrivacyPolicy(IUICommand c)
        {
            Uri uri = null;

            if (CultureInfo.CurrentCulture.Name.Substring(0,2) == "de")
            {
                uri = new Uri("http://www.tempest-rock-studios.com/de/privacy/");
            }
            else
            {
                uri = new Uri("http://www.tempest-rock-studios.com/en/privacy/");
            }
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

    }
}
