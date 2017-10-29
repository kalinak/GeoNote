using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GeoNote.Infrastructure
{
    public abstract class NotifyPropertyChangeBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        volatile PropertyChangeNotifier notifier;
        
        object notifierLock;

        /// <summary>
        /// Gets the notifier. It is lazy loaded.
        /// </summary>
        /// <value>The notifier.</value>
        protected PropertyChangeNotifier Notifier
        {
            get
            {
                /* It is cheaper to create an object to lock, than to instantiate 
                 * the PropertyChangeNotifier, because hooking up the events 
                 * for many instances is expensive. */
                if (notifier == null)
                {
                    lock (notifierLock)
                    {
                        if (notifier == null)
                        {
                            notifier = new PropertyChangeNotifier(this);
                        }
                    }
                }
                return notifier;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            Notifier.NotifyChanged(propertyName);
        }

        protected void OnPropertyChanged<TProperty>(
            Expression<Func<TProperty>> expression)
        {
            Notifier.NotifyChanged(expression);
        }

        protected void OnPropertyChanging<TProperty>(
            string propertyName, TProperty oldValue, TProperty newValue)
        {
            Notifier.NotifyChanging(propertyName, oldValue, newValue);
        }

        protected void OnPropertyChanging<TProperty>(
            Expression<Func<TProperty>> expression, TProperty oldValue, TProperty newValue)
        {
            Notifier.NotifyChanging(expression, oldValue, newValue);
        }

        //[OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            Initialize();
        }

        /// <summary>
        /// Assigns the specified newValue to the specified property
        /// and then notifies listeners that the property has changed.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="propertyName">Name of the property. Can not be null.</param>
        /// <param name="property">A reference to the property that is to be assigned.</param>
        /// <param name="newValue">The value to assign the property.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        protected AssignmentResult Assign<TField>(
            string propertyName, ref TField property, TField newValue)
        {
            return Notifier.Assign<TField>(propertyName, ref property, newValue);
        }

        /// <summary>
        /// Assigns the specified newValue to the specified property
        /// and then notifies listeners that the property has changed.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="propertyName">Name of the property. Can not be null.</param>
        /// <param name="property">A reference to the property that is to be assigned.</param>
        /// <param name="newValue">The value to assign the property.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        protected AssignmentResult Assign<TField>(
            ref TField property, TField newValue, [CallerMemberName] string propertyName = "")
        {
            return Notifier.Assign<TField>(propertyName, ref property, newValue);
        }

        /// <summary> 
        /// Assigns the specified newValue to the specified property
        /// and then notifies listeners that the property has changed.
        /// Assignment nor notification will occur if the specified
        /// property and newValue are equal. 
        /// Not recommended for frequent operations 
        /// as evaluation of the property expression is slow.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <typeparam name="TField">The type of the field. 
        /// When assignment occurs, a downcast is applied.</typeparam>
        /// <param name="propertyExpression">The expression that is used to derive the property name.
        /// Should not be <code>null</code>.</param>
        /// <param name="field">A reference to the property that is to be assigned.</param>
        /// <param name="newValue">The value to assign the property.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        protected AssignmentResult Assign<TProperty, TField>(
            Expression<Func<TProperty>> propertyExpression, ref TField field, TField newValue)
            where TField : TProperty
        {
            return Notifier.Assign<TProperty, TField>(propertyExpression, ref field, newValue);
        }

        /// <summary>
        /// When deserialization occurs fields are not instantiated,
        /// therefore we must instantiate the notifier.
        /// </summary>
        void Initialize()
        {
            notifierLock = new object();
        }

        public NotifyPropertyChangeBase()
        {
            Initialize();
        }

        #region Property change notification

        /// <summary>
        /// Occurs when a property value changes.
        /// <seealso cref="PropertyChangeNotifier"/>
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                Notifier.PropertyChanged += value;
            }
            remove
            {
                Notifier.PropertyChanged -= value;
            }
        }

        /// <summary>
        /// Occurs when a property value is changing.
        /// <seealso cref="PropertyChangeNotifier"/>
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging
        {
            add
            {
                Notifier.PropertyChanging += value;
            }
            remove
            {
                Notifier.PropertyChanging -= value;
            }
        }

        #endregion
    }
}
