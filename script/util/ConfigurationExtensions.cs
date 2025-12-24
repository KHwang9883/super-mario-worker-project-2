using System.Collections.Generic;

namespace SMWP.Util;

public class ConfigurationExtensions {
    public static int GetIntValueOrDefault(Dictionary<string, string> config, string key, int defaultValue = 0) {
        config.TryGetValue(key, out string? value);
        if (string.IsNullOrEmpty(value))
            return defaultValue;
        
        return int.TryParse(value, out int result) ? result : defaultValue;
    }
}
