namespace AufforstungMischwald.Model
{
    /// <summary>
    /// Container der eine Position im zweidimensionalen Raum abbildet.
    /// </summary>
    internal struct Position
    {
        public double X { get; set; }
        public double Y { get; set; }

        public override string ToString()
        {
            return string.Format("X={0} Y={1}", X, Y);
        }
    }
}