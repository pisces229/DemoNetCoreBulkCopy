using System;

namespace SqlServerToSqlServer
{
    internal class MapperConfig
    {
        public int ReadSize { get; set; }
        public int BatchSize { get; set; }
        public string SourceConnectionString { get; set; } = null!;
        public string TargetConnectionString { get; set; } = null!;
        public string SourceTableName { get; set; } = null!;
        public string TargetTableName { get; set; } = null!;
        public List<MapperName> MapperName { get; set; } = null!;
        public string Output { get; set; } = null!;
    }
}
