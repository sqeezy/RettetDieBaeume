using System;
using System.Collections.Generic;
using System.Linq;
using RettetDenWald.Model;

namespace RettetDenWald
{
    internal class Simulation
    {
        private readonly double _breite;
        private readonly double _hoehe;
        private readonly string _path;
        private readonly string _name;
        private readonly List<Baumart> _arten;
        private readonly List<Baum> _ergebnisBaeume;
        private List<Tuple<Baum, Baum>> _zuTestendeBaumpare;

        public Simulation(double breite, double hoehe, string path, string name, IEnumerable<Baumart> arten)
        {
            _breite = breite;
            _hoehe = hoehe;
            _path = path;
            _name = name;
            _arten = new List<Baumart>(arten);
            _ergebnisBaeume = new List<Baum>();
            _zuTestendeBaumpare = new List<Tuple<Baum, Baum>>();
        }

        public IEnumerable<Baum> ErgebnisBaeume
        {
            get { return _ergebnisBaeume; }
        }

        public double Breite
        {
            get { return _breite; }
        }

        public double Hoehe
        {
            get { return _hoehe; }
        }

        public string Path
        {
            get { return _path; }
        }

        public string Name
        {
            get { return _name; }
        }

        public double GetD()
        {
            double result = 1.0;
            foreach (Baumart baumart in _arten)
            {
                result -= Math.Pow((double) baumart.Anzahl/(double) _ergebnisBaeume.Count,2);
            }
            return result;
        }

        public double GetB()
        {
            double d = GetD();
            double summeZaehler = 0;
            foreach (Baumart baumart in _arten)
            {
                summeZaehler += baumart.Anzahl*Math.PI*Math.Pow(baumart.Radius, 2);
            }
            double gesamtFlaeche = Breite*Hoehe;
            double b = d*summeZaehler/gesamtFlaeche;
            return b;
        }

        public void Simuliere()
        {
            PositioniereStartpaar();
#if DEBUG
            DateTime start = DateTime.Now;
#endif
            while (_zuTestendeBaumpare.Count > 0)
            {
                SortiereBaumarten();
                Tuple<Baum, Baum> untersuchtesTuple = _zuTestendeBaumpare.First();
                foreach (Baumart baumart in _arten)
                {
                    List<Position> validePositionen = BerechneValidePositionen(untersuchtesTuple, baumart);
                    if (validePositionen.Count<1)
                    {
                        continue;
                    }
                    Baum neuerBaum = new Baum
                                     {
                                         Art = baumart,
                                         Position = validePositionen[0]
                                     };
                    _zuTestendeBaumpare.Add(new Tuple<Baum, Baum>(untersuchtesTuple.Item1,neuerBaum));
                    _zuTestendeBaumpare.Add(new Tuple<Baum, Baum>(untersuchtesTuple.Item2,neuerBaum));
                    _ergebnisBaeume.Add(neuerBaum);
                    baumart.Anzahl++;
                    break;
                }
                _zuTestendeBaumpare.Remove(untersuchtesTuple);
            }
#if DEBUG
            TimeSpan end = DateTime.Now-start;
            var readable = end.TotalMilliseconds;
#endif

        }

