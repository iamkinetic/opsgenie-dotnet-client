using System;
using System.IO;
using System.Linq;
using CommandLine;
using OpsGenieApi.Model;

namespace OpsGenieCli
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    if (!File.Exists(options.Config) && string.IsNullOrWhiteSpace(options.Key))
                    {
                        Console.WriteLine("Config file not found.");
                        return;
                    }

                    var opsGenieClient = OpsGenieHelper.CreateOpsGenieClient(
                        OpsGenieHelper.GetOpsGenieConfig(options.Config, options.Key));

                    switch (options.Action)
                    {
                        case Action.Raise:
                            opsGenieClient.Raise(
                                new Alert
                                {
                                    Alias = options.Alias,
                                    Message = options.Message,
                                    Source = options.Source,
                                    Description = options.Description,
                                    Recipients = !string.IsNullOrWhiteSpace(options.Recipients)
                                        ? options.Recipients.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .ToList()
                                        : null
                                }
                            ).Wait();
                            break;
                        case Action.Acknowledge:
                            opsGenieClient.Acknowledge(null, options.Alias, options.Note).Wait();
                            break;
                        case Action.Resolve:
                            opsGenieClient.Close(null, options.Alias, options.Note).Wait();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });
        }

        internal class Options
        {
            [Option('c', "config", Default = "OpsGenie.config")]
            public string Config { get; set; }

            [Option('k', "apikey", Default = "")]
            public string Key { get; set; }

            [Option('s', "source", Default = "Sourcet")]
            public string Source { get; set; }

            [Option('m', "message", Default = "")]
            public string Message { get; set; }

            [Option('a', "action", Default = Action.Raise)]
            public Action Action { get; set; }

            [Option('i', "alias", Default = null)]
            public string Alias { get; set; }

            [Option('d', "Description", Default = null)]
            public string Description { get; set; }

            [Option('n', "Note", Default = null)]
            public string Note { get; set; }
           
            [Option('r', "Recipients", Default = null)]
            public string Recipients { get; set; }

        }

        internal enum Action
        {
            Raise,
            Acknowledge,
            Resolve
        }

    }
}