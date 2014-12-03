using System;
using System.Net.Mime;
using RettetDenWald.IO;

namespace RettetDenWald
{
    class Program
    {
        private const string Usagestring = "usage: RettetDenWald.exe <pathToInputFile>";

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine(Usagestring);
                Console.ReadKey();
                return;
            }

            string path = args[0];

            ValidationResult validationResult = FileValidator.Validate(path);
            if (validationResult!=ValidationResult.Ok)
            {
                //switch (validationResult)
                //{
                        
                //}
            }

            Simulation sim = FileReader.Read(path);

            sim.Simuliere();

            FileWriter.Write(sim);
        }
    }
}
