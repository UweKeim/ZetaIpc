namespace ZetaIpc.Runtime.Client
{
    using Helper;
    using System;

    [Serializable]
    public sealed class IpcClientException :
        Exception
    {
        private readonly ExceptionFromXmlLight _h;

        internal IpcClientException(ExceptionFromXmlLight h, Exception inner) :
            base(h.Message, inner)
        {
            _h = h;

            Source = h.Source;
        }

        public override string Message => _h.Message;

        public override string StackTrace => _h.StackTrace;

        public string Dump => _h.Dump;

        public string Type => _h.Type;
    }
}