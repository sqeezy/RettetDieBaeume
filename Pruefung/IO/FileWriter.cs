using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using RettetDenWald.Model;

namespace RettetDenWald.IO
{
    internal static class FileWriter
    {
        private const string FormatStringHeader =
            "reset \r" + "set yrange[0:{0}]\r" + "set xrange[0:{1}]\r" + "set size ratio {2}\r" +
            "set title \"{3} mit D={4}, B={5}\"\r" + "plot '-' using 1:2:3:4 with circles lc var";

        private const string FormatStringBaum = "{0:0.0######} {1:0.0######} {2:0.0######} {3:0.0######}";

        public static void Write(Simulation sim)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var lines = new List<string>
                        {
                            "reset",
                            string.Format("set yrange[0:{0:0.0######}]", sim.Hoehe),
                            string.Format("set xrange[0:{0:0.0######}]", sim.Breite),
                            string.Format("set size ratio {0:0.0######}", (sim.Hoehe/sim.Breite)),
                            string.Format("set title \"{0} mit D={1:0.0######}, B={2:0.0######}\"",
                                          sim.Name,
                                          sim.GetD()
                                             ,
                                          sim.GetB()
                                             ),
                            "plot '-' using 1:2:3:4 with circles lc var"
                        };

            //lines.Add(string.Format(FormatStringHeader,
            //                        sim.Hoehe,
            //                        sim.Breite,
            //                        sim.Hoehe/sim.Breite,
            //                        sim.Name,
            //                        sim.GetD(),
            //                        sim.GetB()));
            foreach (Baum baum in sim.ErgebnisBaeume)
            {
                lines.Add(string.Format(FormatStringBaum,
                                        baum.Position.X,
                                        baum.Position.Y,
                                        baum.Art.Radius,
                                        baum.Art.Index));
            }
            File.WriteAllLines(string.Format("{0}.plt", sim.Path), lines);
        }
    }
}