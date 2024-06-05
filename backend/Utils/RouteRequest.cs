namespace backend.Utils
{
    public class RouteRequest
    {
        public double MyLat { get; set; }
        public double MyLong { get; set; }
        public List<long> Categories { get; set; } = new List<long>();
    }
}
