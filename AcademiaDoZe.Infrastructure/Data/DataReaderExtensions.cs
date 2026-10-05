using System;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure
{
    public static class DataReaderExtensions
    {
        public static T GetValueOrDefault<T>(this DbDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
            {
                return default!;
            }
            return (T)Convert.ChangeType(reader.GetValue(ordinal), typeof(T));
        }
    }
}