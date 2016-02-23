using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace Kalaha.Common
{
    /// <summary>
    /// Wrapper für <see cref="RichTextBlock"/>, der so viele zusätzliche Überlaufspalten
    /// wie nötig erstellt, um den verfügbaren Inhalt unterzubringen
    /// </summary>
    /// <example>
    /// Mit dem nachfolgenden Code wird eine Auflistung von 400 Pixel breiten Spalten mit einem Abstand von 50 Pixeln erstellt,
    /// um beliebige datengebundene Inhalte unterzubringen:
    /// <code>
    /// <RichTextColumns>
    ///     <RichTextColumns.ColumnTemplate>
    ///         <DataTemplate>
    ///             <RichTextBlockOverflow Width="400" Margin="50,0,0,0"/>
    ///         </DataTemplate>
    ///     </RichTextColumns.ColumnTemplate>
    ///     
    ///     <RichTextBlock Width="400">
    ///         <Paragraph>
    ///             <Run Text="{Binding Content}"/>
    ///         </Paragraph>
    ///     </RichTextBlock>
    /// </RichTextColumns>
    /// </code>
    /// </example>
    /// <remarks>Wird normalerweise in einer Region mit horizontalem Bildlauf verwendet, in der eine ungebundene Menge an
    /// Platz ermöglicht, dass alle benötigten Spalten erstellt werden. Bei Verwendung in einer Region mit vertikalem Bildlauf
    /// gibt es niemals zusätzliche Spalten.</remarks>
    [Windows.UI.Xaml.Markup.ContentProperty(Name = "RichTextContent")]
    public sealed class RichTextColumns : Panel
    {
        /// <summary>
        /// Identifiziert die <see cref="RichTextContent"/>-Abhängigkeitseigenschaft.
        /// </summary>
        public static readonly DependencyProperty RichTextContentProperty =
            DependencyProperty.Register("RichTextContent", typeof(RichTextBlock),
            typeof(RichTextColumns), new PropertyMetadata(null, ResetOverflowLayout));

        /// <summary>
        /// Identifiziert die <see cref="ColumnTemplate"/>-Abhängigkeitseigenschaft.
        /// </summary>
        public static readonly DependencyProperty ColumnTemplateProperty =
            DependencyProperty.Register("ColumnTemplate", typeof(DataTemplate),
            typeof(RichTextColumns), new PropertyMetadata(null, ResetOverflowLayout));

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="RichTextColumns"/>-Klasse.
        /// </summary>
        public RichTextColumns()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
        }

        /// <summary>
        /// Ruft den ursprünglichen Rich-Text-Inhalt zur Verwendung als erste Spalte ab, oder legt diesen fest.
        /// </summary>
        public RichTextBlock RichTextContent
        {
            get { return (RichTextBlock)GetValue(RichTextContentProperty); }
            set { SetValue(RichTextContentProperty, value); }
        }

        /// <summary>
        /// Ruft die Vorlage zum Erstellen zusätzlicher
        /// <see cref="RichTextBlockOverflow"/>-Instanzen ab, oder legt diese fest.
        /// </summary>
        public DataTemplate ColumnTemplate
        {
            get { return (DataTemplate)GetValue(ColumnTemplateProperty); }
            set { SetValue(ColumnTemplateProperty, value); }
        }

        /// <summary>
        /// Wird aufgerufen, wenn der Inhalt oder die Überlaufvorlage geändert wird, um das Spaltenlayout neu zu erstellen.
        /// </summary>
        /// <param name="d">Instanz von <see cref="RichTextColumns"/>, in der die Änderung
        /// erfolgt ist.</param>
        /// <param name="e">Ereignisdaten, die die jeweilige Änderung beschreiben.</param>
        private static void ResetOverflowLayout(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Bei tiefgreifenden Änderungen das Spaltenlayout von Grund auf neu erstellen
            var target = d as RichTextColumns;
            if (target != null)
            {
                target._overflowColumns = null;
                target.Children.Clear();
                target.InvalidateMeasure();
            }
        }

        /// <summary>
        /// Listet die bereits erstellten Überlaufspalten auf. Muss eine 1:1-Beziehung mit
        /// Instanzen in der <see cref="Panel.Children"/>-Auflistung aufrechterhalten, die dem anfänglichen
        /// untergeordneten RichTextBlock-Element folgt.
        /// </summary>
        private List<RichTextBlockOverflow> _overflowColumns = null;

        /// <summary>
        /// Bestimmt, ob zusätzliche Überlaufspalten erforderlich sind und ob vorhandene Spalten
        /// entfernt werden können.
        /// </summary>
        /// <param name="availableSize">Die Größe des verfügbaren Platzes zum Beschränken der
        /// Anzahl zusätzlicher Spalten, die erstellt werden können.</param>
        /// <returns>Die daraus resultierende Größe des ursprünglichen Inhalts zuzüglich der zusätzlichen Spalten.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.RichTextContent == null) return new Size(0, 0);

            // Sicherstellen, dass RichTextBlock ein untergeordnetes Element ist. Das Fehlen
            // einer Liste zusätzlicher Spalten dient dabei als Indiz, dass dies noch
            // nicht geschehen ist
            if (this._overflowColumns == null)
            {
                Children.Add(this.RichTextContent);
                this._overflowColumns = new List<RichTextBlockOverflow>();
            }

            // Mit dem Messen des ursprünglichen RichTextBlock-Inhalts beginnen
            this.RichTextContent.Measure(availableSize);
            var maxWidth = this.RichTextContent.DesiredSize.Width;
            var maxHeight = this.RichTextContent.DesiredSize.Height;
            var hasOverflow = this.RichTextContent.HasOverflowContent;

            // Sicherstellen, dass genügend Überlaufspalten zur Verfügung stehen
            int overflowIndex = 0;
            while (hasOverflow && maxWidth < availableSize.Width && this.ColumnTemplate != null)
            {
                // Vorhandene Überlaufspalten so lange verwenden, bis alle aufgebraucht sind, und
                // dann weitere aus der bereitgestellten Vorlage erstellen
                RichTextBlockOverflow overflow;
                if (this._overflowColumns.Count > overflowIndex)
                {
                    overflow = this._overflowColumns[overflowIndex];
                }
                else
                {
                    overflow = (RichTextBlockOverflow)this.ColumnTemplate.LoadContent();
                    this._overflowColumns.Add(overflow);
                    this.Children.Add(overflow);
                    if (overflowIndex == 0)
                    {
                        this.RichTextContent.OverflowContentTarget = overflow;
                    }
                    else
                    {
                        this._overflowColumns[overflowIndex - 1].OverflowContentTarget = overflow;
                    }
                }

                // Die neue Spalte messen und bei Bedarf eine Wiederholung vorbereiten
                overflow.Measure(new Size(availableSize.Width - maxWidth, availableSize.Height));
                maxWidth += overflow.DesiredSize.Width;
                maxHeight = Math.Max(maxHeight, overflow.DesiredSize.Height);
                hasOverflow = overflow.HasOverflowContent;
                overflowIndex++;
            }

            // Zusätzliche Spalten von der Überlaufkette trennen, diese aus unserer privaten Liste
            // von Spalten entfernen, und sie als untergeordnete Elemente entfernen
            if (this._overflowColumns.Count > overflowIndex)
            {
                if (overflowIndex == 0)
                {
                    this.RichTextContent.OverflowContentTarget = null;
                }
                else
                {
                    this._overflowColumns[overflowIndex - 1].OverflowContentTarget = null;
                }
                while (this._overflowColumns.Count > overflowIndex)
                {
                    this._overflowColumns.RemoveAt(overflowIndex);
                    this.Children.RemoveAt(overflowIndex + 1);
                }
            }

            // Abschließend ermittelte Größe melden
            return new Size(maxWidth, maxHeight);
        }

        /// <summary>
        /// Ordnet den ursprünglichen Inhalt und alle zusätzlichen Spalten an.
        /// </summary>
        /// <param name="finalSize">Definiert die Größe des Bereichs, in dem die untergeordneten Elemente angeordnet
        /// werden müssen.</param>
        /// <returns>Die Größe des Bereichs, den die untergeordneten Elemente tatsächlich benötigt haben.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double maxWidth = 0;
            double maxHeight = 0;
            foreach (var child in Children)
            {
                child.Arrange(new Rect(maxWidth, 0, child.DesiredSize.Width, finalSize.Height));
                maxWidth += child.DesiredSize.Width;
                maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
            }
            return new Size(maxWidth, maxHeight);
        }
    }
}
