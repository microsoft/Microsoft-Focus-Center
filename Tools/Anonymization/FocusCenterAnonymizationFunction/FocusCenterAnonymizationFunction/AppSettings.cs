using System;
using System.ComponentModel;

namespace AnonymizationFunctionApp
{
    /// <summary>
    /// AppSetting Class for setup Azure Configuration environment Variables.(Same can be setup from Azure Portal)
    /// </summary>
    public static class AppSettings
    {
        public static string CrmUrl => Environment.GetEnvironmentVariable("CrmUrl");
        public static string CrmAppClientId => Environment.GetEnvironmentVariable("CrmAppClientId");
        public static string CrmAppClientSecret => Environment.GetEnvironmentVariable("CrmAppClientSecret");
        public static TimeSpan CrmConnectionTimeout => Get<TimeSpan>(Environment.GetEnvironmentVariable("CrmConnectionTimeout"), TimeSpan.FromMinutes(10));
        public static int CountPerPage => Get<int>(Environment.GetEnvironmentVariable("CountPerPage"), 500);
        public static int CrmMaxParallelConnections => Get<int>(Environment.GetEnvironmentVariable("CrmMaxParallelConnections"), 10);
        public static int CrmMaxRequestsCountInBulk => Get<int>(Environment.GetEnvironmentVariable("CrmMaxRequestsCountInBulk"), 400);
        public static bool SkipInsert => Get<bool>(Environment.GetEnvironmentVariable("SkipInsert"),false);
        public static bool SkipUpdate => Get<bool>(Environment.GetEnvironmentVariable("SkipUpdate"), false);
        public static bool SkipDelete => Get<bool>(Environment.GetEnvironmentVariable("SkipDelete"), false);


        private static T Get<T>(string key, T? defaultValue = null) where T : struct
        {
            var value = key;
            var converter = TypeDescriptor.GetConverter(typeof(T));

            try
            {
                if (!string.IsNullOrWhiteSpace(value) && converter.CanConvertFrom(typeof(string)) && converter.ConvertFromString(value) is T result)
                {
                    return result;
                }

                if (defaultValue.HasValue)
                {
                    return defaultValue.Value;
                }

                throw new FormatException($"Cannot parse setting with key: {key}, value: \"{value}\" as {typeof(T)}");
            }
            catch
            {
                if (defaultValue.HasValue)
                    return defaultValue.Value;

                throw;
            }
        }
    }
}
