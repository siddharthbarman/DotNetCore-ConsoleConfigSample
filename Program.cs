using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine ;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

namespace CmdLineConfigSample
{
    class Program
    {
        private static IConfigurationProvider GetChainedConfiguration(string jsonSettings, string[] args)
        {
            ConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.Add(new JsonConfigurationSource {
                Path = jsonSettings,
                Optional = false,
                FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)),
                ReloadOnChange = true                
            });
            
            configBuilder.Add(new CommandLineConfigurationSource
            {
                Args = args
            });

            IConfigurationProvider configProvider = new ChainedConfigurationSource
            {
                Configuration = configBuilder.Build()
            }
            .Build(configBuilder);
            
            configProvider.Load();            
            return configProvider;
        }

        static void Main(string[] args)
        {
            IConfigurationProvider config = GetChainedConfiguration("appsettings.json", args);
            string greeting = "How are you?";
            config.TryGet("greeting", out greeting);
            Console.WriteLine($"{greeting}");                        
        }
        
        private static void NonChained(string[] args)
        {
            IConfigurationProvider cmdConfig = new CommandLineConfigurationProvider(args);
            cmdConfig.Load();

            IConfigurationProvider jsonConfig = GetJsonConfiguration("appsettings.json");
            string defaultGreeting = "How are you?";
            jsonConfig.TryGet("greeting", out defaultGreeting);                

            string greeting;
            if (!cmdConfig.TryGet("greeting", out greeting))
            {
                greeting = defaultGreeting;
            }
            
            Console.WriteLine($"{greeting}");
        }        

        private static IConfigurationProvider GetJsonConfiguration(string jsonSettingFilename)
        {            
            JsonConfigurationSource jsonSource = new JsonConfigurationSource {
                Path = jsonSettingFilename,
                Optional = false,
                FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)),
                ReloadOnChange = true
            };

            JsonConfigurationProvider jsonSettings = new JsonConfigurationProvider(jsonSource);
            jsonSettings.Load();
            return jsonSettings;
        }
    }
}
