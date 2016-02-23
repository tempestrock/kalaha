using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Data;

namespace Kalaha.Common
{
    /// <summary>
    /// Implementierung von <see cref="INotifyPropertyChanged"/> zum Vereinfachen von Modellen.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Mehrfach umgewandeltes Ereignis für Eigenschaftsänderungsbenachrichtigungen.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Überprüft, ob eine Eigenschaft mit dem gewünschten Wert bereits übereinstimmt. Legt die Eigenschaft fest, und
        /// benachrichtigt Listener nur bei Bedarf.
        /// </summary>
        /// <typeparam name="T">Typ der Eigenschaft.</typeparam>
        /// <param name="storage">Verweise auf eine Eigenschaft mit Getter und Setter.</param>
        /// <param name="value">Gewünschter Wert für die Eigenschaft.</param>
        /// <param name="propertyName">Name der Eigenschaft zum Benachrichtigen von Listenern. Dieser
        /// Wert ist optional und kann automatisch bereitgestellt werden, wenn ein Aufruf von Compilern erfolgt,
        /// die CallerMemberName unterstützen.</param>
        /// <returns>TRUE, wenn der Wert geändert wurde, FALSE, wenn der vorhandene Wert mit dem
        /// gewünschten Wert übereinstimmt.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Benachrichtigt Listener darüber, dass ein Eigenschaftswert geändert wurde.
        /// </summary>
        /// <param name="propertyName">Name der Eigenschaft zum Benachrichtigen von Listenern. Dieser
        /// Wert ist optional und kann automatisch bereitgestellt werden, wenn ein Aufruf von Compilern erfolgt,
        /// die <see cref="CallerMemberNameAttribute"/> unterstützen.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
