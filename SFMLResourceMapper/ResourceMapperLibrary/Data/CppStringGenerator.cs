using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RES.ResMap.Interfaces.Data;

namespace RES.ResMap.CppGenerator.Data
{
    public class CppStringGenerator
    {
        //Header Tags
        private const string ClassNameUpperTag = "<CLASS_NAME_UPPER>";
        private const string IncludeMarkerTag = "<INCLUDE_MARKER>";
        private const string NamespaceStartTag = "<NAMESPACE_START>";
        private const string NamespaceEndTag = "<NAMESPACE_END>";
        private const string ClassNameTag = "<CLASS_NAME>";
        private const string PublicMarkerTag = "<PUBLIC_MARKER>";
        private const string PrivateMethodMarkerTag = "<PRIVATE_METHOD_DECLARATION_MARKER>";
        private const string PrivateMemberMarkerTag = "<PRIVATE_MEMBER_MARKER>";

        //Definition Tags
        private const string ClassNameLowerTag = "<CLASS_NAME_LOWER>";
        private const string HeaderNamespaceTag = "<HEADER_NAMESPACE>";
        private const string MethodDefinitionMarkerTag = "<METHOD_DEFINITION_MARKER>";


        public (string header, string definition) GenerateClass(string className, IEnumerable<string> includes,
            IEnumerable<MethodDescription> methods, IEnumerable<MemberDescription> members,
            string classNamespace = null)
        {
            //Get base generated files
            var generatedFiles = GenerateClassTemplate(className, classNamespace);

            //Add includes
            var includeString = GetIncludeString(includes);
            generatedFiles.header = generatedFiles.header.Replace(IncludeMarkerTag, includeString);

            var methodDescriptions = methods as MethodDescription[] ?? methods.ToArray();
            var memberDescriptions = members as MemberDescription[] ?? members.ToArray();

            //Add public members and methods
            var publicItems = GetPublicHeaderItems(
                methodDescriptions.Where(m => m.AccessLevel == AccessLevelType.Public),
                memberDescriptions.Where(m => m.AccessLevel == AccessLevelType.Public));
            generatedFiles.header = generatedFiles.header.Replace(PublicMarkerTag, publicItems);

            //Add private methods
            var privateMethods =
                GetPrivateMethodDeclarations(methodDescriptions.Where(m => m.AccessLevel == AccessLevelType.Private));
            generatedFiles.header = generatedFiles.header.Replace(PrivateMethodMarkerTag, privateMethods);

            //Add private members
            var privateMembers =
                GetPrivateMembers(memberDescriptions.Where(m => m.AccessLevel == AccessLevelType.Private));
            generatedFiles.header = generatedFiles.header.Replace(PrivateMemberMarkerTag, privateMembers);

            //Add methods to definition file
            var methodDefinitions = GetMethodDefinitions(methodDescriptions);
            generatedFiles.definition = generatedFiles.definition.Replace(MethodDefinitionMarkerTag, methodDefinitions);

            return generatedFiles;
        }

        private string GetMethodDefinitions(MethodDescription[] methodDescriptions)
        {
            var sb = new StringBuilder();
            foreach (var method in methodDescriptions) sb.AppendLine(method.GetDefinition() + '\n');
            return sb.ToString();
        }

        private static string GetPrivateMembers(IEnumerable<MemberDescription> members)
        {
            var sb = new StringBuilder();

            foreach (var memberDescription in members) sb.AppendLine(memberDescription + ";");

            return sb.ToString();
        }

        private static string GetPrivateMethodDeclarations(IEnumerable<MethodDescription> methods)
        {
            var sb = new StringBuilder();
            foreach (var methodDescription in methods) sb.AppendLine(methodDescription.GetDeclaration(false) + ";");
            return sb.ToString();
        }

        private static string GetPublicHeaderItems(IEnumerable<MethodDescription> methods,
            IEnumerable<MemberDescription> members)
        {
            var sb = new StringBuilder();
            foreach (var methodDescription in methods) sb.AppendLine(methodDescription.GetDeclaration(false) + ";");

            foreach (var memberDescription in members) sb.AppendLine(memberDescription + ";");

            return sb.ToString();
        }

        private static string GetIncludeString(IEnumerable<string> includes)
        {
            var enumerable = includes as string[] ?? includes.ToArray();
            return enumerable.Any() ? string.Join('\n', enumerable.Select(i => $"#include<{i}>")) : "";
        }

        private static (string header, string definition) GenerateClassTemplate(string className,
            string classNamespace = null)
        {
            var header = GenerateHeader(className, classNamespace);
            var definition = GenerateDefinition(className, classNamespace);
            return (header, definition);
        }


        private static string GenerateDefinition(string className, string classNamespace)
        {
            var res = ReadEmbeddedResource("definition_template.txt");
            var classNameLower = className.ToLower(CultureInfo.InvariantCulture);
            res = res.Replace(ClassNameLowerTag, classNameLower);

            var useNamespace = classNamespace == null ? "" : $"using namespace {classNamespace};";

            res = res.Replace(HeaderNamespaceTag, useNamespace);

            return res;
        }

        private static string GenerateHeader(string className, string classNamespace)
        {
            var res = ReadEmbeddedResource("header_template.txt");
            var classNameUpper = className.ToUpper(CultureInfo.InvariantCulture);
            res = res.Replace(ClassNameUpperTag, classNameUpper);
            res = res.Replace(ClassNameTag, className);
            var namespaceStart = classNamespace ?? "";
            var namespaceEnd = classNamespace == null ? "" : "}";

            res = res.Replace(NamespaceStartTag, namespaceStart)
                .Replace(NamespaceEndTag, namespaceEnd);

            return res;
        }

        private static string ReadEmbeddedResource(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null) return null;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}