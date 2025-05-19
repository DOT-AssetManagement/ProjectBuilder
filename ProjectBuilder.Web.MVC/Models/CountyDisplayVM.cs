namespace ProjectBuilder.Web.MVC.Models
{
    public class CountyDisplayVM
    {
        public string CountyName { get; set; }
        public int? District { get; set; }
        public int? CountyId { get; set; }
        public string CountyFullName { get; set; }
        public bool IsDeleteable { get; set; }


    }
}
