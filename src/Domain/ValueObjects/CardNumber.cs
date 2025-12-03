namespace DesafioDev.Domain.ValueObjects;

public sealed class CardNumber
{
    public string Value { get; }

    private CardNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a CardNumber instance from a string value
    /// </summary>
    /// <param name="cardNumber">The card number string</param>
    /// <returns>A CardNumber instance</returns>
    /// <exception cref="ArgumentException">Thrown when card number is invalid</exception>
    public static CardNumber Create(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            throw new ArgumentException("Card number cannot be null or empty", nameof(cardNumber));

        // Ensure only digits or the special character '*'
        if (cardNumber.Length != 12 || cardNumber.Any(ch => !char.IsDigit(ch) && ch != '*'))
        {
            throw new ArgumentException("Card number must have exactly 12 characters (digits or '*')", nameof(cardNumber));
        }

        return new CardNumber(cardNumber);
    }

    /// <summary>
    /// Returns a masked version of the card number (showing only last 4 digits)
    /// </summary>
    /// <returns>Masked card number (e.g., ****-****-1234)</returns>
    public string ToMaskedString()
    {
        return $"****-****-{Value.Substring(8, 4)}";
    }

    /// <summary>
    /// Returns a formatted card number (####-####-####)
    /// </summary>
    public string ToFormattedString()
    {
        return $"{Value.Substring(0, 4)}-{Value.Substring(4, 4)}-{Value.Substring(8, 4)}";
    }

    public override string ToString()
    {
        return Value;
    }
}