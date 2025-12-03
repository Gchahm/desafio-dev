namespace DesafioDev.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary value
/// </summary>
public record Money
{
    /// <summary>
    /// The value in cents (stored as long to avoid floating point precision issues)
    /// </summary>
    public long Value { get; }

    private Money(long value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a Money instance from a raw CNAB value (cents)
    /// </summary>
    /// <param name="cnabValue">The raw value from CNAB file in cents</param>
    /// <returns>A Money instance</returns>
    public static Money FromCnabValue(long cnabValue)
    {
        if (cnabValue < 0)
            throw new ArgumentException("CNAB value cannot be negative", nameof(cnabValue));

        return new Money(cnabValue);
    }

    /// <summary>
    /// Creates a Money instance from a decimal value
    /// </summary>
    /// <param name="value">The decimal value</param>
    /// <returns>A Money instance</returns>
    public static Money FromDecimal(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Money value cannot be negative", nameof(value));

        return new Money((long)(value * 100));
    }

    /// <summary>
    /// Gets the decimal representation of the money value
    /// </summary>
    /// <returns>The decimal value</returns>
    public decimal ToDecimal()
    {
        return Value / 100.00m;
    }

    public override string ToString()
    {
        return ToDecimal().ToString("C2");
    }
}
