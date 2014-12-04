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
            var arten = new List<Baumart>();

            var lines = new List<string>(File.ReadAllLines(path));

            lines = new List<string>(Utils.RemoveComments(lines));

            //Parse Waldname
            string name = lines[0];

            //Parse Breite und Höhe
            string[] lineOneSplit = lines[1].Split(' ');
            double breite = double.Parse(lineOneSplit[0]);
            double hoehe = double.Parse(lineOneSplit[1]);

            //Erzeuge Baumarten
            for (int i = 2; i < lines.Count; i++)
            {
                double radius=double.Parse(lines[i]);
                arten.Add(new Baumart(i - 2, radius));
            }

            return new Simulation(breite, hoehe, name, arten);
        }
    }
}