﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoNote.Infrastructure
{
    /// <summary>
    /// Provides data for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event,
    /// exposed via the <see cref="PropertyChangeNotifier"/>.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public sealed class PropertyChangedEventArgs<TProperty> : PropertyChangedEventArgs
    {
        /// <summary>
        /// Gets the value of the property before it was changed.
        /// </summary>
        /// <value>The old value.</value>
        public TProperty OldValue { get; private set; }

        /// <summary>
        /// Gets the new value of the property after it was changed.
        /// </summary>
        /// <value>The new value.</value>
        public TProperty NewValue { get; private set; }

        /// <summary>
        /// Initializes a new instance 
        /// of the <see cref="PropertyChangedEventArgs{TProperty}"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        /// <param name="oldValue">The old value before the change occurred.</param>
        /// <param name="newValue">The new value after the change occurred.</param>
        public PropertyChangedEventArgs(
            string propertyName, TProperty oldValue, TProperty newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
