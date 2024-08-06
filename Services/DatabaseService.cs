using MySql.Data.MySqlClient;
using skill_composer.Helper;
using skill_composer.Models; 
using System.Data; 
using System.Reflection;
using Task = System.Threading.Tasks.Task;

namespace skill_composer.Services
{
    /// <summary>
    /// Supports database access.
    /// </summary>
    public class DatabaseService  
    {
        private readonly string _connection;
        private readonly string _connectionNoCredentials;
        private readonly bool _enableDebug = true; 

        public const string OutputParameter = "|OUTPUT|";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="sharedSettings"></param>
        public DatabaseService(Settings sharedSettings)
        {
            _connection = sharedSettings.Databases.First().ConnectionString;
            _connectionNoCredentials = _connection;
        }
        /// <inheritdoc />
        public int GetInteger(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName)) return 0;
            var success = int.TryParse(row[columnName].ToString(), out var val);
            if (success) return val;
            else return 0;
        }

        private static string LoggingFormatSqlCommand(MySqlCommand cmd)
        {
            // Start with the CALL statement and the stored procedure name
            var executableCommand = $"CALL {cmd.CommandText}(";

            // Prepare to collect parameter values for the CALL
            var parameterValues = new List<string>();
            // Prepare to collect parameter names and values for commenting
            var parameterComments = new List<string>();

            foreach (MySqlParameter p in cmd.Parameters)
            {
                // Format the value for the CALL
                string valueFormatted = p.Value switch
                {
                    null or DBNull => "NULL",
                    string or DateTime => $"'{p.Value}'",
                    _ => p.Value.ToString()
                };
                parameterValues.Add(valueFormatted);

                // Add the parameter name and value for the comment
                string valueForComment = p.Value switch
                {
                    null or DBNull => "NULL",
                    string or DateTime => $"'{p.Value}'",
                    _ => p.Value.ToString()
                };
                parameterComments.Add($"{p.ParameterName} = {valueForComment}");
            }

            // Join all parameter values with commas for the CALL
            executableCommand += string.Join(", ", parameterValues) + ");\n--";

            // Join all parameter comments with newlines for clarity
            executableCommand += string.Join(", ", parameterComments);

            return executableCommand;
        }

        public async Task<DataTable> ExecuteCommand(string storedProcedure)
        {
            var parameters = new Dictionary<string, string>();
            return await ExecuteCommand(storedProcedure, parameters);
        }

        /// <inheritdoc />
        public async Task<DataTable> ExecuteCommand(string storedProcedure, Dictionary<string, string> parameters, int timeout = 10)
        {
            var lastErrorMessage = "";           

            for (var i = 0; i <= 4; i++)
            {
                var delayInSeconds = ((1d / 2d) * (Math.Pow(2d, i + 1) - 1d)); // exponential back off: 0s, 0.5s, 1.5s, 3.5s, 7.5s, 15.5s
                var delayMs = (int)(delayInSeconds * 1000);
                if(i != 0) LogHelper.Log($"INFO Waiting {delayMs}ms before trying to call db again storedProcedure: {storedProcedure} {lastErrorMessage}", LogType.Database);

                await using (var conn = new MySqlConnection(_connection))
                {
                    await using var cmd = new MySqlCommand(storedProcedure, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = timeout;
                    AddParameters(cmd, parameters);

                    var outputParameters = new Dictionary<string, string>();
                    if(parameters!=null) outputParameters = parameters.Where(parameter => parameter.Value == OutputParameter).ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

                    var sqlLog = LoggingFormatSqlCommand(cmd);
                    
                    var executionStartDateTime = DateTime.Now;

                    try
                    {
                        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                        var sqlDataAdapter = new MySqlDataAdapter(cmd);
                        var data = new DataTable();
                        await sqlDataAdapter.FillAsync(data);

                        if (outputParameters is { Count: > 0 }) // set output parameter values
                        {
                            foreach (var outputParameter in outputParameters)
                            {
                                if (parameters != null) parameters[outputParameter.Key] = cmd.Parameters[outputParameter.Key].Value.ToString();
                            }
                        }

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                        if (data.HasErrors)
                        {
                            LogHelper.Log($"ERROR {sqlLog} total execution time: {totalTime.TotalSeconds}", LogType.Database);
                        }
                        else
                        {
                            LogHelper.Log($"{sqlLog} total execution time: {totalTime.TotalSeconds}", LogType.Database);
                        }

                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();
                        return data;
                    }
                    catch (ArgumentException ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);
                                                
                        LogHelper.Log($"ERROR {ex.Message} DatabaseService.Execute connection: {_connectionNoCredentials} {sqlLog}, total execution time: {totalTime.TotalSeconds}", LogType.Database);

                        return null;
                    }
                    catch (Exception ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();
                            
                        lastErrorMessage = $", {ex.Message}";

                        if (i == 4) // only log error on last attempt in case this is a deadlock
                        {
                            var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                            LogHelper.Log($"ERROR {sqlLog}, total execution time: {totalTime.TotalSeconds}, retry times: {i}, combined waiting time: {delayMs}ms", LogType.General);
                        }

                        if (_enableDebug)
                        {
                            var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                            LogHelper.Log($"ERROR {sqlLog}, total execution time: {totalTime.TotalSeconds}, retry times: {i}, combined waiting time: {delayMs}ms", LogType.Database);

                            throw; // Rethrow the original exception preserving the stack trace
                        }
                    }
                }                
                await Task.Delay(delayMs);
            }
            return null;
        }

        /// <inheritdoc />
        public async Task<DataSet> Execute(string storedProcedure, Dictionary<string, string> parameters, int timeout = 10)
        {
            var lastErrorMessage = "";

            for (var i = 0; i <= 4; i++)
            {
                var delayInSeconds = ((1d / 2d) * (Math.Pow(2d, i + 1) - 1d)); // exponential back off: 0s, 0.5s, 1.5s, 3.5s, 7.5s, 15.5s
                var delayMs = (int)(delayInSeconds * 1000);
                if (i != 0) LogHelper.Log($"INFO Waiting {delayMs}ms before trying to call db again storedProcedure: {storedProcedure}{lastErrorMessage}", LogType.Database);

                await using (var conn = new MySqlConnection(_connection))
                {
                    await using var cmd = new MySqlCommand(storedProcedure, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = timeout;
                    AddParameters(cmd, parameters);

                    var sqlLog = LoggingFormatSqlCommand(cmd);

                    var executionStartDateTime = DateTime.Now;

                    try
                    {
                        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                        var sqlDataAdapter = new MySqlDataAdapter(cmd);
                        var data = new DataSet();
                        await sqlDataAdapter.FillAsync(data);

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                        if (data.HasErrors)
                        {
                            LogHelper.Error($"{sqlLog} total execution time: {totalTime.TotalSeconds}", LogType.Database);
                        }
                        else
                        {
                            LogHelper.Log($"{sqlLog} total execution time: {totalTime.TotalSeconds}", LogType.Database);
                        }

                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();
                        return data;
                    }
                    catch (ArgumentException ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                        LogHelper.Log($"ERROR {ex.Message} DatabaseService.Execute connection: {_connectionNoCredentials} {sqlLog}, total execution time: {totalTime.TotalSeconds}", LogType.Database);

                        return null;
                    }
                    catch (Exception ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        lastErrorMessage = $", {ex.Message}";

                        if (i == 4) // only log error on last attempt in case this is a deadlock
                        {
                            var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                            LogHelper.Log($"ERROR {ex.Message} DatabaseService.Execute connection: {_connectionNoCredentials} {sqlLog}, total execution time: {totalTime.TotalSeconds}, retry times: {i}, combined waiting time: {delayMs}ms", LogType.Database);
                        }

                        if (_enableDebug) throw; // Rethrow the original exception preserving the stack trace
                    }
                }                
                await Task.Delay(delayMs);
            }
            return null;
        }

        /// <inheritdoc />
        public static Dictionary<string, string> GetParameters<T>(T obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(prop => !Attribute.IsDefined(prop, typeof(IgnoreParameterAttribute)));  // Filter out properties with the IgnoreParameter attribute

            var parameters = properties.ToDictionary(prop => $"p_{prop.Name}", prop => GetValue(obj, prop));

            return parameters;
        }

        /// <summary>
        /// Gets the value from the given property and converts it to the 
        /// MySql database equivalent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        private static string GetValue<T>(T obj, PropertyInfo prop)
        {
            var value = prop.GetValue(obj, null);
            return value switch
            {
                DateTime dt => dt.ToMySqlDateTime(),
                bool boolValue => boolValue ? "1" : "0",
                _ => value?.ToString()
            };
        }

 

        /// <inheritdoc />
        public static string GetString(DataRow row, string columnName)
        {
            return !row.Table.Columns.Contains(columnName) ? "" : row[columnName].ToString();
        }

        /// <inheritdoc />
        public static bool GetBoolean(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName)) return false;

            var value = row[columnName].ToString();
            var isNumeric = int.TryParse(value, out var n);

            if (isNumeric)
            {
                return Convert.ToBoolean(n);
            }

            var success = bool.TryParse(row[columnName].ToString(), out var val);

            if (success) return val;
            else return false;
        }

        /// <inheritdoc/>
        /// <inheritdoc/>
        public async Task<bool> CheckHealthAsync()
        {
            var isConnectionValid = false;
            
            try
            {
                await using var conn = new MySqlConnection(_connection);
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                isConnectionValid = true;
                if (conn.State == ConnectionState.Open) await conn.CloseAsync();
            }
            catch (ArgumentException ex)
            {
                LogHelper.Log($"ERROR {ex.Message} DatabaseService.CheckHealthAsync", LogType.Database);
            }
            catch (MySqlException sqlEx)
            {
                isConnectionValid = false;
                LogHelper.Log($"ERROR {sqlEx.Message}  DatabaseService.CheckHealthAsync", LogType.Database);
            }

            return isConnectionValid;
        }

        public async Task<IList<T>> ExecuteQuery<T>(string query, int timeout = 10)
        {
            var lastErrorMessage = "";

            for (var i = 0; i <= 4; i++)
            {
                var delayInSeconds = ((1d / 2d) * (Math.Pow(2d, i + 1) - 1d)); // exponential backoff: 0s, 0.5s, 1.5s, 3.5s, 7.5s, 15.5s
                var delayMs = (int)(delayInSeconds * 1000);
                if (i != 0) LogHelper.Log($"INFO Waiting {delayMs}ms before trying to call db again, {query}{lastErrorMessage}", LogType.Database);

                await using (var conn = new MySqlConnection(_connection))
                {
                    await using var cmd = new MySqlCommand(query, conn);
                    var executionStartDateTime = DateTime.Now;
                     
                    try
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = timeout;

                        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                        var sqlDataAdapter = new MySqlDataAdapter(cmd);
                        var data = new DataTable();
                        await sqlDataAdapter.FillAsync(data);

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                        if (data.HasErrors)
                        {
                            LogHelper.Log($"ERROR DatabaseService.ExecuteCommand CommandText: {query}, " +
                                      $"total execution time: {totalTime.TotalSeconds}", LogType.Database);
                        }
                        else
                        {
                            LogHelper.Log($"INFO DatabaseService.ExecuteCommand CommandText: {query}, " +
                                            $"total execution time: {totalTime.TotalSeconds}", LogType.Database);
                        }

                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        if (data.Rows.Count <= 0)
                        {
                            LogHelper.Log($"WARN TaskService.ExecuteQuery: returned null object for SQL query: {query}", LogType.Database);
                            return null;
                        }

                        var mappedData = PropertyMapper.ConvertTo<T>(data.Rows);
                        return mappedData;
                    }
                    catch (ArgumentException ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                        LogHelper.Log($"ERROR {ex.Message} DatabaseService.Execute connection: {_connectionNoCredentials}" + $", query: {query}, total execution time: {totalTime.TotalSeconds}", LogType.Database);

                        return null;
                    }
                    catch (Exception ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        if (i == 4) // only log error on last attempt in case this is a deadlock
                        {
                            var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                            LogHelper.Log($"ERROR {ex.Message} DatabaseService.Execute connection: {_connectionNoCredentials}, " + $"query: {query}, total execution time: {totalTime.TotalSeconds}, retry times: {i}, combined waiting time: {delayMs}ms", LogType.Database);
                        }

                        if (_enableDebug) throw; // Rethrow the original exception preserving the stack trace
                    }
                }                
                await Task.Delay(delayMs);
            }
            return null;
        }

        /// <inheritdoc />
        public async Task<DataTable> ExecuteQuery(string query, int timeout = 10)
        {
            var lastErrorMessage = "";

            for (var i = 0; i <= 4; i++)
            {
                var delayInSeconds = ((1d / 2d) * (Math.Pow(2d, i + 1) - 1d)); // exponential backoff: 0s, 0.5s, 1.5s, 3.5s, 7.5s, 15.5s
                var delayMs = (int)(delayInSeconds * 1000);
                if (i != 0) LogHelper.Log($"INFO Waiting {delayMs}ms before trying to call db again, query: {query}{lastErrorMessage}", LogType.Database);

                await using (var conn = new MySqlConnection(_connection))
                {
                    await using var cmd = new MySqlCommand(query, conn);
                    var executionStartDateTime = DateTime.Now;                        

                    try
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = timeout;

                        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                        var sqlDataAdapter = new MySqlDataAdapter(cmd);
                        var data = new DataTable();
                        await sqlDataAdapter.FillAsync(data);

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                        if (data.HasErrors) 
                        {
                            LogHelper.Log($"ERROR DatabaseService.ExecuteCommand CommandText: {query}, " +
                                      $"total execution time: {totalTime.TotalSeconds}", LogType.Database); 
                        }
                        else
                        {
                            LogHelper.Log($"INFO DatabaseService.ExecuteCommand CommandText: {query}, " +
                                            $"total execution time: {totalTime.TotalSeconds}", LogType.Database);
                        }
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        return data;
                    }
                    catch (ArgumentException ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                        LogHelper.Log($"ERROR {ex.Message} DatabaseService.Execute connection: {_connectionNoCredentials}, " + $"query: {query}, total execution time: {totalTime.TotalSeconds}", LogType.Database);

                        return null;
                    }
                    catch (Exception ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        lastErrorMessage = $", {ex.Message}";

                        if (i == 4) // only log error on last attempt in case this is a deadlock
                        {
                            var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                            LogHelper.Log($"ERROR {ex.InnerException} DatabaseService.ExecuteCommand connection: {_connectionNoCredentials}" +
                                          $"Query: {query}, total execution time: {totalTime.TotalSeconds}, retry times: {i}, combined waiting time: {delayMs}ms", LogType.Database);
                        }

                        if (_enableDebug) throw; // Rethrow the original exception preserving the stack trace
                    }
                }                
                await Task.Delay(delayMs);
            }
            return null;
        }

        public async Task<DataSet> ExecuteQueryMultiTable(string query, int timeout = 10)
        {
            var lastErrorMessage = "";

            for (var i = 0; i <= 4; i++)
            {
                var delayInSeconds = ((1d / 2d) * (Math.Pow(2d, i + 1) - 1d)); // exponential backoff: 0s, 0.5s, 1.5s, 3.5s, 7.5s, 15.5s
                var delayMs = (int)(delayInSeconds * 1000);
                if (i != 0) LogHelper.Log($"INFO Waiting {delayMs}ms before trying to call db again, query: {query}{lastErrorMessage}", LogType.Database);

                await using (var conn = new MySqlConnection(_connection))
                {
                    await using var cmd = new MySqlCommand(query, conn);
                    var executionStartDateTime = DateTime.Now;

                    try
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = timeout;

                        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

                        var sqlDataAdapter = new MySqlDataAdapter(cmd);
                        var dataSet = new DataSet();
                        await Task.Run(() => sqlDataAdapter.Fill(dataSet)); // Filling the DataSet asynchronously

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                        if (dataSet.HasErrors)
                        {
                            LogHelper.Log($"ERROR DatabaseService.ExecuteCommand CommandText: {query}, " +
                                          $"total execution time: {totalTime.TotalSeconds}", LogType.Database);
                        }
                        else
                        {
                            LogHelper.Log($"INFO DatabaseService.ExecuteCommand CommandText: {query}, " +
                                          $"total execution time: {totalTime.TotalSeconds}", LogType.Database);
                        }
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        return dataSet;
                    }
                    catch (ArgumentException ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                        LogHelper.Log($"ERROR {ex.Message} DatabaseService.Execute connection: {_connectionNoCredentials}, " +
                                      $"query: {query}, total execution time: {totalTime.TotalSeconds}", LogType.Database);

                        return null;
                    }
                    catch (Exception ex)
                    {
                        if (conn.State == ConnectionState.Open) await conn.CloseAsync();

                        lastErrorMessage = $", {ex.Message}";

                        if (i == 4) // only log error on last attempt in case this is a deadlock
                        {
                            var (totalTime, _) = LogHelper.GetTotalExecutionTime(executionStartDateTime);

                            LogHelper.Log($"ERROR {ex.InnerException} DatabaseService.ExecuteCommand connection: {_connectionNoCredentials}" +
                                          $"Query: {query}, total execution time: {totalTime.TotalSeconds}, retry times: {i}, combined waiting time: {delayMs}ms", LogType.Database);
                        }

                        if (_enableDebug) throw; // Rethrow the original exception preserving the stack trace
                    }
                }
                await Task.Delay(delayMs);
            }
            return null;
        }



        /// <summary>
        /// Adds parameters to the Sql Command.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        private static void AddParameters(MySqlCommand cmd, Dictionary<string, string> parameters)
        {
            if ((parameters == null) || (parameters.Count <= 0)) return;
            foreach (var key in parameters.Keys)
            {
                var value = parameters[key];
                if (value == OutputParameter)
                {
                    var outputParam = new MySqlParameter(key, MySqlDbType.String)
                    {
                        Direction = ParameterDirection.Output,
                    };
                    cmd.Parameters.Add(outputParam);
                }
                else // standard input parameter
                {
                    cmd.Parameters.AddWithValue(key, value);
                }
            }
        }
    }
}
