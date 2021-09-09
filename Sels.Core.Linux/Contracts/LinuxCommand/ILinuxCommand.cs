﻿using Microsoft.Extensions.Logging;
using Sels.Core.Components.Commands;
using Sels.Core.Contracts.Commands;
using Sels.Core.Linux.Contracts.LinuxCommand.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Linux.Contracts.LinuxCommand
{
    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    public interface ILinuxCommand : ILinuxCommand<ILinuxCommandResult<string, string>>
    {

    }

    /// <summary>
    /// Used to run linux commands or build linux command strings.
    /// </summary>
    /// <typeparam name="TCommandResult">Type of result returned by the command</typeparam>
    public interface ILinuxCommand<TCommandResult> : ICommand
    {
        /// <summary>
        /// Executes linux command and parses result to <typeparamref name="TCommandResult"/>.
        /// </summary>
        TCommandResult Execute(CommandExecutionOptions options = null);

        /// <summary>
        /// Executes linux command and parses result to <typeparamref name="TCommandResult"/>.
        /// </summary>
        /// <param name="exitCode">Exit code returned from executing command</param>
        TCommandResult Execute(out int exitCode, CommandExecutionOptions options = null);

        /// <summary>
        /// Parses <paramref name="exitCode"/> and <paramref name="error"/> from command execution to an object of type <typeparamref name="TCommandResult"/>.
        /// </summary>
        /// <param name="exitCode">Exit code of command execution</param>
        /// <param name="error">Sterr of command execution</param>
        /// <param name="output">Stout of command execution</param>
        /// <param name="wasSuccesful">Boolean indicating if command execution was successful</param>
        /// <returns>Result from command execution</returns>
        TCommandResult CreateResult(bool wasSuccesful, int exitCode, string output, string error, IEnumerable<ILogger> loggers = null);
    }
}
