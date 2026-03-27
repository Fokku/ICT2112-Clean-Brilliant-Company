using System;
using System.Collections.Generic;

namespace CleanBrilliant.Data
{
    public class RecordSet
    {
        public List<string> Columns { get; set; } = new List<string>();
        public List<Dictionary<string, object>> Rows { get; set; } = new List<Dictionary<string, object>>();

        public bool HasRows => Rows.Count > 0;

        public object GetValue(int rowIndex, string columnName)
        {
            if (rowIndex >= 0 && rowIndex < Rows.Count && Rows[rowIndex].ContainsKey(columnName))
            {
                return Rows[rowIndex][columnName];
            }
            return null;
        }
    }
}