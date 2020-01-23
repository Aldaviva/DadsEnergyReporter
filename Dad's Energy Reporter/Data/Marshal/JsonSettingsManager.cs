using System;
using System.IO;
using Newtonsoft.Json;

namespace DadsEnergyReporter.Data.Marshal {

    public class JsonSettingsManager<T> where T: Validatable, new() {

        private readonly JsonSerializer serializer = new JsonSerializer {
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        private T _cache;
        private string _filename;

        internal string filename {
            get => _filename;
            set => _filename = Environment.ExpandEnvironmentVariables(value);
        }

        public T get() {
            if (_cache == null) {
                _cache = new T();
                reload(_cache);
            }

            return _cache;
        }

        public void reload(T settings) {
            try {
                using var jsonTextReader = new JsonTextReader(File.OpenText(filename));
                serializer.Populate(jsonTextReader, settings);
//                settings.Validate();
            } catch (FileNotFoundException) {
                JsonConvert.PopulateObject("{}", settings);
            }
        }

        public void save(T settings) {
            using var jsonTextWriter = new JsonTextWriter(File.CreateText(filename));
            serializer.Serialize(jsonTextWriter, settings);
        }

    }

}