using System;

namespace SqlServerToSqlServer
{
    internal class MapperConfig
    {
        public string SourceTableName { get; set; } = null!;
        public string TargetTableName { get; set; } = null!;
        public List<MapperName> MapperName { get; set; } = null!;
    }
}
