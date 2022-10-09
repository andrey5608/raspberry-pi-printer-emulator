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
        //  FS  !   1C 21 bx0xx0000
        internal static string DecodeFsSelectPrintModeKanji(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var underline = (mode & 0x80) == 0x80 ? "ON" : "OFF";
            var doublewidth = (mode & 0x20) == 0x20 ? "ON" : "OFF";
            var doubleheight = (mode & 0x10) == 0x10 ? "ON" : "OFF";
            return $"Underline:{underline}, DoubleWidth:{doublewidth}, DoubleHeight:{doubleheight}";
        }

        //  ESC M   1B 4D 00-04/30-34/61/62
        internal static string DecodeFsSelectKanjiCharacterFont(EscPosCmd record, int index)
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
                default:
                    return "Undefined";
            }
        }

        //  FS  ( C 1C 28 43 02 00 30 01/02/31/32
        internal static string DecodeFsSelectCharacterEncodeSystem(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 1:
                case 49:
                    return "1 byte character encoding";
                case 2:
                case 50:
                    return "UTF-8";
                default:
                    return "Undefined";
            }
        }

        //  FS  ( C 1C 28 43 03 00 3C 00/01 00/0B/14/1E/29
        internal static string DecodeFsSetFontPriority(EscPosCmd record, int index)
        {
            string mode;
            switch (record.cmddata[index])
            {
                case 0:
                    mode = "1st";
                    break;
                case 1:
                    mode = "2nd";
                    break;
                default:
                    mode = "Undefined";
                    break;
            }

            string font;
            switch (record.cmddata[index + 1])
            {
                case 0:
                    font = "ANK";
                    break;
                case 11:
                    font = "Japanese";
                    break;
                case 20:
                    font = "Simplified Chinese";
                    break;
                case 30:
                    font = "Traditional Chinese";
                    break;
                case 41:
                    font = "Korean";
                    break;
                default:
                    font = "Undefined";
                    break;
            }

            return $"Priority:{mode}, Font:{font}";
        }

        //  FS  ( E 1C 28 45 06 00 3C 02 30/31 43 4C 52
        internal static string DecodeFsCancelSetValuesTopBottomLogo(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "Top Logo";
                case 49:
                    return "Bottom Logo";
                default:
                    return "Undefined";
            }
        }

        //  FS  ( E 1C 28 45 03 00 3D 02 30-32
        internal static string DecodeFsTransmitSetValuesTopBottomLogo(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "Top Logo";
                case 49:
                    return "Bottom Logo";
                case 50:
                    return "Both Top & Bottom Logo";
                default:
                    return "Undefined";
            }
        }

        //  FS  ( E 1C 28 45 06 00 3E 02 20-7E 20-7E 30-32 00-FF
        internal static string DecodeFsSetTopLogoPrinting(EscPosCmd record, int index)
        {
            var kc1 = record.cmddata[index];
            var kc2 = record.cmddata[index + 1];
            string align;
            switch (record.cmddata[index + 2])
            {
                case 48:
                    align = "Left";
                    break;
                case 49:
                    align = "Center";
                    break;
                case 50:
                    align = "Right";
                    break;
                default:
                    align = "Undefined";
                    break;
            }

            var lines = record.cmddata[index + 3];
            return $"KeyCode1:{kc1:X}, KeyCode2:{kc2:X}, Align:{align}, Remove:{lines:D} Lines";
        }

        //  FS  ( E 1C 28 45 05 00 3F 02 20-7E 20-7E 30-32
        internal static string DecodeFsSetBottomLogoPrinting(EscPosCmd record, int index)
        {
            var kc1 = record.cmddata[index];
            var kc2 = record.cmddata[index + 1];
            string align;
            switch (record.cmddata[index + 2])
            {
                case 48:
                    align = "Left";
                    break;
                case 49:
                    align = "Center";
                    break;
                case 50:
                    align = "Right";
                    break;
                default:
                    align = "Undefined";
                    break;
            }

            return $"KeyCode1:{kc1:X}, KeyCode2:{kc2:X}, Align:{align}";
        }

        //  FS  ( E 1C 28 45 0004-000C 40 02 [30/40-43 30/31]...
        internal static string DecodeFsMakeExtendSettingsTopBottomLogoPrinting(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, 3);
            if ((length < 4) || (length > 12))
            {
                return "Out of range";
            }
            else if ((length & 1) == 1)
            {
                return "Odd length";
            }
            var count = (length / 2) - 1;
            var setting = new List<string>();
            for (int i = 0, currindex = 7; i < count; i++, currindex += 2)
            {
                string currset;
                switch (record.cmddata[currindex])
                {
                    case 48:
                        currset = "While Paper feeding to Cutting position:";
                        break;
                    case 64:
                        currset = "At Power-On:";
                        break;
                    case 65:
                        currset = "When Roll paper cover is Closed:";
                        break;
                    case 66:
                        currset = "While Clearing Buffer to Recover from Recoverble Error:";
                        break;
                    case 67:
                        currset = "After Paper feeding with Paper feed button has Finished:";
                        break;
                    default:
                        currset = "Undefined:";
                        break;
                }

                switch (record.cmddata[currindex + 1])
                {
                    case 48:
                        currset += "Enable";
                        break;
                    case 49:
                        currset += "Disable";
                        break;
                    default:
                        currset += "Undefined";
                        break;
                }

                setting.Add(currset);
            }
            return string.Join<string>(", ", setting);
        }

        //  FS  ( E 1C 28 45 04 00 41 02 30/31 30/31
        internal static string DecodeFsEnableDisableTopBottomLogoPrinting(EscPosCmd record, int index)
        {
            string loc;
            switch (record.cmddata[index])
            {
                case 48:
                    loc = "Top Logo";
                    break;
                case 49:
                    loc = "Bottom Logo";
                    break;
                default:
                    loc = "Undefined";
                    break;
            }

            string mode;
            switch (record.cmddata[index + 1])
            {
                case 48:
                    mode = "Enable";
                    break;
                case 49:
                    mode = "Disable";
                    break;
                default:
                    mode = "Undefined";
                    break;
            }

            return $"Location:{loc}, Mode:{mode}";
        }

        //  FS  ( L 1C 28 4C 0008-001a 21 30-33 [[30-39]... 3B]...
        internal static string DecodeFsPaperLayoutSetting(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 8) || (length > 26))
            {
                return "Length out of range";
            }
            string layoutrefs;
            switch (record.cmddata[index + 3])
            {
                case 48:
                    layoutrefs = "Receipt(no black mark): do not use layout";
                    break;
                case 49:
                    layoutrefs = "Die cut label paper(no black mark): Print Label top edge, Eject Label bottom edge";
                    break;
                case 50:
                    layoutrefs =
                        "Die cut label paper(black mark): Print Black mark bottom edge, Eject Black mark top edge";
                    break;
                case 51:
                    layoutrefs = "Receipt(black mark): Print Black mark top edge, Eject Black mark top edge";
                    break;
                default:
                    layoutrefs = "Undefined";
                    break;
            }

            var layoutvalues = ascii.GetString(record.cmddata, (index + 4), (length - 2));
            return $"Layout Reference:{layoutrefs}, Settings:{layoutvalues}";
        }

        //  FS  ( L 1C 28 4C 02 00 22 40/50
        internal static string DecodeFsPaperLayoutInformationTransmission(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 64:
                    return "Setting value";
                case 80:
                    return "Effective value";
                default:
                    return "Undefined";
            }
        }

        //  FS  ( L 1C 28 4C 02 00 41 30/31
        internal static string DecodeFsFeedPaperLabelPeelingPosition(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "if the paper is in standby at the label peeling position, the printer does not feed.";
                case 49:
                    return
                        "if the paper is in standby at the label peeling position, the printer feeds paper to the next label peeling position.";
                default:
                    return "Undefined";
            }
        }

        //  FS  ( L 1C 28 4C 02 00 42 30/31
        internal static string DecodeFsFeedPaperCuttingPosition(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "if the paper is in standby at the label cutting position, the printer does not feed.";
                case 49:
                    return
                        "if the paper is in standby at the label cutting position, the printer feeds paper to the next label cutting position.";
                default:
                    return "Undefined";
            }
        }

        //  FS  ( L 1C 28 4C 02 00 43 30-32
        internal static string DecodeFsFeedPaperPrintStartingPosition(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return
                        "Feeds to next label, if the paper is in standby at the print starting position, the printer does not feed.";
                case 49:
                    return
                        "Feeds to next label, if the paper is in standby at the print starting position, the printer feeds paper to the next print starting position.";
                case 50:
                    return
                        "Feeds to current label, if the paper is in standby at the print starting position, the printer does not feed.";
                default:
                    return "Undefined";
            }
        }

        //  FS  ( L 1C 28 4C 0002-0003 50 30-39 [30-39]
        internal static string DecodeFsPaperLayoutErrorSpecialMarginSetting(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 2) || (length > 3))
            {
                return "Length out of range";
            }
            var margin = ascii.GetString(record.cmddata, (index + 3), (length - 1));
            return $"Layout Error Special Margin:{margin} x 0.1mm";
        }

        //  FS  ( e 1C 28 65 02 00 33 b0000x000
        internal static string DecodeFsEnableDisableAutomaticStatusBackOptional(EscPosCmd record, int index)
        {
            switch ((record.cmddata[index] & 0x08))
            {
                case 0:
                    return "Disabled";
                case 8:
                    return "Enabled";
                default:
                    return "Undefined";
            }
        }

        //c FS  ( f 1C 28 66 0002-FFFF [00-03/30-33 00-FF|00/01/30/31]...
        internal static string DecodeFsSelectMICRDataHandling(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 2) || ((length & 1) == 1))
            {
                return "Length out of range";
            }
            var config = new List<string> { };
            var count = length / 2;
            var cfgindex = index + 2;
            for (var i = 0; i < count; i++, cfgindex += 2)
            {
                string entry;
                var n = record.cmddata[cfgindex];
                var m = record.cmddata[cfgindex + 1];
                if ((n <= 3) || ((n >= 48) && (n <= 51)))
                {
                    switch ((n & 0x03))
                    {
                        case 0:
                            switch (m)
                            {
                                case 0:
                                    entry =
                                        "processing for unrecognized characters : Reading is stopped when a character that cannot be recognized is detected.";
                                    break;
                                default:
                                    entry =
                                        $"processing for unrecognized characters : The character that cannot be recognized is replaced with the character \"?\" and reading is continued. When the number of characters that are replaced with \"?\" becomes({m} + 1), the reading is stopped.";
                                    break;
                            }

                            break;
                        case 1:
                            switch (m)
                            {
                                case 0:
                                    entry =
                                        "detailed information for the reading result : Not to add detailed information for an abnormal end.";
                                    break;
                                case 1:
                                    entry =
                                        "detailed information for the reading result : Add detailed information for an abnormal end.";
                                    break;
                                default:
                                    entry = "detailed information for the reading result : Undefined";
                                    break;
                            }

                            break;
                        case 2:
                            switch (m)
                            {
                                case 0:
                                case 48:
                                    entry =
                                        "no addition of the reading result in an abnormal end : The MICR function ends after transmission the reading result.";
                                    break;
                                case 1:
                                case 49:
                                    entry =
                                        "no addition of the reading result in an abnormal end : The MICR function is continued after transmission the reading result only for the following abnormal ends";
                                    break;
                                default:
                                    entry = "no addition of the reading result in an abnormal end : Undefined";
                                    break;
                            }

                            break;
                        case 3:
                            switch (m)
                            {
                                case 0:
                                case 48:
                                    entry =
                                        "header for transmission data : The MICR function ends after transmission the reading result.";
                                    break;
                                case 1:
                                case 49:
                                    entry =
                                        "header for transmission data : The MICR function is continued after transmission the reading result only for the following abnormal ends";
                                    break;
                                default:
                                    entry = "header for transmission data : Undefined";
                                    break;
                            }

                            break;
                        default:
                            entry = "";
                            break;
                    }
                }
                else
                {
                    entry = "Function out of range";
                }
                config.Add(entry);
            }
            return string.Join<string>(", ", config);
        }

        //c FS  ( g 1C 28 67 02 00 20 30/31
        internal static string DecodeFsSelectImageScannerCommandSettings(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "Slip Image Scanner [Active Sheet = Check Paper]";
                case 49:
                    return "Card Image Scanner [Active Sheet = Card]";
                default:
                    return "Undefined";
            }
        }

        //c FS  ( g 1C 28 67 05 00 28 30 01/08 31/32 80-7F
        internal static string DecodeFsSetBasicOperationOfImageScanner(EscPosCmd record, int index)
        {
            string color;
            switch (record.cmddata[index])
            {
                case 1:
                    color = "Monochrome";
                    break;
                case 8:
                    color = "256 level gray scale";
                    break;
                default:
                    color = "Undefined";
                    break;
            }

            string sharpness;
            switch (record.cmddata[index + 1])
            {
                case 49:
                    sharpness = "No";
                    break;
                case 50:
                    sharpness = "Yes";
                    break;
                default:
                    sharpness = "Undefined";
                    break;
            }

            var signed = Array.ConvertAll(record.cmddata, b => unchecked((sbyte)b));
            var threshold = signed[index + 2].ToString("D", invariantculture);
            return $"ColorType:{color}, Sharpness:{sharpness}, ThresholdLevel:{threshold}";
        }

        //c FS  ( g 1C 28 67 05 00 29 00-62 00-E4 00/02-64 00/02-E6
        internal static string DecodeFsSetScanningArea(EscPosCmd record, int index)
        {
            var x1 = record.cmddata[index];
            var y1 = record.cmddata[index + 1];
            var x2 = record.cmddata[index + 2];
            var y2 = record.cmddata[index + 3];
            if (x1 > 98)
            {
                return "x1 value out of range";
            }
            if (y1 > 228)
            {
                return "y1 value out of range";
            }
            if ((x2 == 1) || (x2 > 100))
            {
                return "x2 value out of range";
            }
            if ((y2 == 1) || (y2 > 230))
            {
                return "y2 value out of range";
            }
            return $"x1:{x1}, y1:{y1}, x2:{x2}, y2:{y2}";
        }

        //c FS  ( g 1C 28 67 03 00 32 30-32 30-32
        internal static string DecodeFsSelectCompressionMethodForImageData(EscPosCmd record, int index)
        {
            var m = record.cmddata[index];
            var n = record.cmddata[index + 1];
            switch (m)
            {
                case 48:
                    switch (n)
                    {
                        case 48:
                            return "RAW data does not compress";
                        case 49:
                            return "BMP does not compress";
                        case 50:
                            return "TIFF does not compress";
                        default:
                            return "Not compress Undefined";
                    }
                case 49:
                    switch (n)
                    {
                        case 48:
                            return "TIFF Compression with CCITT(Grp4)";
                        default:
                            return "TIFF Undefined";
                    }
                case 50:
                    switch (n)
                    {
                        case 48:
                            return "JPEG High compression rate";
                        case 49:
                            return "JPEG Standard compression rate";
                        case 50:
                            return "JPEG Low compression rate";
                        default:
                            return "JPEG Undefined";
                    }
                default:
                    return "Undefined";
            }
        }

        //c FS  ( g 1C 28 67 02 00 38 00-0A
        internal static string DecodeFsDeleteCroppingArea(EscPosCmd record, int index)
        {
            var area = record.cmddata[index];
            if (area > 10)
            {
                return "Area number value out of range";
            }
            return $"Area Number:{area}";
        }

        //c FS  ( g 1C 28 67 06 00 39 00-0A 00-64 00-E4 02-64 02-E6
        internal static string DecodeFsSetCroppingArea(EscPosCmd record, int index)
        {
            var area = record.cmddata[index];
            var x1 = record.cmddata[index + 1];
            var y1 = record.cmddata[index + 2];
            var x2 = record.cmddata[index + 3];
            var y2 = record.cmddata[index + 4];
            if (area > 10)
            {
                return "Area number value out of range";
            }
            if (x1 > 98)
            {
                return "x1 value out of range";
            }
            if (y1 > 228)
            {
                return "y1 value out of range";
            }
            if ((x2 == 1) || (x2 > 100))
            {
                return "x2 value out of range";
            }
            if ((y2 == 1) || (y2 > 230))
            {
                return "y2 value out of range";
            }
            return $"Area Number:{area}, x1:{x1}, y1:{y1}, x2:{x2}, y2:{y2}";
        }

        //c FS  ( g 1C 28 67 02 00 3C 30-32
        internal static string DecodeFsSelectTransmissionFormatForImageScanningResult(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "Binary data format: max 65,535 bytes";
                case 49:
                    return "Hexadecimal character string format";
                case 50:
                    return "Binary data format: max 4,294,967,295 bytes";
                default:
                    return "Undefined";
            }
        }

        //  FS  -   1C 2D 00-02/30-32
        internal static string DecodeFsTurnKanjiUnderlineMode(EscPosCmd record, int index)
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

        //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 72
        internal static string DecodeFsDefineUserDefinedKanjiCharacters2424(EscPosCmd record, int index)
        {
            var c1 = record.cmddata[index];
            var c2 = record.cmddata[index + 1];
            var code = ((int)c1 << 8) + (int)c2;
            var width = 24;
            var height = 24;
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 2), "1");
            return $"CharacterCode:{code:X}";
        }

        //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 60
        internal static string DecodeFsDefineUserDefinedKanjiCharacters2024(EscPosCmd record, int index)
        {
            var c1 = record.cmddata[index];
            var c2 = record.cmddata[index + 1];
            var code = ((int)c1 << 8) + (int)c2;
            var width = 20;
            var height = 24;
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 2), "1");
            return $"CharacterCode:{code:X}";
        }

        //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 32
        internal static string DecodeFsDefineUserDefinedKanjiCharacters1616(EscPosCmd record, int index)
        {
            var c1 = record.cmddata[index];
            var c2 = record.cmddata[index + 1];
            var code = ((int)c1 << 8) + (int)c2;
            var width = 16;
            var height = 16;
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 2), "1");
            return $"CharacterCode:{code:X}";
        }

        //  FS  ?   1C 3F 7721-777E/EC40-EC9E/FEA1-FEFE
        internal static string DecodeFsCancelUserDefinedKanjiCharacters(EscPosCmd record, int index)
        {
            var c1 = record.cmddata[index];
            var c2 = record.cmddata[index + 1];
            var code = ((int)c1 << 8) + (int)c2;
            return $"CharacterCode:{code:X}";
        }

        //  FS  C   1C 43 00-02/30-32
        internal static string DecodeFsSelectKanjiCharacterCodeSystem(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "JIS";
                case 1:
                case 49:
                    return "ShiftJIS";
                case 2:
                case 50:
                    return "ShiftJIS2004";
                default:
                    return "Undefined";
            }
        }

        //  FS  S   1C 53 00-FF/00-20 00-FF/00-20
        internal static string DecodeFsSetKanjiCharacerSpacing(EscPosCmd record, int index)
        {
            return $"LeftSideSpacing:{record.cmddata[index]} dots, RightSideSpacing:{record.cmddata[index + 1]} dots";
        }

        //c FS  a 0 1C 61 30 b000000xx
        internal static string DecodeFsObsoleteReadCheckPaper(EscPosCmd record, int index)
        {
            switch ((record.cmddata[index] & 0x03))
            {
                case 0:
                    return "E13B";
                case 1:
                    return "CMC7";
                default:
                    return "Reserved";
            }
        }

        //  FS  g 1 1C 67 31 00 00000000-000003FF 0001-0400 20-FF...
        internal static string DecodeFsObsoleteWriteNVUserMemory(EscPosCmd record, int index)
        {
            var start = BitConverter.ToUInt32(record.cmddata, index);
            var startaddress = start < 0x400 ? "0x" + start.ToString("X8", invariantculture) : "Out of range";
            int write = BitConverter.ToUInt16(record.cmddata, (index + 4));
            var writesize = ((write != 0) && (write <= 0x400)) ? "0x" + write.ToString("X4", invariantculture) : "Out of range";
            return $"StartAddress:{startaddress}, Size:{writesize}";
        }

        //  FS  g 2 1C 67 32 00 00000000-000003FF 0001-0400
        internal static string DecodeFsObsoleteReadNVUserMemory(EscPosCmd record, int index)
        {
            var start = BitConverter.ToUInt32(record.cmddata, index);
            var startaddress = start < 0x400 ? "0x" + start.ToString("X8", invariantculture) : "Out of range";
            int read = BitConverter.ToUInt16(record.cmddata, (index + 4));
            var readsize = ((read != 0) && (read <= 0x400)) ? "0x" + read.ToString("X4", invariantculture) : "Out of range";
            return $"StartAddress:{startaddress}, Size:{readsize}";
        }

        //  FS  p   1C 70 01-FF 00-03/30-33
        internal static string DecodeFsObsoletePrintNVBitimage(EscPosCmd record, int index)
        {
            var imageno = record.cmddata[index];
            var imagenumber = imageno != 0 ? imageno.ToString("D", invariantculture) : "0=Unsupported";
            string scalling;
            switch (record.cmddata[index + 1])
            {
                case 0:
                case 48:
                    scalling = "Normal";
                    break;
                case 1:
                case 49:
                    scalling = "DoubleWidth";
                    break;
                case 2:
                case 50:
                    scalling = "DoubleHeight";
                    break;
                case 3:
                case 51:
                    scalling = "Quadruple";
                    break;
                default:
                    scalling = "Undefined";
                    break;
            }

            return $"NVImageNumber:{imagenumber}, Scalling:{scalling}";
        }

        //  FS  q   1C 71 01-FF [0001-03FF 0001-0240 00-FF...]...
        internal static string DecodeFsObsoleteDefineNVBitimage(EscPosCmd record, int index)
        {
            var images = record.cmddata[index];
            var imagecount = images != 0 ? images.ToString("D", invariantculture) : "0=Unsupported";
            var imagelist = new List<System.Drawing.Bitmap>();
            var i = index + 1;
            for (var n = 0; n < images; n++)
            {
                int width = BitConverter.ToUInt16(record.cmddata, i);
                int heightbytes = BitConverter.ToUInt16(record.cmddata, i + 2);
                var height = heightbytes * 8;
                i += 4;
                var datalength = width * heightbytes;
                if (((heightbytes > 0) && (heightbytes <= 0x240)) && ((width > 0) && (width <= 0x3FF)))
                {
                    imagelist.Add(GetBitmap(width, height, ImageDataType.Column, record.cmddata, i, "1"));
                }
                i += datalength;
            }
            record.somebinary = imagelist.ToArray();
            return $"NVImageCount:{imagecount}";
        }
    }
}