        /// <summary>
        /// Berechne mögliche neue Mittelpunkte in abhängigkeit eines Paares von Bäumen und einer gewünschten Baumart.
        /// TODO Grafik zur verdeutlichung
        /// </summary>
        /// <param name="untersuchtesTuple">Das Ausgangspaar.</param>
        /// <param name="baumart">Die gewünschte Baumart.</param>
        /// <returns>Die Möglichen Positionen für neue Bäume.</returns>
        private List<Position> BerechneValidePositionen(Tuple<Baum, Baum> untersuchtesTuple, Baumart baumart)
        {
            Baum baum1 = untersuchtesTuple.Item1; //M1
            Baum baum2 = untersuchtesTuple.Item2; //M2

            Position m1 = baum1.Position;
            Position m2 = baum2.Position;

            double r1 = baum1.Art.Radius;
            double r2 = baum2.Art.Radius;
            double r3 = baumart.Radius;

            double l1 = r2 + r3;
            double l2 = r1 + r3;
            double l3 = r1 + r2;

            double phi = Math.Acos((l2*l2 + l3*l3 - l1*l1)/(2*l2*l3));

            double betragSM3 = Math.Sin(phi)*(l2);
            double betragSM1 = Math.Cos(phi)*(l2);

            double sX = m1.X+(m2.X - m1.X)*(betragSM1/l3);
            double sY = m1.Y+(m2.Y - m1.Y)*(betragSM1/l3);

            var s = new Position
                    {
                        X = sX,
                        Y = sY
                    };

            double richtungsVektorX = (m2.Y - m1.Y);
            double richtungsVektorY = -(m2.X - m1.X);
            double laengeRichtungsVektor =
                Math.Sqrt(richtungsVektorX*richtungsVektorX + richtungsVektorY*richtungsVektorY);

            double normRichtungsVektorX = richtungsVektorX/laengeRichtungsVektor;
            double normRichtungsVektorY = richtungsVektorY/laengeRichtungsVektor;

            var moeglichePosition1 = new Position
                                     {
                                         X = s.X + normRichtungsVektorX*betragSM3,
                                         Y = s.Y + normRichtungsVektorY*betragSM3
                                     };
            var moeglichePosition2 = new Position
                                     {
                                         X = s.X - normRichtungsVektorX*betragSM3,
                                         Y = s.Y - normRichtungsVektorY*betragSM3
                                     };

            return new List<Position>
                   {
                       moeglichePosition1,
                       moeglichePosition2
                   }.Where(position => IstValidePosition(position, r3))
                    .ToList();
        }

        private void SortiereBaumarten()
        {
            _arten.Sort(BaumComparison);
        }

        private void PositioniereStartpaar()
        {
            SortiereBaumarten();
            PositioniereBaumEins();

            SortiereBaumarten();
            PositioniereBaumZwei();
        }

        private void PositioniereBaumEins()
        {
            foreach (Baumart baumart in _arten)
            {
                double radius = baumart.Radius;
                var pos = new Position
                          {
                              X = radius,
                              Y = radius
                          };
                if (!IstValidePosition(pos, radius))
                {
                    continue;
                }

                _ergebnisBaeume.Add(new Baum
                                    {
                                        Art = baumart,
                                        Position = pos
                                    });
                baumart.Anzahl++;
                break;
            }
        }

        private void PositioniereBaumZwei()
        {
            if (_ergebnisBaeume.Count != 1)
            {
                return;
            }

            Baum ersterBaum = ErgebnisBaeume.First();

            foreach (Baumart baumart in _arten)
            {
                double radius = baumart.Radius;
                double radiusBaumEins = ersterBaum.Art.Radius;
                double y = radiusBaumEins +
                           Math.Sqrt(Math.Pow(radiusBaumEins + radius, 2) - Math.Pow(radiusBaumEins - radius, 2));

                var pos = new Position
                          {
                              X = radius,
                              Y = y
                          };
                if (!IstValidePosition(pos, radius))
                {
                    continue;
                }

                var zweiterBaum = new Baum
                                  {
                                      Art = baumart,
                                      Position = pos
                                  };
                _ergebnisBaeume.Add(zweiterBaum);
                baumart.Anzahl++;
                _zuTestendeBaumpare.Add(new Tuple<Baum, Baum>(ersterBaum, zweiterBaum));
                break;
            }
        }

        private static int BaumComparison(Baumart a, Baumart b)
        {
            int diffAnzahl = a.Anzahl - b.Anzahl;
            if (diffAnzahl == 0)
            {
                try
                {
                    return Convert.ToInt32(b.Radius - a.Radius);
                }
                catch (OverflowException)
                {
                    //Leer da einfach auf Differenz der Anzahl zurückgefallen werden kann.
                }
            }
            return diffAnzahl;
        }

        private bool IstValidePosition(Position pos, double radius)
        {
            double minX = pos.X - radius;
            double minY = pos.Y - radius;
            double maxX = pos.X + radius;
            double maxY = pos.Y + radius;

            if (minX < 0 || minY < 0 || maxX > Breite || maxY > Hoehe)
            {
                return false;
            }

            foreach (Baum baum in _ergebnisBaeume)
            {
                double abstand = Math.Sqrt(Math.Pow(pos.X - baum.Position.X, 2) + Math.Pow(pos.Y - baum.Position.Y, 2));
                if (double.IsNaN(abstand))
                {
                    return false;
                }
                double diff =abstand - (radius + baum.Art.Radius);
                if (diff<-1e-5)
                {
                    return false;
                }
            }
            return true;
        }
    }
}