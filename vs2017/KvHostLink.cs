using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

public class KvHostLink
{
    private IPEndPoint _remoteEndPoint;
    private const int Port = 8501;
    private const int BufSize = 4096;

    public KvHostLink(string host)
    {
        _remoteEndPoint = new IPEndPoint(IPAddress.Parse(host), Port);
    }

    private string SendReceive(string command)
    {
        using (var udpClient = new UdpClient(Port))
        {
            udpClient.Client.ReceiveTimeout = 2000;
            byte[] sendBytes = Encoding.ASCII.GetBytes(command + "\r");

            udpClient.Send(sendBytes, sendBytes.Length, _remoteEndPoint);
            IPEndPoint remoteEP = null;
            byte[] receiveBytes = udpClient.Receive(ref remoteEP);

            return Encoding.ASCII.GetString(receiveBytes).TrimEnd('\r', '\0');
        }
    }

    public string Mode(string mode) => SendReceive("M" + mode);
    public string UnitType() => SendReceive("?k");
    public string ErrClr() => SendReceive("ER");
    public string Er() => SendReceive("?E");

    public string SetTime()
    {
        var now = DateTime.Now;
        string data = string.Format("WRT {0:yy} {1} {2} {3} {4} {5} {6}",
            now, now.Month, now.Day, now.Hour, now.Minute, now.Second, (int)now.DayOfWeek);
        return SendReceive(data);
    }

    public string Set(string deviceAddress) => SendReceive("ST " + deviceAddress);
    public string Reset(string deviceAddress) => SendReceive("RS " + deviceAddress);
    public string Sts(string deviceAddress, int num) => SendReceive($"STS {deviceAddress} {num}");
    public string Rss(string deviceAddress, int num) => SendReceive($"RSS {deviceAddress} {num}");
    public string Read(string deviceAddressWithSuffix) => SendReceive("RD " + deviceAddressWithSuffix);
    public string Reads(string deviceAddressWithSuffix, int num) => SendReceive($"RDS {deviceAddressWithSuffix} {num}");
    public string Write(string deviceAddressWithSuffix, string data) => SendReceive($"WR {deviceAddressWithSuffix} {data}");
    public string Writes(string deviceAddressWithSuffix, int num, string data) => SendReceive($"WRS {deviceAddressWithSuffix} {num} {data}");

    public string ReadsAsString(string deviceAddress, int num)
    {
        string raw = Reads(deviceAddress + ".U", num);
        string[] tokens = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        ushort[] ushortValues = tokens.Select(ushort.Parse).ToArray();

        var byteList = new System.Collections.Generic.List<byte>();
        foreach (var val in ushortValues)
        {
            byteList.Add((byte)(val >> 8));
            byteList.Add((byte)(val & 0xFF));
        }

        int nullIndex = byteList.IndexOf(0x00);
        byte[] byteArray = (nullIndex >= 0) ? byteList.Take(nullIndex).ToArray() : byteList.ToArray();
        return Encoding.ASCII.GetString(byteArray);
    }

    public string WritesAsString(string deviceAddress, int num, string data)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(data);
        int wordCount = (bytes.Length + 1) / 2; 
        ushort[] ushortValues = new ushort[wordCount];

        for (int i = 0; i < bytes.Length; i++)
        {
            if ((i % 2) == 0)
                ushortValues[i / 2] |= (ushort)(bytes[i] << 8);
            else
                ushortValues[i / 2] |= bytes[i];
        }

        string joinedData = string.Join(" ", ushortValues.Select(u => u.ToString()));
        return Writes(deviceAddress + ".U", num, joinedData);
    }
}
