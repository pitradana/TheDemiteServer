using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDemiteServer
{
    class PolylineDecoder
    {
        private int index;
        private float lat;
        private float lng;
        private int shift;
        private int result;
        private int bytes;
        private float latitude_change;
        private float longitude_change;
        private float factor;

        public PolylineDecoder()
        {
            this.index = 0;
            this.lat = 0;
            this.lng = 0;
            this.shift = 0;
            this.result = 0;
            this.bytes = 0;
            this.latitude_change = 0;
            this.longitude_change = 0;
        }

        public List<Coordinate> Decode(string str, int precision)
        {
            List<Coordinate> coordinates = new List<Coordinate>();
            this.factor = (float)Math.Pow(10, precision);

            while (index < str.Length)
            {
                bytes = 0;
                shift = 0;
                result = 0;

                do
                {
                    bytes = Convert.ToInt32(str[index++]) - 63;
                    result |= (bytes & 31) << shift;
                    shift += 5;
                } while (bytes >= 0x20 && index < str.Length);

                if (index >= str.Length)
                {
                    break;
                }

                latitude_change = (result & 1) == 1 ? ~(result >> 1) : (result >> 1);

                result = 0;
                shift = 0;

                do
                {
                    bytes = Convert.ToInt32(str[index++]) - 63;
                    result |= (bytes & 31) << shift;
                    shift += 5;
                } while (bytes >= 0x20 && index < str.Length);

                if (index >= str.Length && bytes >= 0x20)
                {
                    break;
                }

                longitude_change = (result & 1) == 1 ? ~(result >> 1) : (result >> 1);

                lat += latitude_change;
                lng += longitude_change;

                Coordinate coor = new Coordinate();
                coor.latitude = lat / factor;
                coor.longitude = lng / factor;

                coordinates.Add(coor);
            }
            return coordinates;
        }
    }
}
