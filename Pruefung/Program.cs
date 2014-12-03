using System;
using AufforstungMischwald.IO;

namespace AufforstungMischwald
{
    /// <summary>
    /// Startklasse.
    /// Leitet die Eingabeparameter in die entsprechenden Programmteile weiter.
    /// </summary>
    internal class Program
    {
        private const string Usagestring = "usage: RettetDenWald.exe <pathToInputFile>";
        private const string NoFileFoundString= "There was no file found under the given path. Maybe a typo?";
        private const string FileWrongFormatString= "The file has a wrong format. See Readme for proper format informations.";

        private static void Main(string[] args)
        {
            //Überprüfung des Parameterarrays
            if (args.Length != 1)
            {
                Console.WriteLine(Usagestring);
                return;
            }

            string path = args[0];

            ValidationResult validationResult = FileValidator.Validate(path);
                switch (validationResult)
                {
                    //Sonderfälle
                    case ValidationResult.NoFileFound:
                        Console.WriteLine(NoFileFoundString);
                        break;
                    case ValidationResult.WrongFormat:
                        Console.WriteLine(FileWrongFormatString);
                        break;

                    //Normalfall
                    case ValidationResult.Ok:
                        Simulation sim = FileReader.Read(path);

                        sim.Simuliere();

                        FileWriter.Write(sim);
                        break;
                }
        }
    }
}