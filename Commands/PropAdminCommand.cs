using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCProphunt.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PropAdminCommand : ICommand, IUsageProvider
    {
        public string Command { get; } = "propadmin";

        public string[] Aliases { get; } = {  };

        public string Description { get; } = "Prophunt Admin Command Base";

        public string[] Usage { get; } = new string[2] { "Option", "Parameters" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            //if (!(sender is PlayerCommandSender))
            //{
            //    response = "This command can only be ran by a player!";
            //    return true;
            //}

            var player = Player.Get(((PlayerCommandSender)sender).ReferenceHub);
            if (!sender.CheckPermission("SCProphunt.admin") && !sender.CheckPermission("ev.prophunt"))
            {
                response = "You do not have access to the Commands for SCProphunt Admin Command Base";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage: propadmin team <player> <prop/hunter> | .propadmin start | .propadmin end";
                return true;
            }

            if (arguments.At(0) == "start")
            {
                if (Player.List.Count() < 2 && !SCProphunt.Instance.Config.Debug)
                {
                    response = $"Prophunt requires at least 2 players!";
                    return false;
                }
                Game.StartProphuntRound(510); // 8:30 Round Start
                response = $"Prophunt Round Started!";
                return true;
            }
            else if (arguments.At(0) == "end" || arguments.At(0) == "stop")
            {
                if (SCProphunt.Instance.PropHuntRound == null)
                {
                    response = $"Prophunt hasn't started yet!";
                    return false;
                }
                Game.EndProphuntRound();
                response = $"Ended Prophunt Round!";
                return true;
            }
            else if (arguments.At(0) == "team")
            {
                if (SCProphunt.Instance.PropHuntRound == null)
                {
                    response = $"Prophunt hasn't started yet!";
                    return false;
                }
                Player _targetPlayer = Player.Get(arguments.At(1));
                if (_targetPlayer != null)
                {
                    if (arguments.Count < 3) {
                        if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.TryGetValue(_targetPlayer.UserId, out string _plrTeam))
                        {
                            response = $"Player '{_targetPlayer.Nickname}' ({_targetPlayer.UserId}) - Team: {_plrTeam}";
                            return true;
                        }
                        else
                        {
                            response = $"Player '{_targetPlayer.Nickname}' ({_targetPlayer.UserId}) - Not In Round";
                            return true;
                        }
                    } else
                    {
                        switch (arguments.At(2))
                        {
                            case "prop":
                            case "hunter":
                                if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.ContainsKey(_targetPlayer.UserId))
                                {
                                    SCProphunt.Instance.PropHuntRound.PropHuntTeam[_targetPlayer.UserId] = arguments.At(2);
                                } else {
                                    SCProphunt.Instance.PropHuntRound.PropHuntTeam.Add(_targetPlayer.UserId, arguments.At(2));
                                }
                                Game.HandlePlayerTeamChange(_targetPlayer, arguments.At(2));
                                response = $"Set Player '{_targetPlayer.Nickname}' ({_targetPlayer.UserId}) Team to {arguments.At(2)}";
                                break;
                            default:
                                response = $"Player '{_targetPlayer.Nickname}' ({_targetPlayer.UserId}) - Not In Round";
                                break;
                        }
                        return true;
                    }
                } else
                {
                    response = "Couldn't find the target player.";
                    return false;
                }
            }
            else
            {
                response = "Invalid Subcommand.";
                return false;
            }

            response = "Something is very wrong. Let me know if you somehow get this result.";
            return false;
        }
    }
}
