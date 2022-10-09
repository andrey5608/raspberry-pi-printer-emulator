using System;

namespace EscPosDecoderApi
{
    public class Utils
    {
        public static int ConvertToCents(double amountInEuro)
        {
            return Convert.ToInt32(Math.Floor(amountInEuro * 100));
        }
    }
}
