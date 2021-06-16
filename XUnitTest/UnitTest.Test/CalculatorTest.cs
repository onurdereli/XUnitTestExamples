using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using UnitTest.App;
using Xunit;

namespace UnitTest.Test
{
    public class CalculatorTest
    {
        public Calculator Calculator { get; set; }
        public Mock<ICalculatorService> CalculatorServiceMock { get; set; }
        public CalculatorTest()
        {
            CalculatorServiceMock = new Mock<ICalculatorService>();

            this.Calculator = new Calculator(CalculatorServiceMock.Object);
        }

        [Theory]
        [InlineData(2,5,7)]
        public void Add_simpleValues_ReturnTotalValue(int a, int b, int expectedTotal)
        {
            CalculatorServiceMock.Setup(x => x.Add(a, b)).Returns(expectedTotal);
            var actualTotal = Calculator.Add(a, b);
            Assert.Equal(expectedTotal, actualTotal);

            CalculatorServiceMock.Verify(x=> x.Add(a,b), Times.Once);
        }

        [Theory]
        [InlineData(0, 5)]
        public void Multip_zeroValues_ReturnsException(int a, int b)
        {
            CalculatorServiceMock.Setup(x => x.Multip(a, b)).Throws(new Exception("a değeri 0 olamaz"));

            var exception = Assert.Throws<Exception>(()=> Calculator.Multip(a,b));

            Assert.Equal("a değeri 0 olamaz", exception.Message);
        }
    }
}
