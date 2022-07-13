namespace OldJsonFormatParser
{
    class Program
    {
        public static void Main(string[] args)
        {
            var parser = new OldJsonParser(@"jsonOld/stagegirls.json");
            parser.ParseActDescriptions();
            //OldJsonParser.MapNewIcons();
        }
    }
}