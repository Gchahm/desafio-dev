using DesafioDev.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace DesafioDev.Domain.UnitTests.ValueObjects;

[TestFixture]
public class CPFTests
{
    #region Valid CPF Creation Tests

    [Test]
    public void Create_WithValidFormattedCPF_ShouldReturnCPFInstance()
    {
        // Arrange
        var formattedCpf = "123.456.789-09";
        var expectedUnformatted = "12345678909";

        // Act
        var cpf = CPF.Create(formattedCpf);

        // Assert
        cpf.ShouldNotBeNull();
        cpf.Value.ShouldBe(expectedUnformatted);
    }

    [Test]
    public void Create_WithValidCPFWithSpaces_ShouldRemoveSpacesAndReturnCPFInstance()
    {
        // Arrange
        var cpfWithSpaces = "123 456 789 09";
        var expectedUnformatted = "12345678909";

        // Act
        var cpf = CPF.Create(cpfWithSpaces);

        // Assert
        cpf.Value.ShouldBe(expectedUnformatted);
    }

    [TestCase("11144477735")] // Valid CPF
    [TestCase("52998224725")] // Valid CPF
    [TestCase("05137518743")] // Valid CPF
    public void Create_WithDifferentValidCPFs_ShouldSucceed(string validCpf)
    {
        // Act
        var cpf = CPF.Create(validCpf);

        // Assert
        cpf.ShouldNotBeNull();
        cpf.Value.ShouldBe(validCpf);
    }

    #endregion

    #region Invalid CPF Creation Tests

    [Test]
    public void Create_WithNullCPF_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => CPF.Create(null!);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("CPF cannot be null or empty (Parameter 'cpf')");
    }

    [Test]
    public void Create_WithEmptyCPF_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => CPF.Create(string.Empty);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("CPF cannot be null or empty (Parameter 'cpf')");
    }

    [Test]
    public void Create_WithWhitespaceCPF_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => CPF.Create("   ");

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("CPF cannot be null or empty (Parameter 'cpf')");
    }

    [TestCase("123")]
    [TestCase("12345")]
    [TestCase("123456789")]
    [TestCase("123456789012")] // 12 digits
    public void Create_WithInvalidLength_ShouldThrowArgumentException(string invalidCpf)
    {
        // Act
        Action act = () => CPF.Create(invalidCpf);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("CPF must have exactly 11 digits (Parameter 'cpf')");
    }

    [TestCase("12345678900")] // Wrong check digits
    [TestCase("11144477736")] // Wrong second check digit
    [TestCase("52998224726")] // Wrong second check digit
    public void Create_WithInvalidCheckDigits_ShouldThrowArgumentException(string invalidCpf)
    {
        // Act
        Action act = () => CPF.Create(invalidCpf);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Invalid CPF (Parameter 'cpf')");
    }

    #endregion

    #region Formatting Tests

    [Test]
    public void ToFormattedString_ShouldReturnFormattedCPF()
    {
        // Arrange
        var cpf = CPF.CreateUnchecked("12345678909");
        var expectedFormatted = "123.456.789-09";

        // Act
        var formatted = cpf.ToFormattedString();

        // Assert
        formatted.ShouldBe(expectedFormatted);
    }

    [Test]
    public void ToString_ShouldReturnUnformattedCPF()
    {
        // Arrange
        var cpfValue = "12345678909";
        var cpf = CPF.CreateUnchecked(cpfValue);

        // Act
        var result = cpf.ToString();

        // Assert
        result.ShouldBe(cpfValue);
    }

    #endregion

    #region Equality Tests

    [Test]
    public void Equals_WithSameCPFValue_ShouldReturnTrue()
    {
        // Arrange
        var cpf1 = CPF.CreateUnchecked("12345678909");
        var cpf2 = CPF.CreateUnchecked("12345678909");

        // Act & Assert
        cpf1.Equals(cpf2).ShouldBeTrue();
        (cpf1 == cpf2).ShouldBeTrue();
        (cpf1 != cpf2).ShouldBeFalse();
    }

    [Test]
    public void Equals_WithDifferentCPFValue_ShouldReturnFalse()
    {
        // Arrange
        var cpf1 = CPF.CreateUnchecked("12345678909");
        var cpf2 = CPF.CreateUnchecked("98765432100");

        // Act & Assert
        cpf1.Equals(cpf2).ShouldBeFalse();
        (cpf1 == cpf2).ShouldBeFalse();
        (cpf1 != cpf2).ShouldBeTrue();
    }

    [Test]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var cpf = CPF.CreateUnchecked("12345678909");

        // Act & Assert
        cpf.Equals(null).ShouldBeFalse();
        (cpf == null).ShouldBeFalse();
        (cpf != null).ShouldBeTrue();
    }

    [Test]
    public void GetHashCode_WithSameCPFValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var cpf1 = CPF.CreateUnchecked("12345678909");
        var cpf2 = CPF.CreateUnchecked("12345678909");

        // Act & Assert
        cpf1.GetHashCode().ShouldBe(cpf2.GetHashCode());
    }

    [Test]
    public void GetHashCode_WithDifferentCPFValue_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var cpf1 = CPF.CreateUnchecked("12345678909");
        var cpf2 = CPF.CreateUnchecked("98765432100");

        // Act & Assert
        cpf1.GetHashCode().ShouldNotBe(cpf2.GetHashCode());
    }

    #endregion

    #region Edge Cases

    [Test]
    public void Create_WithCPFContainingLetters_ShouldThrowArgumentException()
    {
        // Arrange
        var cpfWithLetters = "123ABC78909";

        // Act
        Action act = () => CPF.Create(cpfWithLetters);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("CPF must have exactly 11 digits (Parameter 'cpf')");
    }

    #endregion
}