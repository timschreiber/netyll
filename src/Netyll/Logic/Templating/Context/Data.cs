using DotLiquid;
using Netyll.Logic.Templating.Context.DataParsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Netyll.Logic.Templating.Context
{
    public class Data : Drop
    {
        private readonly IFileSystem _fileSystem;
        private readonly IDirectoryInfo _dataDirectory;
        private readonly Dictionary<string, Lazy<object>> _cachedResults = new Dictionary<string, Lazy<object>>();
        private readonly IList<IDataParser> _dataParsers;

        public Data(IFileSystem fileSystem, IDirectoryInfo dataDirectory)
        {
            _fileSystem = fileSystem;
            _dataDirectory = dataDirectory;
            _dataParsers = new List<IDataParser>
            {
                new YamlJsonDataParser(fileSystem, "yml"),
                new YamlJsonDataParser(fileSystem, "json"),
                new CsvTsvDataParser(fileSystem, "csv"),
                new CsvTsvDataParser(fileSystem, "tsv", "\t")
            };
        }

        public override object this[object method]
        {
            get
            {
                var res = base[method];
                if (res != null)
                {
                    return res;
                }

                if (!_cachedResults.ContainsKey(method.ToString()))
                {
                    var cachedResult = new Lazy<object>(() =>
                    {
                        if (!_fileSystem.Directory.Exists(_dataDirectory.FullName))
                        {
                            return null;
                        }

                        var methodName = method.ToString();
                        foreach (var dataParser in _dataParsers)
                        {
                            if (dataParser.CanParse(_dataDirectory.FullName, methodName))
                            {
                                return dataParser.Parse(_dataDirectory.FullName, methodName);
                            }
                        }

                        var subFolder = Path.Combine(_dataDirectory.FullName, method.ToString());
                        if (_fileSystem.Directory.Exists(subFolder))
                        {
                            return new Data(_fileSystem, _fileSystem.DirectoryInfo.FromDirectoryName(subFolder));
                        }

                        return null;
                    });
                    _cachedResults[method.ToString()] = cachedResult;
                    return cachedResult.Value;
                }

                return _cachedResults[method.ToString()].Value;
            }
        }
    }
}
