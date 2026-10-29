namespace ProjectBuilder.Models.Map.Dtos
{
    public class MapDistrictDto
    {
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
    }

    public class MapCountyDto
    {
        public int Cnty { get; set; }
        public string CountyName { get; set; }
    }

    public class MapRouteDto
    {
        public string Rte { get; set; }
    }
}
