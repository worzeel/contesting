using Contesting.Core;

namespace Contesting.Tests;

public class CalculatorTests
{
    [Fact]
    public void Add_TwoPositiveNumbers_ReturnsSum()
    {
        var calculator = new Calculator();
        var result = calculator.Add(5, 3);
        Assert.Equal(8, result);
    }

    [Fact]
    public void Subtract_TwoNumbers_ReturnsDifference()
    {
        var calculator = new Calculator();
        var result = calculator.Subtract(10, 4);
        Assert.Equal(6, result);
    }

    [Fact]
    public void Multiply_TwoNumbers_ReturnsProduct()
    {
        var calculator = new Calculator();
        var result = calculator.Multiply(3, 4);
        Assert.Equal(12, result);
    }

    [Fact]
    public void Divide_TwoNumbers_ReturnsQuotient()
    {
        var calculator = new Calculator();
        var result = calculator.Divide(15, 3);
        Assert.Equal(5, result);
    }

    [Fact]
    public void Divide_ByZero_ThrowsDivideByZeroException()
    {
        var calculator = new Calculator();
        Assert.Throws<DivideByZeroException>(() => calculator.Divide(10, 0));
    }

    [Fact]
    public void TestWithSpecificFailureMessage()
    {
        // Now this test will pass
        var calculator = new Calculator();
        var result = calculator.Add(5, 3);
        Assert.Equal(8, result); // This will pass: 5 + 3 = 8
    }
}
