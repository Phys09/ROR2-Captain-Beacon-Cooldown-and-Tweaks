using System.Collections.Generic;
using System.Diagnostics;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CaptainBeaconCooldown
{
    // When finished, put this plugin in
    // "BepInEx/plugins/CaptainBeaconCooldown/CaptainBeaconCooldown.dll" to test out.

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class CaptainBeaconCooldown : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Phys09";
        public const string PluginName = "CaptainBeaconCooldown";
        public const string PluginVersion = "1.0.1";

        // Config Entries
        public static ConfigEntry<float> HealingBeaconCD { get; set; }
        public static ConfigEntry<float> ShockingBeaconCD { get; set; }
        public static ConfigEntry<float> EquipmentBeaconCD { get; set; }
        public static ConfigEntry<float> HackingBeaconCD { get; set; }
        public static ConfigEntry<bool> EnableDebug { get; set; }

        public const float DEFAULT_HEALING_BEACON_COOLDOWN = 60.0f;
        public const float DEFAULT_SHOCKING_BEACON_COOLDOWN = 60.0f;
        public const float DEFAULT_EQUIPMENT_BEACON_COOLDOWN = 90.0f;
        public const float DEFAULT_HACKING_BEACON_COOLDOWN = 240.0f;

        public float[] beaconCooldowns;
        public string[] beaconNameTokens = new string[]
        {
            "CAPTAIN_SUPPLY_HEAL_NAME",
            "CAPTAIN_SUPPLY_SHOCKING_NAME",
            "CAPTAIN_SUPPLY_EQUIPMENT_RESTOCK_NAME",
            "CAPTAIN_SUPPLY_HACKING_NAME",
        };
        public float leftBeaconStopwatch = 0.0f;
        public float rightBeaconStopwatch = 0.0f;
        public float leftBeaconCooldown;
        public float rightBeaconCooldown;
        public bool enableDebugMode;

        private void OnRoR2SkillsCaptainSupplyDropSkillDefIsReady(
            On.RoR2.CaptainSupplyDropController.orig_FixedUpdate orig,
            RoR2.CaptainSupplyDropController self
        )
        {
            // In order for the checks to work, make sure these values are set properly
            self.supplyDrop1Skill.maxStock = 1 + self.supplyDrop1Skill.bonusStockFromBody;
            // Setting maxstock for right beacon seems to cause issues with when adding a beacon
            // Default skilldefs causing issues with the skill and internal stopwatch likely
            self.supplyDrop2Skill.maxStock = 1 + self.supplyDrop2Skill.bonusStockFromBody;

            // assign the left beacon and right beacon cooldowns for easy later use
            for (int i = 0; i < beaconNameTokens.Length; i++)
            {
                if (self.supplyDrop1Skill.skillNameToken == beaconNameTokens[i])
                {
                    leftBeaconCooldown = beaconCooldowns[i];
                }
                if (self.supplyDrop2Skill.skillNameToken == beaconNameTokens[i])
                {
                    rightBeaconCooldown = beaconCooldowns[i];
                }
            }

            // increment left beacon's stopwatch if we are not at max stocks
            if (self.supplyDrop1Skill.stock < self.supplyDrop1Skill.maxStock)
            {
                leftBeaconStopwatch += Time.fixedDeltaTime; // increment stopwatch for beacon cooldowns
            }
            // increment right beacon's stopwatch if we are not at max stocks
            // if (self.supplyDrop2Skill.stock < self.supplyDrop2Skill.maxStock)
            const int MAX_SUPPLYDROP2_STOCK = 1; // Right beacon never exceeds 1, hard code it
            if (self.supplyDrop2Skill.stock < MAX_SUPPLYDROP2_STOCK)
            {
                rightBeaconStopwatch += Time.fixedDeltaTime; // increment stopwatch for beacon cooldowns
            }

            // Prevent backup magazines from adding too many stocks of right beacon
            // Since rightbeacon will never have more than 1 stock, hard code it to have 1 unless it's 0
            if (self.supplyDrop2Skill.stock > 1)
            {
                self.supplyDrop2Skill.stock = 1;
            }

            // Check left beacon and add stocks if needed
            if (leftBeaconStopwatch >= leftBeaconCooldown)
            {
                leftBeaconStopwatch = 0.0f; // reset stopwatch for later
                // Because AddOneStick() also resets internal stopwatch, just hard code it and fix it later for v2.0 rewrite
                // self.supplyDrop1Skill.AddOneStock();
                self.supplyDrop1Skill.stock += 1;
            }
            // Check right beacon and add stocks if needed
            if (rightBeaconStopwatch >= rightBeaconCooldown)
            {
                rightBeaconStopwatch = 0.0f; // reset stopwatch for later
                // Because AddOneStick() also resets internal stopwatch, just hard code it and fix it later for v2.0 rewrite
                // self.supplyDrop2Skill.AddOneStock();
                self.supplyDrop2Skill.stock += 1;
            }

            if (this.enableDebugMode && Input.GetKeyDown(KeyCode.L))
            {
                Log.Info(
                    $"==================== Starting to print all values of supplydrop ===================="
                );

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.skillName'");
                Log.Info($"{self.supplyDrop1Skill.skillName}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.bonusStockFromBody'");
                Log.Info($"{self.supplyDrop1Skill.bonusStockFromBody}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.baseStock'");
                Log.Info($"{self.supplyDrop1Skill.baseStock}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.finalRechargeInterval'");
                Log.Info($"{self.supplyDrop1Skill.finalRechargeInterval}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill._cooldownScale'");
                Log.Info($"{self.supplyDrop1Skill._cooldownScale}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill._flatCooldownReduction'");
                Log.Info($"{self.supplyDrop1Skill._flatCooldownReduction}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.baseRechargeStopwatch'");
                Log.Info($"{self.supplyDrop1Skill.baseRechargeStopwatch}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.skillDef'");
                Log.Info($"{self.supplyDrop1Skill.skillDef}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.baseSkill'");
                Log.Info($"{self.supplyDrop1Skill.baseSkill}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.skillNameToken'");
                Log.Info($"{self.supplyDrop1Skill.skillNameToken}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.skillDescriptionToken'");
                Log.Info($"{self.supplyDrop1Skill.skillDescriptionToken}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.baseRechargeInterval'");
                Log.Info($"{self.supplyDrop1Skill.baseRechargeInterval}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.rechargeStock'");
                Log.Info($"{self.supplyDrop1Skill.rechargeStock}");

                Log.Info(
                    "Printing variable named: 'self.supplyDrop1Skill.beginSkillCooldownOnSkillEnd'"
                );
                Log.Info($"{self.supplyDrop1Skill.beginSkillCooldownOnSkillEnd}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.isCombatSkill'");
                Log.Info($"{self.supplyDrop1Skill.isCombatSkill}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.mustKeyPress'");
                Log.Info($"{self.supplyDrop1Skill.mustKeyPress}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.defaultSkillDef'");
                Log.Info($"{self.supplyDrop1Skill.defaultSkillDef}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.maxStock'");
                Log.Info($"{self.supplyDrop1Skill.maxStock}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.stock'");
                Log.Info($"{self.supplyDrop1Skill.stock}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.cooldownScale'");
                Log.Info($"{self.supplyDrop1Skill.cooldownScale}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.flatCooldownReduction'");
                Log.Info($"{self.supplyDrop1Skill.flatCooldownReduction}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.rechargeStopwatch'");
                Log.Info($"{self.supplyDrop1Skill.rechargeStopwatch}");

                Log.Info("Printing variable named: 'self.supplyDrop1Skill.cooldownRemaining'");
                Log.Info($"{self.supplyDrop1Skill.cooldownRemaining}");

                Log.Info($"====================");
            }

            orig(self); // Run original code
        }

        private void OnEnable()
        {
            On.RoR2.CaptainSupplyDropController.FixedUpdate +=
                OnRoR2SkillsCaptainSupplyDropSkillDefIsReady;
        }

        private void OnDisable()
        {
            On.RoR2.CaptainSupplyDropController.FixedUpdate -=
                OnRoR2SkillsCaptainSupplyDropSkillDefIsReady;
        }

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Initialize the logger
            Log.Init(Logger);

            // Config Setup
            HealingBeaconCD = Config.Bind<float>(
                "Beacon Cooldowns",
                "Healing Beacon Cooldown",
                DEFAULT_HEALING_BEACON_COOLDOWN,
                "Captain's Healing Beacon Cooldown in seconds. Default: "
                    + DEFAULT_HEALING_BEACON_COOLDOWN
            );
            ShockingBeaconCD = Config.Bind<float>(
                "Beacon Cooldowns",
                "Shocking Beacon Cooldown",
                DEFAULT_SHOCKING_BEACON_COOLDOWN,
                "Captain's Shocking Beacon Cooldown in seconds. Default: "
                    + DEFAULT_SHOCKING_BEACON_COOLDOWN
            );
            EquipmentBeaconCD = Config.Bind<float>(
                "Beacon Cooldowns",
                "Equipment Beacon Cooldown",
                DEFAULT_EQUIPMENT_BEACON_COOLDOWN,
                "Captain's Equipment Beacon Cooldown in seconds. Default: "
                    + DEFAULT_EQUIPMENT_BEACON_COOLDOWN
            );
            HackingBeaconCD = Config.Bind<float>(
                "Beacon Cooldowns",
                "Hacking Beacon Cooldown",
                DEFAULT_HACKING_BEACON_COOLDOWN,
                "Captain's Hacking Beacon Cooldown in seconds. Default: "
                    + DEFAULT_HACKING_BEACON_COOLDOWN
            );
            EnableDebug = Config.Bind<bool>(
                "Enable Debug",
                "Enable Debugging Keybind(s)",
                false,
                "Enable debug keybinds (Like L to print some values to console)"
            );

            // Populate Beacon Cooldown Array
            this.beaconCooldowns = new float[]
            {
                HealingBeaconCD.Value,
                ShockingBeaconCD.Value,
                EquipmentBeaconCD.Value,
                HackingBeaconCD.Value,
            };
            this.enableDebugMode = EnableDebug.Value;
        }

        /**
         * Perform Instructions on every game update
         */
        private void Update() { }
    }
}
