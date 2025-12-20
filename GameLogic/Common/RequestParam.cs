namespace IdleCs.GameLogic
{
    public class RequestParam
    {
        public RedisRequestType RequestType { get; private set; }
        public string RequestKey { get; private set;}

        public RequestParam(RedisRequestType requestType, string requestkey)
        {
            RequestType = requestType;
            RequestKey = requestkey;
        }
    }
}