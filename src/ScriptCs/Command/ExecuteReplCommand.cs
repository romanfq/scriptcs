﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;

namespace ScriptCs.Command
{
    internal class ExecuteReplCommand : IScriptCommand
    {
        private readonly IFileSystem _fileSystem;
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly IScriptEngine _scriptEngine;
        private readonly IFilePreProcessor _filePreProcessor;

        private readonly ILog _logger;
        private readonly IConsole _console;

        public ExecuteReplCommand(
            IFileSystem fileSystem,
            IScriptPackResolver scriptPackResolver,
            IScriptEngine scriptEngine,
            IFilePreProcessor filePreProcessor,
            ILog logger,
            IConsole console
            )
        {
            _fileSystem = fileSystem;
            _scriptPackResolver = scriptPackResolver;
            _scriptEngine = scriptEngine;
            _filePreProcessor = filePreProcessor;
            _logger = logger;
            _console = console;
        }

        public CommandResult Execute()
        {
            Console.WriteLine("scriptcs (ctrl-c or blank to exit)\r\n");
            var repl = new Repl(_fileSystem, _scriptEngine, _logger, _console, _filePreProcessor);
            repl.Initialize(GetAssemblyPaths(_fileSystem.CurrentDirectory), _scriptPackResolver.GetPacks());
            try
            {
                while (ExecuteLine(repl))
                {
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return CommandResult.Error;              
            }
            repl.Terminate();
            return CommandResult.Success;
        }

        private bool ExecuteLine(Repl repl)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == "")
                return false;

            repl.Execute(line);
            return true;
        }


        private IEnumerable<string> GetAssemblyPaths(string workingDirectory)
        {
            var binFolder = Path.Combine(workingDirectory, "bin");

            if (!_fileSystem.DirectoryExists(binFolder))
                _fileSystem.CreateDirectory(binFolder);

            var assemblyPaths =
                _fileSystem.EnumerateFiles(binFolder, "*.dll")
                .Union(_fileSystem.EnumerateFiles(binFolder, "*.exe"))
                .ToList();

            foreach (var path in assemblyPaths.Select(Path.GetFileName))
            {
                _logger.DebugFormat("Found assembly reference: {0}", path);
            }

            return assemblyPaths;
        }
    }
}
