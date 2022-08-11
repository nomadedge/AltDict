namespace AltDict.Data.Dtos
{
    public class SearchDto
    {
        public string VendorCode1 { get; set; }
        public string Manufacturer1 { get; set; }
        public string VendorCode2 { get; set; }
        public string Manufacturer2 { get; set; }
        public byte SearchDepth { get; set; }
    }
}
