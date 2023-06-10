using Microsoft.Data.SqlClient;
using System.Data;

namespace SqlServerToSqlServer
{
    internal class Utility
    {
        public static Task Log(string file, string name, string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{name}]{message}");
            return File.AppendAllTextAsync(file, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{message}\n");
        }
        public static object MapperValue(SqlDataReader reader, MapperName mapper)
        {
            switch (mapper.MapperType)
            {
                case MapperType.None:
                    {
                        return reader.GetValue(mapper.SourceName);
                    }
                case MapperType.RocDateStringToAdDateTime:
                    {
                        var value = reader.GetString(mapper.SourceName);
                        var datetimeInt = Convert.ToInt32(value) + 19110000;
                        return DateTime.Parse(datetimeInt.ToString("0000-00-00"));
                    }
                default:
                    {
                        return reader.GetValue(mapper.SourceName);
                    }
            }
        }
    }
}
