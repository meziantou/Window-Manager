using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowManager.Core
{
    public class CommandLine
    {
        private readonly IEnumerable<string> _args;

        static readonly CommandLine _default = new CommandLine();
        public static CommandLine Default
        {
            get
            {
                return _default;
            }
        }

        public CommandLine()
            : this(Environment.GetCommandLineArgs())
        {

        }

        public CommandLine(IEnumerable<string> args)
        {
            _args = args;
        }

        public bool Updated
        {
            get { return _args.Contains("/Updated"); }
        }

        public bool ShowMainWindow
        {
            get { return _args.Contains("/ShowMainWindow"); }
        }
    }
}
