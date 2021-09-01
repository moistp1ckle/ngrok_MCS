using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ngrok_MC_Scanner
{
    class Program
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 8;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();
        public static class c
        {
            public static string Main = "\u001B[38;2;250;30;78m";
            public static string Secondary = "\u001B[38;2;85;85;143m";
            public static string White = "\u001B[38;2;255;255;255m";
            public static string Black = "\u001B[38;2;0;0;0m";
            public static string Grey = "\u001B[38;2;85;85;85m";
            public static string Red = "\u001B[38;2;255;53;53m";
            public static string Green = "\u001B[38;2;0;255;0m";
            public static string Blue = "\u001B[38;2;31;200;255m";
            public static string Reset = "\u001B[0m";
        }
        static void Main(string[] args)
        {
            IntPtr stdHandle = Program.GetStdHandle(-11);
            uint lpMode;
            if (!Program.GetConsoleMode(stdHandle, out lpMode))
            {
                Console.WriteLine("failed to get output console mode");
                Console.ReadKey();
            }
            else
            {
                uint dwMode = lpMode | 12U;
                if (!Program.SetConsoleMode(stdHandle, dwMode))
                {
                    Console.WriteLine(string.Format("failed to set output console mode, error code: {0}", (object)Program.GetLastError()));
                    Console.ReadKey();
                }
            }
            int good = 0; int bad = 0; int cpm = 0; int cpm_aux = 0;
            string basename = "ngrok MCS";
            string title = basename;
            int timeout = 0;
            int playersSet = 0;
            int minPlayers = 0;
            int threadNumber = 0;
            string credit = c.White + "Created By " + c.Red + "Sango";
            string banner = $@"{c.Main}
███╗   ██╗ ██████╗ ██████╗  ██████╗ ██╗  ██╗    ███╗   ███╗ ██████╗███████╗
████╗  ██║██╔════╝ ██╔══██╗██╔═══██╗██║ ██╔╝    ████╗ ████║██╔════╝██╔════╝
██╔██╗ ██║██║  ███╗██████╔╝██║   ██║█████╔╝     ██╔████╔██║██║     ███████╗
██║╚██╗██║██║   ██║██╔══██╗██║   ██║██╔═██╗     ██║╚██╔╝██║██║     ╚════██║
██║ ╚████║╚██████╔╝██║  ██║╚██████╔╝██║  ██╗    ██║ ╚═╝ ██║╚██████╗███████║
╚═╝  ╚═══╝ ╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═╝    ╚═╝     ╚═╝ ╚═════╝╚══════╝";
            void reset()
            {
                Console.Title = title;
                Console.Clear();
                WriteCenter(banner);
                WriteCenter(credit);
                int cursorTop = Console.CursorTop;
                Console.CursorTop = 0;
                if (threadNumber > 0)
                {
                    Console.Write(" " + c.Main + "Threads: " + c.Secondary + string.Format("{0:n0}", threadNumber));
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                }
                if (timeout > 0)
                {
                    Console.Write(" " + c.Main + "Timeout: " + c.Secondary + string.Format("{0:n0}", timeout));
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                }
                if (playersSet == 1)
                {
                    Console.Write(" " + c.Main + "Min Players: " + c.Secondary + string.Format("{0:n0}", minPlayers));
                    Console.SetCursorPosition(0, Console.CursorTop + 1);
                }
                Console.CursorTop = cursorTop;
            }
            Console.WriteLine("\u001B[48;2;16;17;26m");
            title = basename + " -> Checking for updates...";
            reset();
            Console.CursorVisible = false;
            WriteCenter("\n" + c.Main + "Checking for updates...");
            string upDate = string.Empty;
            try { upDate = new WebClient().DownloadString("https://leaked.wiki/update?p=ngrok&v=1.0"); }
            catch (Exception)
            {
                title = basename + $" -> Unable to Reach the Update Server";
                reset();
                WriteCenter("\n\u001b[38;2;255;228;0mI was unable to reach the update server, skipping update.");
                Thread.Sleep(3000);
            }
            Console.CursorVisible = true;
            if (!upDate.Contains("Updated") & upDate != string.Empty)
            {
                title = basename + " -> Update Available";
                reset();
                WriteCenter("\n" + c.Red + "There is a new update, you can find it here: " + c.Blue + upDate);
                Console.ReadKey();
                Process.Start(upDate);
                Environment.Exit(0);
            }
            void askThreads()
            {
                title = basename + " -> Threads";
                reset();
                Console.Write(c.Main + " Threads: " + c.Secondary);
                threadNumber = int.Parse(Console.ReadLine());
                title = basename + " -> Timeout";
                reset();
                Console.Write(c.Main + " Timeout " + c.Grey + "(seconds)" + c.Main + ": " + c.Secondary);
                timeout = int.Parse(Console.ReadLine());
                title = basename + " -> Mode";
                reset();
                Console.Write(c.Main + " Minimum Active Players " + c.Grey + "(0 for any)" + c.Main + ": " + c.Secondary);
                minPlayers = int.Parse(Console.ReadLine());
                playersSet = 1;
            }
            askThreads();
            reset();
            void updateTitle()
            {
                while (true)
                {
                    cpm = (cpm + (cpm_aux * 120)) / 2;
                    cpm_aux = 0;
                    Console.Title = $"{basename} | Good: {string.Format("{0:n0}", good)} | Bad: {string.Format("{0:n0}", bad)} | CPM: {string.Format("{0:n0}", cpm)} | Progress: {string.Format("{0:n0}", good + bad)}/30,000 | Cracked.to/Sango";
                    Thread.Sleep(500);
                }
            }
            StreamWriter streamWriter = new StreamWriter("servers.txt");
            new Thread(updateTitle).Start();
            Console.CursorVisible = false;
            Parallel.ForEach(Enumerable.Range(10000, 10000), new ParallelOptions { MaxDegreeOfParallelism = threadNumber }, i =>
            {
                check("0.tcp.ngrok.io", (ushort)i);
                check("1.tcp.ngrok.io", (ushort)i);
                check("2.tcp.ngrok.io", (ushort)i);
                check("4.tcp.ngrok.io", (ushort)i);
                check("6.tcp.ngrok.io", (ushort)i);
                check("8.tcp.ngrok.io", (ushort)i);
            });
            void check(string host, ushort port)
            {
                try
                {
                    MineStat ms = new MineStat(host, port, timeout);
                    if (ms.ServerUp)
                    {
                        cpm_aux++;
                        if (Int32.Parse(ms.CurrentPlayers) >= minPlayers)
                        {
                            Console.WriteLine($" {c.Grey}=================================================\r\n {c.Main}IP: {c.Secondary + ms.Address}:{ms.Port}\r\n {c.Main}Version: {c.Secondary + ms.Version}\r\n {c.Main}Players: {c.Secondary + ms.CurrentPlayers}/{ms.MaximumPlayers}\r\n {c.Main}Latency: {c.Secondary + ms.Latency}ms\r\n {c.Main}MOTD: {color(ms.Motd)}");
                            streamWriter.WriteLine($"=================================================\r\nIP: {ms.Address}:{ms.Port}\r\nVersion: {ms.Version}\r\nPlayers: {ms.CurrentPlayers}/{ms.MaximumPlayers}\r\nLatency: {ms.Latency}ms\r\nMOTD: {uncolor(ms.Motd)}");
                            streamWriter.Flush();
                            good++;
                        }
                        else { bad++; }
                    }
                    else { cpm_aux++; bad++; }
                }
                catch { cpm_aux++; bad++; }
            }
            string color(string text) { return "\u001B[38;2;128;128;128m" + text.Replace("§4", "\u001B[38;2;170;0;0m").Replace("§c", "\u001B[38;2;255;85;85m").Replace("§6", "\u001B[38;2;255;170;0m").Replace("§e", "\u001B[38;2;255;255;85m").Replace("§2", "\u001B[38;2;0;170;0m").Replace("§a", "\u001B[38;2;85;255;85m").Replace("§b", "\u001B[38;2;85;255;255m").Replace("§3", "\u001B[38;2;0;170;170m").Replace("§1", "\u001B[38;2;0;0;170m").Replace("§9", "\u001B[38;2;85;85;255m").Replace("§d", "\u001B[38;2;255;85;255m").Replace("§5", "\u001B[38;2;170;0;170m").Replace("§f", "\u001B[38;2;255;255;255m").Replace("§7", "\u001B[38;2;170;170;170m").Replace("§8", "\u001B[38;2;85;85;85m").Replace("§0", "\u001B[38;2;0;0;0m").Replace("§r", "\u001B[38;2;128;128;128m").Replace("§l", "").Replace("§n", "").Replace("§o", "").Replace("§k", "").Replace("§m", "").Replace("§x", "").Replace("\n", "\r\n "); }
            string uncolor(string text) { return text.Replace("§4", "").Replace("§c", "").Replace("§6", "").Replace("§e", "").Replace("§2", "").Replace("§a", "").Replace("§b", "").Replace("§3", "").Replace("§1", "").Replace("§9", "").Replace("§d", "").Replace("§5", "").Replace("§f", "").Replace("§7", "").Replace("§8", "").Replace("§0", "").Replace("§l", "").Replace("§n", "").Replace("§o", "").Replace("§k", "").Replace("§m", "").Replace("§r", "").Replace("§x", ""); }
            void WriteCenter(string str)
            {
                using (StringReader stringReader = new StringReader(str))
                {
                    string input;
                    while ((input = stringReader.ReadLine()) != null)
                    {
                        Console.SetCursorPosition((Console.WindowWidth - Regex.Replace(input, "\\e\\[[0-9;]*m", "").Length) / 2, Console.CursorTop);
                        Console.WriteLine(input);
                    }
                }
            }
        }
    }
    //minestat https://github.com/FragLand/minestat
    public class MineStat
    {
        public const string MineStatVersion = "2.1.0";
        private const int DefaultTimeout = 5; // default TCP timeout in seconds

        public string Address { get; set; }
        public ushort Port { get; set; }
        public int Timeout { get; set; }
        public string Motd { get; set; }
        public string Version { get; set; }
        public string CurrentPlayers => Convert.ToString(CurrentPlayersInt);
        public int CurrentPlayersInt { get; set; }
        public string MaximumPlayers => Convert.ToString(MaximumPlayersInt);
        public int MaximumPlayersInt { get; set; }
        public bool ServerUp { get; set; }
        public long Latency { get; set; }
        public SlpProtocol Protocol { get; set; }

        public MineStat(string address, ushort port, int timeout = DefaultTimeout, SlpProtocol protocol = SlpProtocol.Automatic)
        {
            Address = address;
            Port = port;
            Timeout = timeout;

            // If the user manually selected a protocol, use that
            switch (protocol)
            {
                case SlpProtocol.Beta:
                    RequestWrapper(RequestWithBetaProtocol);
                    break;
                case SlpProtocol.Legacy:
                    RequestWrapper(RequestWithLegacyProtocol);
                    break;
                case SlpProtocol.ExtendedLegacy:
                    RequestWrapper(RequestWithExtendedLegacyProtocol);
                    break;
                case SlpProtocol.Json:
                    RequestWrapper(RequestWithJsonProtocol);
                    break;
                case SlpProtocol.Automatic:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(protocol), "Invalid SLP protocol specified for parameter 'protocol'");
            }

            // If a protocol was chosen manually, return
            if (protocol != SlpProtocol.Automatic)
            {
                return;
            }

            var result = RequestWrapper(RequestWithLegacyProtocol);

            if (result != ConnStatus.Connfail && result != ConnStatus.Success)
            {
                result = RequestWrapper(RequestWithBetaProtocol);

                if (result != ConnStatus.Connfail)
                    result = RequestWrapper(RequestWithExtendedLegacyProtocol);

                if (result != ConnStatus.Connfail && result != ConnStatus.Success)
                    RequestWrapper(RequestWithJsonProtocol);
            }
        }

        public ConnStatus RequestWithJsonProtocol(NetworkStream stream)
        {
            var jsonPingHandshakePacket = new List<byte> { 0x00 };
            jsonPingHandshakePacket.AddRange(WriteLeb128(-1));
            var serverAddr = Encoding.UTF8.GetBytes(Address);
            jsonPingHandshakePacket.AddRange(WriteLeb128(serverAddr.Length));
            jsonPingHandshakePacket.AddRange(serverAddr);
            var serverPort = BitConverter.GetBytes(Port);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(serverPort);
            jsonPingHandshakePacket.AddRange(serverPort);
            jsonPingHandshakePacket.AddRange(WriteLeb128(1));
            jsonPingHandshakePacket.InsertRange(0, WriteLeb128(jsonPingHandshakePacket.Count));
            stream.Write(jsonPingHandshakePacket.ToArray(), 0, jsonPingHandshakePacket.Count);
            WriteLeb128Stream(stream, 1);
            stream.WriteByte(0x00);
            int responseSize;
            try
            {
                responseSize = ReadLeb128Stream(stream);
            }
            catch (Exception)
            {
                return ConnStatus.Unknown;
            }
            if (responseSize < 3)
            {
                return ConnStatus.Unknown;
            }
            var responsePacketId = ReadLeb128Stream(stream);
            if (responsePacketId != 0x00)
            {
                return ConnStatus.Unknown;
            }
            var responsePayloadLength = ReadLeb128Stream(stream);
            var responsePayload = NetStreamReadExact(stream, responsePayloadLength);
            return ParseJsonProtocolPayload(responsePayload);
        }
        private ConnStatus ParseJsonProtocolPayload(byte[] rawPayload)
        {
            try
            {
                var jsonReader = JsonReaderWriterFactory.CreateJsonReader(rawPayload, new System.Xml.XmlDictionaryReaderQuotas());
                var root = XElement.Load(jsonReader);
                Version = root.XPathSelectElement("//version/name")?.Value;
                var descriptionElement = root.XPathSelectElement("//description");
                if (descriptionElement != null && descriptionElement.Attribute(XName.Get("type"))?.Value == "string")
                {
                    Motd = descriptionElement.Value;
                }
                else if (root.XPathSelectElement("//description/text") != null)
                {
                    Motd = root.XPathSelectElement("//description/text")?.Value;
                }
                CurrentPlayersInt = Convert.ToInt32(root.XPathSelectElement("//players/online")?.Value);
                MaximumPlayersInt = Convert.ToInt32(root.XPathSelectElement("//players/max")?.Value);
            }
            catch (Exception)
            {
                return ConnStatus.Unknown;
            }
            if (Version == null || Motd == null)
            {
                return ConnStatus.Unknown;
            }
            ServerUp = true;
            Protocol = SlpProtocol.Json;
            return ConnStatus.Success;
        }
        public ConnStatus RequestWithExtendedLegacyProtocol(NetworkStream stream)
        {
            var extLegacyPingPacket = new List<byte> { 0xFE, 0x01, 0xFA, 0x00, 0x0B };
            extLegacyPingPacket.AddRange(Encoding.BigEndianUnicode.GetBytes("MC|PingHost"));
            var reqByteLen = BitConverter.GetBytes(Convert.ToInt16(7 + (Address.Length * 2)));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(reqByteLen);
            extLegacyPingPacket.AddRange(reqByteLen);
            extLegacyPingPacket.Add(0x4A);
            var addressLen = BitConverter.GetBytes(Convert.ToInt16(Address.Length));    
            if (BitConverter.IsLittleEndian)
                Array.Reverse(addressLen);
            extLegacyPingPacket.AddRange(addressLen);
            extLegacyPingPacket.AddRange(Encoding.BigEndianUnicode.GetBytes(Address));
            var port = BitConverter.GetBytes(Convert.ToUInt32(Port));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(port);
            extLegacyPingPacket.AddRange(port);
            stream.Write(extLegacyPingPacket.ToArray(), 0, extLegacyPingPacket.Count);
            byte[] responsePacketHeader;
            try
            {
                responsePacketHeader = NetStreamReadExact(stream, 3);
            }
            catch (Exception)
            {
                return ConnStatus.Unknown;
            }
            if (responsePacketHeader[0] != 0xFF)
            {
                return ConnStatus.Unknown;
            }
            var payloadLengthRaw = responsePacketHeader.Skip(1);
            if (BitConverter.IsLittleEndian)
                payloadLengthRaw = payloadLengthRaw.Reverse();
            var payloadLength = BitConverter.ToUInt16(payloadLengthRaw.ToArray(), 0);
            var payload = NetStreamReadExact(stream, payloadLength * 2);
            return ParseLegacyProtocol(payload, SlpProtocol.ExtendedLegacy);
        }
        public ConnStatus RequestWithLegacyProtocol(NetworkStream stream)
        {
            var legacyPingPacket = new byte[] { 0xFE, 0x01 };
            stream.Write(legacyPingPacket, 0, legacyPingPacket.Length);
            byte[] responsePacketHeader;
            try
            {
                responsePacketHeader = NetStreamReadExact(stream, 3);
            }
            catch (Exception)
            {
                return ConnStatus.Unknown;
            }
            if (responsePacketHeader[0] != 0xFF)
            {
                return ConnStatus.Unknown;
            }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(responsePacketHeader);
            var payloadLength = BitConverter.ToUInt16(responsePacketHeader, 0);
            var payload = NetStreamReadExact(stream, payloadLength * 2);
            return ParseLegacyProtocol(payload, SlpProtocol.Legacy);
        }
        private ConnStatus ParseLegacyProtocol(byte[] rawPayload, SlpProtocol protocol = SlpProtocol.ExtendedLegacy)
        {
            var payloadString = Encoding.BigEndianUnicode.GetString(rawPayload, 0, rawPayload.Length);
            var payloadArray = payloadString.Split('\0');
            if (payloadArray.Length != 6)
            {
                return ConnStatus.Unknown;
            }
            Version = payloadArray[2];
            Motd = payloadArray[3];
            CurrentPlayersInt = Convert.ToInt32(payloadArray[4]);
            MaximumPlayersInt = Convert.ToInt32(payloadArray[5]);
            ServerUp = true;
            Protocol = protocol;
            return ConnStatus.Success;
        }
        public ConnStatus RequestWithBetaProtocol(NetworkStream stream)
        {
            var betaPingPacket = new byte[] { 0xFE };
            stream.Write(betaPingPacket, 0, betaPingPacket.Length);
            byte[] responsePacketHeader;
            try
            {
                responsePacketHeader = NetStreamReadExact(stream, 3);
            }
            catch (Exception)
            {
                return ConnStatus.Unknown;
            }
            if (responsePacketHeader[0] != 0xFF)
            {
                return ConnStatus.Unknown;
            }
            if (BitConverter.IsLittleEndian)
                Array.Reverse(responsePacketHeader);
            var payloadLength = BitConverter.ToUInt16(responsePacketHeader, 0);
            var payload = NetStreamReadExact(stream, payloadLength * 2);
            return ParseBetaProtocol(payload);
        }
        private ConnStatus ParseBetaProtocol(byte[] rawPayload)
        {
            var payloadString = Encoding.BigEndianUnicode.GetString(rawPayload, 0, rawPayload.Length);
            var payloadArray = payloadString.Split('§');
            if (payloadArray.Length < 3)
            {
                return ConnStatus.Unknown;
            }
            MaximumPlayersInt = Convert.ToInt32(payloadArray[payloadArray.Length - 1]);
            CurrentPlayersInt = Convert.ToInt32(payloadArray[payloadArray.Length - 2]);
            Motd = String.Join("§", payloadArray.Take(payloadArray.Length - 2).ToArray());
            ServerUp = true;
            Protocol = SlpProtocol.Beta;
            Version = "<= 1.3";
            return ConnStatus.Success;
        }
        private TcpClient TcpClientWrapper()
        {
            var tcpclient = new TcpClient { ReceiveTimeout = Timeout * 1000, SendTimeout = Timeout * 1000 };
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var result = tcpclient.BeginConnect(Address, Port, null, null);
            var isResponsive = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(Timeout));
            if (!isResponsive)
            {
                return null;
            }
            try
            {
                tcpclient.EndConnect(result);
            }
            catch (SocketException)
            {
                return null;
            }
            stopWatch.Stop();
            Latency = stopWatch.ElapsedMilliseconds;
            return tcpclient;
        }
        private static byte[] NetStreamReadExact(NetworkStream stream, int size)
        {
            var totalReadBytes = 0;
            var resultBuffer = new List<byte>();
            do
            {
                var tempBuffer = new byte[size - totalReadBytes];
                var readBytes = stream.Read(tempBuffer, 0, size - totalReadBytes);
                if (readBytes == 0)
                {
                    throw new IOException();
                }
                resultBuffer.AddRange(tempBuffer.Take(readBytes));
                totalReadBytes += readBytes;
            } while (totalReadBytes < size);
            return resultBuffer.ToArray();
        }
        private ConnStatus RequestWrapper(Func<NetworkStream, ConnStatus> toExecute)
        {
            using (var tcpClient = TcpClientWrapper())
            {
                if (tcpClient == null)
                {
                    ServerUp = false;
                    return ConnStatus.Connfail;
                }

                var networkStream = tcpClient.GetStream();
                return toExecute(networkStream);
            }
        }
        #region Obsolete
        [Obsolete]
        public string GetAddress()
        {
            return Address;
        }
        [Obsolete]
        public void SetAddress(string address)
        {
            Address = address;
        }
        [Obsolete]
        public ushort GetPort()
        {
            return Port;
        }
        [Obsolete]
        public void SetPort(ushort port)
        {
            Port = port;
        }
        [Obsolete]
        public string GetMotd()
        {
            return Motd;
        }
        [Obsolete]
        public void SetMotd(string motd)
        {
            Motd = motd;
        }
        [Obsolete]
        public string GetVersion()
        {
            return Version;
        }
        [Obsolete]
        public void SetVersion(string version)
        {
            Version = version;
        }
        [Obsolete]
        public string GetCurrentPlayers()
        {
            return CurrentPlayers;
        }
        [Obsolete]
        public void SetCurrentPlayers(string currentPlayers)
        {
            CurrentPlayersInt = Convert.ToInt32(currentPlayers);
        }
        [Obsolete]
        public string GetMaximumPlayers()
        {
            return MaximumPlayers;
        }
        [Obsolete]
        public void SetMaximumPlayers(string maximumPlayers)
        {
            MaximumPlayersInt = Convert.ToInt32(maximumPlayers);
        }
        [Obsolete]
        public long GetLatency()
        {
            return Latency;
        }
        [Obsolete]
        public void SetLatency(long latency)
        {
            Latency = latency;
        }
        [Obsolete]
        public bool IsServerUp()
        {
            return ServerUp;
        }
        #endregion
        #region LEB128_Utilities
        public static byte[] WriteLeb128(int value)
        {
            var byteList = new List<byte>();
            uint actual = (uint)value;
            do
            {
                byte temp = (byte)(actual & 0b01111111);
                actual >>= 7;
                if (actual != 0)
                {
                    temp |= 0b10000000;
                }
                byteList.Add(temp);
            } while (actual != 0);

            return byteList.ToArray();
        }
        public static void WriteLeb128Stream(Stream stream, int value)
        {
            uint actual = (uint)value;
            do
            {
                byte temp = (byte)(actual & 0b01111111);
                actual >>= 7;
                if (actual != 0)
                {
                    temp |= 0b10000000;
                }
                stream.WriteByte(temp);
            } while (actual != 0);
        }
        private static int ReadLeb128Stream(Stream stream)
        {
            int numRead = 0;
            int result = 0;
            byte read;
            do
            {
                int r = stream.ReadByte();
                if (r == -1)
                {
                    break;
                }
                read = (byte)r;
                int value = read & 0b01111111;
                result |= (value << (7 * numRead));
                numRead++;
                if (numRead > 5)
                {
                    throw new FormatException("VarInt is too big.");
                }
            } while ((read & 0b10000000) != 0);
            if (numRead == 0)
            {
                throw new InvalidOperationException("Unexpected end of VarInt stream.");
            }
            return result;
        }
        #endregion
    }
    public enum ConnStatus
    {
        Success,
        Connfail,
        Timeout,
        Unknown
    }
    public enum SlpProtocol
    {
        Json,
        ExtendedLegacy,
        Legacy,
        Beta,
        Automatic
    }
}
