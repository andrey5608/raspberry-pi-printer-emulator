using System;
using System.IO.Ports;
using System.Text;

public class EcsPosReader
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("arduino-demo <portName> [<baudRate>=9600]");
            return;
        }

        // to get port name you can use SerialPort.GetPortNames()
        string portName = args[0];
        int baudRate = args.Length >= 2 ? int.Parse(args[1]) : 9600;

        using (SerialPort sp = new SerialPort(portName))
        {
            sp.Encoding = Encoding.UTF8;
            sp.BaudRate = baudRate;
            sp.ReadTimeout = 1000;
            sp.WriteTimeout = 1000;
            sp.Open();

            bool finished = false;
            Console.CancelKeyPress += (a, b) =>
            {
                finished = true;
            // close port to kill pending operations
            sp.Close();
            };

            Console.WriteLine("Type '!q' or Ctrl-C to exit...");

            while (!finished)
            {
                var line = Console.ReadLine();
                if (line is object && line == "!q")
                    break;

                try
                {
                    sp.WriteLine(line);
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("ERROR: Sending command timed out");
                }

                if (finished)
                    break;

                // if RATE is set to really high Arduino may fail to respond in time
                // then on the next command you might get an old message
                // ReadExisting will read everything from the internal buffer
                string existingData = sp.ReadExisting();
                Console.Write(existingData);
                if (!existingData.Contains('\n') && !existingData.Contains('\r'))
                {
                    // we didn't get the response yet, let's wait for it then
                    try
                    {
                        Console.WriteLine(sp.ReadLine());
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine($"ERROR: No response in {sp.ReadTimeout}ms.");
                    }
                }
            }
        };

    }
}