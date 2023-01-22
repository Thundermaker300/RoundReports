namespace RoundReports.Commands.MvpCommand
{
    using System;
    using System.Text;
    using CommandSystem;
    using Exiled.API.Features.Pools;

    /// <summary>
    /// Parent command for the MVP system.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Mvp : ParentCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mvp"/> class.
        /// </summary>
        public Mvp() => LoadGeneratedCommands();

        /// <inheritdoc/>
        public override string Command => "mvp";

        /// <inheritdoc/>
        public override string[] Aliases => Array.Empty<string>();

        /// <inheritdoc/>
        public override string Description => "Base command for MVP-related controls";

        /// <inheritdoc/>
        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Add());
            RegisterCommand(new Remove());
        }

        /// <inheritdoc/>
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = StringBuilderPool.Pool.Get();

            sb.AppendLine("Available commands:");
            sb.AppendLine("- MVP ADD <PLAYERS> <SCP/HUMAN> <AMOUNT> <REASON> - Adds specified points to the player(s).");
            sb.AppendLine("- MVP RM <PLAYERS> <SCP/HUMAN> <AMOUNT> <REASON> - Removes points from the player(s).");

            response = StringBuilderPool.Pool.ToStringReturn(sb);
            return false;
        }
    }
}
