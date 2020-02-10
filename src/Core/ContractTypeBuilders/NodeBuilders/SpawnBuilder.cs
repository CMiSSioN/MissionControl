using UnityEngine;

using BattleTech;
using BattleTech.Designed;

using System;
using System.Collections.Generic;

using MissionControl.Rules;
using MissionControl.EncounterFactories;

using Newtonsoft.Json.Linq;

namespace MissionControl.ContractTypeBuilders {
  public class SpawnBuilder : NodeBuilder {
    private ContractTypeBuilder contractTypeBuilder;
    private GameObject parent;
    private string name;
    private string subType;
    private JObject position;
    private JObject rotation;
    private string team;
    private string guid;
    private int spawnPoints;
    private List<string> spawnPointGuids;
    private string spawnType;
    private JArray aiOrdersArray;
    private List<AIOrderBox> orders;

    public SpawnBuilder(ContractTypeBuilder contractTypeBuilder, GameObject parent, JObject spawner) {
      this.contractTypeBuilder = contractTypeBuilder;
      this.parent = parent;
      this.name = spawner["Name"].ToString();
      this.subType = spawner["SubType"].ToString();
      this.team = spawner["Team"].ToString();
      this.guid = spawner["Guid"].ToString();
      this.spawnPoints = (int)spawner["SpawnPoints"];
      this.spawnPointGuids = spawner["SpawnPointGuids"].ToObject<List<string>>();
      this.spawnType = spawner["SpawnType"].ToString();
      this.position = spawner.ContainsKey("Position") ? (JObject)spawner["Position"] : null;
      this.rotation = spawner.ContainsKey("Rotation") ? (JObject)spawner["Rotation"] : null;
      this.aiOrdersArray = spawner.ContainsKey("AI") ? (JArray)spawner["AI"] : null;

      if (this.aiOrdersArray != null) {
        AiOrderBuilder orderBuilder = new AiOrderBuilder(contractTypeBuilder, aiOrdersArray, name);
        orders = orderBuilder.Build();
      }
    }

    public override void Build() {
      SpawnUnitMethodType spawnMethodType = SpawnUnitMethodType.ViaLeopardDropship;
      switch (spawnType) {
        case "Leopard": spawnMethodType = SpawnUnitMethodType.ViaLeopardDropship; break;
        case "DropPod": spawnMethodType = SpawnUnitMethodType.DropPod; break;
        case "Instant": spawnMethodType = SpawnUnitMethodType.InstantlyAtSpawnPoint; break;
        default: Main.LogDebug($"[SpawnBuilder.{contractTypeBuilder.ContractTypeKey}] No support for spawnType '{spawnType}'. Check for spelling mistakes."); break;
      }

      string teamId = EncounterRules.PLAYER_TEAM_ID;
      switch (team) {
        case "Player1": {
          teamId = EncounterRules.PLAYER_TEAM_ID;
          PlayerLanceSpawnerGameLogic playerLanceSpawnerGameLogic = LanceSpawnerFactory.CreatePlayerLanceSpawner(parent, name, guid, teamId, true, spawnMethodType, spawnPointGuids, true);
          if (position != null) SetPosition(playerLanceSpawnerGameLogic.gameObject, position);
          if (rotation != null) SetRotation(playerLanceSpawnerGameLogic.gameObject, rotation);
          break;
        }
        case "Target": {
          teamId = EncounterRules.TARGET_TEAM_ID;
          LanceSpawnerGameLogic lanceSpawnerGameLogic = LanceSpawnerFactory.CreateLanceSpawner(parent, name, guid, teamId, true, spawnMethodType, spawnPointGuids);
          if (position != null) SetPosition(lanceSpawnerGameLogic.gameObject, position);
          if (rotation != null) SetRotation(lanceSpawnerGameLogic.gameObject, rotation);
          if (orders != null) lanceSpawnerGameLogic.aiOrderList.contentsBox = orders;
          break;
        }
        case "TargetAlly": {
          teamId = EncounterRules.TARGETS_ALLY_TEAM_ID;
          LanceSpawnerGameLogic lanceSpawnerGameLogic = LanceSpawnerFactory.CreateLanceSpawner(parent, name, guid, teamId, true, spawnMethodType, spawnPointGuids);
          if (position != null) SetPosition(lanceSpawnerGameLogic.gameObject, position);
          if (rotation != null) SetRotation(lanceSpawnerGameLogic.gameObject, rotation);
          if (orders != null) lanceSpawnerGameLogic.aiOrderList.contentsBox = orders;
          break;
        }
        case "Employer": {
          teamId = EncounterRules.EMPLOYER_TEAM_ID;
          LanceSpawnerGameLogic lanceSpawnerGameLogic = LanceSpawnerFactory.CreateLanceSpawner(parent, name, guid, teamId, true, spawnMethodType, spawnPointGuids);
          if (position != null) SetPosition(lanceSpawnerGameLogic.gameObject, position);
          if (rotation != null) SetRotation(lanceSpawnerGameLogic.gameObject, rotation);
          if (orders != null) lanceSpawnerGameLogic.aiOrderList.contentsBox = orders;
          break;
        }
        default: Main.LogDebug($"[SpawnBuilder.{contractTypeBuilder.ContractTypeKey}] No support for team '{team}'. Check for spelling mistakes."); break;
      }
    }
  }
}