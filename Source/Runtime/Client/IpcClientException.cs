namespace ZetaIpc.Runtime.Client
{
    using System;
    using Helper;

    [Serializable]
    public sealed class IpcClientException :
        Exception
    {
        private ExceptionFromXmlLight _h;

        internal IpcClientException(ExceptionFromXmlLight h, Exception inner) :
            base(h.Message, inner)
        {
            _h = h;

            Source = h.Source;
        }

        public override string Message
        {
            get { return _h.Message; }
        }

        public override string StackTrace
        {
            get { return _h.StackTrace; }
        }

        public string Dump
        {
            get { return _h.Dump; }
        }

        public string Type
        {
            get { return _h.Type; }
        }
    }
}