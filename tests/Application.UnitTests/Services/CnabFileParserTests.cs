using DesafioDev.Application.Common.Models;
using DesafioDev.Application.Services;
using NUnit.Framework;
using Shouldly;
using System.Text;

namespace DesafioDev.Application.UnitTests.Services;

[TestFixture]
public class CnabFileParserTests
{
    private CnabFileParser _parser = null!;

    [SetUp]
    public void Setup()
    {
        _parser = new CnabFileParser();
    }

    #region ParseLine - Valid Cases

    [Test]
    public void ParseLine_WithValidLine_ShouldParseAllFieldsCorrectly()
    {
        // Arrange
        var line = CreateCnabLine(
            type: 3,
            date: "20190301",
            value: 142000,
            cpf: "09620676017",
            card: "4753****3153",
            time: "153453",
            storeOwner: "JOÃO MACEDO",
            storeName: "BAR DO JOÃO");

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.ShouldNotBeNull();
        result.Type.ShouldBe(3);
        result.Date.ShouldBe("20190301");
        result.ValueInCents.ShouldBe(142000);
        result.Cpf.ShouldBe("09620676017");
        result.CardNumber.ShouldBe("4753****3153");
        result.Time.ShouldBe("153453");
        result.StoreOwner.ShouldBe("JOÃO MACEDO");
        result.StoreName.ShouldBe("BAR DO JOÃO");
    }

    [Test]
    public void ParseLine_WithAllTransactionTypes_ShouldParseCorrectly()
    {
        // Test all transaction types 1-9
        for (int type = 1; type <= 9; type++)
        {
            // Arrange
            var line = CreateCnabLine(type: type);

            // Act
            var result = _parser.ParseLine(line);

            // Assert
            result.Type.ShouldBe(type, $"Transaction type {type} should be parsed correctly");
        }
    }

    [Test]
    public void ParseLine_WithZeroValue_ShouldParseCorrectly()
    {
        // Arrange
        var line = CreateCnabLine(value: 0);

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.ValueInCents.ShouldBe(0L);
    }

    [Test]
    public void ParseLine_WithMaxValue_ShouldParseCorrectly()
    {
        // Arrange - Max 10-digit value
        var line = CreateCnabLine(value: 9999999999L);

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.ValueInCents.ShouldBe(9999999999L);
    }

    [Test]
    public void ParseLine_WithTrailingSpaces_ShouldTrimFields()
    {
        // Arrange - Fields with trailing spaces (PadRight already adds them)
        var line = CreateCnabLine(storeOwner: "JOÃO", storeName: "BAR");

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.StoreOwner.ShouldBe("JOÃO");
        result.StoreName.ShouldBe("BAR");
    }

    [Test]
    public void ParseLine_WithLeadingZeros_ShouldParseNumericFieldsCorrectly()
    {
        // Arrange
        var line = CreateCnabLine(value: 100);

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.ValueInCents.ShouldBe(100L);
    }

    #endregion

    #region ParseLine - Invalid Cases

    [Test]
    public void ParseLine_WithNullLine_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _parser.ParseLine(null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>()
            .ParamName.ShouldBe("line");
    }

    [Test]
    public void ParseLine_WithLineTooShort_ShouldThrowArgumentException()
    {
        // Arrange
        var line = "12019030100000142000962067601747";

        // Act
        Action act = () => _parser.ParseLine(line);

        // Assert
        var exception = act.ShouldThrow<ArgumentException>();
        exception.ParamName.ShouldBe("line");
        exception.Message.ShouldContain("must be exactly 81 characters");
        exception.Message.ShouldContain($"got {line.Length}");
    }

    [Test]
    public void ParseLine_WithLineTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var line = "3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       EXTRA";

        // Act
        Action act = () => _parser.ParseLine(line);

