using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ModestTree;
using ModKit;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Systems;
using UnityEngine;

namespace ArmyManager
{
    // ReSharper disable InconsistentNaming
    public class ArmiesUI
    {
        private static TIFactionState? Faction;

        private static TINationState? SelectedNation;
        private static TIRegionState? SelectedRegion;
        private static List<TIArmyState>? MovingArmies;

        private static GUIStyle FlagStyle = new GUIStyle()
        {
        };

        public static void OnGUI()
        {
            if (Faction == null) return;

            var nations = Faction.armies.Select(a => a.currentRegion)
                .DistinctBy(r => r.ID)
                .GroupBy(r => r.nation)
                .ToDictionary(r => r.Key, r => r.ToList());

            using (UI.VerticalScope())
            {
                UI.Space(20);
                using (UI.HorizontalScope())
                {
                    using (UI.VerticalScope((GUIStyle)"box", UI.MinWidth(350), UI.MaxWidth(350)))
                    {
                        UI.Space(10);
                        DrawRegionSelect(nations);
                    }

                    UI.Space(20);
                    using (UI.VerticalScope((GUIStyle)"box"))
                    {
                        UI.Space(10);
                        if (SelectedNation != null || SelectedRegion != null)
                            if (MovingArmies != null)
                                DrawMoveTo();
                            else
                                DrawArmies();
                        else
                            UI.Label(
                                "Select a country or region from the left to manage the armies in that location you have control of.",
                                UIStyles.Hint);

                        UI.Space(10);
                    }
                }
            }
        }

        public static void Update()
        {
            Faction = GameControl.control.activePlayer;
        }

        private static void DrawRegionSelect(Dictionary<TINationState, List<TIRegionState>> nations)
        {
            nations.ForEach(nation =>
            {
                using (UI.VerticalScope())
                {
                    DrawNationSelectButton(nation.Key);
                    UI.Space(10);
                    foreach (var region in nation.Value)
                    {
                        DrawRegionSelectButton(region);
                        UI.Space(10);
                    }

                    UI.Space(20);
                }
            });
        }

        private static void DrawArmies()
        {
            if (Faction == null || (SelectedNation == null && SelectedRegion == null)) return;

            var nation = SelectedNation != null ? SelectedNation : SelectedRegion!.nation;
            var region = SelectedRegion;

            var armies = Faction.armies.Where(IsInSelectedArea)
                .GroupBy(a => a.currentNation)
                .ToDictionary(r => r.Key, r => r.ToList());

            using (UI.VerticalScope())
            {
                DrawSelectedRegionHeader(nation, region);

                UI.Space(10);
                UI.Div();
                UI.Space(10);

                foreach (var armyNation in armies)
                {
                    DrawNationMoveHeader(armyNation.Key);

                    UI.Space(10);

                    DrawArmyHeaders();

                    UI.Space(10);

                    foreach (var army in armyNation.Value)
                    {
                        DrawArmyMove(army);
                        UI.Space(5);
                    }
                    UI.Space(10);
                }

                UI.Space(10);
                UI.Label("N = Has Navy, C = In Conflict".grey(), UIStyles.Hint);
                UI.Space(10);
            }
        }

        private static void DrawNationSelectButton(TINationState nation)
        {
            using (UI.HorizontalScope(UIStyles.AlignLeft))
            {
                var nationSelected = IsSelectedNation(nation);
                var glyph = "";
                if (nationSelected)
                    glyph = UI.ChecklyphOn;

                var title = $"{nation.displayName} {glyph}".ToUpper().bold();
                title = nationSelected ? title.cyan().italic() : title.orange();
                UIElements.Flag(nation);
                UI.Space(10);
                UI.ActionButton(title, () => ToggleNation(nation), UIStyles.NationSelectButton, UI.MinWidth(375));
            }
        }

        private static void DrawRegionSelectButton(TIRegionState region)
        {
            using (UI.HorizontalScope(UIStyles.AlignLeft))
            {
                var regionSelected = IsSelectedRegion(region);
                var glyph = "";
                if (regionSelected) glyph = UI.ChecklyphOn;

                var title = $"{UI.DisclosureGlyphEmpty} {region.displayName} {glyph}";
                if (regionSelected) title = title.cyan().bold();
                UI.Space(50);
                UI.ActionButton(title.ToUpper(), () => ToggleRegion(region), UIStyles.RegionSelectButton, UI.MinWidth(375));
            }
        }

