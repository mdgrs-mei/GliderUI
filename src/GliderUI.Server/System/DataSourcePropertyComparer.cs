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

        try
        {
            return Comparer.Default.Compare(propertyX, propertyY);
        }
        catch (ArgumentException)
        {
            // If the types are different, try converting them to double if they are numeric types.
            if (IsNumericType(propertyX) && IsNumericType(propertyY))
            {
                double doubleX = Convert.ToDouble(propertyX);
                double doubleY = Convert.ToDouble(propertyY);
                return Comparer.Default.Compare(doubleX, doubleY);
            }
            throw;
        }
    }

    private static bool IsNumericType(object? obj)
    {
        if (obj is null)
            return false;

        Type type = obj.GetType();
        var typeCode = Type.GetTypeCode(type);

        return typeCode is
            TypeCode.SByte or
            TypeCode.Byte or
            TypeCode.Int16 or
            TypeCode.UInt16 or
            TypeCode.Int32 or
            TypeCode.UInt32 or
            TypeCode.Int64 or
            TypeCode.UInt64 or
            TypeCode.Single or
            TypeCode.Double or
            TypeCode.Decimal;
    }
}
