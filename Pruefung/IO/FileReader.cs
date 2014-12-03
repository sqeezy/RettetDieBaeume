using System;
using System.Collections.Generic;
using System.IO;
using RettetDenWald.Model;

namespace RettetDenWald.IO
{
    internal static class FileReader
    {
        public static Simulation Read(string path)
        {
            double breite;
            double hoehe;
            string name;
            var arten = new List<Baumart>();

            try
            {
                var lines = new List<string>(File.ReadAllLines(path));

                lines= new List<string>(Utils.RemoveComents(lines));

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
            }
            catch (Exception)
            {
                throw;
            }

            return new Simulation(breite,hoehe,path,name,arten);
        }
    }
}