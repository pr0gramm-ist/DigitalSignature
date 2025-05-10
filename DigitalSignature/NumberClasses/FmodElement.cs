using System;
using System.Collections.Generic;

namespace DigitalSignature
{
    public class FmodElement
    {
        private VeryBigInteger value;
        public VeryBigInteger Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value % p;
            }
        }
        public VeryBigInteger p { get; }

        public FmodElement(VeryBigInteger value, VeryBigInteger p)
        {
            this.p = p;
            this.Value = value;
            while (this.value < 0)
            {
                this.value += p;
            }
        }
        public FmodElement(int value, VeryBigInteger p) : this(new VeryBigInteger(value), p) { }


        public FmodElement Reverse()
        {
            var temp = value;
            if (temp > p)
            {
                temp = temp % p;
            }
            while (temp < 0)
            {
                temp += p;
            }

            var u = VeryBigInteger.Zero();
            var v = VeryBigInteger.Zero();
            var result = VeryBigInteger.GCD(p, temp, out u, out v);
            
            return new FmodElement(v, p);
        }
        public static List<FmodElement> Sqrt(FmodElement x)
        {
            if(x.LegendreSymbol() != 1) return null;

            //алгоритм нахождения корня из числа из Теоретико-числовых методов криптографии
            var rest = x.p & 3; //остаток от деления на 4
            var m = x.p >> 2; //целая часть деления на 4

            var result = new List<FmodElement>();
            if(rest == 3)
            {
                var tempResult = new FmodElement(VeryBigInteger.Pow(x.value, m + 1, x.p), x.p);
                result.Add(tempResult);
                result.Add(tempResult * (-1));
            }
            else
            {
                rest = x.p & 7;//остаток от деления на 8
                m = x.p >> 3; //целая часть деления на 8
                if (rest == 5)
                {
                    var tempResult = new FmodElement(VeryBigInteger.Pow(x.Value, m, x.p), x.p);
                    result.Add(tempResult);
                    result.Add(tempResult * (-1));

                    tempResult = tempResult * new FmodElement(VeryBigInteger.Pow(new VeryBigInteger(2), 2 * m + 1, x.p), x.p);
                    result.Add(tempResult);
                    result.Add(tempResult * (-1));
                }
                else
                {
                    var h = x.p - 1;
                    while((h & 1) == 0)
                    {
                        h = h >> 1;
                    }
                    var temp1 = VeryBigInteger.Pow(x.Value, h, x.p);

                    var b = new FmodElement(2, x.p);
                    while(b.LegendreSymbol() != -1)
                    {
                        b += 1;
                    }
                    var s = new VeryBigInteger(1);
                    var bSquare = b * b;
                    var bPow = bSquare;
                    while(temp1 * bPow != 1)
                    {
                        s += 1;
                        bPow *= bSquare;
                    }

                    var temp2 = VeryBigInteger.Pow(x.Value, (h + 1) / 2, x.p);
                    var temp3 = VeryBigInteger.Pow(b.Value, s, x.p);
                    var tempResult = new FmodElement(temp2 * temp3, x.p);
                    result.Add(tempResult);
                    result.Add(tempResult * (-1));

                }
            }
            return result;
        }

        public int LegendreSymbol()
        {
            if(value == 0)
            {
                return 0;
            }

            var temp = VeryBigInteger.Pow(value, (p - 1) / 2, p);
            if (temp == 1)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        #region Перегрузка операторов
        public static FmodElement operator +(FmodElement firstValue, FmodElement secondValue)
        {
            if (firstValue.p != secondValue.p) throw new ArgumentException("Этот класс предназначен только для операций над одним полем");
            return new FmodElement((firstValue.value + secondValue.value) % firstValue.p, firstValue.p);
        }
        public static FmodElement operator +(FmodElement firstValue, int secondValue)
        {
            return new FmodElement((firstValue.value + secondValue) % firstValue.p, firstValue.p);
        }
        public static FmodElement operator +(int firstValue, FmodElement secondValue)
        {
            return new FmodElement((firstValue + secondValue.value) % secondValue.p, secondValue.p);
        }

        public static FmodElement operator -(FmodElement firstValue, FmodElement secondValue)
        {
            if (firstValue.p != secondValue.p) throw new ArgumentException("Этот класс предназначен только для операций над одним полем");
            return new FmodElement((firstValue.value - secondValue.value) % firstValue.p, firstValue.p);
        }
        public static FmodElement operator -(FmodElement firstValue, int secondValue)
        {
            return new FmodElement((firstValue.value - secondValue) % firstValue.p, firstValue.p);
        }
        public static FmodElement operator -(int firstValue, FmodElement secondValue)
        {
            return new FmodElement((firstValue - secondValue.value) % secondValue.p, secondValue.p);
        }

        public static FmodElement operator *(FmodElement firstValue, FmodElement secondValue)
        {
            if (firstValue.p != secondValue.p) throw new ArgumentException("Этот класс предназначен только для операций над одним полем");
            return new FmodElement((firstValue.value * secondValue.value) % firstValue.p, firstValue.p);
        }

        public static FmodElement operator *(FmodElement firstValue, int secondValue)
        {
            return new FmodElement((firstValue.value * secondValue) % firstValue.p, firstValue.p);
        }
        public static FmodElement operator *(int firstValue, FmodElement secondValue)
        {
            return new FmodElement((firstValue * secondValue.value) % secondValue.p, secondValue.p);
        }
        public static FmodElement operator *(FmodElement firstValue, VeryBigInteger secondValue)
        {
            return new FmodElement((firstValue.value * secondValue) % firstValue.p, firstValue.p);
        }
        public static FmodElement operator *(VeryBigInteger firstValue, FmodElement secondValue)
        {
            return new FmodElement((firstValue * secondValue.value) % secondValue.p, secondValue.p);
        }


        public static FmodElement operator /(FmodElement firstValue, FmodElement secondValue)
        {
            if (firstValue.p != secondValue.p) throw new ArgumentException("Этот класс предназначен только для операций над одним полем");
            return new FmodElement((firstValue.value * secondValue.Reverse().value) % firstValue.p, firstValue.p);
        }
        
        public static bool operator ==(FmodElement firstValue, FmodElement secondValue)
        {
            if (firstValue.p != secondValue.p) return false;
            return (firstValue.value % firstValue.p) == (secondValue.value % secondValue.p);
        }
        public static bool operator ==(FmodElement firstValue, int secondValue)
        {
            return (firstValue.value % firstValue.p) == (secondValue % firstValue.p);
        }

        public static bool operator !=(FmodElement firstValue, FmodElement secondValue)
        {
            if (firstValue.p != secondValue.p) return false;
            return (firstValue.value % firstValue.p) != (secondValue.value % secondValue.p);
        }
        public static bool operator !=(FmodElement firstValue, int secondValue)
        {
            return (firstValue.value % firstValue.p) != (secondValue % firstValue.p);
        }
        
        #endregion

        public override string ToString()
        {
            return value.ToString() + " mod " + p.ToString();
        }
    }

}
