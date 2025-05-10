using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignature.Curves
{
    public class EdwardsCurve
    {
        //eu^2 + v^2 = 1 + du^2 * v^2 (mod p)
        public VeryBigInteger p { get; }
        public FmodElement e { get; }
        public FmodElement d { get; }
        public VeryBigInteger countOfPoints { get; private set; }
        public string Name { get; set; } = "";

        public bool isGoodCurve { get; private set; }

        private static EdwardsCurve Id_tc26_gost_3410_2012_256_paramSetA = null;
        private static EdwardsCurve Id_tc26_gost_3410_2012_512_paramSetC = null;

        public EdwardsCurve(VeryBigInteger e, VeryBigInteger d, VeryBigInteger p, VeryBigInteger countOfPoints = null)
        {
            this.p = p;
            this.e = new FmodElement(e, p);
            this.d = new FmodElement(d, p);
            if(!(countOfPoints is null))
            {
                this.countOfPoints = countOfPoints;
            }
        }

        private EdwardsCurve(VeryBigInteger e, VeryBigInteger d, VeryBigInteger p, VeryBigInteger countOfPoints = null, bool isGoodCurve = false)
        {
            this.p = p;
            this.e = new FmodElement(e, p);
            this.d = new FmodElement(d, p);
            if (!(countOfPoints is null))
            {
                this.countOfPoints = countOfPoints;
            }
            this.isGoodCurve = isGoodCurve;
        }

        #region КривыеИзГОСТ
        public static EdwardsCurve id_tc26_gost_3410_2012_256_paramSetA()
        {
            EdwardsCurve curve;
            if (Id_tc26_gost_3410_2012_256_paramSetA is null)
            {
                var p = new VeryBigInteger("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD97", true);
                var e = new VeryBigInteger("01", true);
                var d = new VeryBigInteger("0605F6B7C183FA81578BC39CFAD518132B9DF62897009AF7E522C32D6DC7BFFB", true);
                var m = new VeryBigInteger("01000000000000000000000000000000003F63377F21ED98D70456BD55B0D8319C", true);
                curve = new EdwardsCurve(e, d, p, m, true);
                curve.Name = "id-tc26-gost-3410-2012-256-paramSetA";
            }
            else
            {
                curve = Id_tc26_gost_3410_2012_256_paramSetA;
            }

            return curve;
        }

        public static EdwardsCurve id_tc26_gost_3410_2012_512_paramSetC()
        {
            EdwardsCurve curve;
            if (Id_tc26_gost_3410_2012_512_paramSetC is null)
            {
                var p = new VeryBigInteger("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFDC7", true);
                var e = new VeryBigInteger("01", true);
                var d = new VeryBigInteger("009E4F5D8C017D8D9F13A5CF3CDF5BFE4DAB402D54198E31EBDE28A0621050439CA6B39E0A515C06B304E2CE43E79E369E91A0CFC2BC2A22B4CA302DBB33EE7550", true);
                var m = new VeryBigInteger("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF26336E91941AAC0130CEA7FD451D40B323B6A79E9DA6849A5188F3BD1FC08FB4", true);
                curve = new EdwardsCurve(e, d, p, m, true);
                curve.Name = "id-tc26-gost-3410-2012-512-paramSetC";
            }
            else
            {
                curve = Id_tc26_gost_3410_2012_512_paramSetC;
            }
            
            return curve;
        }
        #endregion

        public EdwardsCurvePoint GeneratePoint()
        {
            List<FmodElement> vVariants = null;
            FmodElement u = new FmodElement(1, p);

            while (vVariants is null && u != 0)
            {
                u = new FmodElement(VeryBigInteger.NextRandomNumber(), p);
                var vChislitel = new FmodElement((u * u * e).Value - 1, p);
                var vZnamenatel = new FmodElement((u * u * d).Value - 1, p);
                var vSquare = vChislitel / vZnamenatel;
                vVariants = FmodElement.Sqrt(vSquare);
            }

            var random = new Random();
            var v = vVariants[random.Next(vVariants.Count - 1)];
            var groupGenerator = new EdwardsCurvePoint(u, v, this);

            return groupGenerator * 2;
        }

        public static bool operator ==(EdwardsCurve firstValue, EdwardsCurve secondValue)
        {
            if(firstValue.e == secondValue.e && firstValue.d == secondValue.d && firstValue.p == secondValue.p)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool operator !=(EdwardsCurve firstValue, EdwardsCurve secondValue)
        {
            if (firstValue.e == secondValue.e && firstValue.d == secondValue.d && firstValue.p == secondValue.p)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public override string ToString()
        {
            return e.ToString() + "* u^2 + v^2 = 1 + " + d.ToString() + " * u^2*v^2 (mod " + p.ToString() + ")";
        }
    }

    public class EdwardsCurvePoint
    {
        public EdwardsCurve edwardsCurve { get; private set; }
        public FmodElement x { get; }
        public FmodElement y { get; }
        private VeryBigInteger order = null;

        public EdwardsCurvePoint(FmodElement x, FmodElement y, EdwardsCurve edwardsCurve)
        {
            if (edwardsCurve.p != x.p || edwardsCurve.p != y.p) throw new ArgumentException("Точка и кривая должны существовать в одном поле");

            this.edwardsCurve = edwardsCurve;
            this.x = x;
            this.y = y;
        }

        public EdwardsCurvePoint Zero()
        {
            var xCootdinate = new FmodElement(VeryBigInteger.Zero(), x.p);
            var yCootdinate = new FmodElement(VeryBigInteger.One(), x.p);
            return new EdwardsCurvePoint(xCootdinate, yCootdinate, edwardsCurve);
        }
        public VeryBigInteger Order
        {
            get
            {
                if(order is null)
                {
                    FillOrder();
                }
                return order;
            }
        }

        private void FillOrder()
        {
            var pointsCnt = edwardsCurve.countOfPoints;

            var dividers = new List<VeryBigInteger>();
            dividers.Add(new VeryBigInteger(1));

            if (edwardsCurve.isGoodCurve)
            {
                dividers.Add(new VeryBigInteger(2));
                dividers.Add(new VeryBigInteger(4));
                dividers.Add(edwardsCurve.countOfPoints / 4);
                dividers.Add(edwardsCurve.countOfPoints / 2);
                dividers.Add(edwardsCurve.countOfPoints);
            }
            else
            {
                var primes = PrimeNumbers.Numbers;
                var temp = new VeryBigInteger(pointsCnt.value);
                var tempIsPrime = false;
                for (int i = 0; i < primes.Count && temp > VeryBigInteger.One() && !tempIsPrime; i++)
                {
                    var temp2 = VeryBigInteger.One();
                    while (temp % primes[i] == 0)
                    {
                        temp2 *= primes[i];
                        dividers.Add(temp2);
                        dividers.Add(temp / primes[i]);
                        temp /= primes[i];
                    }
                    tempIsPrime = temp.IsPrime();
                }

                if (!tempIsPrime)
                {
                    for (VeryBigInteger i = primes.Last() + 2; i * i < temp; i += 2)
                    {
                        var temp2 = VeryBigInteger.One();
                        while (temp % i == 0)
                        {
                            temp2 *= i;
                            dividers.Add(i);
                            dividers.Add(temp / i);
                            temp /= i;
                        }
                    }
                }
                dividers.Sort();
                dividers.Add(edwardsCurve.countOfPoints);
            }

            foreach (var divider in dividers)
            {
                if(this * divider == Zero())
                {
                    order = divider;
                    break;
                }
            }
        }

        public EdwardsCurvePoint OppositePoint()
        {
            return new EdwardsCurvePoint(-1 * x, y, edwardsCurve);
        }

        public static EdwardsCurvePoint operator +(EdwardsCurvePoint firstValue, EdwardsCurvePoint secondValue)
        {
            var p = firstValue.edwardsCurve.p;

            var xChislitel = firstValue.x * secondValue.y + secondValue.x * firstValue.y;
            var xZnamenatel = 1 + firstValue.edwardsCurve.d * firstValue.x * firstValue.y * secondValue.x * secondValue.y;
            var x = xChislitel / xZnamenatel;

            var yChislitel = firstValue.y * secondValue.y - firstValue.edwardsCurve.e * firstValue.x * secondValue.x;
            var yZnamenatel = 1 - firstValue.edwardsCurve.d * firstValue.x * firstValue.y * secondValue.x * secondValue.y;
            var y = yChislitel / yZnamenatel;

            return new EdwardsCurvePoint(x, y, firstValue.edwardsCurve);

        }

        public static EdwardsCurvePoint operator *(EdwardsCurvePoint x, VeryBigInteger k)
        {
            if (k == 1)
                return x;
            if (k == 2)
                return x + x;
            if ((k & 1) == 0)
            {
                var temp = x + x;
                return temp * (k >> 1);
            }
            else
                return x + x * (k - 1);
        }
        public static EdwardsCurvePoint operator *(VeryBigInteger k, EdwardsCurvePoint x)
        {
            return x * k;
        }
        public static EdwardsCurvePoint operator *(EdwardsCurvePoint x, int k)
        {
            return x * new VeryBigInteger(k);
        }
        public static EdwardsCurvePoint operator *(int k, EdwardsCurvePoint x)
        {
            return x * k;
        }


        public static bool operator ==(EdwardsCurvePoint firstValue, EdwardsCurvePoint secondValue)
        {
            if (firstValue.x == secondValue.x && firstValue.y == secondValue.y && firstValue.edwardsCurve == secondValue.edwardsCurve)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool operator !=(EdwardsCurvePoint firstValue, EdwardsCurvePoint secondValue)
        {
            if (firstValue.x == secondValue.x && firstValue.y == secondValue.y && firstValue.edwardsCurve == secondValue.edwardsCurve)
            {
                return false;
            }
            else
            {
                return true;
            }

        }



        public override string ToString()
        {
            return "(" + x.ToString() + ", " + y.ToString() + ")";
        }

        #region ТочкиИзГОСТ
        public static EdwardsCurvePoint id_tc26_gost_3410_2012_256_paramSetA()
        {
            var edwardCurve = EdwardsCurve.id_tc26_gost_3410_2012_256_paramSetA();
            var u = new VeryBigInteger("0D", true);
            var v = new VeryBigInteger("60CA1E32AA475B348488C38FAB07649CE7EF8DBE87F22E81F92B2592DBA300E7", true);

            return new EdwardsCurvePoint(new FmodElement(u, edwardCurve.p), new FmodElement(v, edwardCurve.p), edwardCurve);
        }
        public static EdwardsCurvePoint id_tc26_gost_3410_2012_512_paramSetC()
        {
            var edwardCurve = EdwardsCurve.id_tc26_gost_3410_2012_512_paramSetC();
            var u = new VeryBigInteger("12", true);
            var v = new VeryBigInteger("469AF79D1FB1F5E16B99592B77A01E2A0FDFB0D01794368D9A56117F7B38669522DD4B650CF789EEBF068C5D139732F0905622C04B2BAAE7600303EE73001A3D", true);

            return new EdwardsCurvePoint(new FmodElement(u, edwardCurve.p), new FmodElement(v, edwardCurve.p), edwardCurve);
        }
        #endregion
    }
}
