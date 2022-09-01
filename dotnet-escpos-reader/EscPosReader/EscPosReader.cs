using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Configuration;

namespace EscPosReader
{
    public class EcsPosReader
    {
        private static void Main()
        {
            var serialPortName = ConfigurationManager.AppSettings["serialPortName"];
            var baudRate = int.Parse(ConfigurationManager.AppSettings["baudRate"]);
            var encoding = ConfigurationManager.AppSettings["encoding"];
            Console.WriteLine($"Configuration: port - {serialPortName}; baudRate - {baudRate}; encoding - {encoding}");

            Console.WriteLine("Incoming Data:");
            try
            {
                using (var sp = new SerialPort(serialPortName, baudRate, Parity.None, 8, StopBits.One))
                {
                    sp.Encoding = Encoding.GetEncoding(encoding);
                    sp.Open();

                    Console.WriteLine("Type Ctrl-C to exit...");

                    var receivedSymbolsAsInt = new List<int>();
                    var cutPaperCommand = new List<int> { 27, 109 };

                    while (true)
                    {
                        try
                        {
                            var existingData = sp.ReadByte();
                            Console.WriteLine(existingData);
                            receivedSymbolsAsInt.Add(existingData);

                            if (!GetTwoLast(receivedSymbolsAsInt).SequenceEqual(cutPaperCommand)) continue;

                            Console.WriteLine(string.Join(", ", receivedSymbolsAsInt.ToArray()));
                            Console.WriteLine("Paper cut");
                            // send to decode
                            var receivedSymbols = receivedSymbolsAsInt.SelectMany(BitConverter.GetBytes).ToArray();
                            SaveBinaryFile(receivedSymbols);
                            Decoder.Decoder.DecodeByteArrayToText(receivedSymbols);

                            receivedSymbolsAsInt.Clear();
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message, e);
                        }
                        
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);
            }
        }

        private static List<int> GetTwoLast(List<int> list)
        {
            return Enumerable.Reverse(list).Take(2).Reverse().ToList();
        }

        private static void SaveBinaryFile(byte[] binaryOutput){
            var datetime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var outputDir = ConfigurationManager.AppSettings["outputDir"];
            var filePath = $"./{outputDir}/{datetime}.bin";
            using (var writer = new BinaryWriter(File.OpenWrite(filePath))){
                writer.Write(binaryOutput);
            }
            Console.WriteLine("Successfully created bin file");
        }
    }
}