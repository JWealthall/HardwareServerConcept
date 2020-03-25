using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HardwareSvr.Controllers
{
    public abstract class HardwareBase : ControllerBase
    {
        #region Static Values
        // Bytes used for control sequences
        protected const byte BytH00 = 0x00;  //   0  NUL
        protected const byte BytH01 = 0x01;  //   1  SOH
        protected const byte BytH02 = 0x02;  //   2  STX
        protected const byte BytH03 = 0x03;  //   3  ETX

        protected const byte BytNUL = 0x00;  //   0  NUL
        protected const byte BytSOH = 0x01;  //   1  SOH
        protected const byte BytSTX = 0x02;  //   2  STX
        protected const byte BytETX = 0x03;  //   3  ETX
        protected const byte BytEOT = 0x04;  //   3  EOT
        protected const byte BytLF = 0x0A;   //  10  LF
        protected const byte BytVT = 0x0B;   //  11  VT
        protected const byte BytFF = 0x0C;   //  12  FF
        protected const byte BytCR = 0x0D;   //  13  CR

        protected const byte BytDLE = 0x10;  //  16  DLE
        protected const byte BytDC1 = 0x11;  //  17  DC1
        protected const byte BytDC2 = 0x12;  //  18  DC2
        protected const byte BytDC3 = 0x13;  //  19  DC3
        protected const byte BytDC4 = 0x14;  //  20  DC4
        protected const byte BytNAK = 0x15;  //  21  NAK
        protected const byte BytSYN = 0x16;  //  22  SYN
        protected const byte BytETB = 0x17;  //  23  ETB
        protected const byte BytCAN = 0x18;  //  24  CAN
        protected const byte BytESC = 0x1B;  //  27  ESC
        protected const byte BytFS = 0x1C;   //  28  FS
        protected const byte BytGS = 0x1D;   //  29  GS
        protected const byte BytRS = 0x1E;   //  30  RS
        protected const byte BytUS = 0x1F;   //  31  US

        // Characters used for control sequences
        protected const char ChrH00 = (char)0x00;  //   0  NUL
        protected const char ChrH01 = (char)0x01;  //   1  SOH
        protected const char ChrH02 = (char)0x02;  //   2  STX
        protected const char ChrH03 = (char)0x03;  //   3  ETX

        protected const char ChrNUL = (char)0x00;  //   0  NUL
        protected const char ChrSOH = (char)0x01;  //   1  SOH
        protected const char ChrSTX = (char)0x02;  //   2  STX
        protected const char ChrETX = (char)0x03;  //   3  ETX
        protected const char ChrEOT = (char)0x04;  //   3  EOT
        protected const char ChrLF = (char)0x0A;   //  10  LF
        protected const char ChrVT = (char)0x0B;   //  11  VT
        protected const char ChrFF = (char)0x0C;   //  12  FF
        protected const char ChrCR = (char)0x0D;   //  13  CR

        protected const char ChrDLE = (char)0x10;  //  16  DLE
        protected const char ChrDC1 = (char)0x11;  //  17  DC1
        protected const char ChrDC2 = (char)0x12;  //  18  DC2
        protected const char ChrDC3 = (char)0x13;  //  19  DC3
        protected const char ChrDC4 = (char)0x14;  //  20  DC4
        protected const char ChrNAK = (char)0x15;  //  21  NAK
        protected const char ChrSYN = (char)0x16;  //  22  SYN
        protected const char ChrETB = (char)0x17;  //  23  ETB
        protected const char ChrCAN = (char)0x18;  //  24  CAN
        protected const char ChrESC = (char)0x1B;  //  27  ESC
        protected const char ChrFS = (char)0x1C;   //  28  FS
        protected const char ChrGS = (char)0x1D;   //  29  GS
        protected const char ChrRS = (char)0x1E;   //  30  RS
        protected const char ChrUS = (char)0x1F;   //  31  US

        protected static char ChrCurrencySymbol = (char)0xA3;

        protected static string StrH00 = ChrH00.ToString();
        protected static string StrH01 = ChrH01.ToString();
        protected static string StrH02 = ChrH02.ToString();
        protected static string StrH03 = ChrH03.ToString();

        protected static string StrNUL = ChrNUL.ToString();
        protected static string StrSOH = ChrSOH.ToString();
        protected static string StrSTX = ChrSTX.ToString();
        protected static string StrETX = ChrETX.ToString();
        protected static string StrEOT = ChrEOT.ToString();
        protected static string StrLF = ChrLF.ToString();
        protected static string StrVT = ChrVT.ToString();
        protected static string StrFF = ChrFF.ToString();
        protected static string StrCR = ChrCR.ToString();

        protected static string StrDLE = ChrDLE.ToString();
        protected static string StrDC1 = ChrDC1.ToString();
        protected static string StrDC2 = ChrDC2.ToString();
        protected static string StrDC3 = ChrDC3.ToString();
        protected static string StrDC4 = ChrDC4.ToString();
        protected static string StrNAK = ChrNAK.ToString();
        protected static string StrSYN = ChrSYN.ToString();
        protected static string StrETB = ChrETB.ToString();
        protected static string StrCAN = ChrCAN.ToString();
        protected static string StrESC = ChrESC.ToString();
        protected static string StrFS = ChrFS.ToString();
        protected static string StrGS = ChrGS.ToString();
        protected static string StrRS = ChrRS.ToString();
        protected static string StrUS = ChrUS.ToString();

        protected static string StrCRLF = ChrCR.ToString() + ChrLF.ToString();

        protected static string StrCurrencySymbol = ChrCurrencySymbol.ToString();

        protected static string StrSelectPrinter = ChrESC.ToString() + "=" + ChrH01.ToString();
        protected static string StrSelectLineDisplay = ChrESC.ToString() + "=" + ChrH02.ToString();
        protected static string StrCmdStart = ChrESC.ToString() + "|";

        private static Encoding AscEnc = new ASCIIEncoding();
        private static Encoding U32Enc = new UTF32Encoding();

        #endregion

        #region Enums
        public enum CursorTypes
        {
            None = 0x0,
            Fixed = 0x1,
            Block = 0x2,
            HalfBlock = 0x4,
            UnderLine = 0x8,
            Reverse = 0x10,
            Other = 0x20,
        }

        public enum TextAttributes
        {
            Normal = 0,
            Blink = 1,
            Reverse = 2,
            BlinkReverse = 3
        }
        #endregion Enums

        #region Members
        protected bool UseEncoding = false;
        #endregion

        #region Methods
        protected virtual void SelectPrinter(SerialPort p)
        {
            p?.Write(StrSelectPrinter);
        }

        protected virtual void SelectLineDisplay(SerialPort p)
        {
            p?.Write(StrSelectLineDisplay);
        }

        private void SetEncoding(SerialPort p)
        {
            if (UseEncoding)
                if (!UseEncoding)
                {
                    //p.Encoding = new UTF32Encoding();
                    p.Encoding = U32Enc;
                    UseEncoding = true;
                }
                else { }
            else
            if (UseEncoding)
            {
                //p.Encoding = new ASCIIEncoding();
                p.Encoding = AscEnc;
                UseEncoding = false;
            }
        }

        protected void Write(SerialPort p, string text)
        {
            if (text == null) return;
            SetEncoding(p);
            p.DtrEnable = true;
            if (p.Handshake != Handshake.RequestToSend) p.RtsEnable = true;
            p.Write(text);
        }

        protected void Write(SerialPort p, string text, int codePage)
        {
            if (text == null) return;

            if (p.Handshake != Handshake.RequestToSend) p.RtsEnable = true;

            Encoding enc = CodePagesEncodingProvider.Instance.GetEncoding(codePage);

            byte[] inBytes = Encoding.UTF32.GetBytes(text);
            byte[] outBytes = Encoding.Convert(Encoding.UTF32, enc, inBytes);

            p.Write(outBytes, 0, outBytes.Length);
        }

        protected void Write(SerialPort p, byte[] buffer, int offset, int count)
        {
            SetEncoding(p);
            p.Write(buffer, offset, count);
        }

        protected void Write(SerialPort p, char[] buffer, int offset, int count)
        {
            SetEncoding(p);
            p.Write(buffer, offset, count);
        }

        protected void WriteLine(SerialPort p, string text)
        {
            p.WriteLine(text);
        }


        #endregion Methods
    }
}
