using System;
using System.Collections.Generic;
using System.Linq;
using AufforstungMischwald.Model;

namespace AufforstungMischwald
{
    /// <summary>
    /// Instanzen dieser Klasse berechnen eine möglichst günstige Bepflanzungs-Ordnung für ein Waldstück gegebener Größe und eine Menge von verwendbaren Baumarten.
    /// Wichtige Kriterien sind die Diversität der Baumarten und ein möglichst hoher Nutzungsgrad der Gesamtfläche.
    /// </summary>
    internal class Simulation
    {
        private const double Toleranz = 1e-5;
        private readonly double _breite;
        private readonly double _hoehe;
        private readonly string _waldName;
        private readonly List<Baumart> _arten;
        private readonly List<Baum> _ergebnisBaeume;
        private readonly List<Tuple<Baum, Baum>> _zuTestendeBaumpare;
        private readonly Random _random = new Random();
        private TimeSpan _laufzeit;

        /// <summary>
        /// Konstruktor für zentrale Simulations-Engine
        /// </summary>
        /// <param name="breite">Die Breite der Waldfläche.</param>
        /// <param name="hoehe">Die Höhe der Waldfläche.</param>
        /// <param name="path">Der Pfad unter dem die Eingabedatei liegt.</param>
        /// <param name="waldName">Der Name des Waldstückes.</param>
        /// <param name="arten">Die Menge aller pflanzbaren Baumarten.</param>
        public Simulation(double breite, double hoehe, string waldName, IEnumerable<Baumart> arten)
        {
            _breite = breite;
            _hoehe = hoehe;
            _waldName = waldName;
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

        public string WaldName
        {
            get { return _waldName; }
        }

        public TimeSpan Laufzeit
        {
            get { return _laufzeit; }
        }

        /// <summary>
        /// Gibt Simpson-Index zurück. Dieser gibt die Diversität der Baumarten an.
        /// </summary>
        public double GetD()
        {
            double result = 1.0;
            foreach (Baumart baumart in _arten)
            {
                result -= Math.Pow(baumart.Anzahl/(double) _ergebnisBaeume.Count, 2);
            }
            return result;
        }

        /// <summary>
        /// Gibt Erweiterten Simpson-Index zurück. Hier liegt das Gewicht stärker auf der abgedeckten Fläche.
        /// </summary>
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

        /// <summary>
        /// Startet die Simulation mit den im Konstruktor definierten Parametern.
        /// Zentrales Element ist die Menge aller Baumpaare, aus welchen sich eventuell eine weitere Position eines Baumes errechnen lässt.
        /// Diese Methode kann pro Instanz dieser Klasse nur einmal gerufen werden.
        /// </summary>
        public void VerteileBaeume()
        {
            PositioniereStartpaar();

            //Zeitmessung
            DateTime start = DateTime.Now;

            //Solange eine Möglichkeit auf eine weiter Pflanzposition besteht, rechnet der Algorithmus.
            while (_zuTestendeBaumpare.Count > 0)
            {
                SortiereBaumarten();

                //Ein Auswürfeln ob man mit den "alten" oder den "neuen" Expansionspunkten fortfährt ist sinnvoll, da die wahrscheinlichkeit für ein terminieren des Algorithmus, vor einer Abdeckung des gesamten Feldes stark sinkt.
                int index = _random.Next(0, 2) - 1;
                if (index == -1)
                {
                    index = _zuTestendeBaumpare.Count - 1;
                }
                Tuple<Baum, Baum> testBaumpaar = _zuTestendeBaumpare[index];

                //Da die Baumarten sortiert sind wird immer ein möglichst seltener Baum mit möglichst großem Radius gewählt, da dies am besten den geforderten Kriterien entspricht.
                foreach (Baumart baumart in _arten)
                {
                    //Mögliche Positionen werden bestimmt und validiert.
                    List<Position> validePositionen = BerechneValidePositionen(testBaumpaar, baumart);
                    if (validePositionen.Count < 1)
                    {
                        continue;
                    }

                    //Der Aufbau des Algorithmus hat zur Folge das stetts nur eine der möglichen Positionen valide ist, da die andere Entweder außerhalb des Waldes liegt oder innerhalb einer bereits bepflanzten Fläche.
                    var neuerBaum = new Baum
                                    {
                                        Art = baumart,
                                        Position = validePositionen[0]
                                    };

                    //Die Menge der zu untersuchenden Bäume wird erweitert.
                    _zuTestendeBaumpare.Add(new Tuple<Baum, Baum>(testBaumpaar.Item1, neuerBaum));
                    _zuTestendeBaumpare.Add(new Tuple<Baum, Baum>(testBaumpaar.Item2, neuerBaum));
                    _ergebnisBaeume.Add(neuerBaum);
                    baumart.Anzahl++;
                    break;
                }

                //Das Baumpaar wurde untersucht und da jedes Baumpaar nur eine valide neue Position liefern kann, wird es aus der Untersuchungsmenge entfernt.
                _zuTestendeBaumpare.Remove(testBaumpaar);
            }

            //Zeitmessung
            _laufzeit = DateTime.Now - start;
        }

        /// <summary>
        /// Gibt bestmöglichen Wert des Simpson-Index zurück. Zu Vergleichszwecken.
        /// </summary>
        public double GetBestD()
        {
            return 1.0 - 1/(double) _arten.Count;
        }

        /// <summary>
        /// Berechnet valide Positionen für einen neuen Baum in Nachbarschaft zu einem Baumpaar.
        /// </summary>
        /// <param name="untersuchtesTuple">Das Baumpaar.</param>
        /// <param name="baumart">Die Art des möglichen neuen Baumes.</param>
        /// <returns>Die möglichen Positionen.</returns>
        private List<Position> BerechneValidePositionen(Tuple<Baum, Baum> untersuchtesTuple, Baumart baumart)
        {
            Baum baum1 = untersuchtesTuple.Item1; //M1
            Baum baum2 = untersuchtesTuple.Item2; //M2

            Position m1 = baum1.Position;
            Position m2 = baum2.Position;

            double r1 = baum1.Art.Radius;
            double r2 = baum2.Art.Radius;
            double r3 = baumart.Radius;

            //Bestimme die Länge aller Seiten des entstehenden Dreiecks
            double l1 = r2 + r3;
            double l2 = r1 + r3;
            double l3 = r1 + r2;

            //Bestimme Winkel an M1
            double phi = Math.Acos((l2*l2 + l3*l3 - l1*l1)/(2*l2*l3));

            //Bestimme Punkt S welcher das rechtwinklige Dreieck M1-S-M3 bildet
            double betragSM3 = Math.Sin(phi)*(l2);
            double betragSM1 = Math.Cos(phi)*(l2);
            double sX = m1.X + (m2.X - m1.X)*(betragSM1/l3);
            double sY = m1.Y + (m2.Y - m1.Y)*(betragSM1/l3);
            var s = new Position
                    {
                        X = sX,
                        Y = sY
                    };

            //Bestimme genormten Richtungsvektor der von S auf M3 bzw. M3' zeigt
            double richtungsVektorX = (m2.Y - m1.Y);
            double richtungsVektorY = -(m2.X - m1.X);
            double laengeRichtungsVektor =
                Math.Sqrt(richtungsVektorX*richtungsVektorX + richtungsVektorY*richtungsVektorY);
            double normRichtungsVektorX = richtungsVektorX/laengeRichtungsVektor;
            double normRichtungsVektorY = richtungsVektorY/laengeRichtungsVektor;

            //Bestimme mögliche Lösungen
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

            //Überprüfe mögliche Lösungen
            return new List<Position>
                   {
                       moeglichePosition1,
                       moeglichePosition2
                   }.Where(position => IstValidePosition(position, r3))
                    .ToList();
        }

        /// <summary>
        /// Sortiert die Baumarten.
        /// </summary>
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

        /// <summary>
        /// Positioniert den ersten Baum in der linken unteren Ecke. (wenn möglich)
        /// Hierfür wird ein möglichst großer Baum genutzt.
        /// </summary>
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

        /// <summary>
        /// Positioniert den zweiten Baum zwischen dem Rand des Waldes und Baum 1(so vorhanden).
        /// Hierfür wird ein möglichst großer Baum genutzt.
        /// </summary>
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

        /// <summary>
        /// Sortiert zuerst nach der Anzahl der vorhandenen Ausprägungen einer Baumart.(aufsteigend)
        /// Danach wird nach der größe des Radius sortiert.(absteigend)
        /// </summary>
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

        /// <summary>
        /// Überprüft ob eine Position ein valider Platz für einen Baum mit einem bestimmten Radius liefert.
        /// </summary>
        private bool IstValidePosition(Position pos, double radius)
        {
            if (!IstInnerhalbDesWaldes(pos, radius))
            {
                return false;
            }

            if (!UeberschneidetSichMitAnderenBaeumen(pos, radius))
            {
                return false;
            }
            return true;
        }

        private bool UeberschneidetSichMitAnderenBaeumen(Position pos, double radius)
        {
            foreach (Baum baum in _ergebnisBaeume)
            {
                double abstandsQuadrat = (pos.X - baum.Position.X)*(pos.X - baum.Position.X) +
                                         (pos.Y - baum.Position.Y)*(pos.Y - baum.Position.Y);
                double diff = abstandsQuadrat - (radius + baum.Art.Radius)*(radius + baum.Art.Radius);

                //Der Versuch diesen Codeblock zu optimieren ,indem man das Ziehen der Wurzel durch ein Quadrieren der Gesamtgleichung auflöst und die Framework Funktion Math.Pow durch eine triviale Quadrierung
                //ersetzt, erbrachte ca. 100% Leistungssteigerung

                //double abstand = Math.Sqrt(Math.Pow(pos.X - baum.Position.X, 2) + Math.Pow(pos.Y - baum.Position.Y, 2));
                //double diff = abstand - (radius + baum.Art.Radius);

                if (diff < -Toleranz)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IstInnerhalbDesWaldes(Position pos, double radius)
        {
            double minX = pos.X - radius;
            double minY = pos.Y - radius;
            double maxX = pos.X + radius;
            double maxY = pos.Y + radius;

            if (minX < -Toleranz || minY < -Toleranz || Breite - maxX < -Toleranz || Hoehe - maxY < -Toleranz)
            {
                return false;
            }
            return true;
        }
    }
}