using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nireus
{
    public interface IConfig
    {
        int cfg_id { get; }
        void fromRow(DataRow row);
    }

    public class GameConfigManager<ConfigBase> where ConfigBase : class, IConfig, new()
    {
        public Dictionary<int, ConfigBase> dic_config { get; protected set; } = new Dictionary<int, ConfigBase>();

        public GameConfigManager()
        {

        }

        public GameConfigManager(String path)
        {
            _addConfigs(path);
        }

        protected void _addConfigs(String path)
        {
            _addConfigs<ConfigBase>(path);
        }

        protected void _addConfigs<Config>(String path) where Config : class, ConfigBase,new()
        {
            var table = ConfigManager.getInstance().getConfigTable(path);

            var count = table.getRowCount();

            if (count <= 0)
            {
                GameDebug.LogErrorFormat("GameConfigManager table count <= 0, path: {0}", path);
            }

            for (var i = 0; i < count; i++)
            {
                var row = table.getDataRow(i);

                if (row == null)
                {
                    continue;
                }
                var cfg = new Config();

                try
                {
                    cfg.fromRow(row);
                }
                catch (Exception e)
                {
                    GameDebug.LogErrorFormat("Table name:{0}, error:{1}", path, e.Message);
                }

                dic_config[cfg.cfg_id] = cfg;

                initializeConfig(cfg);
            }
        }

        protected virtual void initializeConfig(ConfigBase cfg)
        {

        }


        public ConfigBase getConfig(int cfg_id)
        {
            return getConfig<ConfigBase>(cfg_id);
        }

        public Config getConfig<Config>(int cfg_id) where Config : class, ConfigBase
        {
            ConfigBase config;

            var success = dic_config.TryGetValue(cfg_id, out config);

            if (success == false)
            {
                return default(Config);
            }
            return config as Config;
        }

        public List<ConfigBase> getAllConfig()
        {
            if (dic_config == null) return new List<ConfigBase>();
            return dic_config.Values.ToList();
        }
    }

    public interface IGameConfig
    {
        void FromRow(DataRow row);
    }

    public class SingletonGameConfigBase
    {
        protected string _cfg_path;
        protected bool _cfg_loaded = false;
        public SingletonGameConfigBase(string cfg_path)
        {
            _cfg_path = cfg_path;
            Load(false);
        }

        public void Load(bool reload = false)
        {
            if (_cfg_loaded && reload == false) return;
            lock (this)
            {
                if (_cfg_loaded && reload == false) return;
                DataTable table = CSVService.getInstance().FetchRows(_cfg_path, null);
                int rowCount = table.getRowCount();
                if(table.getRowCount() > 0)
                {
                    FromRow(table.getDataRow(0));
                }
                _cfg_loaded = true;
                OnConfigLoaded();
            }
        }

        protected virtual void FromRow(DataRow row)
        {

        }

        protected virtual void OnConfigLoaded()
        {

        }
    }

    public class GameConfigManager<KType, VType> where VType : IGameConfig, new()
    {
        protected string _cfg_path;
        protected Dictionary<KType, VType> _config_map;
        public Dictionary<KType, VType> ConfigMap { get { return _config_map; } }

        public GameConfigManager(string cfg_path)
        {
            _cfg_path = cfg_path;
            Load(false);
        }
        public VType getConfig(KType key) { return GetConfig(key); }
        public VType GetConfig(KType key)
        {
            VType config;
            if (_config_map.TryGetValue(key, out config) == false)
            {
                return default(VType);
            }
            return config;
        }
        public void Load(bool reload = false)
        {
            if (_config_map != null && reload == false) return;
            lock (this)
            {
                if (_config_map != null && reload == false) return;
                _config_map = new Dictionary<KType, VType>();
                DataTable table = CSVService.getInstance().FetchRows(_cfg_path, null);
                int rowCount = table.getRowCount();
                Type key_type = typeof(KType);
                key_type = Nullable.GetUnderlyingType(key_type) ?? key_type;
                for (int i = 0; i < rowCount; ++i)
                {
                    DataRow row = table.getDataRow(i);
                    VType config = new VType();
                    config.FromRow(row);
                    KType key = (KType)Convert.ChangeType(row.getString(0), key_type);
                    try
                    {
                        _config_map.Add(key, config);
                    }
                    catch
                    {
                        GameDebug.Log(key);
                        throw;
                    }
                    OnConfigLoaded(key, config);
                }
                OnConfigAllLoaded();
            }
        }

        protected virtual void OnConfigLoaded(KType key, VType config)
        {

        }

        protected virtual void OnConfigAllLoaded()
        {

        }
    }
}
