using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace DigitalSignature
{
    public static class PrimeNumbers
    {
        private static int needeCountOfPrimes = 276768;
        private static List<VeryBigInteger> numbers = null;
        private static List<VeryBigInteger> modulos = null;

        public static List<VeryBigInteger> Numbers
        {
            get
            {
                if (!(numbers is null)) return numbers;

                numbers = new List<VeryBigInteger>();

                var exePath = AppDomain.CurrentDomain.BaseDirectory;//path to exe file
                var path = Path.Combine(exePath, "..\\..\\..\\DigitalSignature\\NumberClasses\\primes.txt");
                using (var sr = new StreamReader(path))
                {
                    int i = 0;
                    while (i<=needeCountOfPrimes)
                    {
                        i++;
                        var elem = sr.ReadLine();
                        if (elem != null)
                        {
                            numbers.Add(VeryBigInteger.Parse(elem));
                        }
                        else
                        {
                            break;
                        }

                    }
                }
                return numbers;
            }
        }
        
        public static List<VeryBigInteger> Modulos
        {
            get
            {
                if (modulos is null)
                {
                    modulos = new List<VeryBigInteger>();
                    modulos.Add(new VeryBigInteger("340282366920938463463374607431768211507"));
                    modulos.Add(new VeryBigInteger("340282366920938463463374607431768211537"));
                    modulos.Add(new VeryBigInteger("340282366920938463463374607431768211621"));
                    modulos.Add(new VeryBigInteger("340282366920938463463374607431768211729"));
                    modulos.Add(new VeryBigInteger("340282366920938463463374607431768211841"));
                    modulos.Add(new VeryBigInteger("340282366920938463463374607431768211877"));
                    modulos.Add(new VeryBigInteger("340282366920938463463374607431768211919"));
                    modulos.Add(new VeryBigInteger("340282366920938463463374607431768212029"));
                }
                return modulos;
            }

        }

    }
}
