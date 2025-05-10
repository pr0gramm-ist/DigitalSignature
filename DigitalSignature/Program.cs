using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignature
{
    class Program
    {
        static void Main(string[] args)
        {

            VeryBigInteger p = new VeryBigInteger(256, true);
            var upper = (p - 1) / 2;
            var result = new VeryBigInteger(0);
            for(VeryBigInteger i = new VeryBigInteger(0); i<=upper; i++)
            {
                result = (result + i);
                if(result > p)
                {
                    result = result % p;
                }    
            }
            Console.WriteLine(result);
        }
    }
}
