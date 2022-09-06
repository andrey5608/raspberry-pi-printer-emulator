using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace EscPosReader
{
    public class EcsPosReader
    {
        private static void Main()
        {
            try
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));

                if (settings == null)
                {
                    throw new InvalidDataException("settings file is invalid");
                }

                Console.WriteLine($"Configuration: port - {settings.SerialPortName}; baudRate - {settings.BaudRate}; encoding - {settings.Encoding}");

                Console.WriteLine("Incoming Data:");
                using (var sp = new SerialPort(settings.SerialPortName, settings.BaudRate, Parity.None, 8, StopBits.One))
                {
                    sp.Encoding = Encoding.GetEncoding(settings.Encoding);
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
                            SaveBinaryFile(receivedSymbols, settings.OutputDir);
                            SendToDecode(receivedSymbols, settings);

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

        private static void SendToDecode(byte[] receiptData, Settings settings)
        {
            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{settings.ApiUrl}/{settings.MerchantId}"),
                    Headers = {
                        { HttpRequestHeader.Authorization.ToString(), settings.ApiToken },
                        { HttpRequestHeader.Accept.ToString(), "application/json" }
                    },
                    Content = new ByteArrayContent(receiptData)
            };

                var result = client.SendAsync(message).Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine(resultContent);
            }
        }

        private static List<int> GetTwoLast(List<int> list)
        {
            return Enumerable.Reverse(list).Take(2).Reverse().ToList();
        }

        private static void SaveBinaryFile(byte[] binaryOutput, string outputDir){
            var datetime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var filePath = $"./{outputDir}/{datetime}.bin";
            using (var writer = new BinaryWriter(File.OpenWrite(filePath))){
                writer.Write(binaryOutput);
            }
            Console.WriteLine("Successfully created bin file");
        }
    }
}