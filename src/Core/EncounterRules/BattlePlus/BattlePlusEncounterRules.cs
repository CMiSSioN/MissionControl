using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using BattleTech;

using MissionControl.Trigger;
using MissionControl.Logic;

namespace MissionControl.Rules {
  public class BattlePlusEncounterRules : EncounterRules {
    public BattlePlusEncounterRules() : base() { }

    public override void Build() {
      Main.Logger.Log("[BattlePlusEncounterRules] Setting up rule object references");
      BuildAi();
      BuildSpawns();
      BuildAdditionalLances("LanceEnemyOpposingForce", SpawnLogic.LookDirection.AWAY_FROM_TARGET,
         "SpawnerPlayerLance", SpawnLogic.LookDirection.AWAY_FROM_TARGET, 25f, 100f);
    }

    public void BuildAi() {
      EncounterLogic.Add(new IssueFollowLanceOrderTrigger(new List<string>(){ Tags.EMPLOYER_TEAM }, IssueAIOrderTo.ToLance, new List<string>() { Tags.PLAYER_1_TEAM }));
    }

    public void BuildSpawns() {
      Main.Logger.Log("[SimpleBattleEncounterRules] Building spawns rules");
      EncounterLogic.Add(new SpawnLanceAtEdgeOfBoundary(this, "SpawnerPlayerLance", "LanceEnemyOpposingForce", 400f));
    }

    public override void LinkObjectReferences(string mapName) {
      ObjectLookup.Add("LanceEnemyOpposingForce", EncounterLayerData.gameObject.FindRecursive("Lance_Target_EnemyForce"));
    }
  }
}