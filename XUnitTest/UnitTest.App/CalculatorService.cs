using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.App
{
    public class CalculatorService: ICalculatorService
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public int Multip(int a, int b)
        {
            if (a == 0)
            {
                throw new Exception("a değeri 0 olamaz");
            }
            return a * b;
        }
    }
}
