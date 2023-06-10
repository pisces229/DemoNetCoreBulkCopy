using System;

namespace SqlServerToSqlServer
{
    internal class ThreadOption
    {
        public static string Section = "Thread";
        public string Name { get; set; } = null!;
        public string[] Files { get; set; } = null!;
    }
}
