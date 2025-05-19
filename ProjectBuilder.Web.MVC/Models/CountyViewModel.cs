namespace ProjectBuilder.Web.MVC.Models
{
    public class CountyViewModel
    {
        public string CountyName { get; set; }
        public int? District { get; set; }
        public int? CountyId { get; set; }
        public string CountyFullName { get { return $"{CountyId}-{CountyName}"; } }
    }
}
