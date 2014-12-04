using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace AufforstungMischwald.IO
{
    /// <summary>
    /// Klasse zum umwandeln einer erfolgten Simulation in das geforderte Ausgabeformat.
    /// </summary>
    internal static class FileWriter
    {
        private const string FormatStringBaum = "{0:0.0######} {1:0.0######} {2:0.0######} {3:0.0######}";

        public static void Write(Simulation sim, string path)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                var lines = new List<string>
                            {
                                "reset",
                                string.Format("set yrange[0:{0:0.0######}]", sim.Hoehe),
                                string.Format("set xrange[0:{0:0.0######}]", sim.Breite),
                                string.Format("set size ratio {0:0.0######}", (sim.Hoehe/sim.Breite)),
                                string.Format(
                                              "set title \"{0} mit D={1:0.00###}(Dmax={2:0.00###}), B={3:0.00###}, Laufzeit={4:0.#######}s\"",
                                              sim.WaldName,
                                              sim.GetD(),
                                              sim.GetBestD(),
                                              sim.GetB(),
                                              sim.Laufzeit.TotalSeconds),
                                "plot '-' using 1:2:3:4 with circles lc var"
                            };

                lines.AddRange(
                               sim.ErgebnisBaeume.Select(
                                                         baum =>
                                                         string.Format(FormatStringBaum,
                                                                       baum.Position.X,
                                                                       baum.Position.Y,
                                                                       baum.Art.Radius,
                                                                       baum.Art.Index)));

                File.WriteAllLines(string.Format("{0}.plt", path), lines);
            }
            catch (IOException)
            {
                Console.WriteLine(
                                  "Es ist ein fehler während des Schreibens der Ausgabedatei aufgetreten. Überprüfen sie mögliches Fehlen von Schreibrechten.");
            }
            catch (Exception)
            {
                Console.WriteLine("Ein unerwarteter Fehler beim schreiben der Ausgabedatei ist aufgetreten.");
            }
        }
    }
}