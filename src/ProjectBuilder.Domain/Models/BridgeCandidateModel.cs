namespace ProjectBuilder.Models
{
    // One physical bridge matching a District/County/Route/Section combination — plural because
    // tbl_pb_BridgeReferenceDataFromPennshare's Segment isn't a unique key (multiple BRKEYs
    // routinely share a Segment, e.g. ramp placeholders or parallel spans), so the Map treatment
    // editor's location cascade needs every candidate, not just one.
    public class BridgeCandidateModel
    {
        public string BrKey { get; set; }
        public string BridgeId { get; set; }
    }
}
