using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nireus
{
	public class DataTable
	{
		private List<string> _fileds = new List<string>();
		private List<DataRow> _data_rows = new List<DataRow>();

		public void setFileds(List<string> fields) { _fileds = fields; }

		public void insertRow(DataRow data_row)
		{
			data_row.setFields(_fileds);
			_data_rows.Add(data_row);
		}

		public DataRow getDataRow(int index)
		{
			return _data_rows[index];
		}

		public int getRowCount()
		{
			return _data_rows.Count;
		}
	}
}
