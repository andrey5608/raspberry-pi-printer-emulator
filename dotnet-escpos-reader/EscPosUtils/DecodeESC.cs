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
    using System.Drawing;

    public static partial class EscPosDecoder
    {
        //  ESC !   1B 21 bx0xxx00x
        internal static string DecodeEscSelectPrintMode(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var underline = (mode & 0x80) == 0x80 ? "ON" : "OFF";
            var doublewidth = (mode & 0x20) == 0x20 ? "ON" : "OFF";
            var doubleheight = (mode & 0x10) == 0x10 ? "ON" : "OFF";
            var emphasize = (mode & 0x08) == 0x08 ? "ON" : "OFF";
            var font = (mode & 0x01) == 0x01 ? "B" : "A";
            return $"Underline:{underline}, DoubleWidth:{doublewidth}, DoubleHeight:{doubleheight}, Emphasize:{emphasize}, Font:{font}";
        }

        //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
        internal static string DecodeEscDefineUserDefinedCharacters1224(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index, 12, 24);
        }
        internal static string DecodeEscDefineUserDefinedCharacters1024(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index, 10, 24);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0924(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  9, 24);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0917(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  9, 17);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0909(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  9,  9);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0709(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  7,  9);
        }
        internal static string DecodeEscDefineUserDefinedCharacters0816(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index,  8, 16);
        }
        internal static string DecodeEscDefineUserDefinedCharacters(EscPosCmd record, int index, int maxwidth, int height)
        {
            var ybytes = record.cmddata[index];
            var startcode = record.cmddata[index + 1];
            var endcode = record.cmddata[index + 2];
            var count = startcode - endcode + 1;
            var i = index + 3;
            var chars = new List<string>();
            var glyphs = new List<Bitmap>();
            for (var n = 0; n < count; n++)
            {
                var xbytes = record.cmddata[i];
                var size = ybytes * xbytes;
                if (size > 0)
                {
                    glyphs.Add(GetBitmap(xbytes, height, ImageDataType.Column, record.cmddata, (i + 1), "1"));
                }
                else
                {
                    var bitmap = new Bitmap(maxwidth, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                    var palette = bitmap.Palette;
                    palette.Entries[0] = Color.White;
                    palette.Entries[1] = Color.Black;
                    bitmap.Palette = palette;
                    glyphs.Add(bitmap);
                }

                i += size + 1;
                chars.Add($"X:{xbytes} bytes, Size:{size}");
            }
            record.somebinary = glyphs.ToArray();
            var charslist = string.Join(", ", chars);
            return $"VerticalBytes:{ybytes}, StartCode:{startcode}, EndCode:{endcode}, Characters:{charslist}";
        }

        //  ESC ( A 1B 28 41 04 00 30 30-3A 01-3F 0A-FF
        internal static string DecodeEscBeeperBuzzer(EscPosCmd record, int index)
        {
            var pattern = record.cmddata[index];
            var cycles = record.cmddata[index + 1];
            var duration = record.cmddata[index + 2];
            var result = $"Cycles:{cycles}, Duration:{duration} x 100ms, Pattern:{pattern} is ";
            switch (pattern)
            {
                case 48:
                    result += "doesn't beep";
                    break;
                case 49:
                    result += "1320 Hz: 1000 ms beeping";
                    break;
                case 50:
                    result += "2490 Hz: 1000 ms beeping";
                    break;
                case 51:
                    result += "1320 Hz: 200 ms beeping";
                    break;
                case 52:
                    result += "2490 Hz: 200 ms beeping";
                    break;
                case 53:
                    result += "1320 Hz: 200 ms beeping → 200 ms off → 200 ms beeping";
                    break;
                case 54:
                    result += "2490 Hz: 200 ms beeping → 200 ms off → 200 ms beeping";
                    break;
                case 55:
                    result += "1320 Hz: 500 ms beeping";
                    break;
                case 56:
                    result += "2490 Hz: 500 ms beeping";
                    break;
                case 57:
                    result += "1320 Hz: 200 ms beeping → 200 ms off → 200 ms beeping → 200 ms off → 200 ms beeping";
                    break;
                case 58:
                    result += "2490 Hz: 200 ms beeping → 200 ms off → 200 ms beeping → 200 ms off → 200 ms beeping";
                    break;
                default:
                    result += "Undefined";
                    break;
            }

            return result;
        }

        //  ESC ( A 1B 28 41 03 00 61 01-07 00-FF
        internal static string DecodeEscBeeperBuzzerM1a(EscPosCmd record, int index)
        {
            var pattern = record.cmddata[index];
            var cycles = record.cmddata[index + 1];
            var result = $"Cycles:{cycles}, Pattern:{pattern} is ";
            switch (pattern)
            {
                case 1:
                    result += "A";
                    break;
                case 2:
                    result += "B";
                    break;
                case 3:
                    result += "C";
                    break;
                case 4:
                    result += "D";
                    break;
                case 5:
                    result += "E";
                    break;
                case 6:
                    result += "Error";
                    break;
                case 7:
                    result += "Paper-End";
                    break;
                default:
                    result += "Undefined";
                    break;
            }

            return result;
        }

        //  ESC ( A 1B 28 41 05 00 61 64 00-3F 00-FF 00-FF
        internal static string DecodeEscBeeperBuzzerM1b(EscPosCmd record, int index)
        {
            var cycles = record.cmddata[index];
            var onduration = record.cmddata[index + 1];
            var offduration = record.cmddata[index + 2];
            return $"Cycles:{cycles}, On-Duration:{onduration} x 100ms, Off-Duration:{offduration} x 100ms";
        }

        //  ESC ( A 1B 28 41 07 00 62 30-33 01 64 00/FF 01-32/FF 01-32
        internal static string DecodeEscBeeperBuzzerOffline(EscPosCmd record, int index)
        {
            string factor;
            switch (record.cmddata[index])
            {
                case 48:
                    factor = "Cover open";
                    break;
                case 49:
                    factor = "Paper end";
                    break;
                case 50:
                    factor = "Recoverable error";
                    break;
                case 51:
                    factor = "Unrecoverable error";
                    break;
                default:
                    factor = "Undefined";
                    break;
            }

            string beeptype;
            switch (record.cmddata[index + 3])
            {
                case 0:
                    beeptype = "OFF";
                    break;
                case 255:
                    beeptype = "Infinite";
                    break;
                default:
                    beeptype = "Undefined";
                    break;
            }

            var onduration = record.cmddata[index + 4];
            var offduration = record.cmddata[index + 5];
            return $"Factor:{factor}, Type:{beeptype}, On-Duration:{onduration} x 100ms, Off-Duration:{offduration} x 100ms";
        }

        //  ESC ( A 1B 28 41 07 00 63 30 01 64 00/FF 01-32/FF 01-32
        internal static string DecodeEscBeeperBuzzerNearEnd(EscPosCmd record, int index)
        {
            string beeptype;
            switch (record.cmddata[index])
            {
                case 0:
                    beeptype = "OFF";
                    break;
                case 255:
                    beeptype = "Infinite";
                    break;
                default:
                    beeptype = "Undefined";
                    break;
            }

            var onduration = record.cmddata[index + 1];
            string t1;
            if (onduration == 255)
            {
                t1 = "On-Duration:Infinite";
            }
            else if ((onduration >= 1) && (onduration <= 50))
            {
                t1 = $"On-Duration:{onduration} x 100ms";
            }
            else
            {
                t1 = $"On-Duration:{onduration} Out of range";
            }
            var offduration = record.cmddata[index + 2];
            string t2;
            if ((offduration >= 1) && (offduration <= 50))
            {
                t2 = $"Off-Duration:{offduration} x 100ms";
            }
            else
            {
                t2 = $"Off-Duration:{offduration} Out of range";
            }
            return $"Type:{beeptype}, {t1}, {t2}";
        }

        //  ESC ( Y 1B 28 59 02 00 00/01/30/31 00/01/30/31
        internal static string DecodeEscSpecifyBatchPrint(EscPosCmd record, int index)
        {
            string fanc;
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    fanc = "Print bufferd batch data";
                    break;
                case 1:
                case 49:
                    fanc = "Start batch buffering";
                    break;
                default:
                    fanc = "Undefined";
                    break;
            }

            string direction;
            switch (record.cmddata[index + 1])
            {
                case 0:
                case 48:
                    direction = "Normal direction";
                    break;
                case 1:
                case 49:
                    direction = "Reverse direction";
                    break;
                default:
                    direction = "Undefined";
                    break;
            }

            return $"{fanc}, {direction}";
        }

        //  ESC *   1B 2A 00/01/20/21 0001-0960 00-FF...
        internal static string DecodeEscSelectBitImageMode(EscPosCmd record, int index)
        {
            int height;
            switch (record.cmddata[index])
            {
                case 0:
                case 1:
                    height = 8;
                    break;
                case 32:
                case 33:
                    height = 24;
                    break;
                default:
                    height = -1;
                    break;
            }

            string modestr;
            switch (record.cmddata[index])
            {
                case 0:
                    modestr = "8 dot Single(low) density ";
                    break;
                case 1:
                    modestr = "8 dot Double(high) density";
                    break;
                case 32:
                    modestr = "24 dot Single(low) density";
                    break;
                case 33:
                    modestr = "24 dot Double(high) density";
                    break;
                default:
                    modestr = "Undefined";
                    break;
            }

            int width = BitConverter.ToUInt16(record.cmddata, index + 1);
            var widthstr = width.ToString("D", invariantculture);
            if ((height > 0) && ((width > 0)&&(width <= 0x960)))
            {
                record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 3), "1");
            }
            return $"Mode:{modestr}, Width:{widthstr} dot";
        }

        //  ESC -   1B 2D 00-02/30-32
        internal static string DecodeEscUnderlineMode(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "OFF";
                case 1:
                case 49:
                    return "ON 1 dot";
                case 2:
                case 50:
                    return "ON 2 dot";
                default:
                    return "Undefined";
            }
        }

        //  ESC =   1B 3D 01-03 or 00-FF
        internal static string DecodeEscSelectPeripheralDevice(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 1:
                    return "Printer";
                case 2:
                    return "LineDisplay";
                case 3:
                    return "Printer and LineDisplay";
                default:
                    return "Undefined";
            }
        }

        //  ESC ?   1B 3F 20-7E
        internal static string DecodeEscCancelUserDefinedCharacters(EscPosCmd record, int index)
        {
            return $"Code:{record.cmddata[index]}";
        }

        //  ESC D   1B 44 [01-FF]... 00
        internal static string DecodeEscHorizontalTabPosition(EscPosCmd record, int index)
        {
            var length = (int)(record.cmddata[record.cmdlength - 1] == 0 ? record.cmdlength - 3 : record.cmdlength - 2);
            string result;
            if (length > 0)
            {
                result = BitConverter.ToString(record.cmddata, index, length).Replace('-', ',');
            }
            else
            {
                result = "Clear all tab settings.";
            }
            return result;
        }

        //  ESC M   1B 4D 00-04/30-34/61/62
        internal static string DecodeEscSelectCharacterFont(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "A";
                case 1:
                case 49:
                    return "B";
                case 2:
                case 50:
                    return "C";
                case 3:
                case 51:
                    return "D";
                case 4:
                case 52:
                    return "E";
                case 97:
                    return "Special A";
                case 98:
                    return "Special B";
                default:
                    return "Undefined";
            }
        }

        //  ESC R   1B 52 00-11/42-4B/52
        internal static string DecodeEscSelectInternationalCharacterSet(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                    return "U.S.A.";
                case 1:
                    return "France";
                case 2:
                    return "Germany";
                case 3:
                    return "U.K.";
                case 4:
                    return "Denmark I";
                case 5:
                    return "Sweden";
                case 6:
                    return "Italy";
                case 7:
                    return "Spain I";
                case 8:
                    return "Japan";
                case 9:
                    return "Norway";
                case 10:
                    return "Denmark II";
                case 11:
                    return "Spain II";
                case 12:
                    return "Latin America";
                case 13:
                    return "Korea";
                case 14:
                    return "Slovenia / Croatia";
                case 15:
                    return "China";
                case 16:
                    return "Vietnam";
                case 17:
                    return "Arabia";
                case 66:
                    return "India (Devanagari)";
                case 67:
                    return "India (Bengali)";
                case 68:
                    return "India (Tamil)";
                case 69:
                    return "India (Telugu)";
                case 70:
                    return "India (Assamese)";
                case 71:
                    return "India (Oriya)";
                case 72:
                    return "India (Kannada)";
                case 73:
                    return "India (Malayalam)";
                case 74:
                    return "India (Gujarati)";
                case 75:
                    return "India (Punjabi)";
                case 82:
                    return "India (Marathi)";
                default:
                    return "Undefined";
            }
        }

        //  ESC T   1B 54 00-03/30-33
        internal static string DecodeEscSelectPrintDirection(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Left to Right : Normal";
                case 1:
                case 49:
                    return "Bottom to Top : Left90";
                case 2:
                case 50:
                    return "Right to Left : Rotate180";
                case 3:
                case 51:
                    return "Top to Bottom : Right90";
                default:
                    return "Undefined";
            }
        }

        //  ESC V   1B 56 00-02/30-32
        internal static string DecodeEscTurn90digreeClockwiseRotationMode(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Turns OFF 90 digree Clockwise Rotation";
                case 1:
                case 49:
                    return "Turns ON 90 digree Clockwise Rotation : 1 dot Character spacing";
                case 2:
                case 50:
                    return "Turns ON 90 digree Clockwise Rotation : 5 dot Character spacing";
                default:
                    return "Undefined";
            }
        }

        //  ESC W   1B 57 0000-FFFF 0000-FFFF 0001-FFFF 0001-FFFF
        internal static string DecodeEscSetPrintAreaInPageMode(EscPosCmd record, int index)
        {
            int top = BitConverter.ToUInt16(record.cmddata, 2);
            int left = BitConverter.ToUInt16(record.cmddata, 4);
            int width = BitConverter.ToUInt16(record.cmddata, 6);
            int height = BitConverter.ToUInt16(record.cmddata, 8);
            return $"Top:{top} dot, Left:{left} dot, Width:{width} dot, Height:{height} dot";
        }

        //  ESC a   1B 61 00-02/30-32
        internal static string DecodeEscSelectJustification(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Left";
                case 1:
                case 49:
                    return "Centered";
                case 2:
                case 50:
                    return "Right";
                default:
                    return "Undefined";
            }
        }

        //c ESC c 0 1B 63 30 b0000xxxx
        internal static string DecodeEscSelectPaperTypesPrinting(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var validation = (mode & 0x08) == 0x08 ? "Enable" : "Disable";
            var slip = (mode & 0x04) == 0x04 ? "Enable" : "Disable";
            string roll;
            switch ((mode & 0x03))
            {
                case 0:
                    roll = "Disable";
                    break;
                case 1:
                case 2:
                    roll = "Active Sheet";
                    break;
                case 3:
                    roll = "Enable";
                    break;
                default:
                    roll = "";
                    break;
            }

            return $"Validation paper:{validation}, Slip paper:{slip}, Roll paper:{roll}";
        }

        //c ESC c 1 1B 63 31 b0x00xxxx
        internal static string DecodeEscSelectPaperTypesCommandSettings(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var slipback = (mode & 0x20) == 0x20 ? "Enable" : "Disable";
            var validation = (mode & 0x08) == 0x08 ? "Enable" : "Disable";
            var slipface = (mode & 0x04) == 0x04 ? "Enable" : "Disable";
            string roll;
            switch ((mode & 0x03))
            {
                case 0:
                    roll = "Disable";
                    break;
                case 1:
                case 2:
                    roll = "Active Sheet";
                    break;
                case 3:
                    roll = "Enable";
                    break;
                default:
                    roll = "";
                    break;
            }

            return $"Validation paper:{validation}, Face of Slip paper:{slipface}, Back of Slip paper:{slipback}, Roll paper:{roll}";
        }

        //  ESC c 3 1B 63 33 bccccxxxx
        internal static string DecodeEscSelectPaperSensorsPaperEndSignals(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var validation = (mode & 0xC0) == 0x00 ? "Disable" : "Enable";
            var slipBOF = (mode & 0x20) == 0x00 ? "Disable" : "Enable";
            var slipTOF = (mode & 0x20) == 0x00 ? "Disable" : "Enable";
            var empty = (mode & 0x0C) == 0x00 ? "Disable" : "Enable";
            var nearend = (mode & 0x03) == 0x00 ? "Disable" : "Enable";
            return $"Validation:{validation}, SlipBOF:{slipBOF}, SlipTOF:{slipTOF}, Empty:{empty}, Near End:{nearend}";
        }

        //  ESC c 4 1B 63 34 bccccxxxx
        internal static string DecodeEscSelectPaperSensorsStopPrinting(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var validation = (mode & 0xC0) == 0x00 ? "Disable" : "Enable";
            var slipBOF = (mode & 0x20) == 0x00 ? "Disable" : "Enable";
            var slipTOF = (mode & 0x20) == 0x00 ? "Disable" : "Enable";
            var empty = (mode & 0x0C) == 0x00 ? "Disable" : "Enable";
            var nearend = (mode & 0x03) == 0x00 ? "Disable" : "Enable";
            return $"Validation:{validation}, SlipBOF:{slipBOF}, SlipTOF:{slipTOF}, Empty:{empty}, Near End:{nearend}";
        }

        //c ESC f   1B 66 00-0F 00-40
        internal static string DecodeEscCutSheetWaitTime(EscPosCmd record, int index)
        {
            var insert = record.cmddata[index];
            var inswait = (insert <= 15) ? insert.ToString("D", invariantculture) + " minute" : "Out of range";
            var detect = record.cmddata[index + 1];
            var detwait = (detect <= 64) ? detect.ToString("D", invariantculture) + " x 100ms" : "Out of range";
            return $"Insertion Wait:{inswait}, Detection Wait:{detwait}";
        }

        //  ESC p   1B 70 00/01/30/31 00-FF 00-FF
        internal static string DecodeEscGeneratePulse(EscPosCmd record, int index)
        {
            string pin;
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    pin = "2";
                    break;
                case 1:
                case 49:
                    pin = "5";
                    break;
                default:
                    pin = "Undefined";
                    break;
            }

            var onduration = record.cmddata[index + 1];
            var offduration = record.cmddata[index + 2];
            return $"Pin:{pin}, On-Duration:{onduration} x 100ms, Off-Duration:{offduration} x 100ms";
        }

        //  ESC r   1B 72 00/01/30/31
        internal static string DecodeEscSelectPrinterColor(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Black";
                case 1:
                case 49:
                    return "Red";
                default:
                    return "Undefined";
            }
        }

        public static Dictionary<byte, string> GetEmbeddedESCtCodePage(int embeddedcodepage)
        {
            switch (embeddedcodepage)
            {
                case 6:
                    return s_cp06_hiragana; // Page 6: Hiragana Not suitable code page but alternative definition
                case 7:
                    return s_cp07_Kanji01; // Page 7:One-pass printing Kanji characters
                case 8:
                    return s_cp08_Kanji02; // Page 8:One-pass printing Kanji characters
                case 11:
                    return s_cp851; // PC851: Greek
                case 12:
                    return s_cp853; // PC853: Turkish
                case 20:
                    return s_cp20_Thai_CC_42; // Page 20 Thai Character Code 42 Not suitable code page but alternative definition
                case 21:
                    return s_cp21_Thai_CC_11; // Page 21 Thai Character Code 11 Not suitable code page but alternative definition
                case 22:
                    return s_cp22_Thai_CC_13; // Page 22 Thai Character Code 13 Not suitable code page but alternative definition
                case 23:
                    return s_cp23_Thai_CC_14; // Page 23 Thai Character Code 14 Not suitable code page but alternative definition
                case 24:
                    return s_cp24_Thai_CC_16; // Page 24 Thai Character Code 16 Not suitable code page but alternative definition
                case 25:
                    return s_cp25_Thai_CC_17; // Page 25 Thai Character Code 17 Not suitable code page but alternative definition
                case 26:
                    return s_cp26_Thai_CC_18; // Page 26 Thai Character Code 18 Not suitable code page but alternative definition
                case 30:
                    return s_cp30_TCVN_3; // Page 30 TCVN-3: Vietnamese Not suitable code page but alternative definition
                case 31:
                    return s_cp31_TCVN_3; // Page 31 TCVN-3: Vietnamese Not suitable code page but alternative definition
                case 41:
                    return s_cp1098; // PC1098: Farsi
                case 42:
                    return s_cp1118; // PC1118: Lithuanian
                case 43:
                    return s_cp1119; // PC1119: Lithuanian
                case 44:
                    return s_cp1125; // PC1125: Ukrainian
                case 53:
                    return s_kz1048; // KZ-1048: Kazakhstan Not suitable code page but alternative definition
                case 254:
                case 255: // Page255
                default:
                    return s_cpASCII; // Page254
            }
        }

        public static readonly Dictionary<byte, int> PrtESCtCodePage = new Dictionary<byte, int>()
        {
            { 0,    437 },  // PC437: USA, Standard Europe
            { 1,    932 },  // PC932: Katakana
            { 2,    850 },  // PC850: Multilingual
            { 3,    860 },  // PC860: Portuguese
            { 4,    863 },  // PC863: Canadian-French
            { 5,    865 },  // PC865: Nordic
            { 6,      6 },  // Page 6: Hiragana Not suitable code page but alternative definition
            { 7,      7 },  // Page 7:One-pass printing Kanji characters
            { 8,      8 },  // Page 8:One-pass printing Kanji characters
            { 11,    11 },  // PC851: Greek
            { 12,    12 },  // PC853: Turkish
            { 13,   857 },  // PC857: Turkish
            { 14,   737 },  // PC737: Greek
            { 15, 28597 },  // ISO8859-7: Greek
            { 16,  1252 },  // WPC1252
            { 17,   866 },  // PC866: Cyrillic #2
            { 18,   852 },  // PC852: Latin 2
            { 19,   858 },  // PC858: Euro
            { 20,    20 },  // Page 20 Thai Character Code 42 Not suitable code page but alternative definition
            { 21,    21 },  // Page 21 Thai Character Code 11 Not suitable code page but alternative definition
            { 22,    22 },  // Page 22 Thai Character Code 13 Not suitable code page but alternative definition
            { 23,    23 },  // Page 23 Thai Character Code 14 Not suitable code page but alternative definition
            { 24,    24 },  // Page 24 Thai Character Code 16 Not suitable code page but alternative definition
            { 25,    25 },  // Page 25 Thai Character Code 17 Not suitable code page but alternative definition
            { 26,    26 },  // Page 26 Thai Character Code 18 Not suitable code page but alternative definition
            { 30,    30 },  // Page 30 TCVN-3: Vietnamese Not suitable code page but alternative definition
            { 31,    31 },  // Page 31 TCVN-3: Vietnamese Not suitable code page but alternative definition
            { 32,   720 },  // PC720: Arabic
            { 33,   775 },  // WPC775: Baltic Rim
            { 34,   855 },  // PC855: Cyrillic
            { 35,   861 },  // PC861: Icelandic
            { 36,   862 },  // PC862: Hebrew
            { 37,   864 },  // PC864: Arabic
            { 38,   869 },  // PC869: Greek
            { 39, 28592 },  // ISO8859-2: Latin 2
            { 40, 28605 },  // ISO8859-15: Latin 9
            { 41,    41 },  // PC1098: Farsi
            { 42,    42 },  // PC1118: Lithuanian
            { 43,    43 },  // PC1119: Lithuanian
            { 44,    44 },  // PC1125: Ukrainian
            { 45,  1250 },  // WPC1250: Latin 2
            { 46,  1251 },  // WPC1251: Cyrillic
            { 47,  1253 },  // WPC1253: Greek
            { 48,  1254 },  // WPC1254: Turkish
            { 49,  1255 },  // WPC1255: Hebrew
            { 50,  1256 },  // WPC1256: Arabic
            { 51,  1257 },  // WPC1257: Baltic Rim
            { 52,  1258 },  // WPC1258: Vietnamese
            { 53,    53 },  // KZ-1048: Kazakhstan Not suitable code page but alternative definition
            { 66, 57002 },  // Devanagari
            { 67, 57003 },  // Bengali
            { 68, 57004 },  // Tamil
            { 69, 57005 },  // Telugu
            { 70, 57006 },  // Assamese
            { 71, 57007 },  // Oriya
            { 72, 57008 },  // Kannada
            { 73, 57009 },  // Malayalam
            { 74, 57010 },  // Gujarati
            { 75, 57011 },  // Punjabi
            { 82, 57002 },  // Marathi Not suitable code page but alternative definition
            { 254,  254 },  // Page254
            { 255,  255 }   // Page255
        };

        //  ESC t   1B 74 00-08/0B-1A/1E-35/42-4B/52/FE/FF
        internal static string DecodeEscSelectCharacterCodeTable(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                    return "PC437: USA, Standard Europe";
                case 1:
                    return "PC932: Katakana";
                case 2:
                    return "PC850: Multilingual";
                case 3:
                    return "PC860: Portuguese";
                case 4:
                    return "PC863: Canadian-French";
                case 5:
                    return "PC865: Nordic";
                case 6:
                    return "Page 6: Hiragana";
                case 7:
                    return "Page 7: One-pass printing Kanji characters";
                case 8:
                    return "Page 8: One-pass printing Kanji characters";
                case 11:
                    return "PC851: Greek";
                case 12:
                    return "PC853: Turkish";
                case 13:
                    return "PC857: Turkish";
                case 14:
                    return "PC737: Greek";
                case 15:
                    return "ISO8859-7: Greek";
                case 16:
                    return "WPC1252";
                case 17:
                    return "PC866: Cyrillic #2";
                case 18:
                    return "PC852: Latin 2";
                case 19:
                    return "PC858: Euro";
                case 20:
                    return "Page 20 Thai Character Code 42";
                case 21:
                    return "Page 21 Thai Character Code 11";
                case 22:
                    return "Page 22 Thai Character Code 13";
                case 23:
                    return "Page 23 Thai Character Code 14";
                case 24:
                    return "Page 24 Thai Character Code 16";
                case 25:
                    return "Page 25 Thai Character Code 17";
                case 26:
                    return "Page 26 Thai Character Code 18";
                case 30:
                    return "Page 30 TCVN-3: Vietnamese";
                case 31:
                    return "Page 31 TCVN-3: Vietnamese";
                case 32:
                    return "PC720: Arabic";
                case 33:
                    return "WPC775: Baltic Rim";
                case 34:
                    return "PC855: Cyrillic";
                case 35:
                    return "PC861: Icelandic";
                case 36:
                    return "PC862: Hebrew";
                case 37:
                    return "PC864: Arabic";
                case 38:
                    return "PC869: Greek";
                case 39:
                    return "ISO8859-2: Latin 2";
                case 40:
                    return "ISO8859-15: Latin 9";
                case 41:
                    return "PC1098: Farsi";
                case 42:
                    return "PC1118: Lithuanian";
                case 43:
                    return "PC1119: Lithuanian";
                case 44:
                    return "PC1125: Ukrainian";
                case 45:
                    return "WPC1250: Latin 2";
                case 46:
                    return "WPC1251: Cyrillic";
                case 47:
                    return "WPC1253: Greek";
                case 48:
                    return "WPC1254: Turkish";
                case 49:
                    return "WPC1255: Hebrew";
                case 50:
                    return "WPC1256: Arabic";
                case 51:
                    return "WPC1257: Baltic Rim";
                case 52:
                    return "WPC1258: Vietnamese";
                case 53:
                    return "Page 53 KZ-1048: Kazakhstan";
                case 66:
                    return "Devanagari";
                case 67:
                    return "Bengali";
                case 68:
                    return "Tamil";
                case 69:
                    return "Telugu";
                case 70:
                    return "Assamese";
                case 71:
                    return "Oriya";
                case 72:
                    return "Kannada";
                case 73:
                    return "Malayalam";
                case 74:
                    return "Gujarati";
                case 75:
                    return "Punjabi";
                case 82:
                    return "Marathi";
                case 254:
                    return "Page 254";
                case 255:
                    return "Page 255";
                default:
                    return "Undefined";
            }
        }

        //  ESC u   1B 75 00/30
        internal static string DecodeEscObsoleteTransmitPeripheralDeviceStatus(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "DrawerKickConnector Pin 3";
                default:
                    return "Undefined";
            }
        }

        //--------//

        //  ESC &   1B 26 00/01 20-7E 20-7E 00-08 00-FF...
        internal static string DecodeVfdEscDefineUserDefinedCharacters0816(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index, 8, 16);
        }
        internal static string DecodeVfdEscDefineUserDefinedCharacters0507(EscPosCmd record, int index)
        {
            return DecodeEscDefineUserDefinedCharacters(record, index, 5, 7);
        }

        //  ESC =   1B 3D 01/02/03
        internal static string DecodeVfdEscSelectPeripheralDevice(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 1:
                    return "Printer";
                case 3:
                    return "Printer and LineDisplay";
                case 2:
                    return "LineDisplay";
                default:
                    return "Undefined";
            }
        }

        //  ESC ?   1B 3F 20-7E
        internal static string DecodeVfdEscCancelUserDefinedCharacters(EscPosCmd record, int index)
        {
            return $"Code:{record.cmddata[index]}";
        }

        //  ESC R   1B 52 00-11
        internal static string DecodeVfdEscSelectInternationalCharacterSet(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                    return "U.S.A.";
                case 1:
                    return "France";
                case 2:
                    return "Germany";
                case 3:
                    return "U.K.";
                case 4:
                    return "Denmark I";
                case 5:
                    return "Sweden";
                case 6:
                    return "Italy";
                case 7:
                    return "Spain I";
                case 8:
                    return "Japan";
                case 9:
                    return "Norway";
                case 10:
                    return "Denmark II";
                case 11:
                    return "Spain II";
                case 12:
                    return "Latin America";
                case 13:
                    return "Korea";
                case 14:
                    return "Slovenia / Croatia";
                case 15:
                    return "China";
                case 16:
                    return "Vietnam";
                case 17:
                    return "Arabia";
                default:
                    return "Undefined";
            }
        }

        //  ESC W   1B 57 01-04 00/30
        internal static string DecodeVfdEscCancelWindowArea(EscPosCmd record, int index)
        {
            var win = record.cmddata[index];
            string winno;
            if ((win >= 1) && (win <= 4))
            {
                winno = win.ToString("D", invariantculture);
            }
            else
            {
                winno = "Out of range";
            }
            string mode;
            switch (record.cmddata[index + 1])
            {
                case 0:
                case 48:
                    mode = "Release";
                    break;
                default:
                    mode = "Undefined";
                    break;
            }

            return $"Window number:{winno}, Action:{mode}";
        }

        //  ESC W   1B 57 01-04 01/31 01-14 01-14 01/02 01/02
        internal static string DecodeVfdEscSelectWindowArea(EscPosCmd record, int index)
        {
            var win = record.cmddata[index];
            var winno = ((win >= 1) && (win <= 4)) ? win.ToString("D", invariantculture) : "Out of range";
            string mode;
            switch (record.cmddata[index + 1])
            {
                case 1:
                case 49:
                    mode = "Specify";
                    break;
                default:
                    mode = "Undefined";
                    break;
            }

            var x1 = record.cmddata[index + 2];
            var left = ((x1 >= 1) && (x1 <= 20)) ? x1.ToString("D", invariantculture) : "Out of range";
            var y1 = record.cmddata[index + 3];
            var top = ((y1 >= 1) && (y1 <= 2)) ? y1.ToString("D", invariantculture) : "Out of range";
            var x2 = record.cmddata[index + 4];
            var right = ((x2 >= 1) && (x2 <= 20) && (x1 <= x2)) ? x2.ToString("D", invariantculture) : "Out of range";
            var y2 = record.cmddata[index + 5];
            var bottom = ((y2 >= 1) && (y2 <= 2) && (y1 <= y2)) ? y2.ToString("D", invariantculture) : "Out of range";
            return $"Window number:{winno}, Action:{mode}, Left:{left}, Top:{top}, Right:{right}, Bottom:{bottom}";
        }

        public static readonly Dictionary<byte, int> VfdESCtCodePage = new Dictionary<byte, int>()
        {
            { 0,    437 },  // PC437: USA, Standard Europe
            { 1,    932 },  // PC932: Katakana
            { 2,    850 },  // PC850: Multilingual
            { 3,    860 },  // PC860: Portuguese
            { 4,    863 },  // PC863: Canadian-French
            { 5,    865 },  // PC865: Nordic
            { 11,    11 },  // PC851: Greek
            { 12,    12 },  // PC853: Turkish
            { 13,   857 },  // PC857: Turkish
            { 14,   737 },  // PC737: Greek
            { 15, 28597 },  // ISO8859-7: Greek
            { 16,  1252 },  // WPC1252
            { 17,   866 },  // PC866: Cyrillic #2
            { 18,   852 },  // PC852: Latin 2
            { 19,   858 },  // PC858: Euro
            { 30,    30 },  // Page 30 TCVN-3: Vietnamese Not suitable code page but alternative definition
            { 31,    31 },  // Page 31 TCVN-3: Vietnamese Not suitable code page but alternative definition
            { 32,   720 },  // PC720: Arabic
            { 33,   775 },  // WPC775: Baltic Rim
            { 34,   855 },  // PC855: Cyrillic
            { 35,   861 },  // PC861: Icelandic
            { 36,   862 },  // PC862: Hebrew
            { 37,   864 },  // PC864: Arabic
            { 38,   869 },  // PC869: Greek
            { 39, 28592 },  // ISO8859-2: Latin 2
            { 40, 28605 },  // ISO8859-15: Latin 9
            { 41,    41 },  // PC1098: Farsi
            { 42,    42 },  // PC1118: Lithuanian
            { 43,    43 },  // PC1119: Lithuanian
            { 44,    44 },  // PC1125: Ukrainian
            { 45,  1250 },  // WPC1250: Latin 2
            { 46,  1251 },  // WPC1251: Cyrillic
            { 47,  1253 },  // WPC1253: Greek
            { 48,  1254 },  // WPC1254: Turkish
            { 49,  1255 },  // WPC1255: Hebrew
            { 50,  1256 },  // WPC1256: Arabic
            { 51,  1257 },  // WPC1257: Baltic Rim
            { 52,  1258 },  // WPC1258: Vietnamese
            { 53,    53 },  // KZ-1048: Kazakhstan Not suitable code page but alternative definition
            { 254,  254 },  // Page254
            { 255,  255 }   // Page255
        };

        //  ESC t   1B 74 00-13/1E-35/FE/FF
        internal static string DecodeVfdEscSelectCharacterCodeTable(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                    return "PC437: USA, Standard Europe";
                case 1:
                    return "PC932: Katakana";
                case 2:
                    return "PC850: Multilingual";
                case 3:
                    return "PC860: Portuguese";
                case 4:
                    return "PC863: Canadian-French";
                case 5:
                    return "PC865: Nordic";
                case 11:
                    return "PC851: Greek";
                case 12:
                    return "PC853: Turkish";
                case 13:
                    return "PC857: Turkish";
                case 14:
                    return "PC737: Greek";
                case 15:
                    return "ISO8859-7: Greek";
                case 16:
                    return "WPC1252";
                case 17:
                    return "PC866: Cyrillic #2";
                case 18:
                    return "PC852: Latin 2";
                case 19:
                    return "PC858: Euro";
                case 30:
                    return "Page 30 TCVN-3: Vietnamese";
                case 31:
                    return "Page 31 TCVN-3: Vietnamese";
                case 32:
                    return "PC720: Arabic";
                case 33:
                    return "WPC775: Baltic Rim";
                case 34:
                    return "PC855: Cyrillic";
                case 35:
                    return "PC861: Icelandic";
                case 36:
                    return "PC862: Hebrew";
                case 37:
                    return "PC864: Arabic";
                case 38:
                    return "PC869: Greek";
                case 39:
                    return "ISO8859-2: Latin 2";
                case 40:
                    return "ISO8859-15: Latin 9";
                case 41:
                    return "PC1098: Farsi";
                case 42:
                    return "PC1118: Lithuanian";
                case 43:
                    return "PC1119: Lithuanian";
                case 44:
                    return "PC1125: Ukrainian";
                case 45:
                    return "WPC1250: Latin 2";
                case 46:
                    return "WPC1251: Cyrillic";
                case 47:
                    return "WPC1253: Greek";
                case 48:
                    return "WPC1254: Turkish";
                case 49:
                    return "WPC1255: Hebrew";
                case 50:
                    return "WPC1256: Arabic";
                case 51:
                    return "WPC1257: Baltic Rim";
                case 52:
                    return "WPC1258: Vietnamese";
                case 53:
                    return "KZ-1048: Kazakhstan";
                case 254:
                    return "Page 254";
                case 255:
                    return "Page 255";
                default:
                    return "Undefined";
            }
        }
    }
}