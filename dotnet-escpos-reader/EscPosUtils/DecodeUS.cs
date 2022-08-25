/*

   Copyright (C) 2020 Kunio Fukuchi

   This software is provided 'as-is', without any express or implied
   warranty. In no event will the authors be held liable for any damages
   arising from the use of this software.

   Permission is granted to anyone to use this software for any purpose,
   including commercial applications, and to alter it and redistribute it
   freely, subject to the following restrictions:

   1. The origin of this software must not be misrepresented; you must not
      claim that you wrote the original software. If you use this software
      in a product, an acknowledgment in the product documentation would be
      appreciated but is not required.

   2. Altered source versions must be plainly marked as such, and must not be
      misrepresented as being the original software.

   3. This notice may not be removed or altered from any source distribution.

   Kunio Fukuchi

 */

namespace EscPosUtils
{
    using System;
    using System.Collections.Generic;

    public static partial class EscPosDecoder
    {
        //  US  #   1F 23 00/01/30/31 00-14
        internal static string DecodeVfdUsTurnAnnounciatorOnOff(EscPosCmd record, int index)
        {
            string mode;
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    mode = "OFF";
                    break;
                case 1:
                case 49:
                    mode = "ON";
                    break;
                default:
                    mode = "Undefined";
                    break;
            }

            byte number = record.cmddata[index + 1];
            string announciator = number <= 20 ? number.ToString("D", invariantculture) : "Out of range";
            return $"Mode:{mode}, Number:{announciator}";
        }

        //  US  $   1F 24 01-14 01/02
        internal static string DecodeVfdUsMoveCursorSpecifiedPosition(EscPosCmd record, int index)
        {
            byte x = record.cmddata[index];
            string column = ((x >= 1) && (x <= 20)) ? x.ToString("D", invariantculture) : "Out of range";
            string row;
            switch (record.cmddata[index + 1])
            {
                case 1:
                    row = "1";
                    break;
                case 2:
                    row = "2";
                    break;
                default:
                    row = "Out of range";
                    break;
            }

            return $"Column:{column}, Row:{row}";
        }

        //  US  ( A 1F 28 41 0003-FFFF 30 [30/31 00-FF]...
        internal static string DecodeVfdUsSelectDisplays(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, 3);
            if (length < 3)
            {
                return "Out of range";
            }
            else if ((length & 1) == 0)
            {
                return "Even length";
            }
            int count = (length - 1) / 2;
            List<string> displays = new List<string>();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 2)
            {
                string enable;
                switch (record.cmddata[currindex])
                {
                    case 48:
                        enable = "Disabled";
                        break;
                    case 49:
                        enable = "Enabled";
                        break;
                    default:
                        enable = "Undefined";
                        break;
                }

                displays.Add($"Setting:{enable} Display No.:{record.cmddata[currindex + 1]}");
            }
            return string.Join<string>(", ", displays);
        }

        //  US  ( E 1F 28 45 000A-FFFA 03 [09-0F 30-32]...
        internal static string DecodeVfdUsSetMemorySwitchValues(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, 3);
            if ((length < 10) && (length > 65530))
            {
                return "Out of range";
            }
            else if (((length - 1) % 9) != 0)
            {
                return "Miss align length";
            }
            int count = (length - 1) / 9;
            List<string> memorys = new List<string>();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 2)
            {
                string msw;
                switch (record.cmddata[currindex])
                {
                    case 9:
                        msw = "Backlight OFF setting";
                        break;
                    case 10:
                        msw = "Character code table";
                        break;
                    case 11:
                        msw = "International character set";
                        break;
                    case 12:
                        msw = "Brightness adjustment";
                        break;
                    case 13:
                        msw = "Specification of the peripheral device";
                        break;
                    case 14:
                        msw = "Display of the cursor";
                        break;
                    case 15:
                        msw = "Number of display";
                        break;
                    default:
                        msw = "Undefined";
                        break;
                }

                string setting = ascii.GetString(record.cmddata, (currindex + 1), 8).Replace('2', '_');
                memorys.Add($"MemorySwitch:{msw} Setting:{setting}");
            }
            return string.Join<string>(", ", memorys);
        }

        //  US  ( E 1F 28 45 02 00 04 09-0F/6D-70/72/73
        internal static string DecodeVfdUsSendingDisplayingMemorySwitchValues(EscPosCmd record, int index)
        {
            byte m = record.cmddata[index];
            if (((m >= 9) && (m <= 15)) || ((m >= 109) && (m <= 112)) || (m == 114) || (m == 115))
            {
                return m.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  US  ( G 1F 28 47 02 00 61 00/01/30/31
        internal static string DecodeVfdUsSelectKanjiCharacterCodeSystem(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "JIS";
                case 1:
                case 49:
                    return "ShiftJIS";
                default:
                    return "Undefined";
            }
        }

        //  US  T   1F 54 00-17 00-3B
        internal static string DecodeVfdUsSetAndDisplayCountTime(EscPosCmd record, int index)
        {
            string h = record.cmddata[index] <= 23 ? record.cmddata[index].ToString("D", invariantculture) : "Out of range";
            string m = record.cmddata[index + 1] <= 59 ? record.cmddata[index + 1].ToString("D", invariantculture) : "Out of range";
            return $"Hour:{h}, Minute:{m}";
        }

        //  US  X   1F 58 01-04
        internal static string DecodeVfdUsBrightnessAdjustment(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 1:
                    return "20%";
                case 2:
                    return "40%";
                case 3:
                    return "60%";
                case 4:
                    return "100%";
                default:
                    return "Undefined";
            }
        }

        //  US  ^   1F 5E 00-FF 00-FF
        internal static string DecodeVfdUsExecuteMacro(EscPosCmd record, int index)
        {
            return $"InterCharacter Delay:{record.cmddata[index]} x 20ms, InterMacro Idle:{record.cmddata[index + 1]} x 50ms";
        }

        //  US  v   1F 76 00/01/30/31
        internal static string DecodeVfdUsStatusConfirmationByDTRSignal(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "DTR Space";
                case 1:
                case 49:
                    return "DTR Mark";
                default:
                    return "Undefined";
            }
        }
    }
}