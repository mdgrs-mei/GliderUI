using System.ComponentModel;
using Avalonia.Data;
using Avalonia.Data.Core.Plugins;

namespace GliderUI.Server;

// Avalonia does not support binding for dynamic properties, so we add a custom property accessor plugin for DataSource objects.
// This accessor allows Two-way binding.
internal sealed class DataSourcePropertyAccessorPlugin : IPropertyAccessorPlugin
{
    public bool Match(object obj, string propertyName)
    {
        return obj is DataSource;
    }

    public IPropertyAccessor? Start(WeakReference<object?> reference, string propertyName)
    {
        return new PropertyAccessor(reference, propertyName);
    }

    private sealed class PropertyAccessor : PropertyAccessorBase
    {
        private readonly WeakReference<object?> _objReference;
        private readonly string _propertyName;

        public override Type? PropertyType { get; }
        public override object? Value
        {
            get
            {
                if (!_objReference.TryGetTarget(out object? target))
                    return null;

                if (target is not DataSource dataSource)
                    return null;

                _ = dataSource.TryGetMember(_propertyName, out object? value);
                return value;
            }
        }

        public PropertyAccessor(WeakReference<object?> objReference, string propertyName)
        {
            _objReference = objReference;
            _propertyName = propertyName.ToUpperInvariant();

            if (!_objReference.TryGetTarget(out object? target))
                return;

            if (target is not DataSource dataSource)
                return;

            if (dataSource.TryGetMember(_propertyName, out object? propertyValue))
            {
                PropertyType = propertyValue?.GetType();
            }
        }

        public override bool SetValue(object? value, BindingPriority priority)
        {
            if (!_objReference.TryGetTarget(out object? target))
                return false;

            if (target is not DataSource dataSource)
                return false;

            dataSource.SetMember(_propertyName, value);
            return true;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!_objReference.TryGetTarget(out object? target))
                return;

            if (sender == target && e.PropertyName == _propertyName)
            {
                PublishValue(Value);
            }
        }

        protected override void SubscribeCore()
        {
            if (!_objReference.TryGetTarget(out object? target))
                return;

            PublishValue(Value);

            if (target is INotifyPropertyChanged iNotifyPropertyChanged)
            {
                iNotifyPropertyChanged.PropertyChanged += OnPropertyChanged;
            }

        }

        protected override void UnsubscribeCore()
        {
            if (!_objReference.TryGetTarget(out object? target))
                return;

            if (target is INotifyPropertyChanged iNotifyPropertyChanged)
            {
                iNotifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
            }
        }
    }
}
