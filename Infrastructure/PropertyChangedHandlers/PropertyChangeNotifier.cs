using GeoNote.Model.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;

namespace GeoNote.Infrastructure
{
    public enum AssignmentResult
    {
        Success, Cancelled, AlreadyAssigned, OwnerDisposed
    }

    public sealed class PropertyChangeNotifier : INotifyPropertyChanged, INotifyPropertyChanging
    {
        readonly WeakReference ownerWeakReference;

        /// <summary>
        /// Gets the owner for testing purposes.
        /// </summary>
        /// <value>The owner.</value>
        internal object Owner
        {
            get
            {
                if (ownerWeakReference.Target != null)
                {
                    return ownerWeakReference.Target;
                }
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance 
        /// of the <see cref="PropertyChangeNotifier"/> class.
        /// </summary>
        /// <param name="owner">The intended sender 
        /// of the <code>PropertyChanged</code> event.</param>
        public PropertyChangeNotifier(object owner)
            : this(owner, true)
        {
            /* Intentionally left blank. */
        }

        /// <summary>
        /// Initializes a new instance 
        /// of the <see cref="PropertyChangeNotifier"/> class.
        /// </summary>
        /// <param name="owner">The intended sender 
        /// <param name="useExtendedEventArgs">If <c>true</c> the
        /// generic <see cref="PropertyChangedEventArgs{TProperty}"/>
        /// and <see cref="PropertyChangingEventArgs{TProperty}"/> 
        /// are used when raising events. 
        /// Otherwise, the non-generic types are used, and they are cached 
        /// to decrease heap fragmentation.</param>
        /// of the <code>PropertyChanged</code> event.</param>
        public PropertyChangeNotifier(object owner, bool useExtendedEventArgs)
        {
            ArgumentValidator.AssertNotNull(owner, "owner");
            ownerWeakReference = new WeakReference(owner);
            this.useExtendedEventArgs = useExtendedEventArgs;
        }

        #region event PropertyChanged

        event PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (OwnerDisposed)
                {
                    return;
                }
                propertyChanged += value;
            }
            remove
            {
                if (OwnerDisposed)
                {
                    return;
                }
                propertyChanged -= value;
            }
        }

        #region Experimental Explicit UI Thread

        bool maintainThreadAffinity = true;

        /// <summary>
        /// Gets or sets a value indicating whether events will be raised 
        /// on the thread of subscription (either the UI or ViewModel layer).
        /// <c>true</c> by default.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if raising events on the thread 
        /// of subscription; otherwise, <c>false</c>.
        /// </value>
        public bool MaintainThreadAffinity
        {
            get
            {
                return maintainThreadAffinity;
            }
            set
            {
                maintainThreadAffinity = value;
            }
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// If the owner has been GC'd then the event will not be raised.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> 
        /// instance containing the event data.</param>
        void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var owner = ownerWeakReference.Target;
            if (owner != null && propertyChanged != null)
            {
                if (maintainThreadAffinity)
                {
                    Exception exception = null;

                    var dispatcher = System.Windows.Deployment.Current.Dispatcher;
                    dispatcher.BeginInvoke(() => propertyChanged(owner, e));

                    if (exception != null)
                    {
                        throw exception;
                    }
                }
                else
                {
                    propertyChanged(owner, e);
                }
            }
        }

        #endregion

        /// <summary>
        /// Assigns the specified newValue to the specified property
        /// and then notifies listeners that the property has changed.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="propertyName">Name of the property. Can not be null.</param>
        /// <param name="field">A reference to the property that is to be assigned.</param>
        /// <param name="newValue">The value to assign the property.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        public AssignmentResult Assign<TField>(
            string propertyName, ref TField field, TField newValue)
        {
            if (OwnerDisposed)
            {
                return AssignmentResult.OwnerDisposed;
            }

            ArgumentValidator.AssertNotNullOrEmpty(propertyName, "propertyName");
            ValidatePropertyName(propertyName);

            return AssignWithNotification<TField>(propertyName, ref field, newValue);
        }

