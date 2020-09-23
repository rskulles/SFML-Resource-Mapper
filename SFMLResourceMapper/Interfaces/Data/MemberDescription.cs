namespace RES.ResMap.Interfaces.Data
{
    public class MemberDescription
    {
        public AccessLevelType AccessLevel { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }
}