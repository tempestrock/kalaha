//
// InputPopup
//
// A very primitive little page that displays a popup with an input field.
//

using PST_Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;


namespace Kalaha.View
{
    public sealed partial class InputPopup : Kalaha.Common.LayoutAwarePage
    {

        // --- Attributes of the class ---


        // --- Methods of the class ---

        public InputPopup()
            : base()
        {
            this.InitializeComponent();

            var bounds = Window.Current.Bounds;
            this.RootPanel.Width = bounds.Width;
            this.RootPanel.Height = bounds.Height;

            // A bit of a strange workaround in order to get the cursor into the inputField at launch time:
            var t = Dispatcher.RunAsync(CoreDispatcherPriority.High, () => { inputField.Focus(Windows.UI.Xaml.FocusState.Keyboard); });
        }

        /// <summary>
        /// Closes the popup and hands over the string in the parameter.
        /// </summary>
        /// <param name="inputString">The to be handed over</param>
        private void ClosePopup(string inputString)
        {
            Popup popup = this.Parent as Popup;

            // We put the entered text into the popup's tag in order for the closing 
            popup.Tag = inputString;

            // Close the popup:
            popup.IsOpen = false;
        }

        /// <summary>
        /// This event handler is called whenever a character is entered into the text field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputField_TextChanged(object sender, TextChangedEventArgs eventArgs)
        {
            string userChoiceString = inputField.Text;
            if (!userChoiceString.Contains("\n"))
            {
                // The user has not pressed enter, yet. Therefore, we ignore this event.
                return;
            }

            // The user pressed <Enter> in order to end the input.
            // Set the input field empty again:
            inputField.Text = "";

            // Cut off the string where the newline character occurs:
            userChoiceString = userChoiceString.Substring(0, userChoiceString.IndexOf('\n') - 1);

            // The user pressed the Enter key, so we can close the popup:
            ClosePopup(userChoiceString);
        }

        /// <summary>
        /// Handles the Click event of the 'OK' button simulating a save and close
        /// </summary>
        private void SimulateOKClicked(object sender, RoutedEventArgs eventArgs)
        {
//DEBUG     Logging.I.LogMessage("OK button clicked. Textfield: " + inputField.Text + ".\n");

            // The user clicked on the "OK" button, so we can close the popup:
            ClosePopup(inputField.Text);
        }

        /// <summary>
        /// Handles the Click event of the 'Cancel' button simulating a close without save
        /// </summary>
        private void SimulateCancelClicked(object sender, RoutedEventArgs eventArgs)
        {
//DEBUG            Logging.I.LogMessage("Cancel button clicked.\n");

            // In order to show that the cancel button was clicked, we hand over an empty string:
            ClosePopup("");
        }

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
        }

        /// <summary>
        /// Behält den dieser Seite zugeordneten Zustand bei, wenn die Anwendung angehalten oder
        /// die Seite im Navigationscache verworfen wird. Die Werte müssen den Serialisierungsanforderungen
        /// von <see cref="SuspensionManager.SessionState"/> entsprechen.
        /// </summary>
        /// <param name="pageState">Ein leeres Wörterbuch, das mit dem serialisierbaren Zustand aufgefüllt wird.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }
    }
}