        /// <summary>
        /// Assigns the specified newValue to the specified property
        /// and then notifies listeners that the property has changed.
        /// Note: This is new for WP8.
        /// </summary>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="propertyName">Name of the property. Can not be null.</param>
        /// <param name="field">A reference to the property that is to be assigned.</param>
        /// <param name="newValue">The value to assign the property.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        public AssignmentResult Assign<TField>(
            ref TField field, TField newValue, [CallerMemberName] string propertyName = "")
        {
            var result = Assign<TField>(propertyName, ref field, newValue);
            return result;
        }

        /// <summary>
        /// Assigns the specified newValue to the specified property
        /// and then notifies listeners that the property has changed.
        /// Assignment nor notification will occur if the specified
        /// property and newValue are equal. 
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <typeparam name="TField">The type of the field.</typeparam>
        /// <param name="expression">The expression that is used to derive the property name.
        /// Should not be <code>null</code>.</param>
        /// <param name="field">A reference to the property that is to be assigned.</param>
        /// <param name="newValue">The value to assign the property.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        public AssignmentResult Assign<TProperty, TField>(
            Expression<Func<TProperty>> expression, ref TField field, TField newValue)
            where TField : TProperty
        {
            if (OwnerDisposed)
            {
                return AssignmentResult.OwnerDisposed;
            }

            string propertyName = GetPropertyName(expression);
            return AssignWithNotification<TField>(propertyName, ref field, newValue);
        }

        AssignmentResult AssignWithNotification<TProperty>(
            string propertyName, ref TProperty field, TProperty newValue)
        {
            /* Hack for GeoCoordinate comparison bug. */
            if (EqualityComparer<TProperty>.Default.Equals(field, newValue))
            {
                return AssignmentResult.AlreadyAssigned;
            }

            if (useExtendedEventArgs)
            {
                var args = new PropertyChangingEventArgs<TProperty>(propertyName, field, newValue);

                OnPropertyChanging(args);
                if (args.Cancelled)
                {
                    return AssignmentResult.Cancelled;
                }

                var oldValue = field;
                field = newValue;
                OnPropertyChanged(new PropertyChangedEventArgs<TProperty>(
                    propertyName, oldValue, newValue));
            }
            else
            {
                var args = RetrieveOrCreatePropertyChangingEventArgs(propertyName);
                OnPropertyChanging(args);

                var changedArgs = RetrieveOrCreatePropertyChangedEventArgs(propertyName);
                OnPropertyChanged(changedArgs);
            }

            return AssignmentResult.Success;
        }

        readonly Dictionary<string, string> expressions = new Dictionary<string, string>();

        /// <summary>
        /// Notifies listeners that the specified property has changed.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyName">Name of the property. Can not be null.</param>
        /// <param name="oldValue">The old value before the change occurred.</param>
        /// <param name="newValue">The new value after the change occurred.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        public void NotifyChanged<TProperty>(
            string propertyName, TProperty oldValue, TProperty newValue)
        {
            if (OwnerDisposed)
            {
                return;
            }
            ArgumentValidator.AssertNotNullOrEmpty(propertyName, "propertyName");
            ValidatePropertyName(propertyName);

            if (ReferenceEquals(oldValue, newValue))
            {
                return;
            }

            var args = useExtendedEventArgs
                ? new PropertyChangedEventArgs<TProperty>(propertyName, oldValue, newValue)
                : RetrieveOrCreatePropertyChangedEventArgs(propertyName);

            OnPropertyChanged(args);
        }

        /// <summary>
        /// Slow. Not recommended.
        /// Notifies listeners that the property has changed.
        /// Notification will occur if the specified
        /// property and newValue are equal. 
        /// </summary>
        /// <param name="expression">The expression that is used to derive the property name.
        /// Should not be <code>null</code>.</param>
        /// <param name="oldValue">The old value of the property before it was changed.</param>
        /// <param name="newValue">The new value of the property after it was changed.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        public void NotifyChanged<T>(
            Expression<Func<T>> expression, T oldValue, T newValue)
        {
            if (OwnerDisposed)
            {
                return;
            }

            ArgumentValidator.AssertNotNull(expression, "expression");

            string name = GetPropertyName(expression);
            NotifyChanged(name, oldValue, newValue);
        }

        static MemberInfo GetMemberInfo<T>(Expression<Func<T>> expression)
        {
            var member = expression.Body as MemberExpression;
            if (member != null)
            {
                return member.Member;
            }

            /* TODO: Make localizable resource. */
            throw new ArgumentException("MemberExpression expected.", "expression");
        }

        #region INotifyPropertyChanging Implementation
#if !SILVERLIGHT
		[field: NonSerialized]
