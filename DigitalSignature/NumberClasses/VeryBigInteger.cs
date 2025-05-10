using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalSignature
{
    public class VeryBigInteger:IComparable<VeryBigInteger>
    {
        public BigInteger value { get; private set; }

        public VeryBigInteger(BigInteger value)
        {
            this.value = value;
        }
        public VeryBigInteger(int value, bool isBinaryLog = false)
        {
            
            if(isBinaryLog)
            {
                this.value = BigInteger.Pow(new BigInteger(2), value);
            }
            else
            {
                this.value = value;
            }
            
        }
        public VeryBigInteger(string hexString, bool isHexString = false)
        {
            value = 0;

            int convertBase = 0;
            if (isHexString)
            {
                convertBase = 16;
            }
            else
            {
                convertBase = 10;
            }

            BigInteger pow = 1;
            for (int i = hexString.Length - 1; i >= 0; i--)
            {
                switch (hexString[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        value += pow * (int)hexString[i];
                        break;
                    case 'a':
                    case 'A':
                        value += pow * 10;
                        break;
                    case 'b':
                    case 'B':
                        value += pow * 11;
                        break;
                    case 'c':
                    case 'C':
                        value += pow * 12;
                        break;
                    case 'd':
                    case 'D':
                        value += pow * 13;
                        break;
                    case 'e':
                    case 'E':
                        value += pow * 14;
                        break;
                    case 'f':
                    case 'F':
                        value += pow * 15;
                        break;
                }
                pow *= convertBase;
            }
        }

        #region Static Methods
        public static VeryBigInteger Max(VeryBigInteger firstElement, VeryBigInteger secondElement)
        {
            return new VeryBigInteger(BigInteger.Max(firstElement.value, secondElement.value));
        }
        public static VeryBigInteger Min(VeryBigInteger firstElement, VeryBigInteger secondElement)
        {
            return new VeryBigInteger(BigInteger.Min(firstElement.value, secondElement.value));
        }

        public static double Log(VeryBigInteger element, int logBase)
        {
            return BigInteger.Log(element.value, logBase);
        }

        //возвращает ближайшее целое снизу число к корню
        public static VeryBigInteger Sqrt(VeryBigInteger element)
        {
            var log10 = BigInteger.Log10(element.value);
            var countOfDigits = (int)log10;
            if (log10 > countOfDigits)
            {
                countOfDigits++;
            }
            int countOfSqrtDigits = (countOfDigits >> 1) + (countOfDigits & 1);
            var minVariantOfSqrt = BigInteger.Pow(10, countOfSqrtDigits - 1);
            var maxVariantOfSqrt = BigInteger.Pow(10, countOfSqrtDigits) - 1;

            if (maxVariantOfSqrt * maxVariantOfSqrt == element.value)
            {
                return new VeryBigInteger(maxVariantOfSqrt);
            }
            if (minVariantOfSqrt * minVariantOfSqrt == element.value)
            {
                return new VeryBigInteger(minVariantOfSqrt);
            }

            BigInteger midSqrt = minVariantOfSqrt;
            while (midSqrt * midSqrt != element.value && minVariantOfSqrt + 1 != maxVariantOfSqrt)
            {

                midSqrt = (minVariantOfSqrt + maxVariantOfSqrt) >> 1;

                if (midSqrt * midSqrt > element.value)
                {
                    maxVariantOfSqrt = midSqrt;
                }
                else
                {
                    minVariantOfSqrt = midSqrt;
                }
            }

            if (midSqrt * midSqrt == element.value)
            {
                return new VeryBigInteger(midSqrt);
            }

            return new VeryBigInteger(minVariantOfSqrt);

        }

        public static VeryBigInteger Pow(VeryBigInteger element, VeryBigInteger exponent)
        {
            var binExponent = (int)VeryBigInteger.Log(exponent, 2) + 1;
            var binExponents = new VeryBigInteger[binExponent];

            var tempExponent = new VeryBigInteger(exponent.value);
            int i = 0;
            while(tempExponent > 0)
            {
                if((tempExponent & 1) == 1)
                {
                    binExponents[i] = VeryBigInteger.One();
                }
                else
                {
                    binExponents[i] = VeryBigInteger.Zero();
                }
                tempExponent = tempExponent >> 1;
                i++;
            }

            tempExponent = new VeryBigInteger(element.value);
            for(i = 0; i < binExponent; i++)
            {
                if(binExponents[i] == 1)
                {
                    binExponents[i] = tempExponent;
                }
                tempExponent *= tempExponent;
            }

            var result = VeryBigInteger.Zero();
            for (i = 0; i < binExponent; i++)
            {
                result += binExponents[i];
            }

            return result;
        }
        public static VeryBigInteger Pow(VeryBigInteger element, int exponent)
        {
            return Pow(element, new VeryBigInteger(exponent));
        }
        public static VeryBigInteger Pow(VeryBigInteger element, VeryBigInteger exponent, VeryBigInteger mod)
        {
            var tempExponentDiv = exponent;
            var tempExponent = element;
            var result = VeryBigInteger.One();
            while (tempExponentDiv > 0)
            {
                if ((tempExponentDiv & 1) == 1)
                {
                    result *= tempExponent;
                    if (result > mod)
                    {
                        result = result % mod;
                    }
                }
                tempExponentDiv = tempExponentDiv >> 1;
                tempExponent *= tempExponent;
                if (tempExponent > mod)
                {
                    tempExponent = tempExponent % mod;
                }

            }

            return result;
        }
        public static VeryBigInteger Pow(VeryBigInteger element, int exponent, VeryBigInteger mod)
        {
            return Pow(element, new VeryBigInteger(exponent), mod);
        }

        public static VeryBigInteger Zero()
        {
            return new VeryBigInteger(0);
        }
        public static VeryBigInteger One()
        {
            return new VeryBigInteger(1);
        }

        public static VeryBigInteger Parse(string number)
        {
            return new VeryBigInteger(BigInteger.Parse(number));
        }

        public static VeryBigInteger NextRandomNumber()
        {
            var state = new VeryBigInteger(DateTime.Now.Millisecond);
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        //firstNumber * u + secondNumber * v = GCD(firstNumber, secondNumber)
        public static VeryBigInteger GCD(VeryBigInteger firstNumber, VeryBigInteger secondNumber, out VeryBigInteger u, out VeryBigInteger v)
        {
            var a = new VeryBigInteger(firstNumber.value);    
            var b = new VeryBigInteger(secondNumber.value);
            bool reverseUV = false;
            if (b < a)
            {
                var t = a;
                a = b;
                b = t;
                reverseUV = true;
            }

            if (a == 0)
            {
                u = VeryBigInteger.Zero();
                v = VeryBigInteger.One();
                return b;
            }

            var gcd = GCD(b % a, a, out u, out v);

            var newY = u;
            var newX = v - (b / a) * u;

            if (reverseUV)
            {
                u = newY;
                v = newX;
            }
            else 
            {
                u = newX;
                v = newY;
            }
            return gcd;
        }
        #endregion

        public bool IsPrime()
        {
            if ((this & 1) == 0) return false;
            var strongBases = PrimeNumbers.Numbers;
            foreach(var primeBase in strongBases)
            {
                if (this % primeBase == 0) return false;
            }

            //известно, что для чисел, меньших phi13 достаточно проверить 13 оснований
            var phi13 = new VeryBigInteger("3317044064679887385961981");
            var maxCheckedBase = strongBases[strongBases.Count - 1];
            int maxCheckedBaseIndex;
            if (this < phi13)
            {
                maxCheckedBaseIndex = 13;
            }
            else
            {
                var countOfDigits = this.ToString().Length;
                maxCheckedBaseIndex = (int)Math.Ceiling(11.52 * countOfDigits * countOfDigits);
                
            }
            maxCheckedBase = strongBases[maxCheckedBaseIndex];

            //Тест Миллера-Рабина
            var temp = new VeryBigInteger(value - 1);
            var s = 0;
            var t = new VeryBigInteger(0);
            
            while ((temp & 1) == 0)
            {
                s += 1;
                temp = temp >> 1;
            }
            t = temp;

            var countOfTasks = Environment.ProcessorCount;
            var tasks = new Task<bool>[countOfTasks];

            for(int i = 0; i < countOfTasks; i++)
            {
                var startIndex = maxCheckedBaseIndex / countOfTasks * i;
                var endIndex = maxCheckedBaseIndex / countOfTasks * (i + 1);
                Task<bool> task = Task<bool>.Run(() => MillerRabinTest(startIndex, endIndex, t, s));
                tasks[i] = task;
            }

            bool result = true;
            for(int i = 0; i < countOfTasks; i++)
            {
                result = result & tasks[i].Result;
            }

            return result;
        }

        private bool MillerRabinTest(int startIndex, int endIndex, VeryBigInteger t, int s)
        {
            
            var strongBases = PrimeNumbers.Numbers;
            var modulos = PrimeNumbers.Modulos;

            var thisSquare = this * this;
            var summaryModulos = VeryBigInteger.One();
            var countOfModulos = 0;
           
            for(int i = 0; i < modulos.Count && summaryModulos < thisSquare; i++)
            {
                summaryModulos *= modulos[i];
                countOfModulos++;
            }

            var M = new List<VeryBigInteger>();
            for(int i = 0; i < countOfModulos; i++)
            {
                M.Add(summaryModulos / modulos[i]);
            }

            for (int i = 0; i < countOfModulos; i++)
            {
                var u = VeryBigInteger.Zero();
                var v = VeryBigInteger.Zero();
                GCD(M[i], modulos[i], out u, out v);
                while (u < 0)
                {
                    u += summaryModulos;
                }
                M[i] = M[i] * u;
                
            }

            var xOnModulos = new FmodElement[countOfModulos];

            for (int i = startIndex; i <= endIndex; i++)
            {
                var primeBase = strongBases[i];

                VeryBigInteger x = VeryBigInteger.Pow(primeBase, t, this);

                if (x == 1 || x == this - 1)
                {
                    continue;
                }

                for (int j = 0; j < countOfModulos; j++)
                {
                    xOnModulos[j] = new FmodElement(x, modulos[j]);
                }

                bool nextIteration = false;
                for (int j = 0; j < s; j++)
                {
                    x = VeryBigInteger.Zero();
                    for (int k = 0; k < countOfModulos; k++)
                    {
                        xOnModulos[k] = xOnModulos[k] * xOnModulos[k];
                    }

                    for (int k = 0; k < countOfModulos; k++)
                    {
                        x += xOnModulos[k].Value * M[k];
                        if (x > summaryModulos)
                        {
                            x = x % summaryModulos;
                        }
                    }

                    if(x > this)
                    {
                        x = x % this;
                        for (int k = 0; k < countOfModulos; k++)
                        {
                            xOnModulos[k] = new FmodElement(x, modulos[k]);
                        }
                    }

                    if(x == 1) return false;

                    if(x == this - 1)
                    {
                        nextIteration = true;
                        break;
                    }
                }

                if (!nextIteration)
                {
                    return false;
                }
            }

            return true;
        }

        #region Перегрузка арифметических операций
            public static VeryBigInteger operator +(VeryBigInteger firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue.value + secondValue.value);
            }
            public static VeryBigInteger operator +(VeryBigInteger firstValue, int secondValue)
            {
                return new VeryBigInteger(firstValue.value + secondValue);
            }
            public static VeryBigInteger operator +(int firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue + secondValue.value);
            }

            public static VeryBigInteger operator ++(VeryBigInteger firstValue)
            {
                firstValue.value++;
                return firstValue;
            }

            public static VeryBigInteger operator -(VeryBigInteger firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue.value - secondValue.value);
            }
            public static VeryBigInteger operator -(VeryBigInteger firstValue, int secondValue)
            {
                return new VeryBigInteger(firstValue.value - secondValue);
            }
            public static VeryBigInteger operator -(int firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue - secondValue.value);
            }

            public static VeryBigInteger operator --(VeryBigInteger firstValue)
            {
                firstValue.value--;
                return firstValue;
            }

            public static VeryBigInteger operator *(VeryBigInteger firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue.value * secondValue.value);
            }
            public static VeryBigInteger operator *(VeryBigInteger firstValue, int secondValue)
            {
                return new VeryBigInteger(firstValue.value * secondValue);
            }
            public static VeryBigInteger operator *(int firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue * secondValue.value);
            }

            public static VeryBigInteger operator /(VeryBigInteger firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue.value / secondValue.value);
            }
            public static VeryBigInteger operator /(VeryBigInteger firstValue, int secondValue)
            {
                return new VeryBigInteger(firstValue.value / secondValue);
            }
            public static VeryBigInteger operator /(int firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue / secondValue.value);
            }

            public static VeryBigInteger operator %(VeryBigInteger firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue.value % secondValue.value);
            }
            public static VeryBigInteger operator %(VeryBigInteger firstValue, int secondValue)
            {
                return new VeryBigInteger(firstValue.value % secondValue);
            }
            public static VeryBigInteger operator %(int firstValue, VeryBigInteger secondValue)
            {
                return new VeryBigInteger(firstValue % secondValue.value);
            }

            #endregion

        #region Перегрузка операторов сравнения

        public static bool operator ==(VeryBigInteger firstValue, VeryBigInteger secondValue)
        {

            if (firstValue is null && secondValue is null)
            {
                return true;
            }

            if (firstValue is null)
            {
                return false;
            }

            if (secondValue is null)
            {
                return false;
            }

            return firstValue.value == secondValue.value;
        }
        public static bool operator ==(VeryBigInteger firstValue, int secondValue)
        {
            return firstValue.value == secondValue;
        }
        public static bool operator ==(int firstValue, VeryBigInteger secondValue)
        {
            return firstValue == secondValue.value;
        }

        public static bool operator !=(VeryBigInteger firstValue, VeryBigInteger secondValue)
        {
            if(firstValue is null && secondValue is null)
            {
                return true;
            }
            
            if(firstValue is null)
            {
                return false;
            }

            if(secondValue is null)
            {
                return false;
            }

            return firstValue.value != secondValue.value;
        }
        public static bool operator !=(VeryBigInteger firstValue, int secondValue)
        {
            return firstValue.value != secondValue;
        }
        public static bool operator !=(int firstValue, VeryBigInteger secondValue)
        {
            return firstValue != secondValue.value;
        }

        public static bool operator <(VeryBigInteger firstValue, VeryBigInteger secondValue)
        {
            return firstValue.value < secondValue.value;
        }
        public static bool operator <(VeryBigInteger firstValue, int secondValue)
        {
            return firstValue.value < secondValue;
        }
        public static bool operator <(int firstValue, VeryBigInteger secondValue)
        {
            return firstValue < secondValue.value;
        }

        public static bool operator >(VeryBigInteger firstValue, VeryBigInteger secondValue)
        {
            return firstValue.value > secondValue.value;
        }
        public static bool operator >(VeryBigInteger firstValue, int secondValue)
        {
            return firstValue.value > secondValue;
        }
        public static bool operator >(int firstValue, VeryBigInteger secondValue)
        {
            return firstValue > secondValue.value;
        }

        public static bool operator <=(VeryBigInteger firstValue, VeryBigInteger secondValue)
        {
            return firstValue.value <= secondValue.value;
        }
        public static bool operator <=(VeryBigInteger firstValue, int secondValue)
        {
            return firstValue.value <= secondValue;
        }
        public static bool operator <=(int firstValue, VeryBigInteger secondValue)
        {
            return firstValue <= secondValue.value;
        }

        public static bool operator >=(VeryBigInteger firstValue, VeryBigInteger secondValue)
        {
            return firstValue.value >= secondValue.value;
        }
        public static bool operator >=(VeryBigInteger firstValue, int secondValue)
        {
            return firstValue.value >= secondValue;
        }
        public static bool operator >=(int firstValue, VeryBigInteger secondValue)
        {
            return firstValue >= secondValue.value;
        }

        
        #endregion

        #region Перегрузка битовых операций
        public static VeryBigInteger operator >>(VeryBigInteger firstValue, int secondValue)
        {
            return new VeryBigInteger(firstValue.value >> secondValue);
        }

        public static VeryBigInteger operator <<(VeryBigInteger firstValue, int secondValue)
        {
            return new VeryBigInteger(firstValue.value << secondValue);
        }

        public static VeryBigInteger operator &(VeryBigInteger firstValue, int secondValue)
        {
            return new VeryBigInteger(firstValue.value & secondValue);
        }
        public static VeryBigInteger operator ^(VeryBigInteger firstValue, VeryBigInteger secondValue)
        {
            return new VeryBigInteger(firstValue.value ^ secondValue.value);
        }

        #endregion

        public override string ToString()
        {
            return value.ToString();
        }

        public int CompareTo(VeryBigInteger obj)
        {
            return value.CompareTo(obj.value);
        }
    }
}
