using System.Data;

namespace CleanBrilliant.DataSource
{
    public class RecordSet
    {
        private readonly DataTable _dataTable;

        public RecordSet() => _dataTable = new DataTable();
        public RecordSet(DataTable dataTable) => _dataTable = dataTable;

        public DataTable GetDataTable() => _dataTable;
        public int RowCount => _dataTable.Rows.Count;

        public DataRow? GetRow(int index)
        {
            if (index >= 0 && index < _dataTable.Rows.Count)
                return _dataTable.Rows[index];
            return null;
        }

        public List<DataRow> GetAllRows() => _dataTable.Rows.Cast<DataRow>().ToList();
    }
}
