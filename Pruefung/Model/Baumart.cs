namespace AufforstungMischwald.Model
{
    /// <summary>
    /// Klasse deren Instanzen verschiedene Baumarten repräsentieren.
    /// </summary>
    internal class Baumart
    {
        private readonly int _index;
        private readonly double _radius;

        /// <summary>
        /// Konstruktor.
        /// </summary>
        /// <param name="index">Der Index der Baumart in der Eingabedatei.</param>
        /// <param name="radius">Der Radius der Bepflanzungsfläche der Baumart.</param>
        public Baumart(int index, double radius)
        {
            _index = index;
            _radius = radius;
        }

        /// <summary>
        /// Bisherige Menge an Bäumen dieser Art.
        /// </summary>
        public int Anzahl { get; set; }

        public int Index
        {
            get { return _index; }
        }

        public double Radius
        {
            get { return _radius; }
        }

        public override string ToString()
        {
            return string.Format("r={0}, n={1}, i={2}", _radius, Anzahl, _index);
        }
    }
}