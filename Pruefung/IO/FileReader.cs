using System.Collections.Generic;
using System.IO;
using AufforstungMischwald.Model;

namespace AufforstungMischwald.IO
{
    /// <summary>
    /// Utlility-Klasse mit deren Hilfe eine Eingabedatei des vorgegebenen Formats eingelesen werden kann.
    /// Aus dieser Datei wird eine Simulations-Instanz erzeugt.
    /// Diese Klasse ist nur unter vorherigem verwenden der FileValidator Klasse zulässig, da ein unzulässiges Format oder nicht Vorhandensein der Datei Programmabstürze verursachen könnte.
    /// </summary>
    internal static class FileReader
    {
        public static Simulation Read(string path)
        {
            double breite;
            double hoehe;
            string name;
            var arten = new List<Baumart>();

            var lines = new List<string>(File.ReadAllLines(path));

            lines = new List<string>(Utils.RemoveComents(lines));

            //Parse Waldname
            name = lines[0];

            //Parse Breite und Höhe
            string[] lineOneSplit = lines[1].Split(' ');
            double.TryParse(lineOneSplit[0], out breite);
            double.TryParse(lineOneSplit[1], out hoehe);

            //Erzeuge Baumarten
            for (int i = 2; i < lines.Count; i++)
            {
                double radius;
                double.TryParse(lines[i], out radius);
                arten.Add(new Baumart(i - 2, radius));
            }

            return new Simulation(breite, hoehe, path, name, arten);
        }
    }
}