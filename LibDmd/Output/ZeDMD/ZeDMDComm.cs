using System;
using System.IO.Ports;
using System.Linq;

namespace LibDmd.Output.ZeDMD
{
	public class ZeDMDComm // Class for locating the COM port of the ESP32 and communicating with it
	{
		public string nCOM;
		public const int BaudRate = 921600;
		public bool Opened = false;
		private SerialPort _serialPort;
		private const int MAX_SERIAL_WRITE_AT_ONCE = 256;
		public const int N_CTRL_CHARS = 6;
		public const int N_INTERMEDIATE_CTR_CHARS = 4;
		public static readonly byte[] CtrlCharacters = { 0x5a, 0x65, 0x64, 0x72, 0x75, 0x6d };
		byte[] pBytes2 = new byte[N_CTRL_CHARS + MAX_SERIAL_WRITE_AT_ONCE]; // 4 pour la synchro

		private void SafeClose()
		{
			// In case of error discard serial data and close
			_serialPort.DiscardInBuffer();
			_serialPort.DiscardOutBuffer();
			_serialPort.Close();
			System.Threading.Thread.Sleep(100); // otherwise the next device will fail
		}
		private bool Connect(string port, out int width, out int height)
		{
			// Try to find an ESP32 on the COM port and check if it answers with the shake-hand bytes
			try
			{
				_serialPort = new SerialPort(port, BaudRate, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One)
				{
					ReadTimeout = 100,
					WriteBufferSize = MAX_SERIAL_WRITE_AT_ONCE + 100,
					WriteTimeout = SerialPort.InfiniteTimeout
				};
				_serialPort.Open();
				_serialPort.Write(CtrlCharacters.Concat(new byte[] { 12 }).ToArray(), 0, CtrlCharacters.Length + 1);
				System.Threading.Thread.Sleep(100);
				var result = new byte[Math.Max(N_CTRL_CHARS + 1, N_INTERMEDIATE_CTR_CHARS + 4)];
				_serialPort.Read(result, 0, N_INTERMEDIATE_CTR_CHARS + 4);
				System.Threading.Thread.Sleep(200);
				if (!result.Take(4).SequenceEqual(CtrlCharacters.Take(4)))
				{
					SafeClose();
					width = 0;
					height = 0;
					return false;
				}
				width = result[N_INTERMEDIATE_CTR_CHARS] + result[N_INTERMEDIATE_CTR_CHARS + 1] * 256;
				height = result[N_INTERMEDIATE_CTR_CHARS + 2] + result[N_INTERMEDIATE_CTR_CHARS + 3] * 256;
				nCOM = port;
				return true;

			}
			catch
			{
				if (_serialPort != null && _serialPort.IsOpen) SafeClose();
			}
			width = 0;
			height = 0;
			return false;
		}
		private void Disconnect()
		{
			if (_serialPort != null) SafeClose();
		}
        public bool StreamBytes(byte[] pBytes, int nBytes)
        {
            if (_serialPort.IsOpen)
            {
                try
                {
                    int signal = 0;
                    int bytesToWrite = Math.Min(nBytes, MAX_SERIAL_WRITE_AT_ONCE - N_CTRL_CHARS);
                    while (signal != 'R') {
                        signal = _serialPort.ReadByte();
                        if (signal == 'E') return false;
                    }
                    _serialPort.Write(CtrlCharacters, 0, N_CTRL_CHARS);
                    _serialPort.Write(pBytes, 0, bytesToWrite);
                    while (signal != 'A') {
                        signal = _serialPort.ReadByte();
                        if (signal == 'E') return false;
                    }

                    int remainingBytes = nBytes - bytesToWrite;
                    while (remainingBytes > 0) {
                        bytesToWrite = Math.Min(remainingBytes, MAX_SERIAL_WRITE_AT_ONCE);
                        _serialPort.Write(pBytes, nBytes - remainingBytes, bytesToWrite);
                        remainingBytes -= bytesToWrite;
                        while (signal != 'A') {
                            signal = _serialPort.ReadByte();
                            if (signal == 'E') return false;
                        }
                    }
                    return true;
                }
                catch
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.DiscardOutBuffer();
                    }
                    return false;
                }
            }
            return false;
        }
		public void ResetPalettes()
		{
			// Reset ESP32 palette
			byte[] tempbuffer = new byte[N_CTRL_CHARS + 1];
			for (int ti=0;ti<N_CTRL_CHARS;ti++) tempbuf[ti] = CtrlCharacters[ti];
			tempbuf[N_CTRL_CHARS] = 0x6;  // command byte 6 = reset palettes
			StreamBytes(tempbuffer, N_CTRL_CHARS + 1);
		}
		public int Open(out int width, out int height)
		{
			// Try to find an ZeDMD on each COM port available
			bool IsAvailable = false;
			var ports = SerialPort.GetPortNames();
			width = 0;
			height = 0;
			foreach (var portName in ports)
			{
				IsAvailable = Connect(portName, out width, out height);
				if (IsAvailable) break;
			}
			if (!IsAvailable) return 0;
			ResetPalettes();
			Opened = true;
			return 1;
		}

		public bool Close()
		{
			if (Opened)
			{
				Disconnect();
				ResetPalettes();
			}

			Opened = false;
			return true;
		}

	}
}

