using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;

namespace SqlServerToSqlServer
{
    internal class Runner
    {
        private readonly AppOption _appOption;
        public Runner(IOptions<AppOption> appOption)
        {
            _appOption = appOption.Value;
        }
        public async Task<long> Run(ThreadOption threadOption)
        {
            var result = 0L;
            var infoLog = $"{Path.Combine($"{_appOption.Output}", $"app.{threadOption.Name}")}.log";
            var errorLog = $"{Path.Combine($"{_appOption.Output}", $"app.{threadOption.Name}")}.error";
            await Utility.Log(infoLog, threadOption.Name, "[Start]");
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false,
            }; 
            var tasks = new List<Task<long>>();
            foreach (var file in threadOption.Files)
            {
                try
                {
                    using var streamReader = new StreamReader(Path.Combine(_appOption.Input, $"{file}.csv"));
                    using var csvReader = new CsvReader(streamReader, csvConfiguration);
                    var records = csvReader.GetRecords<MapperName>().ToList();
                    tasks.Add(Run(
                        threadOption.Name,
                        records.First().SourceName,
                        records.First().TargetName,
                        records.Skip(1).ToList()
                    ));
                }
                catch (Exception e)
                {
                    await Utility.Log(errorLog, threadOption.Name, e.ToString());
                }
            }
            Task.WaitAll(tasks.ToArray());
            foreach (var task in tasks) 
            {
                result += await task;
            }
            await Utility.Log(infoLog, threadOption.Name, $"[Complete][{result}]");
            await Utility.Log(infoLog, threadOption.Name, "[Stop]");
            return result;
        }
        private async Task<long> Run(string threadName, string sourectTable, string targetTable, List<MapperName> mapperName)
        {
            var result = 0L;
            var infoLog = $"{Path.Combine($"{_appOption.Output}", $"dbo.{targetTable}")}.log";
            var errorLog = $"{Path.Combine($"{_appOption.Output}", $"dbo.{targetTable}")}.error";
            await Utility.Log(infoLog, $"{threadName}][{sourectTable}][{targetTable}", "[Start]");
            try
            {
                using var sourceConnection = new SqlConnection(_appOption.SourceConnectionString);
                await sourceConnection.OpenAsync();
                using var sourceCommand = sourceConnection.CreateCommand();
                sourceCommand.CommandType = CommandType.Text;
                sourceCommand.CommandText = $"SELECT {string.Join(",", mapperName.Select(s => s.SourceName))} FROM {sourectTable} (NOLOCK)";
                using var sourceReader = await sourceCommand.ExecuteReaderAsync();
                using var targetDataTable = new DataTable();
                mapperName.ForEach(f => targetDataTable.Columns.Add(f.TargetName));
                using var targetConnection = new SqlConnection(_appOption.TargetConnectionString);
                await targetConnection.OpenAsync();
                using var targetSqlBulkCopy = new SqlBulkCopy(targetConnection)
                {
                    BatchSize = _appOption.BatchSize,
                    NotifyAfter = _appOption.ReadSize,
                    BulkCopyTimeout = 0,
                    DestinationTableName = $"dbo.{targetTable}",
                };
                targetSqlBulkCopy.SqlRowsCopied += async (sender, args) =>
                {
                    await Utility.Log(infoLog, $"{threadName}][{sourectTable}][{targetTable}", $"[Write][{args.RowsCopied}]");
                };
                foreach (DataColumn dataColumn in targetDataTable.Columns)
                {
                    targetSqlBulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
                }
                while (await sourceReader.ReadAsync())
                {
                    if (targetDataTable.Rows.Count == _appOption.ReadSize)
                    {
                        result += targetDataTable.Rows.Count;
                        await targetSqlBulkCopy.WriteToServerAsync(targetDataTable);
                        targetDataTable.Rows.Clear();
                    }
                    var targetDataRow = targetDataTable.NewRow();
                    mapperName.ForEach(f =>
                    {
                        if (!sourceReader.IsDBNull(f.SourceName))
                        {
                            // MapperType
                            targetDataRow[f.TargetName] = Utility.MapperValue(sourceReader, f);
                        }
                    });
                    targetDataTable.Rows.Add(targetDataRow);
                }
                if (targetDataTable.Rows.Count > 0)
                {
                    result += targetDataTable.Rows.Count;
                    await targetSqlBulkCopy.WriteToServerAsync(targetDataTable);
                    targetDataTable.Rows.Clear();
                }
                await Utility.Log(infoLog, $"{threadName}][{sourectTable}][{targetTable}", $"[Complete][{result}]");
            }
            catch (Exception e)
            {
                await Utility.Log(errorLog, $"[{threadName}][{sourectTable}][{targetTable}]", e.ToString());
            }
            await Utility.Log(infoLog, $"{threadName}][{sourectTable}][{targetTable}", "[Stop]");
            return result;
        }
    }
}
