namespace ZetaIpc.Runtime.Helper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Class that helps in dumping.
    /// </summary>
    internal sealed class DumpBuilder
    {
        private static readonly string[] EscapeChars =
        {
            "\r", "\n", "\t", @"""", @"'", @"\"
        };

        private static readonly string[] EscapeCharReplacements =
        {
            @"\r", @"\n", @"\t", @"\""", @"\'", @"\\"
        };

        private readonly int _indentLevel;
        private readonly List<string> _lines = new List<string>();

        public DumpBuilder()
            : this(0, false, null)
        {
        }

        public DumpBuilder(
            int indentLevel)
            : this(indentLevel, false, null)
        {
        }

        public DumpBuilder(
            int indentLevel,
            bool deep)
            : this(indentLevel, deep, null)
        {
        }

        public DumpBuilder(
            int indentLevel,
            bool deep,
            Type typeToDump)
        {
            _indentLevel = indentLevel;
            IsDeep = deep;

            if (typeToDump != null)
            {
                var s = string.Format(
                    @"Dumping for '{0}':",
                    typeToDump.FullName);

                _lines.Add(s);
            }
        }

        public bool IsDeep { get; private set; }

        public DumpBuilder AddLine(
            string name,
            object value)
        {
            _lines.Add(makeStringToAdd(name, value));
            return this;
        }

        public DumpBuilder AddLine(
            string text)
        {
            _lines.Add(text);
            return this;
        }

        public DumpBuilder InsertLine(
            int index,
            string name,
            object value)
        {
            _lines.Insert(index, makeStringToAdd(name, value));
            return this;
        }

        public DumpBuilder InsertLine(
            int index,
            string text)
        {
            _lines.Insert(index, text);
            return this;
        }

        /// <summary>
        /// Get the dumped content.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var line in _lines.Where(line => !string.IsNullOrEmpty(line)))
            {
                sb.AppendLine(doIndent(line.TrimEnd()));
            }

            return sb.ToString().TrimEnd();
        }

        public static string Dump(
            DataTable table)
        {
            if (table == null || table.Rows == null || table.Rows.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                var sb = new StringBuilder();

                sb.AppendFormat(
                    @"====={0}",
                    Environment.NewLine);

                sb.AppendFormat(
                    @"Table '{0}'{1}",
                    table.TableName,
                    Environment.NewLine);
                sb.AppendFormat(
                    @"{0} rows{1}",
                    table.Rows.Count,
                    Environment.NewLine);

                sb.AppendFormat(
                    @"{0}",
                    Environment.NewLine);

                var rowIndex = 0;
                foreach (DataRow row in table.Rows)
                {
                    sb.AppendFormat(
                        @"Row {0}:{1}{2}{3}",
                        rowIndex + 1,
                        Environment.NewLine,
                        Dump(row),
                        Environment.NewLine);

                    rowIndex++;
                }

                sb.AppendFormat(
                    @"====={0}",
                    Environment.NewLine);

                return sb.ToString().Trim();
            }
        }

        public static string Dump(
            DataRow row)
        {
            if (
                row == null ||
                row.Table == null ||
                row.Table.Columns.Count <= 0)
            {
                return string.Empty;
            }
            else
            {
                var sb = new StringBuilder();

                sb.AppendFormat(
                    @"-----{0}",
                    Environment.NewLine);

                var index = 0;
                foreach (DataColumn column in row.Table.Columns)
                {
                    if (index > 0)
                    {
                        sb.AppendFormat(
                            @",{0}",
                            Environment.NewLine);
                    }

                    var o =
                        row.HasVersion(DataRowVersion.Original)
                            ? row[column, DataRowVersion.Original]
                            : row.HasVersion(DataRowVersion.Original)
                                ? row[column, DataRowVersion.Default]
                                : row[column];

                    if (o == null || o == DBNull.Value)
                    {
                        sb.AppendFormat(
                            @"[{0}] = null",
                            column.ColumnName);
                    }
                    else
                    {
                        sb.AppendFormat(
                            @"[{0}] = '{1}' ({2})",
                            column.ColumnName,
                            o,
                            o.GetType());
                    }

                    index++;
                }

                sb.AppendFormat(
                    @"{0}-----",
                    Environment.NewLine);

                return sb.ToString().Trim();
            }
        }

        public static string Dump(
            Exception x)
        {
            return Dump(x, true);
        }

        public static string Dump(
            Exception x,
            bool deep)
        {
            return Dump(x, 0, deep);
        }

        public static string Dump(
            Exception x,
            int indent)
        {
            return Dump(x, indent, true);
        }

        public static string Dump(
            Exception x,
            int indent,
            bool deep)
        {
            var sb = new StringBuilder();
            reflect(sb, x, indent, deep);

            return sb.ToString();
        }

        /// <summary>
        /// Dumps an arbitrary object.
        /// </summary>
        public static string Dump(
            object obj)
        {
            return Dump(obj, true);
        }

        public static string Dump(
            object obj,
            bool deep)
        {
            return Dump(obj, 0, deep);
        }

        public static string Dump(
            object obj,
            int indent)
        {
            return Dump(obj, indent, true);
        }

        public static string Dump(
            object obj,
            int indent,
            bool deep)
        {
            var sb = new StringBuilder();
            reflect(sb, obj, indent, deep);

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
            if (!(obj.Value is ValueType))
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
            if (!String.IsNullOrEmpty(obj.PropName))
            {
                sb.Append(obj.PropName);
                sb.Append(@"=");
            }

            var childIndent = indent + 1;

            //If value is null, output "null"
            if (obj.Value == null)
            {
                sb.Append(@"null");
            }
            //If value is a string, output value with quotes around it
            else
            {
                var s = obj.Value as string;
                if (s != null)
                {
                    sb.AppendFormat(@"""{0}""", escape(s));
                }
                //If value is a char, output value with single quotes around it
                else if (obj.Value is char)
                {
                    sb.AppendFormat(@"'{0}'", escape(new String((char)obj.Value, 1)));
                }
                else
                {
                    var value = obj.Value as IDictionary;
                    if (value != null)
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
                    }
                    //If it's a Type object, we don't want to endlessly follow long trains of 
                    //interconnected type info objects
                    else
                    {
                        var type1 = obj.Value as Type;
                        if (type1 != null)
                        {
                            sb.Append(@"Type: ");
                            sb.Append(type1.FullName);
                        }
                        //...and similarly for MemberInfo objects
                        else
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
                                    var list = obj.Value as IList;
                                    if (list != null)
                                    {
                                        sb.Append(Environment.NewLine);
                                        //sb.Append( '\t', indent );
                                        //sb.Append( @"[" );
                                        //sb.Append( Environment.NewLine );
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
                                        //sb.Append( '\t', indent );
                                        //sb.Append( @"]" );
                                        //sb.Append( Environment.NewLine );
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
                        }
                    }
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

        private static string makeStringToAdd(
            string name,
            object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(@"name");
            }
            else if (name.Length <= 0)
            {
                throw new ArgumentException(@"name");
            }
            else
            {
                var result = string.Empty;

                result += string.Format(
                    @"{0}: '{1}'",
                    name,
                    value == null ? @"(null)" : value.ToString());

                return result;
            }
        }

        private string doIndent(
            string text)
        {
            var result = new StringBuilder();

            result.Append('\t', _indentLevel);
            result.Append(text.TrimEnd());

            return result.ToString();
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

            public object Value { get; private set; }
            public GraphRef Parent { get; private set; }
            public string PropName { get; private set; }
        }
    }
}