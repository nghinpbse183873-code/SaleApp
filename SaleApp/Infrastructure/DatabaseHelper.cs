using Microsoft.Data.SqlClient;

namespace SaleApp.Infrastructure;

public static class DatabaseHelper
{
    public static SqlConnection GetConnection()
        => new(AppSettings.ConnectionString);

    /// <summary>Execute a query and map results with the provided mapper.</summary>
    public static List<T> ExecuteQuery<T>(string sql,
        Action<SqlParameterCollection>? paramBuilder,
        Func<SqlDataReader, T> mapper)
    {
        var result = new List<T>();
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        paramBuilder?.Invoke(cmd.Parameters);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(mapper(reader));
        return result;
    }

    /// <summary>Execute non-query (INSERT/UPDATE/DELETE) and return rows affected.</summary>
    public static int ExecuteNonQuery(string sql,
        Action<SqlParameterCollection>? paramBuilder = null)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        paramBuilder?.Invoke(cmd.Parameters);
        return cmd.ExecuteNonQuery();
    }

    /// <summary>Execute scalar and return single value.</summary>
    public static object? ExecuteScalar(string sql,
        Action<SqlParameterCollection>? paramBuilder = null)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        paramBuilder?.Invoke(cmd.Parameters);
        return cmd.ExecuteScalar();
    }

    /// <summary>Execute a stored procedure and map results.</summary>
    public static List<T> ExecuteStoredProcedure<T>(string procName,
        Action<SqlParameterCollection>? paramBuilder,
        Func<SqlDataReader, T> mapper)
    {
        var result = new List<T>();
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(procName, conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        paramBuilder?.Invoke(cmd.Parameters);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(mapper(reader));
        return result;
    }

    /// <summary>Execute a stored procedure that returns multiple result sets.</summary>
    public static void ExecuteStoredProcedureMultiResult(string procName,
        Action<SqlParameterCollection>? paramBuilder,
        params Action<SqlDataReader>[] resultHandlers)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(procName, conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        paramBuilder?.Invoke(cmd.Parameters);
        using var reader = cmd.ExecuteReader();

        for (int i = 0; i < resultHandlers.Length; i++)
        {
            while (reader.Read())
                resultHandlers[i](reader);

            if (i < resultHandlers.Length - 1)
                reader.NextResult();
        }
    }

    /// <summary>Safe reader helper – returns default if column is DBNull.</summary>
    public static T? SafeGet<T>(this SqlDataReader reader, string column)
    {
        int ordinal;
        try { ordinal = reader.GetOrdinal(column); }
        catch { return default; }

        if (reader.IsDBNull(ordinal)) return default;

        var val = reader.GetValue(ordinal);
        if (val is T exactVal) return exactVal;

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        try
        {
            return (T)Convert.ChangeType(val, targetType);
        }
        catch
        {
            return (T)val;
        }
    }

    public static string SafeGetString(this SqlDataReader reader, string column)
        => reader.SafeGet<string>(column) ?? string.Empty;

    public static int SafeGetInt(this SqlDataReader reader, string column)
        => reader.SafeGet<int?>(column) ?? 0;

    public static decimal SafeGetDecimal(this SqlDataReader reader, string column)
        => reader.SafeGet<decimal?>(column) ?? 0m;

    public static bool SafeGetBool(this SqlDataReader reader, string column)
        => reader.SafeGet<bool?>(column) ?? false;

    public static DateTime? SafeGetDateTime(this SqlDataReader reader, string column)
        => reader.SafeGet<DateTime?>(column);
}
