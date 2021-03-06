using UnityEngine;

using System;
using System.Collections.Generic;

using BattleTech;
using BattleTech.Designed;
using BattleTech.Framework;

using HBS.Collections;

using MissionControl;

namespace MissionControl.EncounterFactories {
  public class ObjectiveFactory {
    public static DestroyLanceObjective CreateDestroyLanceObjective(string objectiveGuid, GameObject parent, LanceSpawnerRef lanceToDestroy, string lanceGuid, string title, bool showProgress,
    string progressFormat, string description, int priority, bool displayToUser, ObjectiveMark markUnitsWith, string contractObjectiveGameLogicGuid, bool createObjectiveOverride = true) {
      // TODO: Probably want to split out these two main chunks into their own methods
      // OBJECTIVE OBJECTIVE GAME LOGIC
      GameObject destroyWholeLanceObjectiveGo = new GameObject($"Objective_DestroyLance_{lanceGuid}");
      destroyWholeLanceObjectiveGo.transform.parent = parent.transform;
      destroyWholeLanceObjectiveGo.transform.localPosition = Vector3.zero;

      DestroyLanceObjective destroyLanceObjective = destroyWholeLanceObjectiveGo.AddComponent<DestroyLanceObjective>();
      destroyLanceObjective.encounterObjectGuid = objectiveGuid;
      destroyLanceObjective.title = title;
      destroyLanceObjective.showProgress = showProgress;
      destroyLanceObjective.progressFormat = progressFormat;
      destroyLanceObjective.description = description;
      destroyLanceObjective.priority = priority;
      destroyLanceObjective.displayToUser = displayToUser;
      destroyLanceObjective.markUnitsWith = markUnitsWith;
      destroyLanceObjective.lanceToDestroy = lanceToDestroy;

      // Rewards
      List<SimGameEventResult> onSuccessResults = new List<SimGameEventResult>();
      List<Dictionary<string, string>> rewards = Main.Settings.ActiveAdditionalLances.RewardsPerLance;

      foreach (Dictionary<string, string> reward in rewards) {
        string type = Main.Settings.ActiveAdditionalLances.GetRewardType(reward);
        float value = Main.Settings.ActiveAdditionalLances.GetRewardValue(reward);
        onSuccessResults.Add(CreateRewardResult(type, value));
      }

      destroyLanceObjective.OnSuccessResults = onSuccessResults;
      destroyLanceObjective.onSuccessDialogue = new DialogueRef();
      destroyLanceObjective.onFailureDialogue = new DialogueRef();

      ContractObjectiveGameLogic contractObjectiveGameLogic = null;
      if (contractObjectiveGameLogicGuid == null) {
        // For ease of user we track objectives, by default, against the first contract objective
        contractObjectiveGameLogic = MissionControl.Instance.EncounterLayerData.GetComponent<ContractObjectiveGameLogic>();
      } else {
        contractObjectiveGameLogic = MissionControl.Instance.EncounterLayerData.GetContractObjectiveGameLogicByGUID(contractObjectiveGameLogicGuid);
      }

      if (contractObjectiveGameLogic == null) {
        Main.Logger.LogError($"[CreateDestroyLanceObjective] Contract Objective is null!");
      }

      ObjectiveRef objectiveRef = new ObjectiveRef(destroyLanceObjective);
      contractObjectiveGameLogic.objectiveRefList.Add(objectiveRef);

      // OBJECTIVE OVERRIDE - This is needed otherwise the results don't apply in the 'End Contract' screen
      // IF - the objective override is not specified in the contract override .json
      if (createObjectiveOverride) {
        ObjectiveOverride objectiveOverride = new ObjectiveOverride(destroyLanceObjective);
        objectiveOverride.title = destroyLanceObjective.title;
        objectiveOverride.description = destroyLanceObjective.description;
        objectiveOverride.OnSuccessResults = destroyLanceObjective.OnSuccessResults;
        objectiveOverride.OnFailureResults = destroyLanceObjective.OnFailureResults;
        objectiveOverride.OnSuccessDialogueGUID = destroyLanceObjective.onSuccessDialogue.EncounterObjectGuid;
        objectiveOverride.OnFailureDialogueGUID = destroyLanceObjective.onFailureDialogue.EncounterObjectGuid;
        MissionControl.Instance.CurrentContract.Override.objectiveList.Add(objectiveOverride);
      }

      return destroyLanceObjective;
    }

