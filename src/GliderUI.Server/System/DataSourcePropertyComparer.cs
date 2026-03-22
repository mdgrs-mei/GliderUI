using System.Collections;

namespace GliderUI.Server;

#pragma warning disable CA1515 // Consider making public types internal
public sealed class DataSourcePropertyComparer : IComparer
#pragma warning restore CA1515
{
    private readonly string _propertyName;

    public DataSourcePropertyComparer(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        _propertyName = propertyName.ToUpperInvariant();
    }

    public int Compare(object? x, object? y)
    {
        if (x is null)
        {
            return y is null ? 0 : -1;
        }
        if (y is null)
        {
            return 1;
        }

        if (x is not DataSource dataSourceX)
        {
            throw new ArgumentException($"DataSourcePropertyComparer can only take DataSource. It is [{x.GetType()}] instead.");
        }
        if (y is not DataSource dataSourceY)
        {
            throw new ArgumentException($"DataSourcePropertyComparer can only take DataSource. It is [{y.GetType()}] instead.");
        }

        object? propertyX = dataSourceX.GetMember(_propertyName);
        object? propertyY = dataSourceY.GetMember(_propertyName);

        return Comparer.Default.Compare(propertyX, propertyY);
    }
}
