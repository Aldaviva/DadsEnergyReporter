using System;
using System.IO;
using Newtonsoft.Json;

namespace DadsEnergyReporter.Data.Marshal
{
    public class JsonSettingsManager<T> where T : Validatable, new()
    {
        private readonly JsonSerializer serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        private T _cache;
        private string filename;

        internal string Filename
        {
            get => filename;
            set => filename = Environment.ExpandEnvironmentVariables(value);
        }

        public T Get()
        {
            if (_cache == null)
            {
                _cache = new T();
                Reload(_cache);
            }

            return _cache;
        }

        public void Reload(T settings)
        {
            try
            {
                using (var jsonTextReader = new JsonTextReader(File.OpenText(Filename)))
                {
                    serializer.Populate(jsonTextReader, settings);
//                settings.Validate();
                }
            }
            catch (FileNotFoundException)
            {
                JsonConvert.PopulateObject("{}", settings);
            }
        }

        public void Save(T settings)
        {
            using (var jsonTextWriter = new JsonTextWriter(File.CreateText(Filename)))
            {
                serializer.Serialize(jsonTextWriter, settings);
            }
        }
    }
}