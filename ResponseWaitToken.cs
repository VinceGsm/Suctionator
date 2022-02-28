namespace Suctionator
{
    public class ResponseWaitToken
    {
        public int statusCode { get; set; }
        public string message { get; set; }
        
        public Data data { get; set; }
    }

    public class Data
    {
        public string dlLink { get; set; }
    }
}
