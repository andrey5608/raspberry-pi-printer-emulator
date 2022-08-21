using System;
using System.IO.Ports;
using System.Text;
using System.Collections.Generic;

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
            int i = 0;
            var receivedSymbolsAsInt = new List<int>();
            var cutPaperCommand = new List<int> { 27, 61, 0 };

            while (true)
            {
                var existingData = sp.ReadByte();
                receivedSymbolsAsInt.Add(existingData);
                Console.WriteLine(String.Join(", ", receivedSymbolsAsInt.ToArray()));
                if (Enumerable.SequenceEqual(GetThreeLast(receivedSymbolsAsInt), cutPaperCommand))
                {
                    Console.WriteLine("Paper cut");
                    break;
                }
            }
        };
    }
    private static List<int> GetThreeLast(List<int> list)
    {
        return Enumerable.Reverse(list).Take(3).Reverse().ToList();
    }
}