        // Assert
        var exception = act.ShouldThrow<ArgumentException>();
        exception.ParamName.ShouldBe("line");
        exception.Message.ShouldContain("must be exactly 81 characters");
    }

    [Test]
    public void ParseLine_WithInvalidTypeField_ShouldThrowFormatException()
    {
        // Arrange - Type field contains non-numeric character
        var line = "A" + CreateCnabLine().Substring(1); // Replace first char with 'A'

        // Act
        Action act = () => _parser.ParseLine(line);

        // Assert
        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.InnerException.ShouldBeOfType<FormatException>();
        exception.InnerException!.Message.ShouldContain("Type");
    }

    [Test]
    public void ParseLine_WithInvalidValueField_ShouldThrowFormatException()
    {
        // Arrange - Value field contains non-numeric characters
        var validLine = CreateCnabLine();
        var line = validLine.Substring(0, 9) + "ABCDEFGHIJ" + validLine.Substring(19);

        // Act
        Action act = () => _parser.ParseLine(line);

        // Assert
        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.InnerException.ShouldBeOfType<FormatException>();
        exception.InnerException!.Message.ShouldContain("Value");
    }

    #endregion

    #region ParseLine - Edge Cases

    [TestCase("")]
    [TestCase("   ")]
    public void ParseLine_WithEmptyOrWhitespaceLine_ShouldThrowArgumentException(string line)
    {
        // Act
        Action act = () => _parser.ParseLine(line);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("must be exactly 81 characters");
    }

    [Test]
    public void ParseLine_WithSpecialCharactersInTextFields_ShouldParseCorrectly()
    {
        // Arrange - Text fields with special characters
        var line = CreateCnabLine(storeOwner: "JOSÉ-MARÍA", storeName: "BAR&CAFÉ");

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.StoreOwner.ShouldBe("JOSÉ-MARÍA");
        result.StoreName.ShouldBe("BAR&CAFÉ");
    }

    #endregion

    #region ParseFile - Valid Cases

    [Test]
    public void ParseFile_WithValidFile_ShouldParseAllLines()
    {
        // Arrange
        var line1 = CreateCnabLine(type: 3, storeName: "BAR DO JOÃO");
        var line2 = CreateCnabLine(type: 5, storeName: "LOJA DA MARIA");
        var line3 = CreateCnabLine(type: 1, storeName: "MERCADO JOSÉ");
        var cnabContent = $"{line1}\n{line2}\n{line3}";

        var stream = CreateStreamFromString(cnabContent);

        // Act
        var results = _parser.ParseFile(stream).ToList();

        // Assert
        results.ShouldNotBeEmpty();
        results.Count.ShouldBe(3);

        // First line
        results[0].Type.ShouldBe(3);
        results[0].StoreName.ShouldBe("BAR DO JOÃO");

        // Second line
        results[1].Type.ShouldBe(5);
        results[1].StoreName.ShouldBe("LOJA DA MARIA");

        // Third line
        results[2].Type.ShouldBe(1);
        results[2].StoreName.ShouldBe("MERCADO JOSÉ");
    }

    [Test]
    public void ParseFile_WithEmptyLines_ShouldSkipEmptyLines()
    {
        // Arrange
        var line1 = CreateCnabLine(type: 3, storeName: "BAR DO JOÃO");
        var line2 = CreateCnabLine(type: 5, storeName: "LOJA DA MARIA");
        var cnabContent = $"{line1}\n\n{line2}\n";

        var stream = CreateStreamFromString(cnabContent);

        // Act
        var results = _parser.ParseFile(stream).ToList();

        // Assert
        results.Count.ShouldBe(2);
    }

    [Test]
    public void ParseFile_WithSingleLine_ShouldParseSingleLine()
    {
        // Arrange
        var cnabContent = CreateCnabLine(type: 3);
        var stream = CreateStreamFromString(cnabContent);

        // Act
        var results = _parser.ParseFile(stream).ToList();

        // Assert
        results.Count.ShouldBe(1);
        results[0].Type.ShouldBe(3);
    }

    [Test]
    public void ParseFile_WithEmptyFile_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        var stream = CreateStreamFromString(string.Empty);

        // Act
        var results = _parser.ParseFile(stream).ToList();

        // Assert
        results.ShouldBeEmpty();
    }

    [Test]
    public void ParseFile_WithOnlyWhitespaceLines_ShouldReturnEmptyEnumerable()
    {
        // Arrange
        var cnabContent = @"


";
        var stream = CreateStreamFromString(cnabContent);

        // Act
        var results = _parser.ParseFile(stream).ToList();

        // Assert
        results.ShouldBeEmpty();
    }

    #endregion

    #region ParseFile - Invalid Cases

    [Test]
    public void ParseFile_WithNullStream_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => _parser.ParseFile(null!).ToList();

        // Assert
        act.ShouldThrow<ArgumentNullException>()
            .ParamName.ShouldBe("fileStream");
    }

    [Test]
    public void ParseFile_WithUnreadableStream_ShouldThrowArgumentException()
    {
        // Arrange - Create a write-only stream
        var stream = new MemoryStream();
        stream.Close();

        // Act
        Action act = () => _parser.ParseFile(stream).ToList();

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldContain("not readable");
    }

    [Test]
    public void ParseFile_WithInvalidLineInMiddle_ShouldThrowWithLineNumber()
    {
        // Arrange
        var line1 = CreateCnabLine(type: 3);
        var line3 = CreateCnabLine(type: 5);
        var cnabContent = $"{line1}\nINVALID_LINE\n{line3}";

        var stream = CreateStreamFromString(cnabContent);

        // Act
        Action act = () => _parser.ParseFile(stream).ToList();

        // Assert
        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("line 2");
    }

    [Test]
    public void ParseFile_WithInvalidLineFormat_ShouldIncludeDetailedError()
    {
        // Arrange
        var cnabContent = "3201903010000014200096206760174753****3153153453JOÃO"; // Too short

        var stream = CreateStreamFromString(cnabContent);

        // Act
        Action act = () => _parser.ParseFile(stream).ToList();

        // Assert
        var exception = act.ShouldThrow<InvalidOperationException>();
        exception.Message.ShouldContain("line 1");
        exception.InnerException.ShouldNotBeNull();
        exception.InnerException!.Message.ShouldContain("must be exactly 81 characters");
    }

    #endregion

    #region ParseFile - Integration Tests

    [Test]
    public void ParseFile_WithMultipleTransactionTypes_ShouldParseAllCorrectly()
    {
        // Arrange - Sample with all transaction types
        var lines = new List<string>();
        for (int i = 1; i <= 9; i++)
        {
            lines.Add(CreateCnabLine(type: i));
        }
        var cnabContent = string.Join("\n", lines);

        var stream = CreateStreamFromString(cnabContent);

        // Act
        var results = _parser.ParseFile(stream).ToList();

        // Assert
        results.Count.ShouldBe(9);

        for (int i = 0; i < 9; i++)
        {
            results[i].Type.ShouldBe(i + 1, $"Line {i + 1} should have transaction type {i + 1}");
        }
    }

    [Test]
    public void ParseFile_ReturnsEnumerableLazily_ShouldNotReadEntireFileImmediately()
    {
        // Arrange
        var line1 = CreateCnabLine(type: 3);
        var line2 = CreateCnabLine(type: 5);
        var line3 = CreateCnabLine(type: 1);
        var cnabContent = $"{line1}\n{line2}\n{line3}";

        var stream = CreateStreamFromString(cnabContent);

        // Act - Don't materialize the enumerable
        var results = _parser.ParseFile(stream);

        // Assert - Should not throw until enumerated
        results.ShouldNotBeNull();

        // Now enumerate first item only
        var firstItem = results.First();
        firstItem.Type.ShouldBe(3);
    }

    #endregion

    #region Helper Methods

    private static Stream CreateStreamFromString(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

    /// <summary>
    /// Creates a valid CNAB line with exactly 81 characters
    /// </summary>
    private static string CreateCnabLine(
        int type = 1,
        string date = "20190301",
        long value = 142000,
        string cpf = "09620676017",
        string card = "4753****3153",
        string time = "153453",
        string storeOwner = "JOÃO MACEDO",
        string storeName = "BAR DO JOÃO")
    {
        // Ensure fields are the correct length
        var typeStr = type.ToString().PadLeft(1, '0');
        var dateStr = date.PadRight(8);
        var valueStr = value.ToString().PadLeft(10, '0');
        var cpfStr = cpf.PadRight(11);
        var cardStr = card.PadRight(12);
        var timeStr = time.PadRight(6);
        var ownerStr = storeOwner.PadRight(14);
        var nameStr = storeName.PadRight(19);

        var line = typeStr + dateStr + valueStr + cpfStr + cardStr + timeStr + ownerStr + nameStr;

        if (line.Length != 81)
            throw new InvalidOperationException($"Generated line is {line.Length} characters, expected 81");

        return line;
    }

    #endregion

    #region Additional Data Validation Tests

    [Test]
    public void ParseLine_WithDateField_ShouldPreserveExactFormat()
    {
        // Arrange
        var line = CreateCnabLine(date: "20190301");

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.Date.ShouldBe("20190301");
        result.Date.Length.ShouldBe(8);
    }

    [Test]
    public void ParseLine_WithTimeField_ShouldPreserveExactFormat()
    {
        // Arrange
        var line = CreateCnabLine(time: "153453");

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.Time.ShouldBe("153453");
        result.Time.Length.ShouldBe(6);
    }

    [Test]
    public void ParseLine_WithCpfField_ShouldPreserveAllDigits()
    {
        // Arrange
        var line = CreateCnabLine(cpf: "01234567890");

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.Cpf.ShouldBe("01234567890");
        result.Cpf.Length.ShouldBe(11);
    }

    [Test]
    public void ParseLine_WithCardField_ShouldPreserveAllCharacters()
    {
        // Arrange
        var line = CreateCnabLine(card: "4753****3153");

        // Act
        var result = _parser.ParseLine(line);

        // Assert
        result.CardNumber.ShouldBe("4753****3153");
        result.CardNumber.Length.ShouldBe(12);
    }

    #endregion
}