        private static void DrawSelectedRegionHeader(TINationState nation, TIRegionState? region)
        {
            using (UI.HorizontalScope(UIStyles.AlignLeft))
            {
                var header = region == null ? nation.displayName : $"{nation.displayName} > {region.displayName}";

                var armies = Faction!.armies
                    .FindAll(a => a.currentNation == nation && (region == null || region == a.currentRegion));

                UIElements.Flag(nation);
                UI.Space(10);
                UI.Label(header.ToUpper().orange().bold(), UIStyles.Header, UI.MinWidth(773), UI.MaxWidth(773));
                UI.Space(10);
                UI.ActionButton("HOME", () => { MoveHome(armies); }, UIStyles.SimpleButton, UI.MinWidth(100), UI.MaxWidth(100));
                UI.Space(10);
                UI.ActionButton("MOVE", () => { MoveArmies(armies); }, UIStyles.SimpleButton, UI.MinWidth(100), UI.MaxWidth(100));
            }
        }

        private static void DrawNationMoveHeader(TINationState nation)
        {
            var armies = Faction!.armies.FindAll(a => a.currentNation == nation);
            using (UI.HorizontalScope())
            {
                UI.Space(7);
                UIElements.Flag(nation);
                UI.Space(10);
                UI.Label(nation.displayName.ToUpper().orange().bold(), UI.MinWidth(773), UI.MaxWidth(773));
                UI.Space(10);
                UI.ActionButton("HOME", () => { MoveHome(armies); }, UIStyles.SimpleButton, UI.MinWidth(100), UI.MaxWidth(100));
                UI.Space(10);
                UI.ActionButton("MOVE", () => { MoveArmies(armies); }, UIStyles.SimpleButton, UI.MinWidth(100), UI.MaxWidth(100));
            }
        }

        private static void DrawArmyHeaders()
        {
            using (UI.HorizontalScope())
            {
                UI.Space(20);
                UI.Label("ARMY".bold(), UIStyles.Label, UI.MinWidth(275), UI.MaxWidth(275));
                UI.Space(10);
                UI.Label("STR".bold(), UIStyles.Label, UI.MinWidth(50), UI.MaxWidth(50));
                UI.Space(10);
                UI.Label("".bold(), UIStyles.Label, UI.MinWidth(50), UI.MaxWidth(50));
                UI.Space(10);
                UI.Label("HOME REGION".bold(), UIStyles.Label, UI.MinWidth(200), UI.MaxWidth(200));
                UI.Space(10);
                UI.Label("CURRENT REGION".bold(), UIStyles.Label, UI.MinWidth(200), UI.MaxWidth(200));
                UI.Space(10);
            }
        }

        private static void DrawArmyMove(TIArmyState army)
        {
            var value = army.strength;
            var healthText = value.ToPercent("P0").green();
            if (value < 0.2)
                healthText = healthText.red();
            else if (value < 0.5)
                healthText = healthText.orange();
            else if (value < 1.0)
                healthText = healthText.yellow();

            var currentRegion = army.currentRegion;
            var isAtHome = currentRegion == army.homeRegion;
            var canGoHome = CanArmyGoHome(army);
            var canMove = CanArmyMove(army);
            
            var regionName = currentRegion.displayName.ToUpper();
            if (isAtHome) regionName = regionName.green();

            var flags = "";
            if (army.deploymentType == DeploymentType.Naval) flags += "N".cyan() + " ";
            if (army.IsFighting(false)) flags += "C".red() + " ";

            Action moveHomeAction = () => { }; ;
            if (canGoHome)
                moveHomeAction = () => { MoveHome(army); };

            Action moveAction = () => { };
            if (canMove)
                moveAction = () => { MoveArmy(army); };

            Action cancelAction = army.ClearOperations;

            var homeLabel = canGoHome ? "HOME" : "HOME".grey();
            var moveLabel = canMove ? "MOVE" : "MOVE".grey();

            using (UI.HorizontalScope())
            {
                UI.Space(20);
                UI.Label($"{UI.DisclosureGlyphEmpty} {army.displayName}".ToUpper(), UIStyles.Label, UI.MinWidth(275), UI.MaxWidth(275));
                UI.Space(10);
                UI.Label(healthText, UIStyles.Label, UI.MinWidth(50), UI.MaxWidth(50));
                UI.Space(10);
                UI.Label(flags, UIStyles.Label, UI.MinWidth(50), UI.MaxWidth(50));
                UI.Space(10);
                UI.Label(army.homeRegion.displayName.ToUpper(), UIStyles.Label, UI.MinWidth(200), UI.MaxWidth(200));
                UI.Space(10);
                UI.Label(regionName, UIStyles.Label, UI.MinWidth(200), UI.MaxWidth(200));
                UI.Space(10);
                UI.ActionButton(homeLabel, moveHomeAction, UIStyles.SimpleButton, UI.MinWidth(100), UI.MaxWidth(100));
                UI.Space(10);
                if (army.IsMoving)
                    UI.ActionButton("CANCEL", cancelAction, UIStyles.SimpleButton, UI.MinWidth(100), UI.MaxWidth(100));
                else
                    UI.ActionButton(moveLabel, moveAction, UIStyles.SimpleButton, UI.MinWidth(100), UI.MaxWidth(100));
            }

            if (!army.IsMoving) return;
            UI.Space(10);
            UI.Label(army.OperationDescription().grey(), UIStyles.MovingHint, UI.MaxWidth(UI.ummWidth));
        }

