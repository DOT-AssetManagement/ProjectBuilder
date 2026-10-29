namespace ProjectBuilder.Models.ProjectTreatment.Dtos
{
    // One physical bridge matching a District/County/Route/Section combination — see
    // BridgeCandidateModel for why Section alone can resolve to more than one of these.
    public class BridgeCandidateDto
    {
        public string BrKey { get; set; }
        public string BridgeId { get; set; }
    }
}
