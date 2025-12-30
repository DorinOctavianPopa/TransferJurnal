using TransferJurnal;

// Example usage of SqlCommandExecutor
// This file demonstrates how to use the SQL command executor in your code

// Example 1: Execute a simple query without parameters
// var dataTable = await executor.ExecuteQueryAsync("GetAllRecords");

// Example 2: Execute a query with parameters
// var parameters = new Dictionary<string, object>
// {
//     { "@Id", 123 }
// };
// var result = await executor.ExecuteQueryAsync("GetRecordById", parameters);

// Example 3: Execute an INSERT/UPDATE command
// var insertParams = new Dictionary<string, object>
// {
//     { "@Value1", "Sample Text" },
//     { "@Value2", "More Text" }
// };
// var rowsAffected = await executor.ExecuteNonQueryAsync("InsertRecord", insertParams);

// Example 4: Execute an UPDATE command
// var updateParams = new Dictionary<string, object>
// {
//     { "@Value1", "Updated Value" },
//     { "@Id", 123 }
// };
// await executor.ExecuteNonQueryAsync("UpdateRecord", updateParams);

// Example 5: Execute a stored procedure
// var spParams = new Dictionary<string, object>
// {
//     { "@Param1", "Value" },
//     { "@Param2", 456 }
// };
// await executor.ExecuteNonQueryAsync("ExecuteStoredProcedure", spParams);

// Example 6: Handle null values
// var paramsWithNull = new Dictionary<string, object>
// {
//     { "@Value1", null! },  // Will be converted to DBNull.Value
//     { "@Id", 123 }
// };
// await executor.ExecuteNonQueryAsync("UpdateRecord", paramsWithNull);
