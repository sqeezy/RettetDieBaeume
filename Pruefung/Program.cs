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
        private const string FileWrongFormatString= "The file has a wrong format. See documentation for proper format informations.";
        private const int MaximaleWiederholungen = 10;

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
            if (validationResult == ValidationResult.Ok)
            {
                int durchgefuehrteSimulationen = 0;
                while (true)//Wiederholtes simulieren sichert ab das ein gutes Ergebnis gefunden wird, wenn es eines gibt.
                {
                        Simulation sim = FileReader.Read(path);

                        sim.VerteileBaeume();

                    durchgefuehrteSimulationen++;

                    if (sim.GetB()/sim.GetD() > 0.5||durchgefuehrteSimulationen==MaximaleWiederholungen) //Ist die abgedeckte Fläche größer als 50% der Gesamtfläche?
                    {
                        FileWriter.Write(sim,path);
                        break;
                    }
                }
            }
            else
            {
                FehlerAusgabe(validationResult);
            }
        }

        private static void FehlerAusgabe(ValidationResult validationResult)
        {
            if (validationResult==ValidationResult.NoFileFound)
            {
                    Console.WriteLine(NoFileFoundString);
            }
            else
            {
                    Console.WriteLine(FileWrongFormatString);
            }
        }
    }
}