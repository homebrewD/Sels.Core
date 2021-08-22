﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Sels.Core.Contracts.Commands;
using Sels.Core.Linux.Components.LinuxCommand.Commands;
using Sels.Core.Linux.Components.LinuxCommand.Commands.Core;
using Sels.Core.Linux.Contracts.LinuxCommand;
using Sels.Core.Linux.Templates.LinuxCommand.Commands;

namespace Sels.Core.Linux.Contracts.LinuxCommand
{
    /// <summary>
    /// Used to start to setup a command chain for a <see cref="MultiCommand{TCommandResult}"/>.
    /// </summary>
    public interface IMultiCommandStartSetup
    {
        /// <summary>
        /// Sets <paramref name="startCommand"/> as the first command to be executed.
        /// </summary>
        /// <param name="startCommand">Command to execute first</param>
        /// <returns>Setup object to continue building the command chain or returns the <see cref="IMultiCommandChain"/></returns>
        IMultiCommandSetup StartWith(ICommand startCommand);
    }

    /// <summary>
    /// Used to setup and build a command chain for a <see cref="MultiCommand{TCommandResult}"/>.
    /// </summary>
    public interface IMultiCommandSetup
    {
        /// <summary>
        /// Continues the previous command with <paramref name="command"/>.
        /// </summary>
        /// <param name="chain">How the previous <see cref="ICommand"/> should be chained with <paramref name="command"/></param>
        /// <param name="command">Command to chain</param>
        /// <returns>Object to continue building the chain</returns>
        IMultiCommandSetup ContinueWith(CommandChainer chain, ICommand command);

        /// <summary>
        /// Finished the command chain with <paramref name="finalCommand"/> and returns the full command chain.
        /// </summary>
        /// <param name="finalChain">How the previous <see cref="ICommand"/> should be chained with <paramref name="finalCommand"/></param>
        /// <param name="finalCommand">Final command in the chain that will be executed</param>
        /// <returns>The configured command chain</returns>
        IMultiCommandChain EndWith(CommandChainer finalChain, ICommand finalCommand);
    }

    /// <summary>
    /// Represents the order in which command are executed for a <see cref="MultiCommand{TCommandResult}"/>
    /// </summary>
    public interface IMultiCommandChain
    {
        /// <summary>
        /// First command in the chain that will be executed first.
        /// </summary>
        public ICommand StartCommand { get; }
        /// <summary>
        /// List of ordered commands that will be executed in order after <see cref="StartCommand"/>.
        /// </summary>
        public ReadOnlyCollection<(CommandChainer Chain, ICommand Command)> IntermediateCommands { get;}
        /// <summary>
        /// How <see cref="StartCommand"/> or the last command in <see cref="IntermediateCommands"/> should be linked to <see cref="FinalCommand"/>.
        /// </summary>
        public CommandChainer FinalChain { get; }
        /// <summary>
        /// Final command in the chain that will be executed.
        /// </summary>
        public ICommand FinalCommand { get; }
    }
}
