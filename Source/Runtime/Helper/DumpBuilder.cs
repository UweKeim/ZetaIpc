namespace ZetaIpc.Runtime.Helper;

using System;
using System.Collections;
using System.Reflection;
using System.Text;

/// <summary>
/// Class that helps in dumping.
/// </summary>
internal static class DumpBuilder
{
    private static readonly string[] EscapeChars =
    {
        //"\r", "\n", "\t", @"""", @"'", @"\"
    };

    private static readonly string[] EscapeCharReplacements =
    {
        //@"\r", @"\n", @"\t", @"\""", @"\'", @"\\"
    };

    public static string Dump(
        Exception x,
        int indent = 0,
        bool deep = true)
    {
        var sb = new StringBuilder();
        reflect(sb, x, indent, deep);

        return sb.ToString();
    }

    private static void reflect(
        StringBuilder sb,
        object obj,
        int indent,
        bool deep)
    {
        reflect(sb, new GraphRef(null, obj, null), indent, deep);
    }

    private static void reflect(
        StringBuilder sb,
        GraphRef obj,
        int indent,
        bool deep,
        int nestingLevel = 0)
    {
        const int maxDepth = 3;

        //Ensure that we are not following a circular reference path
        if (obj.Value is not ValueType)
        {
            var parentRef = obj.Parent;
            while (parentRef != null)
            {
                if (parentRef.Value == obj.Value)
                {
                    return;
                }
                parentRef = parentRef.Parent;
            }
        }

        sb.Append('\t', indent);

        //Output property name if applicable
        if (!string.IsNullOrEmpty(obj.PropName))
        {
            sb.Append(obj.PropName);
            sb.Append(@"=");
        }

        var childIndent = indent + 1;

        switch (obj.Value)
        {
            //If value is null, output "null"
            case null:
                sb.Append(@"null");
                break;
            case string s:
                sb.AppendFormat(@"""{0}""", escape(s));
                break;
            //If value is a char, output value with single quotes around it
            case char objValue:
                sb.AppendFormat(@"'{0}'", escape(new string(objValue, 1)));
                break;
            case IDictionary value:
            {
                var list = value;
                sb.Append(Environment.NewLine);
                sb.Append('\t', indent);
                sb.Append(@"[");
                sb.Append(Environment.NewLine);

                foreach (DictionaryEntry entry in list)
                {
                    reflect(sb, new GraphRef(obj, entry.Key, null), childIndent, deep, nestingLevel);
                    sb.Append(@" = ");
                    reflect(sb, new GraphRef(obj, entry.Value, null), childIndent, deep, nestingLevel);
                    sb.Append(Environment.NewLine);
                }

                sb.Append('\t', indent);
                sb.Append(@"]");
                sb.Append(Environment.NewLine);
                break;
            }
            case Type type1:
                sb.Append(@"Type: ");
                sb.Append(type1.FullName);
                break;
            //...and similarly for MemberInfo objects
            default:
            {
                var info = obj.Value as MemberInfo;
                if (info != null)
                {
                    sb.Append(info.GetType().Name);
                    sb.Append(@": ");
                    sb.Append(info.Name);
                }
                //If value is not of a basic datatype
                else if (Convert.GetTypeCode(obj.Value) == TypeCode.Object)
                {
                    var type = obj.Value.GetType();
                    sb.Append(type.Name); //might want to use type.FullName instead.

                    if (indent <= maxDepth && (deep || nestingLevel == 0))
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append('\t', indent);
                        sb.Append(@"{");
                        sb.Append(Environment.NewLine);
                        //Get all the properties in the object's type
                        var props = type.GetProperties(
                            BindingFlags.Public |
                            BindingFlags.Instance |
                            BindingFlags.FlattenHierarchy);
                        //Enumerate all the properties and output their values
                        for (var i = 0; i < props.Length; i++)
                        {
                            var pi = props[i];
                            if (pi.GetIndexParameters().Length == 0) //Ignore indexers
                            {
                                try
                                {
                                    reflect(sb,
                                        new GraphRef(obj, pi.GetValue(obj.Value, null),
                                            pi.Name), childIndent, deep, nestingLevel + 1);
                                }
                                catch (Exception e)
                                {
                                    sb.Append(@"<Error getting property value (");
                                    sb.Append(e.GetType().Name);
                                    sb.Append(@")>");
                                }
                                if (i < props.Length - 1)
                                {
                                    sb.Append(',');
                                }
                                sb.Append(Environment.NewLine);
                            }
                        }

                        //If IList, output all the values in the list
                        if (obj.Value is IList list)
                        {
                            sb.Append(Environment.NewLine);
                            for (var i = 0; i < list.Count; i++)
                            {
                                reflect(sb, new GraphRef(obj, list[i], null), childIndent, deep,
                                    nestingLevel + 1);
                                if (i < list.Count - 1)
                                {
                                    sb.Append(',');
                                }
                                sb.Append(Environment.NewLine);
                            }
                        }

                        sb.Append('\t', indent);
                        sb.Append(@"}");
                    }
                }
                //If value is of a basic datatype
                else
                {
                    sb.Append(obj.Value);
                }

                break;
            }
        }
    }

    /// <summary>
    /// Escapes characters in a string using the escaping system used 
    /// in C# string literals
    /// </summary>
    private static string escape(string input)
    {
        var sb = new StringBuilder(input);
        for (var i = 0; i < EscapeChars.Length; i++)
        {
            sb.Replace(EscapeChars[i], EscapeCharReplacements[i]);
        }
        return sb.ToString();
    }

    private class GraphRef
    {
        public GraphRef(
            GraphRef parent,
            object obj,
            string propName)
        {
            Parent = parent;
            Value = obj;
            PropName = propName;
        }

        public object Value { get; }
        public GraphRef Parent { get; }
        public string PropName { get; }
    }
}