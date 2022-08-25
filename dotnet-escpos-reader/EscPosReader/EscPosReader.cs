using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public class EcsPosReader
{
    static void Main(string[] args)
    {
        Console.WriteLine("Incoming Data:");
        using (SerialPort sp = new SerialPort("/dev/serial0", 115200, Parity.None, 8, StopBits.One))
        {
            sp.Encoding = Encoding.GetEncoding("ibm850");
            //sp.BaudRate = 9600;
            //sp.ReadTimeout = 1000;
            //sp.WriteTimeout = 1000;
            sp.Open();

            Console.WriteLine("Type Ctrl-C to exit...");

            var receivedSymbolsAsInt = new List<int>();
            var cutPaperCommand = new List<int> { 27, 109 };

            while (true)
            {
                var existingData = sp.ReadByte();
                Console.WriteLine(existingData);
                receivedSymbolsAsInt.Add(existingData);
                if (Enumerable.SequenceEqual(GetTwoLast(receivedSymbolsAsInt), cutPaperCommand))
                {
                    Console.WriteLine(String.Join(", ", receivedSymbolsAsInt.ToArray()));
                    Console.WriteLine("Paper cut");
                    // send to decode
                    SaveBinaryFile(receivedSymbolsAsInt);
                    receivedSymbolsAsInt = new List<int>();
                    break;
                }
            }
        };
    }
    private static List<int> GetTwoLast(List<int> list)
    {
        return Enumerable.Reverse(list).Take(2).Reverse().ToList();
    }

    private static void SaveBinaryFile(List<int> inputList){
        var datetime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        var filePath = $"./out/{datetime}.bin";
        var binaryOutput = inputList.SelectMany(BitConverter.GetBytes).ToArray();
        using (var writer = new BinaryWriter(File.OpenWrite(filePath))){
            writer.Write(binaryOutput);
        }
        Console.WriteLine("Successfully created bin file");
    }
}