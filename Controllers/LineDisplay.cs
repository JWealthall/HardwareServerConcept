using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO.Ports;
using Microsoft.Extensions.Configuration;

namespace HardwareSvr.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LineDisplay : HardwareBase
    {
        #region Constructors
        public LineDisplay(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion Constructors

        #region Members
        private const int _maxColumns = 20;
        private const int _maxRows = 2;

        private static int _column = 0;
        private static int _row = 0;
        //private static CursorTypes _cursorType = CursorTypes.None;
        private static int _codePage = 437;

        private static SerialPort _port = null;

        private IConfiguration _configuration;
        private PortSettings _portSettings = new PortSettings();
        #endregion Members

        // GET
        [HttpGet]
        public string Index()
        {
            var p = Port();
            return "Hello World";
        }

        [HttpGet]
        [Route("Clear")]
        public object Clear()
        {
            var p = Port();
            if (p == null) return ReturnDe.Fail("Not connected to serial port. " + _portSettings.Message);
            lock (_port)
            {
                try
                {
                    SelectLineDisplay(p);   // Take this out if not using a EPSON device
                    // Clear, Cursor to Top, Overwrite Mode
                    Write(p, StrFF + StrVT + StrUS + StrSOH);
                    // Set cursor 'off'
                    Write(p, StrUS + "C" + StrNUL);
                }
                catch (Exception e)
                {
                    return ReturnDe.Fail("Not clear text. " + e.Message);
                }
            }
            _column = 0;
            _row = 0;
            //_cursorType = CursorTypes.None;
            return ReturnDe.Success("Cleared");
        }

        [HttpGet]
        [Route("Test")]
        public object Test()
        {
            var p = Port();
            if (p == null) return ReturnDe.Fail("Not connected to serial port. " + _portSettings.Message);
            Clear();
            var delay = 250;
            var text = "";
            for (var i = 0; i < 40; i++)
            {
                var c = i % 10;
                Display(new LineDisplayDe( c.ToString("0")));
                System.Threading.Thread.Sleep(delay / 2);
            }
            for (int i = 0; i < 95; i++)
            {
                text = "";
                for (int j = 0; j < 40; j++)
                {
                    text += char.ToString((char)(i + 32));
                }
                Display(new LineDisplayDe(text));
                System.Threading.Thread.Sleep(delay);
            }
            return ReturnDe.Success("Test Run");
        }

        [HttpPost]
        [Route("Display")]
        public ReturnDe Display(LineDisplayDe req)
        {
            var curPosChanged = false;
            if (req.Row.HasValue)
            {
                if (req.Row.Value < 0) return ReturnDe.Fail("Rows cannot be negative: " + req.Row.Value);
                if (req.Row.Value >= _maxRows) return ReturnDe.Fail("Rows cannot exceed " + _maxRows + ": " + req.Row.Value);
                if (_row != req.Row.Value) curPosChanged = true;
                _row = req.Row.Value;
            }
            if (req.Column.HasValue)
            {
                if (req.Column.Value < 0) return ReturnDe.Fail("Columns cannot be negative: " + req.Column.Value);
                if (req.Column.Value >= _maxColumns) return ReturnDe.Fail("Columns cannot exceed " + _maxColumns + ": " + req.Column.Value);
                if (_column != req.Column.Value) curPosChanged = true;
                _column = req.Column.Value;
            }
            var p = Port();
            if (p == null) return ReturnDe.Fail("Not connected to serial port. " + _portSettings.Message);
            lock (_port)
            {
                UseEncoding = false;
                bool useEncoding = false;

                SelectLineDisplay(p);   // Take this out if not using a EPSON device

                if (curPosChanged)
                    Write(p, StrUS + "$" + char.ToString((char)(_column + 1)) + char.ToString((char)(_row + 1)));

                if (req.Text.Length > 40)
                {
                    req.Text = req.Text.Substring(req.Text.Length - 40);
                }

                // THIS IS A HACK DUE TO SOME ISSUES WITH ENCODING
                if (req.Text.Contains(char.ToString((char)339)))
                {
                    req.Text = req.Text.Replace((char)339, (char)156);
                    useEncoding = true;
                }

                if (useEncoding) UseEncoding = true;
                Write(p, req.Text, _codePage);
                if (useEncoding) UseEncoding = false;
            }

            if (req.Text.Length < 40)
            {
                var curPos = _column + (_row * 20);
                curPos += req.Text.Length;
                while (curPos >= 40) curPos -= 40;
                if (curPos < 20)
                {
                    _row = 0;
                    _column = curPos;
                }
                else
                {
                    _row = 1;
                    _column = curPos - 20;
                }
            }
            return ReturnDe.Success("Displayed");
        }

        private bool LoadConfiguration()
        {
            _portSettings.Message = "";
            if (_configuration["ComPort:PortName"] != null) _portSettings.PortName = _configuration["ComPort:PortName"];

            if (_configuration["ComPort:BaudRate"] != null)
            {
                var baudRate = _configuration["ComPort:BaudRate"];
                if (!baudRate.IsInteger(false, 8))
                {
                    _portSettings.Message = "Invalid Baud Rate: " + baudRate;
                    return false;
                }
                try
                {
                    _portSettings.BaudRate = int.Parse(baudRate);
                }
                catch
                {
                    _portSettings.Message = "Invalid Baud Rate: Failed to parse - " + baudRate;
                    return false;
                }
            }

            if (_configuration["ComPort:Parity"] != null)
            {
                var parity = _configuration["ComPort:Parity"];
                switch (parity.ToLowerInvariant())
                {
                    case "none":
                        _portSettings.Parity = Parity.None;
                        break;
                    case "odd":
                        _portSettings.Parity = Parity.Odd;
                        break;
                    case "even":
                        _portSettings.Parity = Parity.Even;
                        break;
                    case "mark":
                        _portSettings.Parity = Parity.Mark;
                        break;
                    case "Space":
                        _portSettings.Parity = Parity.Space;
                        break;
                    default:
                        _portSettings.Message = "Invalid Parity: Not recognized - " + parity;
                        return false;
                }
            }

            if (_configuration["ComPort:DataBits"] != null)
            {
                var dataBits = _configuration["ComPort:DataBits"];
                if (!dataBits.IsInteger(false, 1, 8, 5))
                {
                    _portSettings.Message = "Invalid Data Bits: " + dataBits;
                    return false;
                }
                try
                {
                    _portSettings.DataBits = int.Parse(dataBits);
                }
                catch
                {
                    _portSettings.Message = "Invalid Baud Rate: Failed to parse - " + dataBits;
                    return false;
                }
            }

            if (_configuration["ComPort:StopBits"] != null)
            {
                var stopBits = _configuration["ComPort:StopBits"];
                switch (stopBits.ToLowerInvariant())
                {
                    case "none":
                        _portSettings.StopBits = StopBits.None;
                        break;
                    case "one":
                        _portSettings.StopBits = StopBits.One;
                        break;
                    case "onepointfive":
                        _portSettings.StopBits = StopBits.OnePointFive;
                        break;
                    case "two":
                        _portSettings.StopBits = StopBits.Two;
                        break;
                    default:
                        _portSettings.Message = "Invalid StopBits: Not recognized - " + stopBits;
                        return false;
                }
            }

            if (_configuration["ComPort:Handshake"] != null)
            {
                var handshake = _configuration["ComPort:Handshake"];
                switch (handshake.ToLowerInvariant())
                {
                    case "none":
                        _portSettings.Handshake = Handshake.None;
                        break;
                    case "xonxoff":
                        _portSettings.Handshake = Handshake.XOnXOff;
                        break;
                    case "requesttosend":
                        _portSettings.Handshake = Handshake.RequestToSend;
                        break;
                    case "requesttosendxonxoff":
                        _portSettings.Handshake = Handshake.RequestToSendXOnXOff;
                        break;
                    default:
                        _portSettings.Message = "Invalid Handshake: Not recognized - " + handshake;
                        return false;
                }
            }

            return true;
        }

        private SerialPort Port()
        {
            if (_port == null)
            {
                if (!LoadConfiguration()) return null;
                var p = _portSettings;
                _port = new SerialPort(p.PortName, p.BaudRate, p.Parity, p.DataBits, p.StopBits);
                _port.Handshake = p.Handshake;
                try
                {
                    _port.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return _port;
        }

        private class PortSettings
        {
            public string PortName { get; set; } = "COM1";
            public int BaudRate { get; set; } = 9600;
            public Parity Parity { get; set; } = Parity.None;
            public int DataBits { get; set; } = 8;
            public StopBits StopBits { get; set; } = StopBits.One;
            public Handshake Handshake { get; set; } = Handshake.None;
            public string Message { get; set; } = "";
        }

        public class ReturnDe
        {
            public bool OK { get; set; } = true;
            public int Status { get; set; } = 0;
            public string Message { get; set; } = "";

            public static ReturnDe Fail(string message, int status = -1) { return new ReturnDe() { OK = false, Message = message, Status = status }; }
            public static ReturnDe Success(string message, int status = 0) { return new ReturnDe() { Message = message, Status = status }; }
        }

        public class LineDisplayDe
        {
            public LineDisplayDe() { }
            public LineDisplayDe(string text) { Text = text; }
            public LineDisplayDe(string text, int column, int row) { Text = text; Column = column; Row = row; }

            public string Text { get; set; } = "";
            public int? Row { get; set; } = null;
            public int? Column { get; set; } = null;
            public int? Attribute { get; set; } = null; // EPSON displays don't actually use this.
        }
    }
}