namespace RettetDenWald.Model
{
    internal class Baumart
    {
        private readonly int _index;
        private readonly double _radius;

        public Baumart(int index, double radius)
        {
            _index = index;
            _radius = radius;
        }

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