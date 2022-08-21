using System;
using System.IO.Ports;
using System.Text;

public class EcsPosReader
{
    static void Main(string[] args)
    {
        
         // Attach a method to be called when there
         // is data waiting in the port's buffer 
        // Begin communications 
        Console.WriteLine("Incoming Data:");
        using (SerialPort sp = new SerialPort("/dev/serial0",
      9600))
        {
            //sp.Encoding = Encoding.UTF8;
            sp.Encoding = Encoding.GetEncoding("ibm850");
            sp.BaudRate = 9600;
            //sp.ReadTimeout = 1000;
            //sp.WriteTimeout = 1000;
            sp.Open();

            Console.WriteLine("Type Ctrl-C to exit...");
            int i = 0;

            while (i < 100)
            {
                // if RATE is set to really high Arduino may fail to respond in time
                // then on the next command you might get an old message
                // ReadExisting will read everything from the internal buffer
                var existingData = sp.ReadByte();
                Console.WriteLine(existingData);
                /*
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
                }*/
            }
        };
        // Enter an application loop to keep this thread alive 
        //Console.ReadLine();
        /*
        if (args.Length == 0)
        {
            Console.WriteLine("arduino-demo <portName> [<baudRate>=9600]");
            return;
        }

        // to get port name you can use SerialPort.GetPortNames()
        string portName = args[0];
        int baudRate = args.Length >= 2 ? int.Parse(args[1]) : 9600;

        

        */
    } 
}