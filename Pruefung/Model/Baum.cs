namespace AufforstungMischwald.Model
{
    /// <summary>
    /// Klasse die einen konkreten Baum repräsentiert.
    /// </summary>
    internal class Baum
    {
        public Baumart Art { get; set; }
        public Position Position { get; set; }

        public override string ToString()
        {
            return string.Format("{0} | {1}", Position, Art);
        }
    }
}