using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace AtelierManager
{
    public static class CommandLine
    {
        public static void Init(string[] args)
        {
            var rootCommand = RegisterOptions();
            rootCommand.Invoke(args);
        }
        public static RootCommand RegisterOptions()
        {
            var optionsBinder = new OptionsBinder();
            var rootCommand = new RootCommand()
            {
                optionsBinder.Catalog,
                optionsBinder.Api,
            };

            rootCommand.SetHandler(Program.Run, optionsBinder);

            return rootCommand;
        }
    }
    public class Options
    {
        public FileInfo Catalog { get; set; }
        public string Api { get; set; }
    }

    public class OptionsBinder : BinderBase<Options>
    {
        public readonly Option<FileInfo> Catalog;
        public readonly Option<string> Api;

        public OptionsBinder()
        {
            Catalog = new Option<FileInfo>("--catalog", "Path to local catalog.json").LegalFilePathsOnly();
            Api = new Option<string>("--api", "API to use for downloading");

            Api.SetDefaultValue(string.Empty);
        }

        protected override Options GetBoundValue(BindingContext bindingContext) =>
        new()
        {
            Catalog = bindingContext.ParseResult.GetValueForOption(Catalog),
            Api = bindingContext.ParseResult.GetValueForOption(Api),
        };
    }
}
