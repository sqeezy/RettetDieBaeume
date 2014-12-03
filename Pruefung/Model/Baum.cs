using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RettetDenWald.Model
{
    class Baum
    {
        public Baumart Art { get; set; }
        public Position Position { get; set; }

        public override string ToString()
        {
            return string.Format("{0} | {1}", Position.ToString(), Art.ToString());
        }
    }
}
