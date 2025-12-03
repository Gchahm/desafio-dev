using DesafioDev.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace DesafioDev.Domain.UnitTests.ValueObjects;

[TestFixture]
public class CardNumberTests
{
    #region Valid CardNumber Creation Tests

    [Test]
    public void Create_WithValidUnformattedCardNumber_ShouldReturnCardNumberInstance()
    {
        // Arrange
        var validCardNumber = "123456789012";

        // Act
        var cardNumber = CardNumber.Create(validCardNumber);

        // Assert
        cardNumber.ShouldNotBeNull();
        cardNumber.Value.ShouldBe(validCardNumber);
    }

    [TestCase("000000000000")]
    [TestCase("111111111111")]
    [TestCase("123456789012")]
    [TestCase("999999999999")]
    [TestCase("543210987654")]
    public void Create_WithDifferentValidCardNumbers_ShouldSucceed(string validCardNumber)
    {
        // Act
        var cardNumber = CardNumber.Create(validCardNumber);

        // Assert
        cardNumber.ShouldNotBeNull();
        cardNumber.Value.ShouldBe(validCardNumber);
    }

    [Test]
    public void Create_WithFormattedCardNumber_ShouldRemoveFormatting()
    {
        // Arrange
        var formattedCardNumber = "1234-5678-9012";
        var expectedUnformatted = "123456789012";

        // Act
        var cardNumber = CardNumber.Create(formattedCardNumber);

        // Assert
        cardNumber.Value.ShouldBe(expectedUnformatted);
    }

    [Test]
    public void Create_WithSpaces_ShouldRemoveSpaces()
    {
        // Arrange
        var cardNumberWithSpaces = "1234 5678 9012";
        var expectedUnformatted = "123456789012";

        // Act
        var cardNumber = CardNumber.Create(cardNumberWithSpaces);

        // Assert
        cardNumber.Value.ShouldBe(expectedUnformatted);
    }

    [Test]
    public void Create_WithMixedFormatting_ShouldRemoveAllNonDigits()
    {
        // Arrange
        var mixedFormat = "1234-5678 9012";
        var expectedUnformatted = "123456789012";

        // Act
        var cardNumber = CardNumber.Create(mixedFormat);

        // Assert
        cardNumber.Value.ShouldBe(expectedUnformatted);
    }

    #endregion

    #region Invalid CardNumber Creation Tests

    [Test]
    public void Create_WithNullCardNumber_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => CardNumber.Create(null!);