        private static void DrawMoveTo()
        {
            if (MovingArmies == null) return;

            var nations = MovingArmies.SelectMany(a => a.ReachableRegions)
                .Distinct()
                .GroupBy(r => r.nation)
                .ToDictionary(r => r.Key, r => r.ToList());

            using (UI.VerticalScope())
            {
                UI.Space(10);
                using (UI.HorizontalScope(UIStyles.AlignLeft))
                {
                    UI.Label("MOVE ARMIES TO...".bold().orange(), UIStyles.Header, UI.MinWidth(1003), UI.MaxWidth(1003));
                    UI.Space(10);
                    UI.ActionButton(" X ", () => { MovingArmies = null; }, UIStyles.SimpleButton, UI.MinWidth(50), UI.MaxWidth(50));
                }
                UI.Space(10);

                foreach (var nation in nations)
                {
                    using (UI.VerticalScope())
                    {
                        using (UI.HorizontalScope())
                        {
                            UI.Space(7);
                            UIElements.Flag(nation.Key);
                            UI.Space(10);
                            UI.Label(nation.Key.displayName.ToUpper().orange().bold(), UI.MinWidth(773), UI.MaxWidth(773));
                        }

                        UI.Space(10);

                        var regions = nation.Value.Chunk(5);
                        
                        foreach (var chunk in regions)
                        {
                            using (UI.HorizontalScope())
                            {
                                foreach (var region in chunk)
                                {
                                    UI.ActionButton(region.displayName.ToUpper(), () => DoMoveArmies(region), UIStyles.SimpleButton, UI.MinWidth(200), UI.MaxWidth(200));
                                    UI.Space(5);
                                }
                            }
                            UI.Space(5);
                        }
                    }
                    UI.Space(10);
                }
            }
        }

        private static void MoveHome(TIArmyState army)
        {
            MoveHome(new List<TIArmyState>() { army });
        }

        private static void MoveHome(List<TIArmyState> armies)
        {
            armies
                .FindAll(CanArmyGoHome)
                .ForEach(a =>
                {
                    var operation = new DeployArmyOperation_TargetHome();
                    operation.OnOperationConfirm(a, a.homeRegion);
                });
        }

        private static void MoveArmy(TIArmyState army)
        {
            MoveArmies(new List<TIArmyState>() { army });
        }

        private static void MoveArmies(List<TIArmyState> armies)
        {
            MovingArmies = armies;
        }

        private static void DoMoveArmies(TIRegionState region)
        {
            MovingArmies?.FindAll(a => a.CanGetTo(region))
                .ForEach(a =>
                    {
                        var operation = new DeployArmyOperation_OpenTarget(true);
                        operation.OnOperationConfirm(a, region);
                    }
                );

            MovingArmies = null;
        }

        private static void ToggleRegion(TIRegionState region)
        {
            SelectedNation = null;
            MovingArmies = null;
            SelectedRegion = SelectedRegion?.ID == region.ID ? null : region;
        }

        private static void ToggleNation(TINationState nation)
        {
            SelectedRegion = null;
            MovingArmies = null;
            SelectedNation = SelectedNation?.ID == nation.ID ? null : nation;
        }

        private static bool IsInSelectedArea(TIArmyState army) =>
            (SelectedNation != null && army.currentRegion.nation == SelectedNation) ||
            (SelectedRegion != null && army.currentRegion == SelectedRegion);

        private static bool IsSelectedNation(TINationState nation) => SelectedNation?.ID == nation.ID;
        private static bool IsSelectedRegion(TIRegionState region) => SelectedRegion?.ID == region.ID;

        private static bool CanArmyGoHome(TIArmyState army) => army.currentRegion != army.homeRegion && army.currentOperations.IsEmpty() && !army.homeNation.atWar && army.CanGetTo(army.homeRegion);

        private static bool CanArmyMove(TIArmyState army) => army.currentOperations.IsEmpty() && army.CanGetTo(army.homeRegion);
    }
}