using DesafioDev.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace DesafioDev.Domain.UnitTests.ValueObjects;

[TestFixture]
public class MoneyTests
{
    #region FromCnabValue Tests

    [Test]
    public void FromCnabValue_WithValidValue_ShouldStoreCnabValueAsIs()
    {
        // Arrange
        long cnabValue = 12345;
        decimal expectedDecimalValue = 123.45m;

        // Act
        var money = Money.FromCnabValue(cnabValue);

        // Assert
        money.ShouldNotBeNull();
        money.Value.ShouldBe(cnabValue);
        money.ToDecimal().ShouldBe(expectedDecimalValue);
    }

    [TestCase(0, 0.00)]
    [TestCase(1, 0.01)]
    [TestCase(100, 1.00)]
    [TestCase(999, 9.99)]
    [TestCase(10000, 100.00)]
    [TestCase(123456789, 1234567.89)]
    public void FromCnabValue_WithDifferentValues_ShouldConvertToDecimalCorrectly(long cnabValue, decimal expectedDecimalValue)
    {
        // Act
        var money = Money.FromCnabValue(cnabValue);

        // Assert
        money.Value.ShouldBe(cnabValue);
        money.ToDecimal().ShouldBe(expectedDecimalValue);
    }

    [Test]
    public void FromCnabValue_WithZero_ShouldReturnZeroMoney()
    {
        // Arrange
        long cnabValue = 0;

        // Act
        var money = Money.FromCnabValue(cnabValue);

        // Assert
        money.Value.ShouldBe(0L);
        money.ToDecimal().ShouldBe(0m);
    }

    [Test]
    public void FromCnabValue_WithNegativeValue_ShouldThrowArgumentException()
    {
        // Arrange
        long cnabValue = -100;

        // Act
        Action act = () => Money.FromCnabValue(cnabValue);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("CNAB value cannot be negative (Parameter 'cnabValue')");
    }

    [Test]
    public void FromCnabValue_WithLargeValue_ShouldHandleCorrectly()
    {
        // Arrange
        long cnabValue = long.MaxValue;
        decimal expectedDecimalValue = cnabValue / 100.00m;

        // Act
        var money = Money.FromCnabValue(cnabValue);

        // Assert
        money.Value.ShouldBe(cnabValue);
        money.ToDecimal().ShouldBe(expectedDecimalValue);
    }

    #endregion

    #region FromDecimal Tests

    [Test]
    public void FromDecimal_WithValidValue_ShouldReturnMoneyInstance()
    {
        // Arrange
        decimal value = 123.45m;
        long expectedCentsValue = 12345L;

        // Act
        var money = Money.FromDecimal(value);

        // Assert
        money.ShouldNotBeNull();
        money.Value.ShouldBe(expectedCentsValue);
        money.ToDecimal().ShouldBe(value);
    }

    [TestCase(0.00)]
    [TestCase(0.01)]
    [TestCase(1.00)]
    [TestCase(9.99)]
    [TestCase(100.00)]
    [TestCase(1234567.89)]
    public void FromDecimal_WithDifferentValues_ShouldStoreCorrectly(decimal value)
    {
        // Act
        var money = Money.FromDecimal(value);

        // Assert
        money.ToDecimal().ShouldBe(value);
    }

    [Test]
    public void FromDecimal_WithZero_ShouldReturnZeroMoney()
    {
        // Arrange
        decimal value = 0m;

        // Act
        var money = Money.FromDecimal(value);

        // Assert
        money.Value.ShouldBe(0L);
        money.ToDecimal().ShouldBe(0m);
    }

    [Test]
    public void FromDecimal_WithNegativeValue_ShouldThrowArgumentException()
    {
        // Arrange
        decimal value = -10.50m;

        // Act
        Action act = () => Money.FromDecimal(value);

        // Assert
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Money value cannot be negative (Parameter 'value')");
    }