        // Assert
        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe("Card number cannot be null or empty (Parameter 'cardNumber')");
    }

    [Test]
    public void Create_WithEmptyCardNumber_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => CardNumber.Create(string.Empty);

        // Assert
        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe("Card number cannot be null or empty (Parameter 'cardNumber')");
    }

    [Test]
    public void Create_WithWhitespaceCardNumber_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => CardNumber.Create("   ");

        // Assert

        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe("Card number cannot be null or empty (Parameter 'cardNumber')");
    }

    [TestCase("123")]
    [TestCase("12345")]
    [TestCase("12345678901")] // 11 digits
    [TestCase("1234567890123")] // 13 digits
    [TestCase("123456789012345")] // 15 digits
    public void Create_WithInvalidLength_ShouldThrowArgumentException(string invalidCardNumber)
    {
        // Act
        Action act = () => CardNumber.Create(invalidCardNumber);

        // Assert
        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe("Card number must have exactly 12 digits (Parameter 'cardNumber')");
    }

    [Test]
    public void Create_WithLetters_ShouldThrowArgumentException()
    {
        // Arrange
        var cardNumberWithLetters = "1234ABCD5678";

        // Act
        Action act = () => CardNumber.Create(cardNumberWithLetters);

        // Assert
        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe("Card number must have exactly 12 digits (Parameter 'cardNumber')");
    }

    [Test]
    public void Create_WithSpecialCharactersOnly_ShouldThrowArgumentException()
    {
        // Arrange
        var specialChars = "****-****-****";

        // Act
        Action act = () => CardNumber.Create(specialChars);

        // Assert
        var ex = act.ShouldThrow<ArgumentException>();
        ex.Message.ShouldBe("Card number must have exactly 12 digits (Parameter 'cardNumber')");
    }

    #endregion

    #region Formatting Tests

    [Test]
    public void ToMaskedString_ShouldMaskAllButLastFourDigits()
    {
        // Arrange
        var cardNumber = CardNumber.Create("123456789012");
        var expectedMasked = "****-****-9012";

        // Act
        var masked = cardNumber.ToMaskedString();

        // Assert
        masked.ShouldBe(expectedMasked);
    }

    [TestCase("123456789012", "****-****-9012")]
    [TestCase("000000001234", "****-****-1234")]
    [TestCase("999999995678", "****-****-5678")]
    [TestCase("111111110000", "****-****-0000")]
    public void ToMaskedString_WithDifferentCardNumbers_ShouldMaskCorrectly(string cardNum, string expectedMasked)
    {
        // Arrange
        var cardNumber = CardNumber.Create(cardNum);

        // Act
        var masked = cardNumber.ToMaskedString();

        // Assert
        masked.ShouldBe(expectedMasked);
    }

    [Test]
    public void ToFormattedString_ShouldReturnFormattedCardNumber()
    {
        // Arrange
        var cardNumber = CardNumber.Create("123456789012");
        var expectedFormatted = "1234-5678-9012";

        // Act
        var formatted = cardNumber.ToFormattedString();

        // Assert
        formatted.ShouldBe(expectedFormatted);
    }

    [TestCase("123456789012", "1234-5678-9012")]
    [TestCase("000000000000", "0000-0000-0000")]
    [TestCase("999999999999", "9999-9999-9999")]
    [TestCase("111122223333", "1111-2222-3333")]
    public void ToFormattedString_WithDifferentCardNumbers_ShouldFormatCorrectly(string cardNum,
        string expectedFormatted)
    {
        // Arrange
        var cardNumber = CardNumber.Create(cardNum);

        // Act
        var formatted = cardNumber.ToFormattedString();

        // Assert
        formatted.ShouldBe(expectedFormatted);
    }

    [Test]
    public void ToString_ShouldReturnUnformattedCardNumber()
    {
        // Arrange
        var cardNumberValue = "123456789012";
        var cardNumber = CardNumber.Create(cardNumberValue);

        // Act
        var result = cardNumber.ToString();

        // Assert
        result.ShouldBe(cardNumberValue);
    }

    #endregion

    #region Equality Tests

    [Test]
    public void Equals_WithDifferentCardNumberValue_ShouldReturnFalse()
    {
        // Arrange
        var cardNumber1 = CardNumber.Create("123456789012");
        var cardNumber2 = CardNumber.Create("987654321098");

        // Act & Assert
        cardNumber1.Equals(cardNumber2).ShouldBeFalse();
        (cardNumber1 == cardNumber2).ShouldBeFalse();
        (cardNumber1 != cardNumber2).ShouldBeTrue();
    }

    [Test]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var cardNumber = CardNumber.Create("123456789012");

        // Act & Assert
        cardNumber.Equals(null).ShouldBeFalse();
        (cardNumber == null).ShouldBeFalse();
        (cardNumber != null).ShouldBeTrue();
    }

    [Test]
    public void Equals_WithNullOnBothSides_ShouldReturnTrue()
    {
        // Arrange
        CardNumber? cardNumber1 = null;
        CardNumber? cardNumber2 = null;

        // Act & Assert
        (cardNumber1 == cardNumber2).ShouldBeTrue();
        (cardNumber1 != cardNumber2).ShouldBeFalse();
    }

    #endregion

    #region Value Property Tests

    [Test]
    public void Value_ShouldBeReadOnly()
    {
        // Arrange
        var cardNumber = CardNumber.Create("123456789012");

        // Act
        var value = cardNumber.Value;

        // Assert
        value.ShouldBe("123456789012");

        // Verify immutability
        var propertyInfo = typeof(CardNumber).GetProperty(nameof(CardNumber.Value));
        propertyInfo.ShouldNotBeNull();
        propertyInfo!.CanWrite.ShouldBeFalse("CardNumber should be immutable");
    }

    #endregion

    #region Edge Cases and Business Logic Tests

    [Test]
    public void Create_WithAllZeros_ShouldSucceed()
    {
        // Arrange
        var allZeros = "000000000000";

        // Act
        var cardNumber = CardNumber.Create(allZeros);

        // Assert
        cardNumber.ShouldNotBeNull();
        cardNumber.Value.ShouldBe(allZeros);
    }

    [Test]
    public void Create_WithAllNines_ShouldSucceed()
    {
        // Arrange
        var allNines = "999999999999";

        // Act
        var cardNumber = CardNumber.Create(allNines);

        // Assert
        cardNumber.ShouldNotBeNull();
        cardNumber.Value.ShouldBe(allNines);
    }

    [Test]
    public void Create_WithMultipleFormattingCharacters_ShouldCleanAndCreate()
    {
        // Arrange
        var messyFormat = "12-34 56.78/90:12";
        var expectedClean = "123456789012";

        // Act
        var cardNumber = CardNumber.Create(messyFormat);

        // Assert
        cardNumber.Value.ShouldBe(expectedClean);
    }

    [Test]
    public void ToMaskedString_WithAllZerosInLastFour_ShouldShowZeros()
    {
        // Arrange
        var cardNumber = CardNumber.Create("123456780000");

        // Act
        var masked = cardNumber.ToMaskedString();

        // Assert
        masked.ShouldBe("****-****-0000");
    }

    [Test]
    public void ToFormattedString_AndToMaskedString_ShouldHaveSameStructure()
    {
        // Arrange
        var cardNumber = CardNumber.Create("123456789012");

        // Act
        var formatted = cardNumber.ToFormattedString();
        var masked = cardNumber.ToMaskedString();

        // Assert
        formatted.Length.ShouldBe(masked.Length);
        formatted.Count(c => c == '-').ShouldBe(masked.Count(c => c == '-'));
    }

    [Test]
    public void Create_PreservesOnlyDigits()
    {
        // Arrange
        var input = "abc123def456ghi789jkl012";
        var expectedDigits = "123456789012";

        // Act
        var cardNumber = CardNumber.Create(input);

        // Assert
        cardNumber.Value.ShouldBe(expectedDigits);
    }

    [Test]
    public void Create_WithLeadingAndTrailingSpaces_ShouldClean()
    {
        // Arrange
        var input = "  123456789012  ";
        var expected = "123456789012";

        // Act
        var cardNumber = CardNumber.Create(input);

        // Assert
        cardNumber.Value.ShouldBe(expected);
    }

    #endregion

    #region Security and Privacy Tests

    [Test]
    public void ToMaskedString_ShouldNotRevealSensitiveDigits()
    {
        // Arrange
        var cardNumber = CardNumber.Create("123456789012");

        // Act
        var masked = cardNumber.ToMaskedString();

        // Assert
        masked.ShouldNotContain("1234");
        masked.ShouldNotContain("5678");
        masked.ShouldContain("9012");
        masked.ShouldContain("****");
    }

    [Test]
    public void ToMaskedString_ConsistentMasking()
    {
        // Arrange
        var cardNumber = CardNumber.Create("123456789012");

        // Act
        var masked1 = cardNumber.ToMaskedString();
        var masked2 = cardNumber.ToMaskedString();

        // Assert
        masked1.ShouldBe(masked2, "masking should be consistent");
    }

    #endregion
}