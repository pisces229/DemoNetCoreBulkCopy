using CsvHelper.Configuration.Attributes;

namespace SqlServerToSqlServer
{
    internal class MapperName
    {
        [Index(0)]
        public string SourceName { get; set; } = null!;
        [Index(1)]
        public string TargetName { get; set; } = null!;
        [Index(2)]
        public MapperType MapperType { get; set; }
    }
}
