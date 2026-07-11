namespace FunMatchPlugin;

using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

// gameRules not working https://github.com/roflmuffin/CounterStrikeSharp/issues/489
public class FunC4EveryWhere : FunBaseClass
{
    public override string Decription => "C4 EveryWhere 10s to explode C4大战 10s爆炸";
    public FunC4EveryWhere(FunMatchPlugin plugin) : base(plugin){}
    public CCSGameRules? gameRules = null;
    private Timer? roundTimer = null;

    private Timer? roundTimer90 = null;
    private Timer? roundTimer60 = null;
    private Timer? roundTimer110 = null;
    private bool TeamHasWon = false;
    private CTeam? Team_CT;
    private CTeam? Team_T;

    private BasePlugin.GameEventHandler<EventBombPlanted>? EventBombPlantedHandler=null;
    private BasePlugin.GameEventHandler<EventPlayerDeath>? EventPlayerDeathHandler=null;
    private bool previousAutoKick;
    private int previousC4Timer;
    private bool previousPlantAnywhere;
    private bool previousCannotBeDefused;
    private bool previousAnyoneCanPickup;
    private bool previousIgnoreRoundWinConditions;
    private bool hasConVarSnapshot;
    public override void Fun(FunMatchPlugin plugin)
    {
        if (Enabled) return;
        Enabled = true;
        Team_CT = null;
        Team_T = null;
        previousAutoKick = ConVar.Find("mp_autokick")!.GetPrimitiveValue<bool>();
        previousC4Timer = ConVar.Find("mp_c4timer")!.GetPrimitiveValue<int>();
        previousPlantAnywhere = ConVar.Find("mp_plant_c4_anywhere")!.GetPrimitiveValue<bool>();
        previousCannotBeDefused = ConVar.Find("mp_c4_cannot_be_defused")!.GetPrimitiveValue<bool>();
        previousAnyoneCanPickup = ConVar.Find("mp_anyone_can_pickup_c4")!.GetPrimitiveValue<bool>();
        previousIgnoreRoundWinConditions = ConVar.Find("mp_ignore_round_win_conditions")!.GetPrimitiveValue<bool>();
        hasConVarSnapshot = true;
        WithCheats(() =>
        {
            ConVar.Find("mp_autokick")!.SetValue(false);
            ConVar.Find("mp_c4timer")!.SetValue(10);
            ConVar.Find("mp_plant_c4_anywhere")!.SetValue(true);
            ConVar.Find("mp_c4_cannot_be_defused")!.SetValue(true);
            ConVar.Find("mp_anyone_can_pickup_c4")!.SetValue(true);
        });

        var gameRulesProxie = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
        var CTeamArray = Utilities.FindAllEntitiesByDesignerName<CTeam>("cs_team_manager").ToArray();
        gameRules = gameRulesProxie.FirstOrDefault()?.GameRules;
        foreach (var t in CTeamArray)
        {
            if (t.Teamname == "CT") 
            Team_CT = t;
            if (t.Teamname == "TERRORIST") 
            Team_T = t;
        }

        if (gameRules is null || Team_CT is null || Team_T is null)
        {
            Console.WriteLine("[C4 everywhere] Exception No Gamesrules/Team CTorT Found");
            EndFun(plugin);
            return;
        }

        var Allplayers = Utilities.GetPlayers();
        foreach (var p in Allplayers)
        {
            if (!p.IsValid) continue;
            p.RemoveWeapons();
            p.GiveNamedItem(CsItem.C4);
            p.GiveNamedItem(CsItem.Knife);
            p.RemoveAllItemsOnNextRoundReset = true;
        }
        plugin.RegisterEventHandler<EventBombPlanted> (EventBombPlantedHandler = (@event,info)=>
        {
            if (Enabled == false) return HookResult.Stop;
            if (@event.Userid is null) return HookResult.Continue;
            var player = @event.Userid.OriginalControllerOfCurrentPawn.Get();
            if (player is null || !player.IsValid) return HookResult.Continue;
            player.GiveNamedItem(CsItem.C4);
            return HookResult.Continue;
        });

        if(gameRules.WarmupPeriod) return;

        roundTimer = plugin.AddTimer(120, ()=>CTWin() , TimerFlags.STOP_ON_MAPCHANGE);
        roundTimer60 = plugin.AddTimer(60, ()=>Server.PrintToChatAll(StringExtensions.ReplaceColorTags("{RED}") + "[C4 Everywhere] 60s Left 剩余60s") , TimerFlags.STOP_ON_MAPCHANGE);
        roundTimer90 = plugin.AddTimer(90, ()=>Server.PrintToChatAll(StringExtensions.ReplaceColorTags("{RED}") + "[C4 Everywhere] 30s Left 剩余30s") , TimerFlags.STOP_ON_MAPCHANGE);
        roundTimer110 = plugin.AddTimer(110, ()=>Server.PrintToChatAll(StringExtensions.ReplaceColorTags("{RED}") + "[C4 Everywhere] 10s Left 剩余10s") , TimerFlags.STOP_ON_MAPCHANGE);

        plugin.RegisterEventHandler<EventPlayerDeath> (EventPlayerDeathHandler = (@event,info)=>
        {
            if (Enabled == false) return HookResult.Stop;
            if (TeamHasWon == true) return HookResult.Stop;
            int ct_num = AlivePlayerNum(CsTeam.CounterTerrorist);
            int t_num = AlivePlayerNum(CsTeam.Terrorist);
            if (@event.Userid is null || !@event.Userid.IsValid) return HookResult.Continue;
            if (@event.Userid.Team == CsTeam.CounterTerrorist)
            ct_num --;
            if (@event.Userid.Team == CsTeam.Terrorist)
            t_num --;
            if (t_num == 0 && ct_num == 0)
            {
                //gameRules.TerminateRound(3.0f,RoundEndReason.RoundDraw);
                TerminateRoundFix(3.0f,RoundEndReason.RoundDraw);
                KillRoundTimers();
                TeamHasWon = true;
                var q = @event.Userid.Team;
            }
            else if (t_num == 0)
            {
                //gameRules.TerminateRound(3.0f,RoundEndReason.CTsWin);
                TerminateRoundFix(3.0f,RoundEndReason.TargetBombed);
                KillRoundTimers();
                TeamHasWon = true;
                Team_CT!.Score++;
                Utilities.SetStateChanged(Team_CT,"CTeam", "m_iScore");
            }
            else if (ct_num == 0)
            {
                //gameRules.TerminateRound(3.0f,RoundEndReason.TerroristsWin);
                TerminateRoundFix(3.0f,RoundEndReason.BombDefused);
                KillRoundTimers();
                TeamHasWon = true;
                Team_T!.Score++;
                Utilities.SetStateChanged(Team_T,"CTeam", "m_iScore");
            }
            return HookResult.Continue;
        });

    }
    private void CTWin()
    {
        if (!Enabled) return;
        //gameRules!.TerminateRound(0.1f,RoundEndReason.CTsWin);
        //gameRules!.RoundWinStatus = 8;
        //gameRules!.TotalRoundsPlayed++;
        //gameRules!.ITotalRoundsPlayed++;
        TerminateRoundFix(3.0f,RoundEndReason.CTsWin);
        KillRoundTimers();
        TeamHasWon = true;
        Team_CT!.Score++;
        Utilities.SetStateChanged(Team_CT,"CTeam", "m_iScore");

    }

