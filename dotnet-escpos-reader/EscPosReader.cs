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
        using (SerialPort sp = new SerialPort("/dev/serial0",
      9600))
        {
            sp.Encoding = Encoding.GetEncoding("ibm850");
            sp.BaudRate = 9600;
            //sp.ReadTimeout = 1000;
            //sp.WriteTimeout = 1000;
            sp.Open();

            Console.WriteLine("Type Ctrl-C to exit...");

            var receivedSymbolsAsInt = new List<int>();
            var cutPaperCommand = new List<int> { 27, 61, 0 };

            while (true)
            {
                var existingData = sp.ReadByte();
                receivedSymbolsAsInt.Add(existingData);
                if (Enumerable.SequenceEqual(GetThreeLast(receivedSymbolsAsInt), cutPaperCommand))
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
    private static List<int> GetThreeLast(List<int> list)
    {
        return Enumerable.Reverse(list).Take(3).Reverse().ToList();
    }

    private static void SaveBinaryFile(List<int> inputList){
        var datetime = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
        var filePath = $"./{datetime}.bin";
        var binaryOutput = inputList.SelectMany(BitConverter.GetBytes).ToArray();
        using (var writer = new BinaryWriter(File.OpenWrite(filePath))){
            writer.Write(binaryOutput);
        }
        Console.WriteLine("Successfully created bin file");
    }
}