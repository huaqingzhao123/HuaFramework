using System;
using System.Collections.Generic;

namespace Nireus
{
	public class ConfigManager
	{
		static private ConfigManager _instance;
		static public ConfigManager getInstance() { return _instance == null ? _instance = new ConfigManager() : _instance; }

		Dictionary<string, DataTable> _data_table_map = new Dictionary<string, DataTable>();
		Dictionary<string, Dictionary<int, int>> _key_row_maps = new Dictionary<string, Dictionary<int, int>>();


		public string getValue(string config_name, string field, int key)
		{
			DataRow row = getDataRow(config_name, key);
			return row.getString(field);
		}

		public DataRow getDataRow(string config_name, int key)
		{
			DataTable table = getConfigTable(config_name);
			int row_index = findLineByKey(table, config_name, key);
			if (row_index >= 0)
			{
				return table.getDataRow(row_index);
			}
			return null;
		}

		public DataTable getConfigTable(string config_name, bool reload = false)
		{
			DataTable table;
			if (_data_table_map.TryGetValue(config_name, out table))
            {
                if (reload)
                {
                    _data_table_map.Remove(config_name);
                }
                else
                {
                    return table;
                }
            }

			table = CSVService.getInstance().FetchRows(config_name);
			_data_table_map.Add(config_name, table);

			return table;
		}

		private int findLineByKey(DataTable table, string config_name, int key)
		{
			Dictionary<int, int> key_row_map = getKeyRowMap(table, config_name);
			int row_index;
			if (key_row_map.TryGetValue(key, out row_index)) return row_index;
			return -1;	
		}

		private Dictionary<int, int> getKeyRowMap(DataTable table, string config_name)
		{
			Dictionary<int, int> key_row_map;
			if (_key_row_maps.TryGetValue(config_name, out key_row_map)) return key_row_map;

			key_row_map = new Dictionary<int, int>();
			for (int i = 0; i < table.getRowCount(); ++i)
			{
				int key = table.getDataRow(i).getInt32(0);
				if (key_row_map.ContainsKey(key) == false)
				{
					key_row_map.Add(key, i);
				}
			}
			_key_row_maps.Add(config_name, key_row_map);
			return key_row_map;
		}
	}
}
