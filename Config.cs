using Exiled.API.Interfaces;
using SCProphunt.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace SCProphunt.Config
{
    public class Config : IConfig
    {

        /// <summary>
        ///  Will the plugin run?
        /// </summary>
        [Description("Will the plugin run?")]
        public bool IsEnabled { get; set; } = true;
        /// <summary>
        ///  Will the plugin print Debug Text?
        /// </summary>
        [Description("Will the plugin print Debug Text?")]
        public bool Debug { get; set; } = false;
        /// <summary>
        ///  How many extra props per room to spawn per player (Default is 0.5)
        /// </summary>
        [Description("How many extra props per room to spawn per player (Default is 0.5)")]
        public float PropCompensationRatio { get; set; } = 0.5f;
        /// <summary>
        ///  Default HP amount for Hunters
        /// </summary>
        [Description("Default HP amount for Hunters")]
        public float HunterHP { get; set; } = 125;
        /// <summary>
        ///  How much will hunters heal from killing props?
        /// </summary>
        [Description("How much will hunters heal from killing props?")]
        public int PropKillHP { get; set; } = 25;
        /// <summary>
        ///  Enable Taunting?
        /// </summary>
        [Description("Enable Taunting?")]
        public bool EnableTaunts { get; set; } = true;
        /// <summary>
        ///  How many seconds should there be between taunts?
        /// </summary>
        [Description("How many seconds should there be between taunts?")]
        public float TauntCooldown { get; set; } = 60;
        /// <summary>
        ///  Manual Prop Taunting Cooldown
        /// </summary>
        [Description("Manual Prop Taunting Cooldown")]
        public float ManualTauntCooldown { get; set; } = 7.5f;
        /// <summary>
        ///  How many Taunt sound files are there? NOTE: MUST BE MONO .OGG, numbered 1 through # (Ex: 1.ogg, 2.ogg... 35.ogg)
        /// </summary>
        [Description("How many Taunt sound files are there? NOTE: MUST BE MONO .OGG, numbered 1 through # (Ex: 1.ogg, 2.ogg... 35.ogg)")]
        public int NumberOfTaunts { get; set; } = 35;
        /// <summary>
        ///  Default Prop Information (Item is Irrelevant) - Set player scale, health, and item offsets
        /// </summary>
        [Description("Default Prop Information (Item is Irrelevant) - Set player scale, health, and item offsets")]
        public PropConfig DefaultPropInformation { get; set; } = new PropConfig() {
            PlayerHP = 50,
            PlayerScale = new Vector3(0.25f,0.25f,0.25f),
            Position = Vector3.zero,
            Rotation = new Vector3(0f,0f,0f),
            Scale = Vector3.one,
        };
        // <summary>
        //  Lists the prohibited types of Items to spawn as props randomly
        // </summary>
        [Description("Lists the prohibited types of Items to spawn as props randomly. Players Cannot use these Props.")]
        public List<ItemType> ProhibitedPropTypes { get; set; } = new List<ItemType> { 
            ItemType.SCP244a, 
            ItemType.SCP244b,
            ItemType.SCP018,
            ItemType.SCP2176,
            ItemType.Coin
        };
        // <summary>
        //  Prop-Specific Information Config - Set player scale, health, and item offsets
        // </summary>
        [Description("Prop-Specific Information Config - Set player scale, health, and item offsets")]
        public Dictionary<ItemType, PropConfig> SpecificPropInformation { get; set; } = new Dictionary<ItemType, PropConfig>() { 
            [ItemType.Coin] = new PropConfig()
            {
                PlayerHP = 5,
                PlayerScale = new Vector3(0.25f, 0.25f, 0.25f),
                Position = Vector3.zero,
                Rotation = new Vector3(0f,0f,0f),
                Scale = Vector3.one,
            }
        };
        // <summary>
        //  Prop-Category-Specific Information Config - Set player scale, health, and item offsets for broad categories of props
        // </summary>
        [Description("Prop-Category-Specific Information Config - Set player scale, health, and item offsets for broad categories of props")]
        public Dictionary<ItemCategory, PropConfig> CategoryPropInformation { get; set; } = new Dictionary<ItemCategory, PropConfig>()
        {
            [ItemCategory.Keycard] = new PropConfig()
            {
                PlayerHP = 30,
                PlayerScale = new Vector3(0.25f, 0.25f, 0.25f),
                Position = Vector3.zero,
                Rotation = new Vector3(0f, 0f, 0f),
                Scale = Vector3.one,
            },
            [ItemCategory.Grenade] = new PropConfig()
            {
                PlayerHP = 30,
                PlayerScale = new Vector3(0.25f, 0.25f, 0.25f),
                Position = Vector3.zero,
                Rotation = new Vector3(0f, 0f, 0f),
                Scale = Vector3.one,
            },
            [ItemCategory.Ammo] = new PropConfig()
            {
                PlayerHP = 30,
                PlayerScale = new Vector3(0.25f, 0.25f, 0.25f),
                Position = Vector3.zero,
                Rotation = new Vector3(0f, 0f, 0f),
                Scale = Vector3.one,
            },
            [ItemCategory.Firearm] = new PropConfig()
            {
                PlayerHP = 60,
                PlayerScale = new Vector3(0.25f, 0.25f, 0.25f),
                Position = Vector3.zero,
                Rotation = new Vector3(0f, 0f, -90f),
                Scale = Vector3.one,
            },
            [ItemCategory.SCPItem] = new PropConfig()
            {
                PlayerHP = 50,
                PlayerScale = new Vector3(0.35f, 0.35f, 0.35f),
                Position = Vector3.zero,
                Rotation = new Vector3(0f, 0f, 0f),
                Scale = Vector3.one,
            },
            [ItemCategory.Armor] = new PropConfig()
            {
                PlayerHP = 75,
                PlayerScale = new Vector3(0.25f, 0.25f, 0.25f),
                Position = Vector3.zero,
                Rotation = new Vector3(0f, 0f, -90f),
                Scale = Vector3.one,
            },
            [ItemCategory.MicroHID] = new PropConfig()
            {
                PlayerHP = 80,
                PlayerScale = new Vector3(0.5f, 0.5f, 0.5f),
                Position = Vector3.zero,
                Rotation = new Vector3(0f, 0f, 0f),
                Scale = Vector3.one,
            },
            [ItemCategory.Radio] = new PropConfig()
            {
                PlayerHP = 20,
                PlayerScale = new Vector3(0.25f, 0.25f, 0.25f),
                Position = Vector3.zero,
                Rotation = new Vector3(0f, 0f, 0f),
                Scale = Vector3.one,
            }
        };
    }
}
