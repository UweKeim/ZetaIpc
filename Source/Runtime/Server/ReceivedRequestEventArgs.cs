namespace ZetaIpc.Runtime.Server
{
    public class ReceivedRequestEventArgs
    {
        public string Request { get; }
        public string Response { get; set; }
        public bool Handled { get; set; }

        public ReceivedRequestEventArgs(string request)
        {
            Request = request;
        }   
    }
}