#endif
        event PropertyChangingEventHandler propertyChanging;

        public event PropertyChangingEventHandler PropertyChanging
        {
            add
            {
                if (OwnerDisposed)
                {
                    return;
                }
                propertyChanging += value;
            }
            remove
            {
                if (OwnerDisposed)
                {
                    return;
                }
                propertyChanging -= value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanging"/> event.
        /// If the owner has been GC'd then the event will not be raised.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangingEventArgs"/> 
        /// instance containing the event data.</param>
        void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            var owner = ownerWeakReference.Target;
            if (owner != null && propertyChanging != null)
            {
                propertyChanging(owner, e);
            }
        }

        /// <summary>
        /// Notifies listeners that the specified property is about to change.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyName">Name of the property. Can not be null.</param>
        /// <param name="oldValue">The old value before the change occurred.</param>
        /// <param name="newValue">The new value after the change occurred.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        public void NotifyChanging<TProperty>(
            string propertyName, TProperty oldValue, TProperty newValue)
        {
            if (OwnerDisposed)
            {
                return;
            }

            ArgumentValidator.AssertNotNullOrEmpty(propertyName, "propertyName");
            ValidatePropertyName(propertyName);

            if (ReferenceEquals(oldValue, newValue))
            {
                return;
            }

            var args = useExtendedEventArgs
                ? new PropertyChangingEventArgs<TProperty>(propertyName, oldValue, newValue)
                : RetrieveOrCreatePropertyChangingEventArgs(propertyName);

            OnPropertyChanging(args);
        }

        /// <summary>
        /// Slow. Not recommended.
        /// Notifies listeners that the property has changed.
        /// Notification will occur if the specified
        /// property and newValue are equal. 
        /// </summary>
        /// <param name="expression">The expression that is used to derive the property name.
        /// Should not be <code>null</code>.</param>
        /// <param name="oldValue">The old value of the property before it was changed.</param>
        /// <param name="newValue">The new value of the property after it was changed.</param>
        /// <exception cref="ArgumentNullException">
        /// Occurs if the specified propertyName is <code>null</code>.</exception>
        /// <exception cref="ArgumentException">
        /// Occurs if the specified propertyName is an empty string.</exception>
        public void NotifyChanging<T>(
            Expression<Func<T>> expression, T oldValue, T newValue)
        {
            if (OwnerDisposed)
            {
                return;
            }

            ArgumentValidator.AssertNotNull(expression, "expression");

            string name = GetPropertyName(expression);
            NotifyChanging(name, oldValue, newValue);
        }
        #endregion

        readonly object expressionsLock = new object();

        string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            string name;
            if (!expressions.TryGetValue(expression.ToString(), out name))
            {
                lock (expressionsLock)
                {
                    if (!expressions.TryGetValue(expression.ToString(), out name))
                    {
                        var memberInfo = GetMemberInfo(expression);
                        if (memberInfo == null)
                        {
                            /* TODO: Make localizable resource. */
                            throw new InvalidOperationException("MemberInfo not found.");
                        }
                        name = memberInfo.Name;
                        expressions.Add(expression.ToString(), name);
                    }
                }
            }

            return name;
        }

        bool cleanupOccurred;

        bool OwnerDisposed
        {
            get
            {
                /* We slightly improve performance here 
                 * by avoiding multiple Owner property calls 
                 * after the Owner has been disposed. */
                if (cleanupOccurred)
                {
                    return true;
                }
                var owner = Owner;
                if (owner != null)
                {
                    return false;
                }
                cleanupOccurred = true;
                var changedSubscribers = propertyChanged.GetInvocationList();
                foreach (var subscriber in changedSubscribers)
                {
                    propertyChanged -= (PropertyChangedEventHandler)subscriber;
                }
                var changingSubscribers = propertyChanging.GetInvocationList();
                foreach (var subscriber in changingSubscribers)
                {
                    propertyChanging -= (PropertyChangingEventHandler)subscriber;
                }

                /* Events should be null at this point. Nevertheless... */
                propertyChanged = null;
                propertyChanging = null;
                propertyChangedEventArgsCache.Clear();
                propertyChangingEventArgsCache.Clear();

                return true;
            }
        }

        [Conditional("DEBUG")]
        void ValidatePropertyName(string propertyName)
        {

        }

        bool useExtendedEventArgs;
        readonly Dictionary<string, PropertyChangedEventArgs> propertyChangedEventArgsCache = new Dictionary<string, PropertyChangedEventArgs>();
        readonly Dictionary<string, PropertyChangingEventArgs> propertyChangingEventArgsCache = new Dictionary<string, PropertyChangingEventArgs>();

        readonly object propertyChangingEventArgsCacheLock = new object();

        PropertyChangingEventArgs RetrieveOrCreatePropertyChangingEventArgs(string propertyName)
        {
            var result = RetrieveOrCreateEventArgs(
                propertyName,
                propertyChangingEventArgsCacheLock,
                propertyChangingEventArgsCache,
                x => new PropertyChangingEventArgs(x));

            return result;
        }

        readonly object propertyChangedEventArgsCacheLock = new object();

        PropertyChangedEventArgs RetrieveOrCreatePropertyChangedEventArgs(string propertyName)
        {
            var result = RetrieveOrCreateEventArgs(
                propertyName,
                propertyChangedEventArgsCacheLock,
                propertyChangedEventArgsCache,
                x => new PropertyChangedEventArgs(x));

            return result;
        }

        static TArgs RetrieveOrCreateEventArgs<TArgs>(
            string propertyName, object cacheLock, Dictionary<string, TArgs> argsCache,
            Func<string, TArgs> createFunc)
        {
            ArgumentValidator.AssertNotNull(propertyName, "propertyName");
            TArgs result;

            lock (cacheLock)
            {
                if (argsCache.TryGetValue(propertyName, out result))
                {
                    return result;
                }

                result = createFunc(propertyName);
                argsCache[propertyName] = result;
            }
            return result;
        }

        public void NotifyChanged(string propertyName)
        {
            var args = RetrieveOrCreatePropertyChangedEventArgs(propertyName);
            OnPropertyChanged(args);
        }

        public void NotifyChanged<TProperty>(
            Expression<Func<TProperty>> expression)
        {
            string propertyName = GetPropertyName(expression);
            var args = RetrieveOrCreatePropertyChangedEventArgs(propertyName);
            OnPropertyChanged(args);
        }
    }
}
