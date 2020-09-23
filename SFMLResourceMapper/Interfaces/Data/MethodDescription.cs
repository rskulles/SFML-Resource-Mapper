using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RES.ResMap.Interfaces.Data
{
    public sealed class MethodDescription
    {
        public enum MethodType
        {
            Method,
            Constructor,
            Destructor
        }

        public MethodDescription()
        {
            Parameters = new List<(string, string)>();
        }

        public AccessLevelType AccessLevel { get; set; }
        public MethodType Type { get; set; }
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<(string, string)> Parameters { get; set; }
        public string Body { get; set; }
        public string ClassName { get; set; }

        public string GetDeclaration(bool includeClassName)
        {
            var paramList = GetParameterString();
            var fullMethodName = (includeClassName ? ClassName + "::" : "") + Name;
            switch (Type)
            {
                case MethodType.Method:

                    return $"{ReturnType} {fullMethodName}({paramList})";
                case MethodType.Constructor:
                    return $"{fullMethodName}({paramList})";
                case MethodType.Destructor:
                    return $"{fullMethodName}()";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetParameterString()
        {
            var list = Parameters.Select(valueTuple => $"{valueTuple.Item1} {valueTuple.Item2}").ToList();
            return string.Join(", ", list);
        }

        public string GetDefinition()
        {
            var sb = new StringBuilder();
            var declaration = GetDeclaration(true);
            sb.AppendLine(declaration + "{");
            sb.AppendLine(Body);
            sb.AppendLine("}");
            return sb.ToString();
        }

        public override string ToString()
        {
            return GetDefinition();
        }
    }
}