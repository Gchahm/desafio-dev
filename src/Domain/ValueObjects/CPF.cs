namespace DesafioDev.Domain.ValueObjects;

/// <summary>
/// Value object representing a Brazilian CPF (Cadastro de Pessoas Físicas)
/// </summary>
public record CPF 
{
    /// <summary>
    /// The CPF value without formatting (11 digits)
    /// </summary>
    public string Value { get; }

    private CPF(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a CPF instance from a string value
    /// </summary>
    /// <param name="cpf">The CPF string (can be formatted or unformatted)</param>
    /// <returns>A CPF instance</returns>
    /// <exception cref="ArgumentException">Thrown when CPF is invalid</exception>
    public static CPF Create(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF cannot be null or empty", nameof(cpf));

        // Remove formatting (dots, hyphens, spaces)
        var cleanedCpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cleanedCpf.Length != 11)
            throw new ArgumentException("CPF must have exactly 11 digits", nameof(cpf));

        // Check for known invalid CPFs (all same digits)
        if (cleanedCpf.Distinct().Count() == 1)
            throw new ArgumentException("CPF cannot have all same digits", nameof(cpf));

        // Validate CPF using the verification algorithm
        if (!IsValidCpf(cleanedCpf))
            throw new ArgumentException("Invalid CPF", nameof(cpf));

        return new CPF(cleanedCpf);
    }

    /// <summary>
    /// Creates a CPF instance without validation (use carefully, only for trusted sources)
    /// </summary>
    /// <param name="cpf">The CPF string</param>
    /// <returns>A CPF instance</returns>
    public static CPF CreateUnchecked(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF cannot be null or empty", nameof(cpf));

        var cleanedCpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cleanedCpf.Length != 11)
            throw new ArgumentException("CPF must have exactly 11 digits", nameof(cpf));

        return new CPF(cleanedCpf);
    }

    /// <summary>
    /// Validates a CPF using the verification digit algorithm
    /// </summary>
    private static bool IsValidCpf(string cpf)
    {
        // Calculate first verification digit
        var sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (cpf[i] - '0') * (10 - i);

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        if (firstDigit != (cpf[9] - '0'))
            return false;

        // Calculate second verification digit
        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += (cpf[i] - '0') * (11 - i);

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return secondDigit == (cpf[10] - '0');
    }

    /// <summary>
    /// Returns the formatted CPF (###.###.###-##)
    /// </summary>
    public string ToFormattedString()
    {
        return $"{Value.Substring(0, 3)}.{Value.Substring(3, 3)}.{Value.Substring(6, 3)}-{Value.Substring(9, 2)}";
    }

    public override string ToString()
    {
        return Value;
    }

}
