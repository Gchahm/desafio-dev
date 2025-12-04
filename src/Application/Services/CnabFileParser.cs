using DesafioDev.Application.Common.Interfaces;
using DesafioDev.Application.Common.Models;

namespace DesafioDev.Application.Services;

/// <summary>
/// Concrete implementation of CNAB file parser
/// Handles the business logic of parsing the CNAB fixed-width format
/// This is APPLICATION logic - it's about understanding the business format of financial transactions
/// </summary>
public class CnabFileParser : ICnabFileParser
{
    private const int CnabLineLength = 81;

    // Field positions (0-based indexing for Substring)
    private const int TypeStart = 0;
    private const int TypeLength = 1;

    private const int DateStart = 1;
    private const int DateLength = 8;

    private const int ValueStart = 9;
    private const int ValueLength = 10;

    private const int CpfStart = 19;
    private const int CpfLength = 11;

    private const int CardStart = 30;
    private const int CardLength = 12;

    private const int TimeStart = 42;
    private const int TimeLength = 6;

    private const int StoreOwnerStart = 48;
    private const int StoreOwnerLength = 14;

    private const int StoreNameStart = 62;
    private const int StoreNameLength = 19;

    /// <summary>
    /// Parses a CNAB file stream and yields parsed line data
    /// </summary>
    public IEnumerable<CnabLineData> ParseFile(Stream fileStream)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));

        if (!fileStream.CanRead)
            throw new ArgumentException("Stream is not readable", nameof(fileStream));

        return ParseFileInternal(fileStream);
    }

    private IEnumerable<CnabLineData> ParseFileInternal(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        string? line;
        int lineNumber = 0;

        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            CnabLineData parsedLine;
            try
            {
                parsedLine = ParseLine(line, lineNumber);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error parsing line {lineNumber}: {ex.Message}",
                    ex);
            }

            yield return parsedLine;
        }
    }

    /// <summary>
    /// Parses a single CNAB line
    /// </summary>
    public CnabLineData ParseLine(string line, int lineNumber = 0)
    {
        if (line == null)
            throw new ArgumentNullException(nameof(line));

        if (line.Length != CnabLineLength)
            throw new ArgumentException(
                $"CNAB line must be exactly {CnabLineLength} characters, got {line.Length}",
                nameof(line));

        try
        {
            return new CnabLineData
            {
                LineNumber = lineNumber,
                Type = ParseInt(line, TypeStart, TypeLength, "Type"),
                Date = ParseString(line, DateStart, DateLength),
                ValueInCents = ParseLong(line, ValueStart, ValueLength, "Value"),
                Cpf = ParseString(line, CpfStart, CpfLength),
                CardNumber = ParseString(line, CardStart, CardLength),
                Time = ParseString(line, TimeStart, TimeLength),
                StoreOwner = ParseString(line, StoreOwnerStart, StoreOwnerLength),
                StoreName = ParseString(line, StoreNameStart, StoreNameLength)
            };
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            throw new InvalidOperationException(
                $"Failed to parse CNAB line: {ex.Message}",
                ex);
        }
    }

    private static int ParseInt(string line, int start, int length, string fieldName)
    {
        var value = line.Substring(start, length);

        if (!int.TryParse(value, out var result))
        {
            throw new FormatException(
                $"Field '{fieldName}' contains invalid integer value: '{value}'");
        }

        return result;
    }

    private static long ParseLong(string line, int start, int length, string fieldName)
    {
        var value = line.Substring(start, length);

        if (!long.TryParse(value, out var result))
        {
            throw new FormatException(
                $"Field '{fieldName}' contains invalid long value: '{value}'");
        }

        return result;
    }

    private static string ParseString(string line, int start, int length)
    {
        return line.Substring(start, length).Trim();
    }
}