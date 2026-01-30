using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

/// <summary>
/// Device interactions and general methods for the UI to call.
/// </summary>
public static class Device
{
    public static DataType dataType = DataType.Char;
    public static SerialPort port = new();
    static Stopwatch stopwatch = new Stopwatch();
    static byte[] buffer = new byte[255];
    static int offset = 0;
    static int count = 0;

    public static event EventHandler<SerialResponseEventArgs> ResponseReceived = delegate { };
    public static event EventHandler<TextLogEventArgs> TextMessageReceived = delegate { };

    public static void Init()
    {
        port.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);
    }

    public static void Connect(string portName, int baudrate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
    {
        port.Handshake = Handshake.None;
        port.PortName = portName;
        port.BaudRate = baudrate;
        port.Parity = parity;
        port.DataBits = dataBits;
        port.StopBits = stopBits;
        
        port.Open();
        // try
        // {
        //     port.Open();
        // }
        // catch (Exception e)
        // {
        //     TextMessageReceived.Invoke(null, new(e.Message));
        // }
    }
    public static void SetDataType (DataType dt)
    {
        dataType = dt;
    }

    public static void SendMessage(string input)
    {
        if (!port.IsOpen)
            TextMessageReceived.Invoke(null, new("Port is not open!"));
        else
        {
            port.Write(input);
        }
    }
    public static void SendMessage(byte[] input)
    {
        if (!port.IsOpen)
            TextMessageReceived.Invoke(null, new("Port is not open!"));
        else
        {
            port.Write(input, 0, input.Length);
        }
    }

    static void SerialDataReceived(object sender, EventArgs e)
    {
        if (dataType == DataType.Byte)
        {
            //reset values on every event call;
            buffer = new byte[255];
            offset = 0;
            count = 0;
            try
            {
                stopwatch.Restart();
                while (stopwatch.ElapsedMilliseconds < port.ReadTimeout)
                {
                    if (port.BytesToRead > 0)
                    {
                        stopwatch.Restart();
                        count = port.BytesToRead;
                        port.Read(buffer, offset, count);
                        offset += count;
                        stopwatch.Restart();
                    }
                }
                ResponseReceived.Invoke(null, new SerialResponseEventArgs(buffer));
                int i;
                for (i = buffer.Length - 1; i >= 0; i--)
                {
                    if (buffer[i] != 0x00)
                        break;
                }

                byte[] message = new byte[i+1];
                for (int j = 0; j <= i; j++)
                    message[j] = buffer[j];
                
                // ResponseReceived.Invoke(null, new SerialResponseEventArgs(message));
            }
            catch (Exception err)
            {
                Console.WriteLine("Error when receiving message from serial port: " + err.Message + "\n"+err.StackTrace);
            }

            port.DiscardInBuffer();
        }
        else
        {
            string line = port.ReadLine();
            TextMessageReceived.Invoke(port, new(line));
        }
    }
    public static string ByteArrayToString(byte[] bytes, bool cleanEmpty = true)
    {
        byte[] res;

        if (cleanEmpty)
            res = CleanByteArray(bytes);
        else res = bytes;

        string dataString = BitConverter.ToString(res);
        string result = "";
        for (int i = 0; i < dataString.Length; i++)
        {
            if (dataString[i] == '-')
                result += " ";
            else result += dataString[i];
        }

        return result;
    }
    public static byte[] CleanByteArray(byte[] bytes)
    {
        int length = bytes.Length - 1;
        // snip off the empty bytes at the end
        for (int i = length; i >= 0; i--)
        {
            if (bytes[i] != 0)
            {
                length = i + 1;
                break;
            }
        }

        byte[] res = new byte[length];
        for (int i = 0; i < length; i++) { res[i] = bytes[i]; }

        return res;
    }
    
}

public class TextLogEventArgs : EventArgs
{
    public string Message{get;}

    public TextLogEventArgs(string msg)
    {
        this.Message = msg;
    }
}

public class SerialResponseEventArgs : EventArgs
{
    public byte[] Message{get;}
    public SerialResponseEventArgs(byte[] message)
    {
        this.Message = message;
    }
}

public enum DataType {Byte, Char}