    [Test]
    public void FromDecimal_WithLargeValue_ShouldHandleCorrectly()
    {
        // Arrange
        decimal value = 999999999999.99m;
        long expectedCentsValue = 99999999999999L;

        // Act
        var money = Money.FromDecimal(value);

        // Assert
        money.Value.ShouldBe(expectedCentsValue);
        money.ToDecimal().ShouldBe(value);
    }

    [Test]
    public void FromDecimal_WithPreciseDecimalValue_ShouldTruncateToCents()
    {
        // Arrange
        decimal value = 123.456789m;
        long expectedCentsValue = 12345L; // Truncates to 123.45

        // Act
        var money = Money.FromDecimal(value);

        // Assert
        money.Value.ShouldBe(expectedCentsValue);
        money.ToDecimal().ShouldBe(123.45m);
    }

    #endregion

    #region Equality Tests

    [Test]
    public void Equals_WithSameMoneyValue_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.50m);
        var money2 = Money.FromDecimal(100.50m);

        // Act & Assert
        money1.Equals(money2).ShouldBeTrue();
        (money1 == money2).ShouldBeTrue();
        (money1 != money2).ShouldBeFalse();
    }

    [Test]
    public void Equals_WithDifferentMoneyValue_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.50m);
        var money2 = Money.FromDecimal(200.75m);

        // Act & Assert
        money1.Equals(money2).ShouldBeFalse();
        (money1 == money2).ShouldBeFalse();
        (money1 != money2).ShouldBeTrue();
    }

    [Test]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var money = Money.FromDecimal(100.50m);

        // Act & Assert
        money.Equals(null).ShouldBeFalse();
        (money == null).ShouldBeFalse();
        (money != null).ShouldBeTrue();
    }

    [Test]
    public void GetHashCode_WithSameMoneyValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.50m);
        var money2 = Money.FromDecimal(100.50m);

        // Act & Assert
        money1.GetHashCode().ShouldBe(money2.GetHashCode());
    }

    [Test]
    public void GetHashCode_WithDifferentMoneyValue_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.50m);
        var money2 = Money.FromDecimal(200.75m);

        // Act & Assert
        money1.GetHashCode().ShouldNotBe(money2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Test]
    public void ToString_ShouldReturnFormattedCurrency()
    {
        // Arrange
        var money = Money.FromDecimal(100.50m);

        // Act
        var result = money.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        // Should contain the numeric values (culture-independent check)
        result.ShouldMatch(@"100[.,]50", "currency format should contain the value");
    }

    [Test]
    public void ToString_WithZero_ShouldReturnFormattedZero()
    {
        // Arrange
        var money = Money.FromDecimal(0m);

        // Act
        var result = money.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldMatch(@"0[.,]00", "currency format should contain zero");
    }

    [Test]
    public void ToString_WithSmallValue_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.FromDecimal(0.01m);

        // Act
        var result = money.ToString();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldMatch(@"0[.,]01", "currency format should contain the value");
    }

    #endregion

    #region Edge Cases and Business Logic Tests

    [Test]
    public void FromCnabValue_AndFromDecimal_WithEquivalentValues_ShouldBeEqual()
    {
        // Arrange
        long cnabValue = 12345; // 123.45
        decimal decimalValue = 123.45m;

        // Act
        var moneyFromCnab = Money.FromCnabValue(cnabValue);
        var moneyFromDecimal = Money.FromDecimal(decimalValue);

        // Assert
        moneyFromCnab.ShouldBe(moneyFromDecimal);
        moneyFromCnab.Value.ShouldBe(moneyFromDecimal.Value);
        moneyFromCnab.ToDecimal().ShouldBe(moneyFromDecimal.ToDecimal());
    }

    [Test]
    public void FromCnabValue_Precision_ShouldBeTwoDecimalPlaces()
    {
        // Arrange
        long cnabValue = 12399; // 123.99

        // Act
        var money = Money.FromCnabValue(cnabValue);

        // Assert
        money.Value.ShouldBe(12399L);
        money.ToDecimal().ShouldBe(123.99m);
    }

    #endregion
}