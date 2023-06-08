using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace SqlServerToSqlServer
{
    internal class Runner
    {
        private readonly MapperConfig _mapperConfig;
        public Runner(MapperConfig mapperConfig)
        {
            _mapperConfig = mapperConfig;
        }
        public async Task Run()
        {
            try
            {
                Log($"Start");
                var count = 0;

                using var sourceConnection = new SqlConnection(_mapperConfig.SourceConnectionString);
                await sourceConnection.OpenAsync();
                using var sourceCommand = sourceConnection.CreateCommand();
                sourceCommand.CommandType = CommandType.Text;
                sourceCommand.CommandText = $"SELECT {string.Join(",", _mapperConfig.MapperName.Select(s => s.SourceName))} FROM {_mapperConfig.SourceTableName}";
                using var sourceReader = await sourceCommand.ExecuteReaderAsync();

                using var targetDataTable = new DataTable();
                _mapperConfig.MapperName.ForEach(f => targetDataTable.Columns.Add(f.TargetName));

                using var targetConnection = new SqlConnection(_mapperConfig.TargetConnectionString);
                await targetConnection.OpenAsync();
                using var targetSqlBulkCopy = new SqlBulkCopy(targetConnection)
                {
                    BatchSize = _mapperConfig.BatchSize,
                    NotifyAfter = _mapperConfig.ReadSize,
                    BulkCopyTimeout = 0,
                    DestinationTableName = $"dbo.{_mapperConfig.TargetTableName}",
                };
                targetSqlBulkCopy.SqlRowsCopied += (sender, args) =>
                {
                    Log($"Write[{args.RowsCopied}]");
                };
                foreach (DataColumn dataColumn in targetDataTable.Columns)
                {
                    targetSqlBulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
                }
                while (await sourceReader.ReadAsync())
                {
                    if (targetDataTable.Rows.Count == _mapperConfig.ReadSize)
                    {
                        count += targetDataTable.Rows.Count;
                        await targetSqlBulkCopy.WriteToServerAsync(targetDataTable);
                        targetDataTable.Rows.Clear();
                    }
                    var targetDataRow = targetDataTable.NewRow();
                    _mapperConfig.MapperName.ForEach(f =>
                    {
                        if (!sourceReader.IsDBNull(f.SourceName))
                        {
                            // MapperType
                            targetDataRow[f.SourceName] = MapperValue(sourceReader, f);
                        }
                    });
                    targetDataTable.Rows.Add(targetDataRow);
                }
                if (targetDataTable.Rows.Count > 0)
                {
                    count += targetDataTable.Rows.Count;
                    await targetSqlBulkCopy.WriteToServerAsync(targetDataTable);
                    targetDataTable.Rows.Clear();
                }
                Log($"Stop");
                Log($"Complete[{count}]");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{_mapperConfig.SourceTableName}][{_mapperConfig.TargetTableName}]:{e}");
                File.AppendAllText($"{_mapperConfig.Output}.error", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]:{e}\n");
            }
        }
        private object MapperValue(SqlDataReader reader, MapperName mapper)
        {
            switch (mapper.MapperType)
            {
                case MapperType.None:
                    {
                        return reader.GetValue(mapper.TargetName);
                    }
                case MapperType.RocDateStringToAdDateTime:
                    {
                        var value = reader.GetString(mapper.TargetName);
                        var datetimeInt = Convert.ToInt32(value) + 19110000;
                        return DateTime.Parse(datetimeInt.ToString("0000-00-00"));
                    }
                default:
                    {
                        return reader.GetValue(mapper.TargetName);
                    }
            }
        }
        private void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}][{_mapperConfig.SourceTableName}][{_mapperConfig.TargetTableName}]:{message}");
            File.AppendAllText($"{_mapperConfig.Output}.log", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]:{message}\n");
        }
    }
}
