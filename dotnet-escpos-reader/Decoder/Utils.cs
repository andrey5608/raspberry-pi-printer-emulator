using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decoder
{
    public class Utils
    {
        public static int ConvertToCents(double amountInEuro)
        {
            return Convert.ToInt32(Math.Floor(amountInEuro * 100));
        }
    }
}
