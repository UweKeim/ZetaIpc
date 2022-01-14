namespace ZetaIpc.Runtime.Helper;

using System;
using System.Xml;

internal sealed class ExceptionToXmlLight
{
    public const string Magic = @"7363b2b6-335d-4a3f-8933-9c1e906baf29";
    private readonly Exception _exception;

    public ExceptionToXmlLight(Exception exception)
    {
        _exception = exception;
    }

    public string ToXmlString()
    {
        var doc = new XmlDocument();

        var root = doc.CreateElement(@"exception");
        doc.AppendChild(root);

        root.SetAttribute(@"magic", Magic);

        var msg = doc.CreateElement(@"message");
        root.AppendChild(msg);
        msg.InnerText = _exception.Message;

        var type = doc.CreateElement(@"type");
        root.AppendChild(type);
        type.InnerText = _exception.GetType().ToString();

        var source = doc.CreateElement(@"source");
        root.AppendChild(source);
        source.InnerText = _exception.Source;

        var st = doc.CreateElement(@"stackTrace");
        root.AppendChild(st);
        st.InnerText = _exception.StackTrace;

        var dump = doc.CreateElement(@"dump");
        root.AppendChild(dump);
        dump.InnerText = DumpBuilder.Dump(_exception);

        return doc.OuterXml;
    }
}