    public static SimGameEventResult CreateRewardResult(string type, float value) {
      SimGameEventResult rewardResult = new SimGameEventResult();
      rewardResult.Scope = EventScope.Company;

      SimGameStat rewardStat = new SimGameStat(type, value);
      rewardResult.Stats = new SimGameStat[] { rewardStat };

      return rewardResult;
    }

    public static ContractObjectiveGameLogic CreateContractObjective(ObjectiveGameLogic objective) {
      ContractObjectiveGameLogic contractObjectiveGameLogic = objective.transform.parent.gameObject.AddComponent<ContractObjectiveGameLogic>();
      contractObjectiveGameLogic.encounterObjectGuid = Guid.NewGuid().ToString();
      contractObjectiveGameLogic.objectiveRefList.Add(new ObjectiveRef(objective));
      return contractObjectiveGameLogic;
    }

    public static OccupyRegionObjective CreateOccupyRegionObjective(string objectiveGuid, GameObject parent, string requiredLanceSpawnerGuid, string regionGameLogicGuid,
    string objectName, string title, string progressFormat, string description, bool useDropship) {
      GameObject occupyRegionObjectiveGo = new GameObject($"Objective_{objectName}");
      occupyRegionObjectiveGo.transform.parent = parent.transform;
      occupyRegionObjectiveGo.transform.localPosition = Vector3.zero;

      OccupyRegionObjective occupyRegionObjective = occupyRegionObjectiveGo.AddComponent<OccupyRegionObjective>();
      occupyRegionObjective.title = occupyRegionObjectiveGo.name;
      occupyRegionObjective.encounterObjectGuid = objectiveGuid;
      occupyRegionObjective.requiredTagsOnUnit = new TagSet(new string[] { "player_unit" });

      LanceSpawnerRef lanceSpawnerRef = new LanceSpawnerRef();
      lanceSpawnerRef.EncounterObjectGuid = requiredLanceSpawnerGuid;
      occupyRegionObjective.requiredLance = lanceSpawnerRef;

      occupyRegionObjective.durationType = DurationType.AfterMoveComplete;
      occupyRegionObjective.durationToOccupy = 1;

      occupyRegionObjective.applyTagsWhen = ApplyTagsWhen.OnCompleteObjective;

      occupyRegionObjective.requiredTagsOnOpposingUnits = new TagSet(new string[] { "opposing_unit" });

      RegionRef regionRef = new RegionRef();
      regionRef.EncounterObjectGuid = regionGameLogicGuid;
      occupyRegionObjective.occupyTargetRegion = regionRef;

      occupyRegionObjective.triggerDropshipFlybyPickupOnSuccess = useDropship;
      occupyRegionObjective.extractViaDropship = useDropship;
      occupyRegionObjective.title = title;
      occupyRegionObjective.showProgress = true;
      occupyRegionObjective.progressFormat = progressFormat;
      occupyRegionObjective.description = description;
      occupyRegionObjective.priority = 3;
      occupyRegionObjective.displayToUser = true;
      occupyRegionObjective.checkObjectiveFlag = false;
      occupyRegionObjective.useBeacon = true;
      occupyRegionObjective.markUnitsWith = ObjectiveMark.None;
      occupyRegionObjective.enableObjectiveLogging = true;

      occupyRegionObjective.onSuccessDialogue = new DialogueRef();

      return occupyRegionObjective;
    }
  }
}