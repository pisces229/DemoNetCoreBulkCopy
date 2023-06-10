using System;

namespace SqlServerToSqlServer
{
    internal class AppOption
    {
        public static string Section = "App";
        public string Input { get; set; } = null!;
        public string Output { get; set; } = null!;
        public int ReadSize { get; set; }
        public int BatchSize { get; set; }
        public string SourceConnectionString { get; set; } = null!;
        public string TargetConnectionString { get; set; } = null!;
    }
}
