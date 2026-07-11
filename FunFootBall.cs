using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace FunMatchPlugin;

using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;

public class FunFootBall : FunBaseClass
{
    public override string Decription => "FootBall Mode T aims to take soccerball to CTspawn 足球模式 T需要将球踢进CT出生点";

    private CPhysicsProp? SoccerBall;
    private CDynamicProp? ScoreFlag;
    private Timer? BallStatusTimer;
    private Vector ScoreCenter = new(0, 0, 0);
    private Vector SoccerLastPosition = new(0, 0, 0);
    private Vector SoccerSpawnPosition = new(0, 0, 0);
    private int soccer_same_pos_times = 0;
    private BasePlugin.GameEventHandler<EventRoundFreezeEnd>? EventRoundFreezeEndHandler;
    private BasePlugin.GameEventHandler<EventRoundEnd>? EventRoundEndHandler;
    private float previousBuyTime;
    private bool previousAutoKick;
    private int previousBuyDuringImmunity;
    private bool hasConVarSnapshot;
    public override void EndFun(FunMatchPlugin plugin)
    {
        Enabled = false;
        if (hasConVarSnapshot)
        {
            ConVar.Find("mp_buytime")!.SetValue(previousBuyTime);
            ConVar.Find("mp_autokick")!.SetValue(previousAutoKick);
            ConVar.Find("mp_buy_during_immunity")!.SetValue(previousBuyDuringImmunity);
            hasConVarSnapshot = false;
        }
        if (EventRoundFreezeEndHandler is not null)
            plugin.DeregisterEventHandler(EventRoundFreezeEndHandler);
        if (EventRoundEndHandler is not null)
            plugin.DeregisterEventHandler(EventRoundEndHandler);
        EventRoundFreezeEndHandler = null;
        EventRoundEndHandler = null;
        if (BallStatusTimer is not null)
            BallStatusTimer.Kill();
        BallStatusTimer = null;
        RemoveRoundEntities();
    }
    public override void Fun(FunMatchPlugin plugin)
    {
        if (Enabled) return;
        Enabled = true;
        ScoreCenter = new(0, 0, 0);
        SoccerLastPosition = new(0, 0, 0);
        SoccerSpawnPosition = new(0, 0, 0);
        soccer_same_pos_times = 0;
        previousBuyTime = ConVar.Find("mp_buytime")!.GetPrimitiveValue<float>();
        previousAutoKick = ConVar.Find("mp_autokick")!.GetPrimitiveValue<bool>();
        previousBuyDuringImmunity = ConVar.Find("mp_buy_during_immunity")!.GetPrimitiveValue<int>();
        hasConVarSnapshot = true;
        ConVar.Find("mp_buytime")!.SetValue(0.0f);
        ConVar.Find("mp_autokick")!.SetValue(false);
        ConVar.Find("mp_buy_during_immunity")!.SetValue(1);
        var Allplayers = Utilities.GetPlayers();
        foreach (var p in Allplayers)
        {
            if (!p.IsValid || !p.PawnIsAlive) continue;
            p.RemoveWeapons();
        }
        /*
        var gameRulesProxie = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
        var gameRules = gameRulesProxie.First().GameRules!;
        var spawnpoints_ct = gameRules.CTSpawnPoints.GetEnumerator();
        foreach (var sp in spawnpoints_ct)
        {
            //ScoreCenter.X += sp!.AbsOrigin!.X;
            //ScoreCenter.Y += sp!.AbsOrigin!.Y;
            //ScoreCenter.Z += sp!.AbsOrigin!.Z;
            Console.WriteLine($"scorecenter{sp!.AbsOrigin!.X} {sp!.AbsOrigin!.Y} {sp!.AbsOrigin!.Z}");
        }
        ScoreCenter.X = ScoreCenter.X/gameRules.SpawnPointCount_CT;
        ScoreCenter.Y = ScoreCenter.Y/gameRules.SpawnPointCount_CT;
        ScoreCenter.Z = ScoreCenter.Z/gameRules.SpawnPointCount_CT;
        Console.WriteLine($"scorecenter{ScoreCenter.X} {ScoreCenter.Y} {ScoreCenter.Z}");
        */
        plugin.RegisterEventHandler<EventRoundFreezeEnd>(EventRoundFreezeEndHandler = (@event, info) =>
        {
            if (!Enabled) return HookResult.Continue;
            BallStatusTimer?.Kill();
            RemoveRoundEntities();
            var Allplayers = Utilities.GetPlayers();
            bool HasGivenBall = false;
            bool HasSetScorePoint = false;
            //mp_buytime
            foreach (var p in Allplayers)
            {
                if (!p.IsValid || !p.PawnIsAlive) continue;
                p.RemoveWeapons();
                if (p.PawnIsAlive && p.Team == CsTeam.Terrorist && HasGivenBall == false)
                {
                    var pawn = p.PlayerPawn.Get();
                    SoccerBall = Utilities.CreateEntityByName<CPhysicsProp>("prop_physics_multiplayer");
                    if (pawn?.AbsOrigin is null || SoccerBall is null) continue;
                    HasGivenBall = true;
                    Vector position = new(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + 100);
                    SoccerBall.Teleport(position);
                    SoccerBall.SetModel("models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl");
                    //models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl_c
                    SoccerSpawnPosition.X = SoccerBall!.AbsOrigin!.X;
                    SoccerSpawnPosition.Y = SoccerBall!.AbsOrigin!.Y;
                    SoccerSpawnPosition.Z = SoccerBall!.AbsOrigin!.Z;
                    //SoccerBall.GlowColor = Color.GreenYellow;

                    SoccerBall.DispatchSpawn();
                }
                if (p.PawnIsAlive && p.Team == CsTeam.CounterTerrorist && HasSetScorePoint == false)
                {
                    //models/props_fairgrounds/fairgrounds_flagpole01.vmdl
                    ScoreFlag = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
                    var pawn = p.PlayerPawn.Get();
                    if (pawn?.AbsOrigin is null || ScoreFlag is null) continue;
                    HasSetScorePoint = true;
                    //Vector position = new(pawn!.AbsOrigin!.X,pawn!.AbsOrigin!.Y,pawn!.AbsOrigin!.Z);
                    ScoreFlag.Teleport(pawn!.AbsOrigin);
                    ScoreFlag.SetModel("models/props_fairgrounds/fairgrounds_flagpole01.vmdl");
                    ScoreFlag.DispatchSpawn();
                    ScoreCenter = ScoreFlag.AbsOrigin!;
                }
            }
            //CCSPlayerResource siteA siteB position?
            //Utilities.FindAllEntitiesByDesignerName<CCSPlayerResource>("???");
            BallStatusTimer = plugin.AddTimer(1, () => CheckBallStatus(), TimerFlags.REPEAT);
            return HookResult.Stop;
        });

        plugin.RegisterEventHandler<EventRoundEnd>(EventRoundEndHandler = (@event, info) =>
        {
            BallStatusTimer?.Kill();
            BallStatusTimer = null;
            RemoveRoundEntities();
            return HookResult.Continue;
        });
    }

