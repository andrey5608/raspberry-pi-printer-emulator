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
    using System.IO;
    using System.Drawing;
    using System.Drawing.Imaging;

    public static partial class EscPosDecoder
    {
        //  GS  !   1D 21 b0xxx0yyy
        internal static string DecodeGsSelectCharacterSize(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            if ((mode & 0x88) != 0)
            {
                return "Undefined pattern value";
            }
            var h = (mode >> 4) + 1;
            var v = (mode & 7) + 1;
            return $"Horizontal:{h}, Vertical:{v}";
        }

        //  GS  ( A 1D 28 41 02 00 00-02/30-32 01-03/31-33/40
        internal static string DecodeGsExecuteTestPrint(EscPosCmd record, int index)
        {
            string paper;
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    paper = "Basic sheet";
                    break;
                case 1:
                case 49:
                case 2:
                case 50:
                    paper = "Roll paper";
                    break;
                case 3:
                case 51:
                    paper = "Slip(face)";
                    break;
                case 4:
                case 52:
                    paper = "Validation";
                    break;
                case 5:
                case 53:
                    paper = "Slip(back)";
                    break;
                default:
                    paper = "Undefined";
                    break;
            }

            string pattern;
            switch (record.cmddata[index + 1])
            {
                case 1:
                case 49:
                    pattern = "Hexadecimal dump";
                    break;
                case 2:
                case 50:
                    pattern = "Printer status";
                    break;
                case 3:
                case 51:
                    pattern = "Rolling pattern";
                    break;
                case 64:
                    pattern = "Automatic setting of paper layout";
                    break;
                default:
                    pattern = "Undefined";
                    break;
            }

            return $"Print to:{paper}, Test pattern:{pattern}";
        }

        //c GS  ( B 1D 28 42 0002/0003/0005/0007/0009 61 00|[31/33/45/46 2C/2D/37/38]...
        internal static string DecodeGsCustomizeASBStatusBits(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length == 2)
            {
                return (record.cmddata[index + 4] == 0) ? "All Disabled" : "Undefined";
            }
            else if ((length < 2) || (length > 9) || ((length & 1) == 0))
            {
                return "Length out of range";
            }
            // 49,44 "1,", 51,45 "3-", 69,56 "E8", 70,55 "F7", xx,54 "?6"
            var count = (length - 1) / 2;
            var status = new List<string>();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 2)
            {
                string entry;
                switch (ascii.GetString(record.cmddata, currindex, 2))
                {
                    case "1,":
                        entry = "cut sheet insertion waiting status";
                        break;
                    case "3-":
                        entry = "cut sheet removal waiting status";
                        break;
                    case "E8":
                        entry = "card sensor status";
                        break;
                    case "F7":
                        entry = "slip paper ejection sensor status";
                        break;
                    default:
                        //"?6" => "paper width sensor status",
                        entry = "Undefined";
                        break;
                }

                status.Add(entry);
            }
            return string.Join<string>(", ", status);
        }

        //  GS  ( C 1D 28 43 05 00 00 00/30 00 20-7E 20-7E
        internal static string DecodeGsDeleteSpecifiedRecord(EscPosCmd record, int index)
        {
            var c1 = record.cmddata[index];
            var c2 = record.cmddata[index + 1];
            return $"Code1:{c1:X}, Code2:{c2:X}";
        }

        //  GS  ( C 1D 28 43 0006-FFFF 00 01/31 00 20-7E 20-7E 20-FF...
        internal static string DecodeGsStoreDataSpecifiedRecord(EscPosCmd record, int index)
        {
            var c1 = record.cmddata[index];
            var c2 = record.cmddata[index + 1];
            return $"Code1:{c1:X}, Code2:{c2:X}";
        }

        //  GS  ( C 1D 28 43 05 00 00 02/32 00 20-7E 20-7E
        internal static string DecodeGsTransmitDataSpecifiedRecord(EscPosCmd record, int index)
        {
            var c1 = record.cmddata[index];
            var c2 = record.cmddata[index + 1];
            return $"Code1:{c1:X}, Code2:{c2:X}";
        }

        //  GS  ( D 1D 28 44 0003/0005 14 [01/02 00/01/30/31]...
        internal static string DecodeGsEnableDisableRealtimeCommand(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length != 3) && (length != 5))
            {
                return "Length out of range";
            }
            var count = (length - 1) / 2;
            var cmds = new List<string>();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 2)
            {
                string cmdtype;
                switch (record.cmddata[currindex])
                {
                    case 1:
                        cmdtype = "Generate pulse in real-time";
                        break;
                    case 2:
                        cmdtype = "Execute power-off sequence";
                        break;
                    default:
                        cmdtype = "Undefined";
                        break;
                }

                string enable;
                switch (record.cmddata[currindex + 1])
                {
                    case 0:
                    case 48:
                        enable = "Disable";
                        break;
                    case 1:
                    case 49:
                        enable = "Enable";
                        break;
                    default:
                        enable = "Undefined";
                        break;
                }

                cmds.Add($"Type:{cmdtype}, {enable}");
            }
            return string.Join<string>(", ", cmds);
        }

        //  GS  ( E 1D 28 45 000A-FFFA 03 [01-08 30-32...]...
        internal static string DecodeGsChangeMeomorySwitch(EscPosCmd record, int index)
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
            var count = (length - 1) / 9;
            var memorys = new List<string>();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 9)
            {
                string msw;
                switch (record.cmddata[currindex])
                {
                    case 1:
                        msw = "Msw1";
                        break;
                    case 2:
                        msw = "Msw2";
                        break;
                    case 3:
                        msw = "Msw3";
                        break;
                    case 4:
                        msw = "Msw4";
                        break;
                    case 5:
                        msw = "Msw5";
                        break;
                    case 6:
                        msw = "Msw6";
                        break;
                    case 7:
                        msw = "Msw7";
                        break;
                    case 8:
                        msw = "Msw8";
                        break;
                    default:
                        msw = "Undefined";
                        break;
                }

                var setting = ascii.GetString(record.cmddata, (currindex + 1), 8).Replace('2', '_');
                memorys.Add($"MemorySwitch:{msw} Setting:{setting}");
            }
            return string.Join<string>(", ", memorys);
        }

        //  GS  ( E 1D 28 45 02 00 04 01-08
        internal static string DecodeGsTransmitSettingsMemorySwitch(EscPosCmd record, int index)
        {
            var m = record.cmddata[index];
            if ((m >= 1) && (m <= 8))
            {
                return "MemorySwitch:Msw" + m.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( E 1D 28 45 0004-FFFD 05 [01-03/05-0D/14-16/46-48/61/62/64-69/6F/70/74-C2 0000-FFFF]...
        internal static string DecodeGsSetCustomizeSettinValues(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, 3);
            if ((length < 4) && (length > 65533))
            {
                return "Out of range";
            }
            else if (((length - 1) % 3) != 0)
            {
                return "Miss align length";
            }
            var count = (length - 1) / 3;
            var memorys = new List<string>();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 3)
            {
                var swno = record.cmddata[currindex];
                string swnostr;
                if (((swno >= 1) && (swno <= 14))
                    || ((swno >= 20) && (swno <= 22))
                    || (swno == 70) || (swno == 71) || (swno == 73)
                    || (swno == 97) || (swno == 98)
                    || ((swno >= 100) && (swno <= 106))
                    || (swno == 111) || (swno == 112)
                    || ((swno >= 116) && (swno <= 194)))
                {
                    swnostr = swno.ToString("D", invariantculture);
                }
                else
                {
                    return "Out of range";
                }
                int msv = BitConverter.ToUInt16(record.cmddata, (currindex + 1));
                memorys.Add($"MemorySwitch:{swnostr} Setting:{msv:D}");
            }
            return string.Join<string>(", ", memorys);
        }

        //  GS  ( E 1D 28 45 02 00 06 01-03/05-0D/14-16/46-48/61/62/64-69/6F-71/74-C1
        internal static string DecodeGsTransmitCustomizeSettingValues(EscPosCmd record, int index)
        {
            var swno = record.cmddata[index];
            if (((swno >= 1) && (swno <= 14))
                || ((swno >= 20) && (swno <= 22))
                || (swno == 70) || (swno == 71) || (swno == 73)
                || (swno == 97) || (swno == 98)
                || ((swno >= 100) && (swno <= 106))
                || (swno >= 111) || (swno <= 113)
                || ((swno >= 116) && (swno <= 195)))
            {
                return swno.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( E 1D 28 45 04 00 07 0A/0C/11/12 1D/1E 1E/1D
        internal static string DecodeGsCopyUserDefinedPage(EscPosCmd record, int index)
        {
            string font;
            switch (record.cmddata[index])
            {
                case 10:
                    font = "Width 9 dot, Hwight 17 dot";
                    break;
                case 12:
                    font = "Width 12 dot, Hwight 24 dot";
                    break;
                case 17:
                    font = "Width 8 dot, Hwight 16 dot";
                    break;
                case 18:
                    font = "Width 10 dot, Hwight 24 dot";
                    break;
                default:
                    font = "Undefined";
                    break;
            }

            string direction;
            switch (ascii.GetString(record.cmddata, (index + 1), 2))
            {
                case "\x1E\x1D":
                    direction = "FromStorage ToWork";
                    break;
                case "\x1D\x1E":
                    direction = "FromWork ToStorage";
                    break;
                default:
                    direction = "Undefined";
                    break;
            }

            return $"Font size:{font}, Direction:{direction}";
        }

        //  GS  ( E 1D 28 45 0005-FFFF 08 02/03 20-7E 20-7E [08/09/0A/0C 00-FF...]...
        internal static string DecodeGsDefineColumnFormatCharacterCodePage(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length <= 5)
            {
                return "Length out of range";
            }
            int y = record.cmddata[index + 3];
            string ysize;
            switch (y)
            {
                case 2:
                    ysize = "2";
                    break;
                case 3:
                    ysize = "3";
                    break;
                default:
                    ysize = "Undefined";
                    break;
            }

            var c1 = record.cmddata[index + 4];
            var c2 = record.cmddata[index + 5];
            var count = c2 - c1 + 1;
            var fonts = new List<string>();
            var glyphs = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = 9; (i < count) && (currindex < record.cmdlength); i++)
            {
                int x = record.cmddata[currindex];
                string xsize;
                switch (x)
                {
                    case 8:
                        xsize = "8";
                        break;
                    case 9:
                        xsize = "9";
                        break;
                    case 10:
                        xsize = "10";
                        break;
                    case 12:
                        xsize = "12";
                        break;
                    default:
                        xsize = "Undefined";
                        break;
                }

                var fdsize = (x * y);
                var bitmap = new System.Drawing.Bitmap((y * 8), x, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = Color.Black;
                bitmap.Palette = palette;
                if (fdsize > 0)
                {
                    var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                    var ptr = bmpData.Scan0;
                    System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (i + 1), ptr, fdsize);
                    bitmap.UnlockBits(bmpData);
                }
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                glyphs.Add(bitmap);

                fonts.Add($"X size:{xsize} dot, this Length:{fdsize}");
                currindex += fdsize + 1;
            }
            record.somebinary = glyphs.ToArray();
            var fdlist = string.Join<string>(", ", fonts);
            return $"Length:{length}, Y size:{ysize} byte, 1st code:{c1:X}, Last code:{c2:X}, Each data: {fdlist}";
        }

        //  GS  ( E 1D 28 45 0005-FFFF 09 01/02 20-7E 20-7E [10/11/18 00-FF...]...
        internal static string DecodeGsDefineRasterFormatCharacterCodePage(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length <= 5)
            {
                return "Length out of range";
            }
            int x = record.cmddata[index + 3];
            string xsize;
            switch (x)
            {
                case 1:
                    xsize = "1";
                    break;
                case 2:
                    xsize = "2";
                    break;
                default:
                    xsize = "Undefined";
                    break;
            }

            var c1 = record.cmddata[index + 4];
            var c2 = record.cmddata[index + 5];
            var count = c2 - c1 + 1;
            var fonts = new List<string>();
            var glyphs = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = 9; (i < count) && (currindex < record.cmdlength); i++)
            {
                int y = record.cmddata[currindex];
                string ysize;
                switch (y)
                {
                    case 16:
                        ysize = "16";
                        break;
                    case 17:
                        ysize = "17";
                        break;
                    case 24:
                        ysize = "24";
                        break;
                    default:
                        ysize = "Undefined";
                        break;
                }

                var fdsize = (x * y);
                var bitmap = new System.Drawing.Bitmap((x * 8), y, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = Color.Black;
                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (i + 1), ptr, fdsize);
                bitmap.UnlockBits(bmpData);
                glyphs.Add(bitmap);

                fonts.Add($"Y size:{ysize} dot, this Length:{fdsize}");
                currindex += fdsize + 1;
            }
            record.somebinary = glyphs.ToArray();
            var fdlist = string.Join<string>(", ", fonts);
            return $"Length:{length}, X size:{xsize} byte, 1st code:{c1:X}, Last code:{c2:X}, Each data: {fdlist}";
        }

        //  GS  ( E 1D 28 45 03 00 0A 80-FF 80-FF
        internal static string DecodeGsDeleteCharacterCodePage(EscPosCmd record, int index)
        {
            var c1 = record.cmddata[index];
            var c2 = record.cmddata[index + 1];
            return $"1st code:{c1:X}, Last code:{c2:X}";
        }

        //  GS  ( E 1D 28 45 0003-0008 0B 01-04 30-39...
        internal static string DecodeGsSetSerialInterface(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            int mode = record.cmddata[index + 3];
            int data = record.cmddata[index + 4];
            string config;
            switch (mode)
            {
                case 1:
                    config = "Transmission speed";
                    break;
                case 2:
                    config = "Parity";
                    break;
                case 3:
                    config = "Flow control";
                    break;
                case 4:
                    config = "Data bits length";
                    break;
                default:
                    config = "Undefined";
                    break;
            }

            string value;
            switch (mode)
            {
                case 1:
                    value = ascii.GetString(record.cmddata, 4, (length - 2));
                    break;
                case 2:
                    switch (data)
                    {
                        case 48:
                            value = "None parity";
                            break;
                        case 49:
                            value = "Odd parity";
                            break;
                        case 50:
                            value = "Even parity";
                            break;
                        default:
                            value = "Undefined parity";
                            break;
                    }

                    break;
                case 3:
                    switch (data)
                    {
                        case 48:
                            value = "Flow control of DTR/DSR";
                            break;
                        case 49:
                            value = "Flow control of XON/XOFF";
                            break;
                        default:
                            value = "Undefined flow control";
                            break;
                    }

                    break;
                case 4:
                    switch (data)
                    {
                        case 55:
                            value = "7 bits length";
                            break;
                        case 56:
                            value = "8 bits length";
                            break;
                        default:
                            value = "Undefined bits length";
                            break;
                    }

                    break;
                default:
                    value = "Undefined";
                    break;
            }

            return $"Setting:{config}, Value:{value}";
        }

        //  GS  ( E 1D 28 45 02 00 0C 01-04
        internal static string DecodeGsTransmitSerialInterface(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 1:
                    return "Transmission speed";
                case 2:
                    return "Parity";
                case 3:
                    return "Flow control";
                case 4:
                    return "Data bits length";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( E 1D 28 45 0003-0021 0D [31/41/46/49 20-7E...]...
        internal static string DecodeGsSetBluetoothInterface(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            string config;
            switch (record.cmddata[index + 3])
            {
                case 49:
                    config = "Passkey";
                    break;
                case 65:
                    config = "Device name";
                    break;
                case 70:
                    config = "Bundle Seed ID";
                    break;
                case 73:
                    config = "Automatic reconnection with iOS device";
                    break;
                default:
                    config = "Undefined";
                    break;
            }

            var value = ascii.GetString(record.cmddata, 4, (length - 2));
            return $"Setting:{config}, Value:{value}";
        }

        //  GS  ( E 1D 28 45 02 00 0E 30/31/41/46/49
        internal static string DecodeGsTransmitBluetoothInterface(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "Device address";
                case 49:
                    return "Passkey";
                case 65:
                    return "Device name";
                case 70:
                    return "Bundle Seed ID";
                case 73:
                    return "Automatic reconnection with iOS device";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( E 1D 28 45 03 00 0F 01/20 30/31
        internal static string DecodeGsSetUSBInterface(EscPosCmd record, int index)
        {
            int value = record.cmddata[index + 1];
            switch (record.cmddata[index])
            {
                case 1:
                    switch (value)
                    {
                        case 48:
                            return "Class settings: Vendor-defined class";
                        case 49:
                            return "Class settings: Printer class";
                        default:
                            return "Undefined class settings";
                    }
                case 32:
                    switch (value)
                    {
                        case 48:
                            return "IEEE1284 DeviceID settings: Do not transmit";
                        case 49:
                            return "IEEE1284 DeviceID settings: Transmits";
                        default:
                            return "Undefined IEEE1284 DeviceID settings";
                    }
                default:
                    return "Undefined";
            }
        }

        //  GS  ( E 1D 28 45 02 00 10 01/20
        internal static string DecodeGsTransmitUSBInterface(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 1:
                    return "Class settings";
                case 32:
                    return "IEEE1284 DeviceID settings";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( E 1D 28 45 0009-0024 31 {34 38/34 39/36 34} 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B
        internal static string DecodeGsSetPaperLayout(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            string mode;
            switch (record.cmddata[index + 3])
            {
                case 48:
                    mode = "None(does not use layout)";
                    break;
                case 49:
                    mode = "Top of black mark";
                    break;
                case 64:
                    mode = "Bottom of label";
                    break;
                default:
                    mode = "Undefined";
                    break;
            }

            var value = ascii.GetString(record.cmddata, 8, (length - 3));
            return $"Setting:{mode}, Value:{value}";
        }

        //  GS  ( E 1D 28 45 02 00 32 40/50
        internal static string DecodeGsTransmitPaperLayout(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 64:
                    return "Setting value of the paper layout (unit: 0.1 mm {0.004\"})";
                case 80:
                    return "Actual value of the paper layout (unit: dot)";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( E 1D 28 45 002D-0048 33 20-7E...
        internal static string DecodeGsSetControlLabelPaperAndBlackMarks(EscPosCmd record, int index)
        {
            return "T.B.D.";
        }

        //  GS  ( E 1D 28 45 11 00 34 20-7E... 00
        internal static string DecodeGsTransmitControlSettingsLabelPaperAndBlackMarks(EscPosCmd record, int index)
        {
            return "T.B.D.";
        }

        //  GS  ( E 1D 28 45 000E-FFFF 63 [01-05 [00/01 00-64]...]...
        internal static string DecodeGsSetInternalBuzzerPatterns(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 14) || (((length - 1) % 13) != 0))
            {
                return "Length out of range or alignment";
            }
            var count = (length - 1) / 13;
            var beeps = new List<string>();
            for (int i = 0, currindex = 6; (i < count) && (currindex < record.cmdlength); i++)
            {
                string n;
                switch (record.cmddata[currindex])
                {
                    case 1:
                        n = "A";
                        break;
                    case 2:
                        n = "B";
                        break;
                    case 3:
                        n = "C";
                        break;
                    case 4:
                        n = "D";
                        break;
                    case 5:
                        n = "E";
                        break;
                    default:
                        n = "Undefined";
                        break;
                }

                var onoff = new List<string>();
                currindex++;
                for (var j = 0; j < 6; j++, currindex += 2)
                {
                    string sound;
                    switch (record.cmddata[currindex])
                    {
                        case 0:
                            sound = "Off";
                            break;
                        case 1:
                            sound = "On";
                            break;
                        default:
                            sound = "Undefined";
                            break;
                    }

                    var duration = record.cmddata[currindex + 1] <= 100 ? record.cmddata[currindex + 1].ToString("D", invariantculture) : "Out of range";
                    onoff.Add($"Sound:{sound}, Duration:{duration} x 100ms");
                }
                beeps.Add($"Pattern:{n} dot, this Length:{string.Join<string>(", ", onoff)}");
            }
            var beeplist = string.Join<string>(", ", beeps);
            return $"Length:{length}, Each data: {beeplist}";
        }

        //  GS  ( E 1D 28 45 02 00 64 01-05
        internal static string DecodeGsTransmitInternalBuzzerPatterns(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 1:
                    return "A";
                case 2:
                    return "B";
                case 3:
                    return "C";
                case 4:
                    return "D";
                case 5:
                    return "E";
                default:
                    return "Undefined";
            }
        }

        //c GS  ( G 1D 28 47 02 00 30 04/44
        internal static string DecodeGsSelectSideOfSlipFaceOrBack(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 4:
                    return "Face of slip";
                case 68:
                    return "Back of slip";
                default:
                    return "Undefined";
            }
        }

        //c GS  ( G 1D 28 47 04 00 3C 01 00 00/01
        internal static string DecodeGsReadMagneticInkCharacterAndTransmitReadingResult(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                    return "E13B.";
                case 1:
                    return "CMC7.";
                default:
                    return "Undefined";
            }
        }

        //c GS  ( G 1D 28 47 0005-0405 40 0000-FFFF 30 01-03 00/01 30 0000 00-FF...
        internal static string DecodeGsReadDataAndTransmitResultingInformation(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 5) || (length > 1029))
            {
                return "Length out of range";
            }
            int dataid = BitConverter.ToUInt16(record.cmddata, (index + 3));
            string scanning;
            switch (record.cmddata[index + 6])
            {
                case 1:
                    scanning = "Magnetic ink character";
                    break;
                case 2:
                    scanning = "Image data";
                    break;
                case 3:
                    scanning = "Magnetic ink character and Image data";
                    break;
                default:
                    scanning = "Undefined";
                    break;
            }

            string font;
            switch (record.cmddata[index])
            {
                case 0:
                    font = "E13B.";
                    break;
                case 1:
                    font = "CMC7.";
                    break;
                default:
                    font = "Undefined";
                    break;
            }

            return $"Length:{length}, Data ID:{dataid}, Scanning:{scanning}, Font:{font}";
        }

        //c GS  ( G 1D 28 47 0005-0405 41 0001-FFFF 30/31 30 00-FF...
        internal static string DecodeGsScanImageDataAndTransmitImageScanningResult(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 5) || (length > 1029))
            {
                return "Length out of range";
            }
            int dataid = BitConverter.ToUInt16(record.cmddata, (index + 3));
            string store;
            switch (record.cmddata[index + 5])
            {
                case 48:
                    store = "To Work area temporarily";
                    break;
                case 49:
                    store = "To NV Memory Image data storage";
                    break;
                default:
                    store = "Undefined";
                    break;
            }

            return $"Length:{length}, Data ID:{dataid}, Store:{store}";
        }

        //c GS  ( G 1D 28 47 0003-0004 42 0001-FFFF [30/31]
        internal static string DecodeGsRetransmitImageScanningResult(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 3) || (length > 4))
            {
                return "Length out of range";
            }
            int dataid = BitConverter.ToUInt16(record.cmddata, (index + 3));
            string side;
            switch (length)
            {
                case 3:
                    side = "No specified side";
                    break;
                case 4:
                    switch (record.cmddata[index + 5])
                    {
                        case 48:
                            side = "Face side";
                            break;
                        case 49:
                            side = "Back side";
                            break;
                        default:
                            side = "Undefined";
                            break;
                    }

                    break;
                default:
                    side = "Undefined";
                    break;
            }

            return $"Length:{length}, Data ID:{dataid}, Side:{side}";
        }

        //c GS  ( G 1D 28 47 04 00 44 30 0001-FFFF
        internal static string DecodeGsDeleteImageScanningResultWithSpecifiedDataID(EscPosCmd record, int index)
        {
            int dataid = BitConverter.ToUInt16(record.cmddata, index);
            return $"Data ID:{dataid}";
        }

        //c GS  ( G 1D 28 47 02 00 50 b00xxxxxx
        internal static string DecodeGsSelectActiveSheet(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var check = (mode & 0x20) == 0x20 ? "Select" : "Do not Select";
            var card = (mode & 0x10) == 0x10 ? "Select" : "Do not Select";
            var validation = (mode & 0x08) == 0x08 ? "Select" : "Do not Select";
            var slip = (mode & 0x04) == 0x04 ? "Select" : "Do not Select";
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

            return $"Check:{check}, Card:{card}, Validation:{validation}, Slip:{slip}, Roll paper:{roll}";
        }

        //c GS  ( G 1D 28 47 02 00 55 30/31
        internal static string DecodeGsFinishProcessingOfCutSheet(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "Ejecting operation";
                case 49:
                    return "Releasing operation";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( H 1D 28 48 06 00 30 30 20-7E 20-7E 20-7E 20-7E
        internal static string DecodeGsSpecifiesProcessIDResponse(EscPosCmd record, int index)
        {
            return ascii.GetString(record.cmddata, index, 4);
        }

        //  GS  ( H 1D 28 48 03 00 31 30 00-02/30-32
        internal static string DecodeGsSpecifiesOfflineResponse(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Turns off the offline response transmission.";
                case 1:
                case 49:
                    return "Specifies the offline response transmission (not including the offline cause).";
                case 2:
                case 50:
                    return "Specifies the offline response transmission (including the offline cause).";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( K 1D 28 4B 02 00 30 00-04/30-34
        internal static string DecodeGsSelectPrintControlMode(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Print mode when power is turned on";
                case 1:
                case 49:
                    return "Print control mode 1";
                case 2:
                case 50:
                    return "Print control mode 2";
                case 3:
                case 51:
                    return "Print control mode 3";
                case 4:
                case 52:
                    return "Print control mode 4";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( K 1D 28 4B 02 00 31 80-7F
        internal static string DecodeGsSelectPrintDensity(EscPosCmd record, int index)
        {
            string value;
            switch(record.cmddata[index])
            {
                case 250: value = " x 70%"; break;
                case 251: value = " x 75%"; break;
                case 252: value = " x 80%"; break;
                case 253: value = " x 85%"; break;
                case 254: value = " x 90%"; break;
                case 255: value = " x 95%"; break;
                case 0: value = ""; break;
                case 1: value = " x 105%"; break;
                case 2: value = " x 110%"; break;
                case 3: value = " x 115%"; break;
                case 4: value = " x 120%"; break;
                case 5: value = " x 125%"; break;
                case 6: value = " x 130%"; break;
                case 7: value = " x 135%"; break;
                case 8: value = " x 140%"; break;
                default: value = " Undefined"; break;
            };
            return "Criterion density" + value;
        }

        //  GS  ( K 1D 28 4B 02 00 32 00-0D/30-39
        internal static string DecodeGsSelectPrintSpeed(EscPosCmd record, int index)
        {
            var value = string.Empty;
            switch(record.cmddata[index])
            {
                case 0: value = "Customized value"; break;
                case 48: value = "Customized value"; break;
                case 1: value = "1"; break;
                case 49: value = "1"; break;
                case 2: value = "2"; break;
                case 50: value = "2"; break;
                case 3: value = "3"; break;
                case 51: value = "3"; break;
                case 4: value = "4"; break;
                case 52: value = "4"; break;
                case 5: value = "5"; break;
                case 53: value = "5"; break;
                case 6: value = "6"; break;
                case 54: value = "6"; break;
                case 7: value = "7"; break;
                case 55: value = "7"; break;
                case 8: value = "8"; break;
                case 56: value = "8"; break;
                case 9: value = "9"; break;
                case 57: value = "9"; break;
                case 10: value = "10"; break;
                case 58: value = "10"; break;
                case 11: value = "11"; break;
                case 59: value = "11"; break;
                case 12: value = "12"; break;
                case 13: value = "13"; break;
                case 14: value = "14"; break;
                default: value = "Undefined"; break;
            };
            return "Print speed level " + value;
        }

        //  GS  ( K 1D 28 4B 02 00 61 00-04/30-34/80
        internal static string DecodeGsSelectNumberOfPartsThermalHeadEnergizing(EscPosCmd record, int index)
        {
            string value;
            switch(record.cmddata[index])
            {
                case 0: value = "Customized value"; break;
                case 48: value = "Customized value"; break;
                case 1: value = "One-part"; break;
                case 49: value = "One-part"; break;
                case 2: value = "Two-part"; break;
                case 50: value = "Two-part"; break;
                case 3: value = "Three-part"; break;
                case 51: value = "Three-part"; break;
                case 4: value = "Four-part"; break;
                case 52: value = "Four-part"; break;
                case 14: value = "Automatic control"; break;
                default: value = "Undefined"; break;
            }

            return value + " energizing";
        }

        //  GS  ( L 1D 28 4C 000C-FFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        internal static string DecodeGsDefineNVGraphicsDataRasterW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 4])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            var keycode = ascii.GetString(record.cmddata, (index + 5), 2);
            int plane = record.cmddata[index + 7];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            var size = ((width + 7) / 8) * height;
            var buffers = new List<string>();
            var planes = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = (index + 12); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c;
                switch (record.cmddata[currindex])
                {
                    case 49:
                        c = "1";
                        break;
                    case 50:
                        c = "2";
                        break;
                    case 51:
                        c = "3";
                        break;
                    case 52:
                        c = "4";
                        break;
                    default:
                        c = "Undefined";
                        break;
                }

                var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                switch (c)
                {
                    case "1":
                        palette.Entries[1] = Color.Black;
                        break;
                    case "2":
                        palette.Entries[1] = Color.Red;
                        break;
                    case "3":
                        palette.Entries[1] = Color.Green;
                        break;
                    case "4":
                        palette.Entries[1] = Color.Blue;
                        break;
                    default:
                        palette.Entries[1] = Color.Yellow;
                        break;
                }

                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                bitmap.UnlockBits(bmpData);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            var bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        internal static string DecodeGsDefineNVGraphicsDataRasterDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 6])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            var keycode = ascii.GetString(record.cmddata, (index + 7), 2);
            int plane = record.cmddata[index + 9];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            var size = ((width + 7) / 8) * height;
            var buffers = new List<string>();
            var planes = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = (index + 14); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c;
                switch (record.cmddata[currindex])
                {
                    case 49:
                        c = "1";
                        break;
                    case 50:
                        c = "2";
                        break;
                    case 51:
                        c = "3";
                        break;
                    case 52:
                        c = "4";
                        break;
                    default:
                        c = "Undefined";
                        break;
                }

                var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                switch (c)
                {
                    case "1":
                        palette.Entries[1] = Color.Black;
                        break;
                    case "2":
                        palette.Entries[1] = Color.Red;
                        break;
                    case "3":
                        palette.Entries[1] = Color.Green;
                        break;
                    case "4":
                        palette.Entries[1] = Color.Blue;
                        break;
                    default:
                        palette.Entries[1] = Color.Yellow;
                        break;
                }

                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                bitmap.UnlockBits(bmpData);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            var bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  ( L 1D 28 4C 000C-FFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        internal static string DecodeGsDefineNVGraphicsDataColumnW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 4])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            var keycode = ascii.GetString(record.cmddata, (index + 5), 2);
            int plane = record.cmddata[index + 7];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            var size = width * ((height + 7) / 8);
            var buffers = new List<string>();
            var planes = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = (index + 12); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c;
                switch (record.cmddata[currindex])
                {
                    case 49:
                        c = "1";
                        break;
                    case 50:
                        c = "2";
                        break;
                    case 51:
                        c = "3";
                        break;
                    case 52:
                        c = "4";
                        break;
                    default:
                        c = "Undefined";
                        break;
                }

                var bitmap = new System.Drawing.Bitmap(height, width, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                switch (c)
                {
                    case "1":
                        palette.Entries[1] = Color.Black;
                        break;
                    case "2":
                        palette.Entries[1] = Color.Red;
                        break;
                    case "3":
                        palette.Entries[1] = Color.Green;
                        break;
                    case "4":
                        palette.Entries[1] = Color.Blue;
                        break;
                    default:
                        palette.Entries[1] = Color.Yellow;
                        break;
                }

                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            var bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        internal static string DecodeGsDefineNVGraphicsDataColumnDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 6])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            var keycode = ascii.GetString(record.cmddata, (index + 7), 2);
            int plane = record.cmddata[index + 9];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            var size = width * ((height + 7) / 8);
            var buffers = new List<string>();
            var planes = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = (index + 14); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c;
                switch (record.cmddata[currindex])
                {
                    case 49:
                        c = "1";
                        break;
                    case 50:
                        c = "2";
                        break;
                    case 51:
                        c = "3";
                        break;
                    case 52:
                        c = "4";
                        break;
                    default:
                        c = "Undefined";
                        break;
                }

                var bitmap = new System.Drawing.Bitmap(height, width, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                switch (c)
                {
                    case "1":
                        palette.Entries[1] = Color.Black;
                        break;
                    case "2":
                        palette.Entries[1] = Color.Red;
                        break;
                    case "3":
                        palette.Entries[1] = Color.Green;
                        break;
                    case "4":
                        palette.Entries[1] = Color.Blue;
                        break;
                    default:
                        palette.Entries[1] = Color.Yellow;
                        break;
                }

                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            var bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  ( L 1D 28 4C 000C-FFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        internal static string DecodeGsDefineDownloadGraphicsDataRasterW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 4])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            var keycode = ascii.GetString(record.cmddata, (index + 5), 2);
            int plane = record.cmddata[index + 7];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            var size = ((width + 7) / 8) * height;
            var buffers = new List<string>();
            var planes = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = (index + 12); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c;
                switch (record.cmddata[currindex])
                {
                    case 49:
                        c = "1";
                        break;
                    case 50:
                        c = "2";
                        break;
                    case 51:
                        c = "3";
                        break;
                    case 52:
                        c = "4";
                        break;
                    default:
                        c = "Undefined";
                        break;
                }

                var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                switch (c)
                {
                    case "1":
                        palette.Entries[1] = Color.Black;
                        break;
                    case "2":
                        palette.Entries[1] = Color.Red;
                        break;
                    case "3":
                        palette.Entries[1] = Color.Green;
                        break;
                    case "4":
                        palette.Entries[1] = Color.Blue;
                        break;
                    default:
                        palette.Entries[1] = Color.Yellow;
                        break;
                }

                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                bitmap.UnlockBits(bmpData);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            var bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        internal static string DecodeGsDefineDownloadGraphicsDataRasterDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 6])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            var keycode = ascii.GetString(record.cmddata, (index + 7), 2);
            int plane = record.cmddata[index + 9];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            var size = ((width + 7) / 8) * height;
            var buffers = new List<string>();
            var planes = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = (index + 14); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c;
                switch (record.cmddata[currindex])
                {
                    case 49:
                        c = "1";
                        break;
                    case 50:
                        c = "2";
                        break;
                    case 51:
                        c = "3";
                        break;
                    case 52:
                        c = "4";
                        break;
                    default:
                        c = "Undefined";
                        break;
                }

                var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                switch (c)
                {
                    case "1":
                        palette.Entries[1] = Color.Black;
                        break;
                    case "2":
                        palette.Entries[1] = Color.Red;
                        break;
                    case "3":
                        palette.Entries[1] = Color.Green;
                        break;
                    case "4":
                        palette.Entries[1] = Color.Blue;
                        break;
                    default:
                        palette.Entries[1] = Color.Yellow;
                        break;
                }

                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                bitmap.UnlockBits(bmpData);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            var bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  ( L 1D 28 4C 000C-FFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        internal static string DecodeGsDefineDownloadGraphicsDataColumnW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 4])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            var keycode = ascii.GetString(record.cmddata, (index + 5), 2);
            int plane = record.cmddata[index + 7];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            var size = width * ((height + 7) / 8);
            var buffers = new List<string>();
            var planes = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = (index + 12); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c;
                switch (record.cmddata[currindex])
                {
                    case 49:
                        c = "1";
                        break;
                    case 50:
                        c = "2";
                        break;
                    case 51:
                        c = "3";
                        break;
                    case 52:
                        c = "4";
                        break;
                    default:
                        c = "Undefined";
                        break;
                }

                var bitmap = new System.Drawing.Bitmap(height, width, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                switch (c)
                {
                    case "1":
                        palette.Entries[1] = Color.Black;
                        break;
                    case "2":
                        palette.Entries[1] = Color.Red;
                        break;
                    case "3":
                        palette.Entries[1] = Color.Green;
                        break;
                    case "4":
                        palette.Entries[1] = Color.Blue;
                        break;
                    default:
                        palette.Entries[1] = Color.Yellow;
                        break;
                }

                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            var bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        internal static string DecodeGsDefineDownloadGraphicsDataColumnDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 12)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 6])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            var keycode = ascii.GetString(record.cmddata, (index + 7), 2);
            int plane = record.cmddata[index + 9];
            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            var size = width * ((height + 7) / 8);
            var buffers = new List<string>();
            var planes = new List<System.Drawing.Bitmap>();
            for (int i = 0, currindex = (index + 14); (i < plane) && (currindex < record.cmdlength); i++)
            {
                string c;
                switch (record.cmddata[currindex])
                {
                    case 49:
                        c = "1";
                        break;
                    case 50:
                        c = "2";
                        break;
                    case 51:
                        c = "3";
                        break;
                    case 52:
                        c = "4";
                        break;
                    default:
                        c = "Undefined";
                        break;
                }

                var bitmap = new System.Drawing.Bitmap(height, width, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                switch (c)
                {
                    case "1":
                        palette.Entries[1] = Color.Black;
                        break;
                    case "2":
                        palette.Entries[1] = Color.Red;
                        break;
                    case "3":
                        palette.Entries[1] = Color.Green;
                        break;
                    case "4":
                        palette.Entries[1] = Color.Blue;
                        break;
                    default:
                        palette.Entries[1] = Color.Yellow;
                        break;
                }

                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (currindex + 1), ptr, size);
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                planes.Add(bitmap);

                currindex += size + 1;
                buffers.Add($"Color:{c}, Size:{size}");
            }
            record.somebinary = planes.ToArray();
            var bufferlist = string.Join<string>(", ", buffers);
            return $"Length:{length}, Tone:{tone}, KeyCode:{keycode}, Width:{width}, Height:{height}, Plane:{plane}, BufferList:{bufferlist}";
        }

        //  GS  ( L 1D 28 4C 000B-FFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
        internal static string DecodeGsStoreGraphicsDataToPrintBufferRasterW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 11)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 4])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            string bx;
            switch (record.cmddata[index + 5])
            {
                case 1:
                    bx = "1";
                    break;
                case 2:
                    bx = "2";
                    break;
                default:
                    bx = "Undefined";
                    break;
            }

            string by;
            switch (record.cmddata[index + 6])
            {
                case 1:
                    by = "1";
                    break;
                case 2:
                    by = "2";
                    break;
                default:
                    by = "Undefined";
                    break;
            }

            string color;
            switch (record.cmddata[index + 7])
            {
                case 49:
                    color = "1";
                    break;
                case 50:
                    color = "2";
                    break;
                case 51:
                    color = "3";
                    break;
                case 52:
                    color = "4";
                    break;
                default:
                    color = "Undefined";
                    break;
            }

            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            var size = ((width + 7) / 8) * height;
            record.somebinary = GetBitmap(width, height, ImageDataType.Raster, record.cmddata, (index + 12), color);
            return $"Length:{length}, Tone:{tone}, X times:{bx}, Y times:{by}, Color:{color}, Width:{width}, Height:{height}, Size:{size}";
        }

        //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
        internal static string DecodeGsStoreGraphicsDataToPrintBufferRasterDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 11)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 6])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            string bx;
            switch (record.cmddata[index + 7])
            {
                case 1:
                    bx = "1";
                    break;
                case 2:
                    bx = "2";
                    break;
                default:
                    bx = "Undefined";
                    break;
            }

            string by;
            switch (record.cmddata[index + 8])
            {
                case 1:
                    by = "1";
                    break;
                case 2:
                    by = "2";
                    break;
                default:
                    by = "Undefined";
                    break;
            }

            string color;
            switch (record.cmddata[index + 9])
            {
                case 49:
                    color = "1";
                    break;
                case 50:
                    color = "2";
                    break;
                case 51:
                    color = "3";
                    break;
                case 52:
                    color = "4";
                    break;
                default:
                    color = "Undefined";
                    break;
            }

            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            var size = ((width + 7) / 8) * height;
            record.somebinary = GetBitmap(width, height, ImageDataType.Raster, record.cmddata, (index + 14), color);
            return $"Length:{length}, Tone:{tone}, X times:{bx}, Y times:{by}, Color:{color}, Width:{width}, Height:{height}, Size:{size}";
        }

        //  GS  ( L 1D 28 4C 000B-FFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
        internal static string DecodeGsStoreGraphicsDataToPrintBufferColumnW(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 11)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 4])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            string bx;
            switch (record.cmddata[index + 5])
            {
                case 1:
                    bx = "1";
                    break;
                case 2:
                    bx = "2";
                    break;
                default:
                    bx = "Undefined";
                    break;
            }

            string by;
            switch (record.cmddata[index + 6])
            {
                case 1:
                    by = "1";
                    break;
                case 2:
                    by = "2";
                    break;
                default:
                    by = "Undefined";
                    break;
            }

            string color;
            switch (record.cmddata[index + 7])
            {
                case 49:
                    color = "1";
                    break;
                case 50:
                    color = "2";
                    break;
                case 51:
                    color = "3";
                    break;
                case 52:
                    color = "4";
                    break;
                default:
                    color = "Undefined";
                    break;
            }

            int width = BitConverter.ToUInt16(record.cmddata, (index + 8));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 10));
            var size = width * ((height + 7) / 8);
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 12), color);
            return $"Length:{length}, Tone:{tone}, X times:{bx}, Y times:{by}, Color:{color}, Width:{width}, Height:{height}, Size:{size}";
        }

        //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
        internal static string DecodeGsStoreGraphicsDataToPrintBufferColumnDW(EscPosCmd record, int index)
        {
            long length = BitConverter.ToUInt32(record.cmddata, index);
            if (length < 11)
            {
                return "Length out of range";
            }
            string tone;
            switch (record.cmddata[index + 6])
            {
                case 48:
                    tone = "Monochrome";
                    break;
                case 52:
                    tone = "Multiple tone";
                    break;
                default:
                    tone = "Undefined";
                    break;
            }

            string bx;
            switch (record.cmddata[index + 7])
            {
                case 1:
                    bx = "1";
                    break;
                case 2:
                    bx = "2";
                    break;
                default:
                    bx = "Undefined";
                    break;
            }

            string by;
            switch (record.cmddata[index + 8])
            {
                case 1:
                    by = "1";
                    break;
                case 2:
                    by = "2";
                    break;
                default:
                    by = "Undefined";
                    break;
            }

            string color;
            switch (record.cmddata[index + 9])
            {
                case 49:
                    color = "1";
                    break;
                case 50:
                    color = "2";
                    break;
                case 51:
                    color = "3";
                    break;
                case 52:
                    color = "4";
                    break;
                default:
                    color = "Undefined";
                    break;
            }

            int width = BitConverter.ToUInt16(record.cmddata, (index + 10));
            int height = BitConverter.ToUInt16(record.cmddata, (index + 12));
            var size = width * ((height + 7) / 8);
            record.somebinary = GetBitmap(width, height, ImageDataType.Column, record.cmddata, (index + 14), color);
            return $"Length:{length}, Tone:{tone}, X times:{bx}, Y times:{by}, Color:{color}, Width:{width}, Height:{height}, Size:{size}";
        }

        //  GS  ( L 1D 28 4C 04 00 30 01/31 32/33 32/33
        internal static string DecodeGsSetReferenceDotDensityGraphics(EscPosCmd record, int index)
        {
            var dpi = "Undefined";
            if ((record.cmddata[7]) == (record.cmddata[8]))
            {
                switch (record.cmddata[7])
                {
                    case 50:
                        dpi = "180";
                        break;
                    case 51:
                        dpi = "360";
                        break;
                    default:
                        dpi = "Undefined";
                        break;
                }
            }
            return dpi;
        }

        //  GS  ( L 1D 28 4C 04 00 30 42 20-7E 20-7E
        internal static string DecodeGsDeleteSpecifiedNVGraphicsData(EscPosCmd record, int index)
        {
            return ascii.GetString(record.cmddata, index, 2);
        }

        //  GS  ( L 1D 28 4C 06 00 30 45 20-7E 20-7E 01/02 01/02
        internal static string DecodeGsPrintSpecifiedNVGraphicsData(EscPosCmd record, int index)
        {
            var keycode = ascii.GetString(record.cmddata, index, 2);
            string x;
            switch (record.cmddata[index + 2])
            {
                case 1:
                    x = "1";
                    break;
                case 2:
                    x = "2";
                    break;
                default:
                    x = "Undefined";
                    break;
            }

            string y;
            switch (record.cmddata[index + 3])
            {
                case 1:
                    y = "1";
                    break;
                case 2:
                    y = "2";
                    break;
                default:
                    y = "Undefined";
                    break;
            }

            return $"Keycode:{keycode}, X times:{x}, Y times:{y}";
        }

        //  GS  ( L 1D 28 4C 04 00 30 52 20-7E 20-7E
        internal static string DecodeGsDeleteSpecifiedDownloadGraphicsData(EscPosCmd record, int index)
        {
            return ascii.GetString(record.cmddata, index, 2);
        }

        //  GS  ( L 1D 28 4C 06 00 30 55 20-7E 20-7E 01/02 01/02
        internal static string DecodeGsPrintSpecifiedDownloadGraphicsData(EscPosCmd record, int index)
        {
            var keycode = ascii.GetString(record.cmddata, index, 2);
            string x;
            switch (record.cmddata[index + 2])
            {
                case 1:
                    x = "1";
                    break;
                case 2:
                    x = "2";
                    break;
                default:
                    x = "Undefined";
                    break;
            }

            string y;
            switch (record.cmddata[index + 3])
            {
                case 1:
                    y = "1";
                    break;
                case 2:
                    y = "2";
                    break;
                default:
                    y = "Undefined";
                    break;
            }

            return $"Keycode:{keycode}, X times:{x}, Y times:{y}";
        }

        //  GS  ( M 1D 28 4D 02 00 01/31 01/31
        internal static string DecodeGsSaveSettingsValuesFromWorkToStorage(EscPosCmd record, int index)
        {
            return ((record.cmddata[index] == 1) || (record.cmddata[index] == 49)) ? "" : "Undefined";
        }

        //  GS  ( M 1D 28 4D 02 00 02/32 00/01/30/31
        internal static string DecodeGsLoadSettingsValuesFromStorageToWork(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Initial value loaded";
                case 1:
                case 49:
                    return "1st saved value loaded";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( M 1D 28 4D 02 00 03/33 00/01/30/31
        internal static string DecodeGsSelectSettingsValuesToWorkAfterInitialize(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Select initial value";
                case 1:
                case 49:
                    return "Select 1st saved value";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( N 1D 28 4E 02 00 30 30-33
        internal static string DecodeGsSetCharacterColor(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "None(not print)";
                case 49:
                    return "Color 1";
                case 50:
                    return "Color 2";
                case 51:
                    return "Color 3";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( N 1D 28 4E 02 00 31 30-33
        internal static string DecodeGsSetBackgroundColor(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "None(not print)";
                case 49:
                    return "Color 1";
                case 50:
                    return "Color 2";
                case 51:
                    return "Color 3";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( N 1D 28 4E 03 00 32 00/01/30/31 30-33
        internal static string DecodeGsTurnShadingMode(EscPosCmd record, int index)
        {
            string onoff;
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    onoff = "OFF";
                    break;
                case 1:
                case 49:
                    onoff = "ON";
                    break;
                default:
                    onoff = "Undefined";
                    break;
            }

            string color;
            switch (record.cmddata[index])
            {
                case 48:
                    color = "None(not print)";
                    break;
                case 49:
                    color = "1";
                    break;
                case 50:
                    color = "2";
                    break;
                case 51:
                    color = "3";
                    break;
                default:
                    color = "Undefined";
                    break;
            }

            return $"Shadow mode:{onoff}, Color:{color}";
        }

        //  GS  ( P 1D 28 50 08 00 30 FFFF 0001-FFFF 0000 01
        internal static string DecodeGsSetPrinttableArea(EscPosCmd record, int index)
        {
            int wx = BitConverter.ToUInt16(record.cmddata, index);
            int wy = BitConverter.ToUInt16(record.cmddata, index + 2);
            int ox = BitConverter.ToUInt16(record.cmddata, index + 4);
            string c;
            switch (record.cmddata[6])
            {
                case 1:
                    c = "1";
                    break;
                default:
                    c = "Undefined";
                    break;
            }

            return $"Horizontal size:{wx}, Vertical size:{wy}, Horizontal offset:{ox}, c:{c}";
        }

        //  GS  ( Q 1D 28 51 0C 00 30 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30
        internal static string DecodeGsDrawLineInPageMode(EscPosCmd record, int index)
        {
            int x1 = BitConverter.ToUInt16(record.cmddata, index);
            int y1 = BitConverter.ToUInt16(record.cmddata, index + 2);
            int x2 = BitConverter.ToUInt16(record.cmddata, index + 4);
            int y2 = BitConverter.ToUInt16(record.cmddata, index + 6);
            string c;
            switch (record.cmddata[8])
            {
                case 1:
                    c = "1";
                    break;
                default:
                    c = "Undefined";
                    break;
            }

            string m1;
            switch (record.cmddata[9])
            {
                case 1:
                    m1 = "Single line : Thin";
                    break;
                case 2:
                    m1 = "Single line : Moderately Thick";
                    break;
                case 3:
                    m1 = "Single line : Thick";
                    break;
                case 4:
                    m1 = "Double line : Thin";
                    break;
                case 5:
                    m1 = "Double line : Moderately Thick";
                    break;
                case 6:
                    m1 = "Double line : Thick";
                    break;
                default:
                    m1 = "Undefined";
                    break;
            }

            string m2;
            switch (record.cmddata[10])
            {
                case 48:
                    m2 = "0";
                    break;
                default:
                    m2 = "Undefined";
                    break;
            }

            return $"X start:{x1}, Y start:{y1}, X end:{x2}, Y end:{y2}, c:{c}, Line style:{m1}, m2:{m2}";
        }

        //  GS  ( Q 1D 28 51 0E 00 31 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30 30 01
        internal static string DecodeGsDrawRectangleInPageMode(EscPosCmd record, int index)
        {
            int x1 = BitConverter.ToUInt16(record.cmddata, index);
            int y1 = BitConverter.ToUInt16(record.cmddata, index + 2);
            int x2 = BitConverter.ToUInt16(record.cmddata, index + 4);
            int y2 = BitConverter.ToUInt16(record.cmddata, index + 6);
            string c;
            switch (record.cmddata[8])
            {
                case 1:
                    c = "1";
                    break;
                default:
                    c = "Undefined";
                    break;
            }

            string m1;
            switch (record.cmddata[9])
            {
                case 1:
                    m1 = "Single line : Thin";
                    break;
                case 2:
                    m1 = "Single line : Moderately Thick";
                    break;
                case 3:
                    m1 = "Single line : Thick";
                    break;
                case 4:
                    m1 = "Double line : Thin";
                    break;
                case 5:
                    m1 = "Double line : Moderately Thick";
                    break;
                case 6:
                    m1 = "Double line : Thick";
                    break;
                default:
                    m1 = "Undefined";
                    break;
            }

            string m2;
            switch (record.cmddata[10])
            {
                case 48:
                    m2 = "0";
                    break;
                default:
                    m2 = "Undefined";
                    break;
            }

            string m3;
            switch (record.cmddata[11])
            {
                case 48:
                    m3 = "0";
                    break;
                default:
                    m3 = "Undefined";
                    break;
            }

            string m4;
            switch (record.cmddata[12])
            {
                case 1:
                    m4 = "1";
                    break;
                default:
                    m4 = "Undefined";
                    break;
            }

            return $"X start:{x1}, Y start:{y1}, X end:{x2}, Y end:{y2}, c:{c}, Line style:{m1}, m2:{m2}, m3:{m3}, m4:{m4}";
        }

        //  GS  ( Q 1D 28 51 09 00 32 0000-023F 0000-023F 01-FF 01 01-06 30
        internal static string DecodeGsDrawHorizontalLineInStandardMode(EscPosCmd record, int index)
        {
            int x1 = BitConverter.ToUInt16(record.cmddata, index);
            int x2 = BitConverter.ToUInt16(record.cmddata, index + 2);
            var n = record.cmddata[4];
            string c;
            switch (record.cmddata[5])
            {
                case 1:
                    c = "1";
                    break;
                default:
                    c = "Undefined";
                    break;
            }

            string m1;
            switch (record.cmddata[6])
            {
                case 1:
                    m1 = "Single line : Thin";
                    break;
                case 2:
                    m1 = "Single line : Moderately Thick";
                    break;
                case 3:
                    m1 = "Single line : Thick";
                    break;
                case 4:
                    m1 = "Double line : Thin";
                    break;
                case 5:
                    m1 = "Double line : Moderately Thick";
                    break;
                case 6:
                    m1 = "Double line : Thick";
                    break;
                default:
                    m1 = "Undefined";
                    break;
            }

            string m2;
            switch (record.cmddata[7])
            {
                case 48:
                    m2 = "0";
                    break;
                default:
                    m2 = "Undefined";
                    break;
            }

            return $"X start:{x1}, X end:{x2}, Feed:{n}, c:{c}, Line style:{m1}, m2:{m2}";
        }

        //  GS  ( Q 1D 28 51 07 00 33 0000-023F 00/01 01 01-06 30
        internal static string DecodeGsDrawVerticalLineInStandardMode(EscPosCmd record, int index)
        {
            int x = BitConverter.ToUInt16(record.cmddata, index);
            string a;
            switch (record.cmddata[2])
            {
                case 0:
                    a = "Draw stop";
                    break;
                case 1:
                    a = "Draw start";
                    break;
                default:
                    a = "Undefined";
                    break;
            }

            string c;
            switch (record.cmddata[3])
            {
                case 1:
                    c = "1";
                    break;
                default:
                    c = "Undefined";
                    break;
            }

            string m1;
            switch (record.cmddata[4])
            {
                case 1:
                    m1 = "Single line : Thin";
                    break;
                case 2:
                    m1 = "Single line : Moderately Thick";
                    break;
                case 3:
                    m1 = "Single line : Thick";
                    break;
                case 4:
                    m1 = "Double line : Thin";
                    break;
                case 5:
                    m1 = "Double line : Moderately Thick";
                    break;
                case 6:
                    m1 = "Double line : Thick";
                    break;
                default:
                    m1 = "Undefined";
                    break;
            }

            string m2;
            switch (record.cmddata[5])
            {
                case 48:
                    m2 = "0";
                    break;
                default:
                    m2 = "Undefined";
                    break;
            }

            return $"X position:{x}, Action:{a}, c:{c}, Line style:{m1}, m2:{m2}";
        }

        //  GS  ( k 1D 28 6B 03 00 30 41 00-1E
        internal static string DecodeGsPDF417SetNumberOfColumns(EscPosCmd record, int index)
        {
            return record.cmddata[index] <= 30 ? record.cmddata[index].ToString("D", invariantculture) : "Out of range";
        }

        //  GS  ( k 1D 28 6B 03 00 30 42 00/3-5A
        internal static string DecodeGsPDF417SetNumberOfRows(EscPosCmd record, int index)
        {
            var rows = record.cmddata[index];
            if ((rows == 0) || ((rows >= 3) && (rows <= 90)))
            {
                return rows.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 03 00 30 43 01-08
        internal static string DecodeGsPDF417SetWidthOfModule(EscPosCmd record, int index)
        {
            var modules = record.cmddata[index];
            if ((modules >= 1) && (modules <= 8))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 03 00 30 44 02-08
        internal static string DecodeGsPDF417SetRowHeight(EscPosCmd record, int index)
        {
            var height = record.cmddata[index];
            if ((height >= 2) && (height <= 8))
            {
                return height.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 04 00 30 45 30/31 30-38/00-28
        internal static string DecodeGsPDF417SetErrorCollectionLevel(EscPosCmd record, int index)
        {
            var m = record.cmddata[index];
            var n = record.cmddata[index + 1];
            string collection;
            var value = "";
            switch (m)
            {
                case 48:
                    collection = "Level";
                    value = ((n >= 48) && (n <= 56)) ? "Level " + ascii.GetString(record.cmddata, (index + 1), 1) : "Out of range";
                    break;

                case 49:
                    collection = "Ratio";
                    value = ((n >= 1) && (n <= 40)) ? (n * 10).ToString("D", invariantculture) + " %" : "Out of range";
                    break;

                default:
                    collection = "Undefined";
                    break;
            };
            return $"Error collection type:{collection}, Value:{value}";
        }

        //  GS  ( k 1D 28 6B 03 00 30 46 00/01
        internal static string DecodeGsPDF417SelectOptions(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                    return "Standard PDF417";
                case 1:
                    return "Truncated PDF417";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( k 1D 28 6B 0004-FFFF 30 50 30 00-FF...
        internal static string DecodeGsPDF417StoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 4)
            {
                return "Length out of range";
            }
            return (length - 3).ToString("D", invariantculture);
        }

        //  GS  ( k 1D 28 6B 04 00 31 41 31-33 00
        internal static string DecodeGsQRCodeSelectModel(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "Model 1";
                case 49:
                    return "Model 2";
                case 50:
                    return "Micro QR Code";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( k 1D 28 6B 03 00 31 43 01-10
        internal static string DecodeGsQRCodeSetSizeOfModule(EscPosCmd record, int index)
        {
            var modules = record.cmddata[index];
            if ((modules >= 1) && (modules <= 16))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 03 00 31 45 30-33
        internal static string DecodeGsQRCodeSetErrorCollectionLevel(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 48:
                    return "L";
                case 49:
                    return "M";
                case 50:
                    return "Q";
                case 51:
                    return "H";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( k 1D 28 6B 0004-1BB4 31 50 30 00-FF...
        internal static string DecodeGsQRCodeStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 4) || (length > 7092))
            {
                return "Length out of range";
            }
            return (length - 3).ToString("D", invariantculture);
        }

        //  GS  ( k 1D 28 6B 03 00 32 41 32-36
        internal static string DecodeGsMaxiCodeSelectMode(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 50:
                    return "2";
                case 51:
                    return "3";
                case 52:
                    return "4";
                case 53:
                    return "5";
                case 54:
                    return "6";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( k 1D 28 6B 0004-008D 32 50 30 00-FF...
        internal static string DecodeGsMaxiCodeStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 4) || (length > 141))
            {
                return "Length out of range";
            }
            return (length - 3).ToString("D", invariantculture);
        }

        //  GS  ( k 1D 28 6B 03 00 33 43 02-08
        internal static string DecodeGs2DGS1DBSetWidthOfModule(EscPosCmd record, int index)
        {
            var modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 8))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 04 00 33 47 0000/006A-0F70
        internal static string DecodeGs2DGS1DBSetExpandStackedMaximumWidth(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            return ((length == 0) || ((length >= 106) && (length <= 3952))) ? length.ToString("D", invariantculture) : "Length out of range";
        }

        //  GS  ( k 1D 28 6B 0006-0103 33 50 30 20-22/25-2F/30-39/3A-3F/41-5A/61-7A...
        internal static string DecodeGs2DGS1DBStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 6) || (length > 259))
            {
                return "Length out of range";
            }
            string symbol;
            switch (record.cmddata[index + 5])
            {
                case 72:
                    symbol = "GS1 DataBar Stacked";
                    break;
                case 73:
                    symbol = "GS1 DataBar Stacked Onmidirectional";
                    break;
                case 76:
                    symbol = "GS1 DataBar Expanded Stacked";
                    break;
                default:
                    symbol = "Undefined";
                    break;
            }

            length -= 4;
            return $"Type:{symbol}, Length:{length}";
        }

        //  GS  ( k 1D 28 6B 03 00 34 43 02-08
        internal static string DecodeGsCompositeSetWidthOfModule(EscPosCmd record, int index)
        {
            var modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 8))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 04 00 34 47 0000/006A-0F70
        internal static string DecodeGsCompositeSetExpandStackedMaximumWidth(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            return ((length == 0) || ((length >= 106) && (length <= 3952))) ? length.ToString("D", invariantculture) : "Length out of range";
        }

        //  GS  ( k 1D 28 6B 03 00 34 48 00-05/30-35/61/62
        internal static string DecodeGsCompositeSelectHRICharacterFont(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "No HRI";
                case 1:
                case 49:
                    return "With Font A HRI";
                case 2:
                case 50:
                    return "With Font B HRI";
                case 3:
                case 51:
                    return "With Font C HRI";
                case 4:
                case 52:
                    return "With Font D HRI";
                case 5:
                case 53:
                    return "With Font E HRI";
                case 97:
                    return "With Special Font A HRI";
                case 98:
                    return "With Special Font B HRI";
                default:
                    return "Undefined";
            }
        }

        //  GS  ( k 1D 28 6B 0006-093E 34 50 30 00-FF...
        internal static string DecodeGsCompositeStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 6) || (length > 2366))
            {
                return "Length out of range";
            }
            var a = record.cmddata[index + 5];
            var b = record.cmddata[index + 6];
            string element;
            var symbol = "";
            var data = "";
            var k = length - 5;
            var bcindex = 10;
            switch (a)
            {
                case 48:
                    element = "Linear element";
                    switch (b)
                    {
                        case 65:
                            symbol = "EAN8";
                            data = (k == 7) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 66:
                            symbol = "EAN13";
                            data = (k == 12) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 67:
                            symbol = "UPC-A";
                            data = (k == 11) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 68:
                            symbol = "UPC-E 6 digits";
                            data = (k == 6) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 69:
                            symbol = "UPC-E 11 digits";
                            data = (k == 11) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 70:
                            symbol = "GS1 DataBar Omnidirectional";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 71:
                            symbol = "GS1 DataBar Truncated";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 72:
                            symbol = "GS1 DataBar Stacked";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 73:
                            symbol = "GS1 DataBar Stacked Omnidirectional";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 74:
                            symbol = "GS1 DataBar Limited";
                            data = (k == 13) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 75:
                            symbol = "GS1 DataBar Expanded";
                            data = ((k >= 2) && (k <= 255)) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 76:
                            symbol = "GS1 DataBar Expanded Stacked";
                            data = ((k >= 2) && (k <= 255)) ? ascii.GetString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 77:
                            symbol = "GS1-128";
                            data = ((k >= 2) && (k <= 255)) ? BitConverter.ToString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        default:
                            symbol = "Undefined";
                            break;
                    };
                    break;

                case 49:
                    element = "2D Composite element";
                    switch (b)
                    {
                        case 65:
                            symbol = "Automatic selection according to number of digits";
                            data = ((k >= 1) && (k <= 2361)) ? BitConverter.ToString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        case 66:
                            symbol = "CC-C";
                            data = ((k >= 1) && (k <= 2361)) ? BitConverter.ToString(record.cmddata, bcindex, k) : "Invalid length";
                            break;

                        default:
                            symbol = "Undefined";
                            break;
                    };
                    break;

                default:
                    element = "Undefined";
                    break;
            };
            return $"Length:{length}, Element:{element}, Symbol:{symbol}, Data:{data}";
        }

        //  GS  ( k 1D 28 6B 04 00 35 42 00/01/30/31 00-20
        internal static string DecodeGsAztecCodeSetModeTypesAndDataLayer(EscPosCmd record, int index)
        {
            string mode;
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    mode = "Full-Range";
                    break;
                case 1:
                case 49:
                    mode = "Compact";
                    break;
                default:
                    mode = "Undefined";
                    break;
            }

            var n2 = record.cmddata[index + 1];
            string layers;
            if (n2 == 0)
            {
                layers = "Automatic processing";
            }
            else if ((n2 >= 1) && (n2 <= 32))
            {
                layers = n2.ToString("D", invariantculture);
            }
            else
            {
                layers = "Out of range";
            }
            return $"Mode:{mode}, Data Layers:{layers}";
        }

        //  GS  ( k 1D 28 6B 03 00 35 43 02-10
        internal static string DecodeGsAztecCodeSetSizeOfModule(EscPosCmd record, int index)
        {
            var modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 16))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 03 00 35 45 05-5F
        internal static string DecodeGsAztecCodeSetErrorCollectionLevel(EscPosCmd record, int index)
        {
            var level = record.cmddata[index];
            if ((level >= 5) && (level <= 95))
            {
                return level.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 0004-0EFB 35 50 30 00-FF...
        internal static string DecodeGsAztecCodeStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 4) || (length > 3835))
            {
                return "Length out of range";
            }
            var k = length - 3;
            return $"Length:{length}, Data:{BitConverter.ToString(record.cmddata, 8, k)}";
        }

        private static readonly List<byte> s_DMSquare = new List<byte>()
        {
            0,
            10, 12, 14, 16, 18,
            20, 22, 24, 26,
            32, 36,
            40, 44, 48,
            52,
            64,
            72,
            80, 88,
            96,
            104,
            120,
            132,
            144
        };

        private static readonly List<byte> s_DMRectCol = new List<byte>() { 8, 12, 16 };
        private static readonly List<byte> s_DMRect08 = new List<byte>() { 0, 18, 32 };
        private static readonly List<byte> s_DMRect12 = new List<byte>() { 0, 26, 36 };
        private static readonly List<byte> s_DMRect16 = new List<byte>() { 0, 36, 48 };

        //  GS  ( k 1D 28 6B 05 00 36 42 00/01/30/31 00-90 00-90
        internal static string DecodeGsDataMatrixSetSymbolTypeColumnsRows(EscPosCmd record, int index)
        {
            var m = record.cmddata[index];
            var d1 = record.cmddata[index + 1];
            var d2 = record.cmddata[index + 2];
            string symboltype;
            string columnsrows;
            switch (m)
            {
                case 0:
                case 48:
                    symboltype = "Suare";
                    if (d1 != d2) { columnsrows = "No square Columns, Rows"; break; }
                    if (d1 == 0) { columnsrows = "Automatic processing"; break; }
                    columnsrows = s_DMSquare.Contains(d1) ? $"{d1}, {d2}" : "Out of range";
                    break;

                case 1:
                case 49:
                    symboltype = "Rectangle";
                    if (!s_DMRectCol.Contains(d1)
                        || ((d1 == 8) && !s_DMRect08.Contains(d2))
                        || ((d1 == 12) && !s_DMRect12.Contains(d2))
                        || ((d1 == 16) && !s_DMRect16.Contains(d2))
                    )
                    {
                        columnsrows = "Out of range"; break;
                    }
                    columnsrows = (d2 == 0) ? $"Columns:{d1}, Rows:Automatic processing" : $"Columns:{d1}, Rows:{d2}";
                    break;

                default:
                    symboltype = "Undefined";
                    columnsrows = "";
                    break;
            };
            return $"SymbolType:{symboltype}, {columnsrows}";
        }

        //  GS  ( k 1D 28 6B 03 00 36 43 02-10
        internal static string DecodeGsDataMatrixSetSizeOfModule(EscPosCmd record, int index)
        {
            var modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 16))
            {
                return modules.ToString("D", invariantculture);
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  ( k 1D 28 6B 0004-0C2F 36 50 30 00-FF...
        internal static string DecodeGsDataMatrixStoreData(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if ((length < 4) || (length > 3119))
            {
                return "Length out of range";
            }
            var k = length - 3;
            return $"Length:{length}, Data:{BitConverter.ToString(record.cmddata, 8, k)}";
        }

        //c GS  ( z 1D 28 7A 0003-FFFF 2A [3C/40-42/46 00/01/30/31]...
        internal static string DecodeGsSetReadOperationsOfCheckPaper(EscPosCmd record, int index)
        {
            int length = BitConverter.ToUInt16(record.cmddata, index);
            if (length < 3)
            {
                return "Length out of range";
            }
            else if ((length & 1) != 1)
            {
                return "Invalid alignment";
            }
            var count = (length - 1) / 2;
            var ops = new List<string>();
            for (int i = 0, currindex = 6; i < count; i++, currindex += 2)
            {
                var n = record.cmddata[currindex];
                var m = record.cmddata[currindex + 1];
                string detection;
                switch (n)
                {
                    case 60:
                        detection = "Multi feed detected : read check paper";
                        break;
                    case 64:
                        detection = "Magnetic waveforms cannot detected : read check paper";
                        break;
                    case 65:
                        detection =
                            "Number of unrecognizable characters has exceeded specified number : Magnetic waveforms analysis";
                        break;
                    case 66:
                        detection = "Abnormality detected : noise measurement";
                        break;
                    case 70:
                        detection = "reading process check paper";
                        break;
                    default:
                        detection = "Undefined";
                        break;
                }

                string operation;
                switch (m)
                {
                    case 0:
                    case 48:
                        operation = "Continues";
                        break;
                    case 1:
                    case 49:
                        operation = "Cancels";
                        break;
                    default:
                        operation = "Undefined";
                        break;
                }

                if (n == 70)
                {
                    switch (m)
                    {
                        case 0:
                        case 48:
                            operation = "Paperjam detection level : High";
                            break;
                        case 1:
                        case 49:
                            operation = "Paperjam detection level : Low";
                            break;
                        default:
                            operation = "Undefined";
                            break;
                    }
                }
                ops.Add($"Type:{detection}, {operation}");
            }
            return string.Join<string>(", ", ops);
        }

        //c GS  ( z 1D 28 7A 0C 00 3E 33 01-09 00000000-3B9ACAFF 00/20/30 00000000-3B9ACAFF
        internal static string DecodeGsSetCounterForReverseSidePrint(EscPosCmd record, int index)
        {
            var k = record.cmddata[index];
            long n = BitConverter.ToUInt32(record.cmddata, index + 1);
            var d1 = record.cmddata[index + 5];
            long c = BitConverter.ToUInt32(record.cmddata, index + 6);
            var digits = ((k >= 1) && (k <= 9)) ? k.ToString("D", invariantculture) : "Digits out of range";
            string layout;
            switch (d1)
            {
                case 32:
                    layout = "Right align with leading spaces";
                    break;
                case 48:
                    layout = "Right align with leading 0";
                    break;
                case 0:
                    layout = "Left align with trailing spaces";
                    break;
                default:
                    layout = "Undefined";
                    break;
            }

            return $"Digits:{digits}, Layout:{layout}, Default Value:{n}, Inclemental Value:{c}";
        }

        //  GS  *   1D 2A 01-FF 01-FF 00-FF...
        internal static string DecodeGsObsoleteDefineDownloadedBitimage(EscPosCmd record, int index)
        {
            var x = record.cmddata[index];
            var y = record.cmddata[index + 1];
            if ((x == 0) || (y == 0))
            {
                return $"Invalid value Width:{x} dots, Height:{y} x 8 dots";
            }
            var length = x * y * 8;
            var bitmap = new System.Drawing.Bitmap((y * 8), x, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            var palette = bitmap.Palette;
            palette.Entries[0] = Color.White;
            palette.Entries[1] = Color.Black;
            bitmap.Palette = palette;
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            var ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (index + 2), ptr, length);
            bitmap.UnlockBits(bmpData);
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            record.somebinary = bitmap.Clone();
            return $"Width:{x} dots, Height:{y} x 8 dots, Length:{length}";
        }

        //  GS  /   1D 2F 00-03/30-33
        internal static string DecodeGsObsoletePrintDownloadedBitimage(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Normal";
                case 1:
                case 49:
                    return "Double Width";
                case 2:
                case 50:
                    return "Double Hight";
                case 3:
                case 51:
                    return "Quadruple";
                default:
                    return "Undefined";
            }
        }

        //  GS  C 0 1D 43 30 00-05 00-02/30-32
        internal static string DecodeGsObsoleteSelectCounterPrintMode(EscPosCmd record, int index)
        {
            string digits;
            switch (record.cmddata[index])
            {
                case 0:
                    digits = "actual digits";
                    break;
                case 1:
                    digits = "1";
                    break;
                case 2:
                    digits = "2";
                    break;
                case 3:
                    digits = "3";
                    break;
                case 4:
                    digits = "4";
                    break;
                case 5:
                    digits = "5";
                    break;
                default:
                    digits = "Out of range";
                    break;
            }

            string layout;
            switch (record.cmddata[index + 1])
            {
                case 0:
                case 48:
                    layout = "Right align with leading spaces";
                    break;
                case 1:
                case 49:
                    layout = "Right align with leading 0";
                    break;
                case 2:
                case 50:
                    layout = "Left align with trailing spaces";
                    break;
                default:
                    layout = "Undefined";
                    break;
            }

            return $"Digits:{digits}, Layout:{layout}";
        }

        //  GS  C 1 1D 43 31 0000-FFFF 0000-FFFF 00-FF 00-FF
        internal static string DecodeGsObsoleteSelectCounterModeA(EscPosCmd record, int index)
        {
            int a = BitConverter.ToUInt16(record.cmddata, index);
            int b = BitConverter.ToUInt16(record.cmddata, index + 2);
            var n = record.cmddata[4];
            var r = record.cmddata[5];
            var mode = "";
            if ((a < b) && (n != 0) && (r != 0))
            {
                mode = "Count Up";
            }
            else if ((a > b) && (n != 0) && (r != 0))
            {
                mode = "Count Down";
            }
            else if ((a == b) || (n == 0) || (r == 0))
            {
                mode = "Count Stop";
            }
            return $"Count mode:{mode}, Range:{a}, {b}, Stepping amount:{n}, Reputation number:{r}";
        }

        //  GS  C ; 1D 43 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B
        internal static string DecodeGsObsoleteSelectCounterModeB(EscPosCmd record, int index)
        {
            return ascii.GetString(record.cmddata, index, (int)(record.cmdlength - index));
        }

        //  GS  D   1D 44 30 43 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
        internal static string DecodeGsDefineWindowsBMPNVGraphicsData(EscPosCmd record, int index)
        {
            var keycode = ascii.GetString(record.cmddata, index, 2);
            string b;
            switch (record.cmddata[index + 2])
            {
                case 48:
                    b = "Monochrome(digital)";
                    break;
                case 52:
                    b = "Multiple tone";
                    break;
                default:
                    b = "Undefined";
                    break;
            }

            var c = (record.cmddata[index + 3] == 49) ? "Color 1" : "Undefined";
            using (Stream stream = new MemoryStream(record.cmddata, (index + 4), (int)(record.cmdlength - 9), false))
            {
                using (var img = System.Drawing.Image.FromStream(stream))
                {
                    record.somebinary = (System.Drawing.Bitmap)img.Clone();
                    var width = img.Width;
                    var height = img.Height;
                    long bmpsize = BitConverter.ToUInt32(record.cmddata, (index + 6));
                    return $"Length:{record.cmdlength}, Tone:{b}, KeyCode:{keycode}, Width:{width}, Height:{height}, Color:{c}, BMPsize:{bmpsize}";
                }
            }
        }

        //  GS  D   1D 44 30 53 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
        internal static string DecodeGsDefineWindowsBMPDownloadGraphicsData(EscPosCmd record, int index)
        {
            var keycode = ascii.GetString(record.cmddata, index, 2);
            string b;
            switch (record.cmddata[index + 2])
            {
                case 48:
                    b = "Monochrome(digital)";
                    break;
                case 52:
                    b = "Multiple tone";
                    break;
                default:
                    b = "Undefined";
                    break;
            }

            var c = (record.cmddata[index + 3] == 49) ? "Color 1" : "Undefined";
            using (Stream stream = new MemoryStream(record.cmddata, (index + 4), (int)(record.cmdlength - 9), false))
            {
                using (var img = System.Drawing.Image.FromStream(stream))
                {
                    record.somebinary = (System.Drawing.Bitmap)img.Clone();
                    var width = img.Width;
                    var height = img.Height;
                    long bmpsize = BitConverter.ToUInt32(record.cmddata, (index + 6));
                    return $"Length:{record.cmdlength}, Tone:{b}, KeyCode:{keycode}, Width:{width}, Height:{height}, Color:{c}, BMPsize:{bmpsize}";
                }
            }
        }

        //c GS  E   1D 45 b000x0x0x
        internal static string DecodeGsObsoleteSelectHeadControlMethod(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var head = (mode & 1) == 1 ? "Normal" : "Copy";
            var quality = (mode & 4) == 4 ? "Fine" : "Economy";
            var speed = (mode & 0x10) == 0x10 ? "Low" : "High";
            return $"Head energizing time:{head}, Print quality:{quality}, Printing speed:{speed}";
        }

        //  GS  H   1D 48 00-03/30-33
        internal static string DecodeGsSelectPrintPositionHRICharacters(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Not Printed";
                case 1:
                case 49:
                    return "Above Barcode";
                case 2:
                case 50:
                    return "Below Barcode";
                case 3:
                case 51:
                    return "Both Above and Below Barcode";
                default:
                    return "Undefined";
            }
        }

        //  GS  I   1D 49 01-03/31-33/21/23/24/41-45/60/6E-70
        internal static string DecodeGsTransmitPrinterID(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 1:
                case 49:
                    return "Printer Model ID";
                case 2:
                case 50:
                    return "Type ID";
                case 3:
                case 51:
                    return "Version ID";
                case 33:
                    return "Type Information";
                case 35:
                    return "Model specific information 35";
                case 36:
                    return "Model specific information 36";
                case 65:
                    return "Firmware Version";
                case 66:
                    return "Maker name";
                case 67:
                    return "Model name";
                case 68:
                    return "Serial number";
                case 69:
                    return "Font language";
                case 96:
                    return "Model specific information 96";
                case 110:
                    return "Model specific information 110";
                case 111:
                    return "Model specific information 111";
                case 112:
                    return "Model specific information 112";
                default:
                    return "Undefined";
            }
        }

        //  GS  P   1D 50 00-FF 00-FF
        internal static string DecodeGsSetHorizontalVerticalMotionUnits(EscPosCmd record, int index)
        {
            var x = record.cmddata[index] == 0 ? "Initial value" : record.cmddata[index].ToString("D", invariantculture);
            var y = record.cmddata[index + 1] == 0 ? "Initial value" : record.cmddata[index + 1].ToString("D", invariantculture);
            return $"Basic motion units Horizontal:{x}, Vertical:{y}";
        }

        //  GS  Q 0 1D 51 30 00-03/30-33 0001-10A0 0001-0010 00-FF...
        internal static string DecodeGsObsoletePrintVariableVerticalSizeBitimage(EscPosCmd record, int index)
        {
            string m;
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    m = "Normal";
                    break;
                case 1:
                case 49:
                    m = "Double Width";
                    break;
                case 2:
                case 50:
                    m = "Double Hight";
                    break;
                case 3:
                case 51:
                    m = "Quadruple";
                    break;
                default:
                    m = "Undefined";
                    break;
            }

            int x = BitConverter.ToUInt16(record.cmddata, index + 1);
            var xvalue = ((x >= 1) && (x <= 4256)) ? x.ToString("D", invariantculture) : "Out of range";
            int y = BitConverter.ToUInt16(record.cmddata, index + 3);
            var yvalue = ((y >= 1) && (y <= 16)) ? y.ToString("D", invariantculture) : "Out of range";
            var k = x * y;
            if (((y > 0) && (y <= 16)) && ((x > 0) && (x <= 4256)))
            {
                var bitmap = new System.Drawing.Bitmap((y * 8), x, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = Color.Black;
                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (index + 5), ptr, k);
                bitmap.UnlockBits(bmpData);
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                record.somebinary = bitmap.Clone();
            }
            return $"Mode:{m}, Width:{xvalue} dots, Height:{yvalue} bytes, Size:{k} bytes";
        }

        //  GS  T   1D 54 00/01/30/31
        internal static string DecodeGsSetPrintPositionBeginningOfPrintLine(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    return "Erases buffer data, then move print position to beginning of line";
                case 1:
                case 49:
                    return "Prints buffer data, then move print position to beginning of line";
                default:
                    return "Undefined";
            }
        }

        //  GS  ^   1D 5E 01-FF 00-FF 00/01
        internal static string DecodeGsExecuteMacro(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 0:
                    return "Continuous execution";
                case 1:
                    return "Execution by button";
                default:
                    return "Undefined";
            }
        }

        //  GS  a   1D 61 b0x00xxxx
        internal static string DecodeGsEnableDisableAutomaticStatusBack(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var panel = (mode & 0x40) == 0x40 ? "Enabled" : "Disabled";
            var rollpaper = (mode & 0x08) == 0x08 ? "Enabled" : "Disabled";
            var error = (mode & 0x04) == 0x04 ? "Enabled" : "Disabled";
            var online = (mode & 0x02) == 0x02 ? "Enabled" : "Disabled";
            var drawer = (mode & 0x01) == 0x01 ? "Enabled" : "Disabled";
            return $"Panel switch:{panel}, Roll Paper Sensor:{rollpaper}, Error:{error}, Online/Offline:{online}, Drawer kick out connector:{drawer}";
        }

        //  GS  f   1D 66 00-04/30-34/61/62
        internal static string DecodeGsSelectFontHRICharacters(EscPosCmd record, int index)
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

        //  GS  g 0 1D 67 30 00 000A-004F
        internal static string DecodeGsInitializeMaintenanceCounter(EscPosCmd record, int index)
        {
            int n = BitConverter.ToUInt16(record.cmddata, index);
            string category;
            switch ((n / 10))
            {
                case 1:
                    category = "Serial impact head";
                    break;
                case 2:
                    category = "Thermal head";
                    break;
                case 3:
                    category = "Ink jet head";
                    break;
                case 4:
                    category = "Shuttle head";
                    break;
                case 5:
                    category = "Standard devices";
                    break;
                case 6:
                    category = "Optional devices";
                    break;
                case 7:
                    category = "Time";
                    break;
                default:
                    category = "Undefined";
                    break;
            }

            return $"Value:{n}, Category:{category}";
        }

        //  GS  g 2 1D 67 32 00 000A-004F/008A-00CF
        internal static string DecodeGsTransmitMaintenanceCounter(EscPosCmd record, int index)
        {
            int n = BitConverter.ToUInt16(record.cmddata, index);
            var countertype = (n & 0x80) == 0x80 ? "Comulative" : "Resettable";
            string category;
            switch (((n & 0x7F) / 10))
            {
                case 1:
                    category = "Serial impact head";
                    break;
                case 2:
                    category = "Thermal head";
                    break;
                case 3:
                    category = "Ink jet head";
                    break;
                case 4:
                    category = "Shuttle head";
                    break;
                case 5:
                    category = "Standard devices";
                    break;
                case 6:
                    category = "Optional devices";
                    break;
                case 7:
                    category = "Time";
                    break;
                default:
                    category = "Undefined";
                    break;
            }

            return $"Value:{n}, Type:{countertype}, Category:{category}";
        }

        //  GS  j   1D 6A b000000xx
        internal static string DecodeGsEnableDisableAutomaticStatusBackInk(EscPosCmd record, int index)
        {
            var mode = record.cmddata[index];
            var online = (mode & 0x02) == 0x02 ? "Enabled" : "Disabled";
            var ink = (mode & 0x01) == 0x01 ? "Enabled" : "Disabled";
            return $"Online/Offline:{online}, Ink status:{ink}";
        }

        //  GS  k   1D 6B 00-06 20/24/25/2A/2B/2D-2F/30-39/41-5A/61-64... 00
        internal static string DecodeGsPrintBarcodeAsciiz(EscPosCmd record, int index)
        {
            var m = record.cmddata[index];
            string symbol;
            switch (m)
            {
                case 0:
                    symbol = "UPC-A";
                    break;
                case 1:
                    symbol = "UPC-E";
                    break;
                case 2:
                    symbol = "EAN13";
                    break;
                case 3:
                    symbol = "EAN8";
                    break;
                case 4:
                    symbol = "CODE39";
                    break;
                case 5:
                    symbol = "ITF";
                    break;
                case 6:
                    symbol = "CODABAR";
                    break;
                default:
                    symbol = "Undefined";
                    break;
            }

            var barcode = ascii.GetString(record.cmddata, (index + 1), (int)(record.cmdlength - 3));
            return $"Barcode Type:{symbol}, Data:{barcode}";
        }

        //  GS  k   1D 6B 41-4F 01-FF 00-FF...
        internal static string DecodeGsPrintBarcodeSpecifiedLength(EscPosCmd record, int index)
        {
            var m = record.cmddata[index];
            string symbol;
            switch (m)
            {
                case 65:
                    symbol = "UPC-A";
                    break;
                case 66:
                    symbol = "UPC-E";
                    break;
                case 67:
                    symbol = "EAN13";
                    break;
                case 68:
                    symbol = "EAN8";
                    break;
                case 69:
                    symbol = "CODE39";
                    break;
                case 70:
                    symbol = "ITF";
                    break;
                case 71:
                    symbol = "CODABAR";
                    break;
                case 72:
                    symbol = "CODE93";
                    break;
                case 73:
                    symbol = "CODE128";
                    break;
                case 74:
                    symbol = "GS1-128";
                    break;
                case 75:
                    symbol = "GS1 DataBar Omnidirectional";
                    break;
                case 76:
                    symbol = "GS1 DataBar Truncated";
                    break;
                case 77:
                    symbol = "GS1 DataBar Limited";
                    break;
                case 78:
                    symbol = "GS1 DataBar Expanded";
                    break;
                case 79:
                    symbol = "Code128 auto";
                    break;
                default:
                    symbol = "Undefined";
                    break;
            }

            var n = record.cmddata[index + 1];
            var barcode = ascii.GetString(record.cmddata, (index + 2), n);
            return $"Barcode Type:{symbol}, Data:{barcode}";
        }

        //  GS  r   1D 72 01/02/04/31/32/34
        internal static string DecodeGsTransmitStatus(EscPosCmd record, int index)
        {
            switch (record.cmddata[index])
            {
                case 1:
                case 49:
                    return "Paper sensor";
                case 2:
                case 50:
                    return "Drawer kick out connector";
                case 4:
                case 52:
                    return "Ink";
                default:
                    return "Undefined";
            }
        }

        //  GS  v 0 1D 76 30 00-03/30-33 0001-FFFF 0001-11FF 00-FF...
        internal static string DecodeGsObsoletePrintRasterBitimage(EscPosCmd record, int index)
        {
            string m;
            switch (record.cmddata[index])
            {
                case 0:
                case 48:
                    m = "Normal";
                    break;
                case 1:
                case 49:
                    m = "Double Width";
                    break;
                case 2:
                case 50:
                    m = "Double Hight";
                    break;
                case 3:
                case 51:
                    m = "Quadruple";
                    break;
                default:
                    m = "Undefined";
                    break;
            }

            int x = BitConverter.ToUInt16(record.cmddata, index + 1);
            var xvalue = (x >= 1) ? x.ToString("D", invariantculture) : "Out of range";
            int y = BitConverter.ToUInt16(record.cmddata, index + 3);
            var yvalue = ((y >= 1) && (y <= 4607)) ? y.ToString("D", invariantculture) : "Out of range";
            var k = x * y;
            if (((y > 0) && (y <= 4607)) && (x > 0))
            {
                var bitmap = new System.Drawing.Bitmap((x * 8), y, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var palette = bitmap.Palette;
                palette.Entries[0] = Color.White;
                palette.Entries[1] = Color.Black;
                bitmap.Palette = palette;
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                var ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(record.cmddata, (index + 5), ptr, k);
                bitmap.UnlockBits(bmpData);
                record.somebinary = bitmap.Clone();
            }
            return $"Mode:{m}, Width:{xvalue} bytes, Height:{yvalue} dots, Size:{k} bytes";
        }

        //  GS  w   1D 77 02-06/44-4C
        internal static string DecodeGsSetBarcodeWidth(EscPosCmd record, int index)
        {
            var modules = record.cmddata[index];
            if ((modules >= 2) && (modules <= 6))
            {
                return modules.ToString("D", invariantculture);
            }
            else if ((modules >= 68) && (modules <= 76))
            {
                var i = (modules - 64) / 2;
                var f = ((modules & 1) == 1) ? ".5" : ".0";
                return i.ToString("D", invariantculture) + f;
            }
            else
            {
                return "Out of range";
            }
        }

        //  GS  z 0 1D 7A 30 00-FF 00-FF
        internal static string DecodeGsSetOnlineRecoveryWaitTime(EscPosCmd record, int index)
        {
            return $"Paper loading wait:{record.cmddata[index]} x 500ms, Recovery confirmation time:{record.cmddata[index + 1]} x 500ms";
        }
    }
}