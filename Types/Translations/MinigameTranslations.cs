using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCProphunt.Types
{
    public class MinigameTranslations
    {
        public string prophunt { get; set; } = "PropHunt";
        public string props { get; set; } = "Props";
        public string hunters { get; set; } = "Hunters";
        public string round_end { get; set; } = "Round Ended";
        public string broadcast_until_hunter_release { get; set; } = "{releaseTime} until Hunters are Released!";
        public string broadcast_time_left { get; set; } = "{timeLeft} Left";
        public string broadcast_prop_win { get; set; } = "Props Win!";
        public string broadcast_hunter_win { get; set; } = "Hunters Win!";
        public string broadcast_next_taunt { get; set; } = "Next Prop Taunts in {nextTaunt}";
        public string voting_instruction { get; set; } = "Press [Ctrl] / Switch to a Keycard to Vote!";
        public string voting_current_selection { get; set; } = "You are Currently Voting For: {voteZone}";
        public string voting_vote_count { get; set; } = "{voteCount} Votes";
        public string voting_invalid_label { get; set; } = "None";
        public string voting_light_label { get; set; } = "Light Containment";
        public string voting_light_keycard { get; set; } = "Scientist Keycard";
        public string voting_heavy_label { get; set; } = "Heavy Containment";
        public string voting_heavy_keycard { get; set; } = "Guard Keycard";
        public string voting_entrance_label { get; set; } = "Entrance Zone";
        public string voting_entrance_keycard { get; set; } = "Researcher Keycard";
        public string voting_broadcast_time_left { get; set; } = "{votingTime} Left to Vote on a Map!";
        public string voting_broadcast_chosen_map { get; set; } = "Chosen Map: {chosenMap}";
        public string voting_broadcast_starting_in { get; set; } = "Starting in: {startingIn}";
        public string killfeed_hunters_released { get; set; } = "[ ℹ ] Hunters have been Released!";
        public string killfeed_explosion_self { get; set; } = "[💀] {victim} Blew Themselves Up";
        public string killfeed_found_player { get; set; } = "[💀] {attacker} Found {victim}";
        public string killfeed_death { get; set; } = "[💀] {victim} Died";
        public string killfeed_cant_find { get; set; } = "[💀] {victim} Couldn't Guess Correctly";
        public string killfeed_disconnect { get; set; } = "[🚪] {victim} Disconnected";
        public string prop_ui_prohibited { get; set; } = "This prop is prohibited!";
        public string prop_ui_rotation_control_hint { get; set; } = "Press L-ALT (Or Noclip Key) to Lock/Unlock Rotation";
        public string prop_ui_rotation_locked { get; set; } = "Rotation Locked!";
        public string prop_ui_rotation_unlocked { get; set; } = "Rotation Unlocked!";
        public string prop_ui_taunt_hint { get; set; } = "Press G (Or Switch to Flashbang) to Taunt";
        public string prop_ui_taunt_ready { get; set; } = "Ready!";
    }
}