    private void CheckBallStatus()
    {
        if (Enabled == false)
        {
            BallStatusTimer?.Kill();
            return;
        }
        if (SoccerBall == null)
        {
            BallStatusTimer?.Kill();
            return;
        }
        if (IsBallIn())
        {
            TWin();
            return;
        }
        if (IsBallFalling() || IsBallStuck())
        {
            if (SoccerBall!.IsValid)
                SoccerBall.Teleport(SoccerSpawnPosition);
            SoccerLastPosition.X = SoccerSpawnPosition.X;
            SoccerLastPosition.Y = SoccerSpawnPosition.Y;
            SoccerLastPosition.Z = SoccerSpawnPosition.Z;
        }
    }

    private bool IsBallIn()
    {
        if (!SoccerBall!.IsValid) return false;
        var ball_x = SoccerBall.AbsOrigin!.X;
        var ball_y = SoccerBall.AbsOrigin!.Y;
        var ball_z = SoccerBall.AbsOrigin!.Z;
        if (Math.Abs(ScoreCenter.X - ball_x) <= 500 && Math.Abs(ScoreCenter.Y - ball_y) <= 500 && Math.Abs(ScoreCenter.Z - ball_z) <= 50)
            return true;
        else return false;
    }

    private void TWin()
    {
        var gameRulesProxie = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
        var gameRules = gameRulesProxie.FirstOrDefault()?.GameRules;
        if (gameRules is null || gameRules.WarmupPeriod)
            return;
        var Allplayers = Utilities.GetPlayers();
        foreach (var p in Allplayers)
        {
            if (!p.IsValid) continue;
            if (p.Team == CsTeam.CounterTerrorist)
            {
                p.CommitSuicide(false, true);
            }
        }
    }

    private bool IsBallStuck()
    {
        if (SoccerBall!.AbsOrigin!.X == SoccerLastPosition.X && SoccerBall!.AbsOrigin.Y == SoccerLastPosition.Y && SoccerBall!.AbsOrigin.Z == SoccerLastPosition.Z)
        {
            soccer_same_pos_times++;
        }
        else
        {
            soccer_same_pos_times = 0;
            SoccerLastPosition.X = SoccerBall!.AbsOrigin!.X;
            SoccerLastPosition.Y = SoccerBall!.AbsOrigin!.Y;
            SoccerLastPosition.Z = SoccerBall!.AbsOrigin!.Z;
        }
        if (soccer_same_pos_times >= 5)
        {
            Server.PrintToChatAll(StringExtensions.ReplaceColorTags("{RED}") + "[FootBallMode] " + "Ball Stuck Resetting");
            return true;
        }

        return false;
    }

    private bool IsBallFalling()
    {
        if (SoccerBall!.AbsOrigin!.Z - SoccerLastPosition.Z < -400)
        {
            Server.PrintToChatAll(StringExtensions.ReplaceColorTags("{RED}") + "[FootBallMode] " + "Ball Falling Resetting");
            return true;
        }
        return false;
    }

    private void RemoveRoundEntities()
    {
        if (SoccerBall is not null && SoccerBall.IsValid) SoccerBall.Remove();
        if (ScoreFlag is not null && ScoreFlag.IsValid) ScoreFlag.Remove();
        SoccerBall = null;
        ScoreFlag = null;
    }
}
