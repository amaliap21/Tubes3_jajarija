// Make converter from image to binary

using System;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Media.Imaging;

class Converter
{
    public static bool[,] ConvertToBinary(Bitmap bitmap, int threshold)
    {
        int width = bitmap.PixelSize.Width;
        int height = bitmap.PixelSize.Height;

        int startX = 0;
        int startY = 0;
        int regionWidth = Math.Min(32, width);
        int regionHeight = Math.Min(32, height);

        // Calculate startX and startY to center the 32x32 region
        if (width > 32)
        {
            startX = (width - 32) / 2;
        }

        if (height > 32)
        {
            startY = (height - 32) / 2;
        }

        bool[,] binaryImage = new bool[32, 32];
        int stride = regionWidth * 4; // Each pixel is 4 bytes (RGBA)

        // Allocate unmanaged memory for the buffer
        IntPtr buffer = Marshal.AllocHGlobal(regionHeight * stride);

        try
        {
            Avalonia.PixelRect pixelRect = new(startX, startY, regionWidth, regionHeight);
            bitmap.CopyPixels(pixelRect, buffer, regionHeight * stride, stride);

            for (int y = 0; y < regionHeight; y++)
            {
                for (int x = 0; x < regionWidth; x++)
                {
                    int pixelX = startX + x;
                    int pixelY = startY + y;

                    // If the pixel is outside the image bounds, set it to 'false'
                    if (pixelX < 0 || pixelX >= width || pixelY < 0 || pixelY >= height)
                    {
                        binaryImage[x, y] = false;
                        continue;
                    }

                    int index = pixelY * stride + pixelX * 4;
                    byte b = Marshal.ReadByte(buffer, index);
                    byte g = Marshal.ReadByte(buffer, index + 1);
                    byte r = Marshal.ReadByte(buffer, index + 2);

                    // Convert pixel to grayscale
                    int gray = (int)(0.3 * r + 0.59 * g + 0.11 * b);

                    // Apply threshold
                    binaryImage[x, y] = gray >= threshold;
                }
            }
        }
        finally
        {
            // Free unmanaged memory
            Marshal.FreeHGlobal(buffer);
        }

        // Fill the rest of the binaryImage array with 'false' if the image is smaller than 32x32
        for (int y = regionHeight; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                binaryImage[x, y] = false;
            }
        }

        return binaryImage;
    }

    public static string ConvertImageToAsciiFull(Bitmap bitmap, int threshold)
    {
        bool[,] binaryImage = ConvertToBinary(bitmap, threshold);
        return ConvertBinaryToAscii(binaryImage);
    }


    public static string ConvertBinaryToAscii(bool[,] binary)
    {
        const int width = 32;
        const int height = 32;

        StringBuilder result = new();

        int toDec = 0;
        int bitCount = 0;

        // Iterate over each bit in the binary array
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate the decimal value by summing up the powers of 2
                toDec += (binary[x, y] ? 1 : 0) << (7 - bitCount);

                bitCount++;

                // If we've processed 8 bits, convert to ASCII character and append to result
                if (bitCount == 8)
                {
                    result.Append((char)toDec);

                    // Reset counters for the next character
                    toDec = 0;
                    bitCount = 0;
                }
            }
        }

        return result.ToString();
    }

    public static void PrintBinaryImage(bool[,] binaryImage)
    {
        int width = binaryImage.GetLength(0);
        int height = binaryImage.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Console.Write(binaryImage[x, y] ? "1" : "0");
            }
            Console.WriteLine();
        }
    }


}