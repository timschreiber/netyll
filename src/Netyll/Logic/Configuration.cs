using Netyll.Logic.Extensions;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;


namespace Netyll.Logic
{
    public interface IConfiguration
    {
        object this[string key] { get; }

        bool ContainsKey(string key);

        bool TryGetValue(string key, out object value);

        IDictionary<string, object> ToDictionary();

        IDefaultsConfiguration Defaults { get; }

        void ReadFromFile(IDirectoryInfo path);
    }

    internal sealed class Configuration : IConfiguration
    {
        private const string CONFIG_FILENAME = "_config.yml";
        public const string DEFAULT_PERMALINK = "date";

        private IDictionary<string, object> _config;
        private readonly IFileSystem _fileSystem;
        public object this[string key] => _config[key];

        public IDefaultsConfiguration Defaults { get; private set; }

        internal Configuration()
        {
            _config = new Dictionary<string, object>();
            EnsureDefaults();
        }

        public Configuration(IFileSystem fileSystem)
            : this()
        {
            _fileSystem = fileSystem;
        }

        private void EnsureDefaults()
        {
            if (!_config.ContainsKey("permalink"))
            {
                _config.Add("permalink", DEFAULT_PERMALINK);
            }

            Defaults = new DefaultsConfiguration(_config);
        }

        public void ReadFromFile(IDirectoryInfo path)
        {
            var configFilePath = _fileSystem.Path.Combine(path.FullName, CONFIG_FILENAME);
            _config = new Dictionary<string, object>();
            if (_fileSystem.File.Exists(configFilePath))
            {
                _config = _fileSystem.File.ReadAllText(configFilePath).ParseYaml();
                EnsureDefaults();
            }
        }

        public bool ContainsKey(string key)
        {
            return _config.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _config.TryGetValue(key, out value);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>(_config);
        }
    }
}
