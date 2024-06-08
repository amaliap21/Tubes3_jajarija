using System;
using System.Text;

public class Program
{
    public static void Main()
    {
        // Sample data
        string data = "\"3273041101747880\",\"Tony Okuneva\",\"Lake Bridgette\",\"1974-01-10\",\"Laki-laki\",\"AB\",\"911 Kozey Spur\",\"Hindu\",\"Belum Menikah\",\"Dynamic Operations Executive\",\"Indonesia\"";

        // Prepare data for encryption
        byte[] preparedData = PrepareDataForEncryption(data);
        Console.WriteLine("Prepared data: " + BitConverter.ToString(preparedData).Replace("-", "")); // Convert byte array to hexadecimal string for display
    }

    public static byte[] PrepareDataForEncryption(string data)
    {
        // Convert string data to bytes using UTF-8 encoding
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        // Pad the data
        byte[] paddedData = PadData(dataBytes);
        return paddedData;
    }

    public static byte[] PadData(byte[] data)
    {
        // Calculate the number of bytes to pad
        int paddingLength = 16 - (data.Length % 16);
        // Pad the data with bytes containing the padding length
        byte[] paddedData = new byte[data.Length + paddingLength];
        Array.Copy(data, paddedData, data.Length);
        for (int i = data.Length; i < paddedData.Length; i++)
        {
            paddedData[i] = (byte)paddingLength;
        }
        return paddedData;
    }
}
