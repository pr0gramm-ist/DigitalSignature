using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DigitalSignature.Curves;
using System.Collections;

namespace DigitalSignature
{
    public class GOST3410_2018
    {
        public static bool ItsGoodCurve(EdwardsCurve edwardsCurve)
        {
            if (!edwardsCurve.p.IsPrime()) return false;
            if (edwardsCurve.countOfPoints == edwardsCurve.p) return false;
            var q = edwardsCurve.countOfPoints / 4;
            if (q.IsPrime() == false) return false;

            var number2_254 = new VeryBigInteger(254, true);
            var number2_256 = new VeryBigInteger(256, true);
            var number2_508 = new VeryBigInteger(508, true);
            var number2_512 = new VeryBigInteger(512, true);
            var B = 1;
            if(q > number2_254 && q < number2_256)
            {
                B = 31;
            }
            else if (q > number2_508 && q < number2_512)
            {
                B = 131;
            }
            else
            {
                return false;
            }

            var tempp = new FmodElement(edwardsCurve.p, q);
            for(int t = 1; t <= B; t++)
            {
                if (tempp == 1) return false;
                tempp *= edwardsCurve.p;
            }

            var pmod4 = edwardsCurve.p % 4;
            if(pmod4 == 3 && edwardsCurve.e.LegendreSymbol() == edwardsCurve.d.LegendreSymbol() && edwardsCurve.e.LegendreSymbol() == -1)
            {
                return false;
            }
            if (pmod4 == 1 && edwardsCurve.e.LegendreSymbol() == edwardsCurve.d.LegendreSymbol() && edwardsCurve.e.LegendreSymbol() == 1)
            {
                return false;
            }

            return true;
        }

        public static BitArray[] SignAMessage(byte[] messageBytes, EdwardsCurvePoint point, VeryBigInteger privateKey)
        {
            var signLen = 256;
            if(point.Order > new VeryBigInteger(260, true))
            {
                signLen = 512;
            }

            var temp = new byte[messageBytes.Length];
            for(int i = 0; i < temp.Length; i++)
            {
                temp[i] = messageBytes[i];
            }

            byte[] hash = Streebog.GetHash(messageBytes, signLen);
            var hashBits = new BitArray(hash);

            var e = bitsToNumber(hashBits);
            e = e % point.Order;
            if (e == 0) e = VeryBigInteger.One();

            var r = VeryBigInteger.Zero();
            var k = VeryBigInteger.One();
            var s = new FmodElement(0, point.Order);
            while (r == 0 && s == 0)
            {
                k = VeryBigInteger.NextRandomNumber() % point.Order;
                var C = k * point;
                r = C.x.Value % point.Order;

                s = new FmodElement(r * privateKey + k * e, point.Order);
            }

            var rVector = numberToBits(r, signLen);
            var sVector = numberToBits(s.Value, signLen);

            var sign = new BitArray[2];
            sign[0] = rVector;
            sign[1] = sVector;
            return sign;
        }
        public static async Task<BitArray[]> SignAMessageAsync(byte[] messageBytes, EdwardsCurvePoint point, VeryBigInteger privateKey)
        {
            var result = await Task.Run(() => SignAMessage(messageBytes, point, privateKey));
            return result;
        }

        public static bool CheckSign(byte[] messageBytes, BitArray[] sign, EdwardsCurvePoint P, EdwardsCurvePoint Q)
        {
            var r = bitsToNumber(sign[0]);
            if(r <= 0 || r >= P.Order)
            {
                return false;
            }
            var s = bitsToNumber(sign[1]);
            if (s <= 0 || s >= P.Order)
            {
                return false;
            }

            var signLen = sign[0].Length;
            byte[] hash = Streebog.GetHash(messageBytes, signLen);
            var hashBits = new BitArray(hash);

            var e = bitsToNumber(hashBits) % P.Order;
            if (e == 0) e = VeryBigInteger.One();

            var v = new FmodElement(e, P.Order);
            v = v.Reverse();

            var C = v.Value *(s * P + (r * Q).OppositePoint());

            if(C.x.Value % P.Order == r)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static async Task<bool> CheckSignAsync(byte[] messageBytes, BitArray[] sign, EdwardsCurvePoint P, EdwardsCurvePoint Q)
        {
            var result = await Task.Run(() => CheckSign(messageBytes, sign, P, Q));
            return result;
        }

        private static VeryBigInteger bitsToNumber(BitArray bits)
        {
            var binaryPow = VeryBigInteger.One();
            var result = VeryBigInteger.Zero();
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    result += binaryPow;
                }
                binaryPow *= 2;
            }
            return result;
        }

        private static BitArray numberToBits(VeryBigInteger number, int outLen)
        {
            var result = new BitArray(outLen);
            var temp = number;

            int i = 0;
            while(temp > 0)
            {
                var digit = (int)VeryBigInteger.Log(temp, 2);
                result[digit] = true;
                temp -= new VeryBigInteger(digit, true);

            }
            return result;

        }
    }
}
