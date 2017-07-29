namespace Adfc.Msbuild
{
    public class BuildError
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public string FileName { get; set; }

        public int? LineNumber { get; set; }

        public int? LinePosition { get; set; }
    }
}
