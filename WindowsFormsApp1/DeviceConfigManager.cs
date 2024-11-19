using System.IO;
using Newtonsoft.Json;

namespace ZamanPardazSDK
{


    public class DeviceConfigManager
    {
        private const string ConfigFilePath = "device_config.json";

        // بازیابی اطلاعات از فایل JSON
        public static DeviceConnectionInfo LoadDeviceConnectionInfo()
        {
            if (!File.Exists(ConfigFilePath))
            {
                throw new FileNotFoundException("Configuration file not found.");
            }

            string jsonContent = File.ReadAllText(ConfigFilePath);
            var result =  JsonConvert.DeserializeObject<DeviceConnectionInfo>(jsonContent);
            return result;
        }

        // ذخیره اطلاعات در فایل JSON
        public static void SaveDeviceConnectionInfo(DeviceConnectionInfo connectionInfo)
        {
            string jsonContent = JsonConvert.SerializeObject(connectionInfo, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, jsonContent);
        }
    }

}
