using System;
using System.Numerics;

namespace DigitalSignature
{
   
    class EllipticCurve
    {
        //y^2 = x^3 + ax + b (mod p)
        public VeryBigInteger p { get; }
        public FmodElement a { get; }
        public FmodElement b { get; }
        public FmodElement j { get; }
        public VeryBigInteger countOfPoints { get; }

        public EllipticCurve(VeryBigInteger a, VeryBigInteger b, VeryBigInteger p)
        {
            if(!p.IsPrime()) throw new ArgumentException("Число р должно быть простым");
            this.p = p;
            this.a = new FmodElement(a, p);
            this.b = new FmodElement(b, p);
            var fmod1728 = new FmodElement(1728, p);
            var fmod4 = new FmodElement(4, p);
            var fmod27 = new FmodElement(27, p);
            this.j = fmod1728 * fmod4 * this.a * this.a * this.a / (fmod4 * this.a * this.a * this.a + fmod27 * this.b * this.b);

            this.countOfPoints = CountOfPoints();

        }
        private VeryBigInteger CountOfPoints()
        {
            VeryBigInteger count = VeryBigInteger.One(); // Учитываем точку на бесконечности
            FmodElement x = new FmodElement(VeryBigInteger.Zero(), p);
            do
            {
                FmodElement y1, y2;
                try
                {
                    var sqrts = FmodElement.Sqrt(x * x * x + x * this.a + this.b);
                    y1 = sqrts[0];
                    y2 = sqrts[1];
                }
                catch
                {
                    x += 1;
                    continue;
                }

                if (y1 == y2)
                    count++;
                else
                    count += 2;

                x += 1;

            }
            while (x != 0);

            return count;
        }
    }

    class EllipticCurvePoint
    {
        EllipticCurve ellipticCurve;
        public FmodElement x { get; }
        public FmodElement y { get; }
        bool isPInfinity;
        public VeryBigInteger order { get; private set; }

        public EllipticCurvePoint(FmodElement x, FmodElement y, EllipticCurve ellipticCurve, bool isPInfinity = false)
        {
            if (ellipticCurve.p != x.p || ellipticCurve.p != y.p) throw new ArgumentException("Точка и кривая должны существовать в одном поле");

            this.ellipticCurve = ellipticCurve;
            this.x = x;
            this.y = y;
            this.isPInfinity = isPInfinity;
            FillOrder();
        }

        private void FillOrder()
        {
            var pointsCnt = ellipticCurve.countOfPoints;

            if (this.isPInfinity)
            {
                order = pointsCnt;
                return;
            }

            for (var i = new VeryBigInteger(2); (i << 2) <= pointsCnt; i++)
            {
                if (pointsCnt % i == 0)
                {
                    var temp = this * i;
                    if (temp + this == this || temp.isPInfinity)
                    {
                        order = i;
                        return;
                    }
                }
            }
            order = pointsCnt;
        }

        public static EllipticCurvePoint operator +(EllipticCurvePoint firstValue, EllipticCurvePoint secondValue)
        {
            var p = firstValue.ellipticCurve.p;
            if (firstValue.x != secondValue.x)
            {
                var lambda = (secondValue.y - firstValue.y) / (secondValue.x - firstValue.x);
                var resultX = lambda * lambda - firstValue.x - secondValue.x;
                var resultY = lambda * (firstValue.x - resultX) - firstValue.y;
                return new EllipticCurvePoint(resultX, resultY, firstValue.ellipticCurve);
            }

            if (firstValue.y == secondValue.y && firstValue.y != new FmodElement(0, p))
            {
                var lambda = (new FmodElement(3, p) * firstValue.x * firstValue.x + firstValue.ellipticCurve.a) / (new FmodElement(2, p) * firstValue.y);
                var resultX = lambda * lambda - firstValue.x - secondValue.x;
                var resultY = lambda * (firstValue.x - resultX) - firstValue.y;
                return new EllipticCurvePoint(resultX, resultY, firstValue.ellipticCurve);
            }
            
            return new EllipticCurvePoint(new FmodElement(0, p), new FmodElement(0, p), firstValue.ellipticCurve, true);

        }

        public static EllipticCurvePoint operator *(EllipticCurvePoint x, VeryBigInteger k)
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

        public override string ToString()
        {
            return "(" + x.ToString() + ", " + y.ToString() + ")";
        }
    }
}
