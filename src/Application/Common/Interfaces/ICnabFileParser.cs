using DesafioDev.Application.Common.Models;

namespace DesafioDev.Application.Common.Interfaces;

/// <summary>
/// Interface for parsing CNAB (Centro Nacional de Automação Bancária) files
/// </summary>
public interface ICnabFileParser
{
    /// <summary>
    /// Parses a CNAB file stream and returns parsed line data
    /// </summary>
    /// <param name="fileStream">The CNAB file stream to parse</param>
    /// <returns>Enumerable of parsed CNAB line data</returns>
    IEnumerable<CnabLineData> ParseFile(Stream fileStream);

    /// <summary>
    /// Parses a single line from the CNAB file
    /// </summary>
    /// <param name="line">The CNAB line to parse (must be exactly 81 characters)</param>
    /// <returns>Parsed CNAB line data</returns>
    CnabLineData ParseLine(string line);
}