    private int AlivePlayerNum(CsTeam csTeam)
    {
        int players = 0;
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid && player.Connected == PlayerConnectedState.PlayerConnected && player.PawnIsAlive))
        {
            if (player.Team == csTeam)
            players++;
        }
        return players;
    } 

    public override void EndFun(FunMatchPlugin plugin)
    {
        Enabled = false;
        if (hasConVarSnapshot)
        {
            WithCheats(() =>
            {
                ConVar.Find("mp_autokick")!.SetValue(previousAutoKick);
                ConVar.Find("mp_c4timer")!.SetValue(previousC4Timer);
                ConVar.Find("mp_plant_c4_anywhere")!.SetValue(previousPlantAnywhere);
                ConVar.Find("mp_c4_cannot_be_defused")!.SetValue(previousCannotBeDefused);
                ConVar.Find("mp_anyone_can_pickup_c4")!.SetValue(previousAnyoneCanPickup);
                ConVar.Find("mp_ignore_round_win_conditions")!.SetValue(previousIgnoreRoundWinConditions);
            });
            hasConVarSnapshot = false;
        }
        KillRoundTimers();
        TeamHasWon = false;
        if (EventBombPlantedHandler is not null)
        plugin.DeregisterEventHandler(EventBombPlantedHandler);
        if (EventPlayerDeathHandler is not null)
        plugin.DeregisterEventHandler(EventPlayerDeathHandler);
        EventBombPlantedHandler = null;
        EventPlayerDeathHandler = null;
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid))
            player.RemoveAllItemsOnNextRoundReset = false;

    }

    private void KillRoundTimers()
    {
        roundTimer?.Kill();
        roundTimer60?.Kill();
        roundTimer90?.Kill();
        roundTimer110?.Kill();
        roundTimer = null;
        roundTimer60 = null;
        roundTimer90 = null;
        roundTimer110 = null;
    }

    // to be fixed in https://github.com/roflmuffin/CounterStrikeSharp/issues/489
    public void TerminateRoundFix(float delay, RoundEndReason roundEndReason)
    {
        if (gameRules is null) return;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            TerminateRound(gameRules!.Handle,delay, roundEndReason, 0, 0);
        else
            TerminateRoundLinux(gameRules!.Handle, roundEndReason, 0, 0, delay);
            
    }
    public static MemoryFunctionVoid<nint, float, RoundEndReason, nint, uint> TerminateRoundFunc =
        new(GameData.GetSignature("CCSGameRules_TerminateRound"));
    public static Action<IntPtr, float, RoundEndReason, nint, uint> TerminateRound = TerminateRoundFunc.Invoke;
    public static MemoryFunctionVoid<nint, RoundEndReason, nint, uint, float> TerminateRoundLinuxFunc =
    new(GameData.GetSignature("CCSGameRules_TerminateRound"));
    public static Action<IntPtr, RoundEndReason, nint, uint, float> TerminateRoundLinux = TerminateRoundLinuxFunc.Invoke;
    
}

