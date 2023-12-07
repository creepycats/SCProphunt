using SCProphunt;
using SCProphunt.Types;
using System.Collections.Generic;
using System;
using Exiled.API.Enums;

public class PropHuntRoundInfo
{
    public long VotingTimeStart = 0;
    public long VotingTimeFinish = 0;
    public Dictionary<string, ZoneType> MapChoice { get; set; } = new Dictionary<string, ZoneType>();
    public bool StillInLobby = true;
    public ZoneType ChosenMap = ZoneType.Unspecified;
    public bool VoteWasTied = false;

    public long TimeFinished = 0;
    public long TimeStarted = 0;
    public long LastTaunt = 0;

    public bool HuntersReleased = false;

    public bool RoundEnded = false;

    public Dictionary<string, string> PropHuntTeam { get; set; } = new Dictionary<string, string>();

    public Dictionary<string, long> KillFeed { get; set; } = new Dictionary<string, long>();

    public List<string> KilledPlayers { get; set; } = new List<string>();

    public Dictionary<string, long> DeniedPropPlayers { get; set; } = new Dictionary<string, long>();

    public Dictionary<string, long> TauntedPropPlayers { get; set; } = new Dictionary<string, long>();

    public List<NonPropCache> CachedProps { get; set; } = new List<NonPropCache> { };

    public PropHuntRoundInfo()
    {
        TimeStarted = (long)(new DateTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
        TimeFinished = (long)(new DateTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
        LastTaunt = (long)(new DateTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

        HuntersReleased = false;

        StillInLobby = true;
        VotingTimeStart = (long)(new DateTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
        VotingTimeFinish = (long)(new DateTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

        PropHuntTeam = new Dictionary<string, string>();
    }
}