using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Telnet Connection on port 5025 to an instrument
/// </summary>
public class TelnetConnection : IDisposable
{
    TcpClient m_Client;
    NetworkStream m_Stream;
    bool m_IsOpen = false;
    string m_Hostname;
    int m_ReadTimeout = 1000; // ms

    public bool IsOpen { get { return m_IsOpen; } }

    public TelnetConnection(string hostname) : this(hostname, false) { }
    public TelnetConnection(string hostname, bool open)
    {
        m_Hostname = hostname;

        if (open)
            Open();
    }
    void CheckOpen()
    {
        if (!IsOpen)
            throw new Exception("Connection not open.");
        if (m_Stream == null)
            throw new Exception("Stream is not open");
    }
    public string Hostname
    {
        get { return m_Hostname; }
    }
    public int ReadTimeout
    {
        set
        {
            m_ReadTimeout = value;
            if (m_Stream != null)
                m_Stream.ReadTimeout = value;
        }
        get { return m_ReadTimeout; }
    }
    public void Write(string str)
    {
        CheckOpen();
        byte[] bytes = Encoding.ASCII.GetBytes(str);
        m_Stream.Write(bytes, 0, bytes.Length);
        m_Stream.Flush();
    }
    public void WriteLine(string str)
    {
        CheckOpen();
        byte[] bytes = Encoding.ASCII.GetBytes(str);
        m_Stream.Write(bytes, 0, bytes.Length);
        WriteTerminator();
    }
    void WriteTerminator()
    {
        byte[] bytes = Encoding.ASCII.GetBytes("\r\n\0");
        m_Stream.Write(bytes, 0, bytes.Length);
        m_Stream.Flush();
    }
    public string Read()
    {
        CheckOpen();
        return Encoding.ASCII.GetString(ReadBytes());
    }
    /// <summary>
    /// Reads bytes from the socket and returns them as a byte[].
    /// </summary>
    /// <returns></returns>
    public byte[] ReadBytes()
    {
        int i = m_Stream.ReadByte();
        byte b = (byte)i;
        int bytesToRead = 0;
        var bytes = new List<byte>();
        if ((char)b == '#')
        {
            bytesToRead = ReadLengthHeader();
            if (bytesToRead > 0)
            {
                i = m_Stream.ReadByte();
                if ((char)i != '\n') // discard carriage return after length header.
                    bytes.Add((byte)i);
            }
        }
        if (bytesToRead == 0)
        {
            while (i != -1 && b != (byte)'\n')
            {
                bytes.Add(b);
                i = m_Stream.ReadByte();
                b = (byte)i;
            }
        }
        else
        {
            int bytesRead = 0;
            while (bytesRead < bytesToRead && i != -1)
            {
                i = m_Stream.ReadByte();
                if (i != -1)
                {
                    bytesRead++;
                    // record all bytes except \n if it is the last char.
                    if (bytesRead < bytesToRead || (char)i != '\n')
                        bytes.Add((byte)i);
                }
            }
        }
        return bytes.ToArray();
    }
    int ReadLengthHeader()
    {
        int numDigits = Convert.ToInt32(new string(new char[] { (char)m_Stream.ReadByte() }));
        string bytes = "";
        for (int i = 0; i < numDigits; ++i)
            bytes = bytes + (char)m_Stream.ReadByte();
        return Convert.ToInt32(bytes);
    }
    public void Open()
    {
        if (IsOpen)
            Close();
        m_Client = new TcpClient(m_Hostname, 5025);
        m_Stream = m_Client.GetStream();
        m_Stream.ReadTimeout = ReadTimeout;
        m_IsOpen = true;
    }
    public void Close()
    {
        if (!m_IsOpen)
            return;

        m_Stream?.Close();
        m_Client?.Close();
        m_IsOpen = false;
    }

    public void Dispose()
    {
        Close();
    }
}
