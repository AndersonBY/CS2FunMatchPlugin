using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Cvars;

namespace FunMatchPlugin;

public class FunJumpOrDie : FunBaseClass
{
    public float BurnAfterSecond = 1F;
    public override string Decription => "Jump OR Die 地板烫脚";
    public int BurnDamage = 5;

    private class playerTimer
    {
        public CounterStrikeSharp.API.Modules.Timers.Timer? timer;

        public playerTimer(){}

        public playerTimer(CounterStrikeSharp.API.Modules.Timers.Timer? t) {timer = t;}
    }

    private Dictionary<int,playerTimer> playerTimersDict = new ();
    private BasePlugin.GameEventHandler<EventPlayerJump>? EventPlayerJumpHandler;
    private BasePlugin.GameEventHandler<EventRoundFreezeEnd>? EventRoundFreezeEndHandler;
    private BasePlugin.GameEventHandler<EventPlayerDisconnect>? EventPlayerDisconnectHandler;
    private BasePlugin.GameEventHandler<EventPlayerConnectFull>? EventPlayerConnectFullHandler;
    private bool previousAutoKick;
    private bool hasAutoKickSnapshot;
    public FunJumpOrDie (FunMatchPlugin plugin) : base (plugin){}
    public override void Fun(FunMatchPlugin plugin)
    {
        if (Enabled) return;
        Enabled = true;
        previousAutoKick = ConVar.Find("mp_autokick")!.GetPrimitiveValue<bool>();
        hasAutoKickSnapshot = true;
        ConVar.Find("mp_autokick")!.SetValue(false);
        plugin.RegisterEventHandler <EventRoundFreezeEnd>(EventRoundFreezeEndHandler = (@event, info) =>
        {
            
            if (Enabled == false) return HookResult.Stop;
            var Allplayers = Utilities.GetPlayers();
            foreach (var p in Allplayers)
            {
                if (p.UserId is null || (int)p.UserId < 0 || !p.PawnIsAlive || p.PlayerPawn is null) continue;
                //if (p.IsBot) continue;
                CCSPlayerPawn ?pawn = p.OriginalControllerOfCurrentPawn.Get()?.PlayerPawn.Get();
                if (pawn is null) continue;
                ReplacePlayerTimer(plugin, (int)p.UserId, pawn);
            }
            return HookResult.Stop;
        });

        plugin.RegisterEventHandler <EventPlayerDisconnect> (EventPlayerDisconnectHandler = (@event, info) =>
        {
            
            if (Enabled == false) return HookResult.Stop;
            playerTimer ?playerTimer;
            if (@event.Userid?.UserId is null) return HookResult.Continue;
            playerTimersDict.TryGetValue((int)@event.Userid.UserId,out playerTimer);
            if (playerTimer is null) return HookResult.Continue;
            if (playerTimer.timer is not null) playerTimer.timer.Kill();
            playerTimersDict.Remove((int)@event.Userid.UserId);
            return HookResult.Continue;
        });

        plugin.RegisterEventHandler <EventPlayerConnectFull> (EventPlayerConnectFullHandler = (@event, info) =>
        {
            
            if (Enabled == false) return HookResult.Stop;
            if (@event.Userid is null || !@event.Userid.IsValid || @event.Userid.UserId is null) return HookResult.Continue;
            var oringin = @event.Userid.OriginalControllerOfCurrentPawn.Get();
            if (oringin is null) return HookResult.Continue;
            CCSPlayerPawn ?pawn = oringin.PlayerPawn.Get();
            if (pawn is null) return HookResult.Continue;
            ReplacePlayerTimer(plugin, (int)@event.Userid.UserId, pawn);
            return HookResult.Continue;
        });

        plugin.RegisterEventHandler <EventPlayerJump>(EventPlayerJumpHandler = (@event, info) =>
        {
            if (Enabled == false) return HookResult.Stop;
            if (@event is null) return HookResult.Continue;
            if (@event.Userid is null) return HookResult.Continue;
            if (@event.Userid.UserId is null) return HookResult.Continue;
            var pawn = @event.Userid.OriginalControllerOfCurrentPawn.Get()?.PlayerPawn.Get();
            if (pawn is null) return HookResult.Continue;
            ReplacePlayerTimer(plugin, (int)@event.Userid.UserId, pawn);
            return HookResult.Continue;
        });
    }

    public override void EndFun(FunMatchPlugin plugin)
    {
        Enabled = false;
        if (hasAutoKickSnapshot)
        {
            ConVar.Find("mp_autokick")!.SetValue(previousAutoKick);
            hasAutoKickSnapshot = false;
        }
        if (EventPlayerJumpHandler is not null) plugin.DeregisterEventHandler(EventPlayerJumpHandler);
        if (EventRoundFreezeEndHandler is not null) plugin.DeregisterEventHandler(EventRoundFreezeEndHandler);
        if (EventPlayerDisconnectHandler is not null) plugin.DeregisterEventHandler(EventPlayerDisconnectHandler);
        if (EventPlayerConnectFullHandler is not null) plugin.DeregisterEventHandler(EventPlayerConnectFullHandler);
        EventPlayerJumpHandler = null;
        EventRoundFreezeEndHandler = null;
        EventPlayerDisconnectHandler = null;
        EventPlayerConnectFullHandler = null;

        foreach (var value in playerTimersDict.Values)
        {
            if (value.timer is not null)
            value.timer.Kill();
        }
        playerTimersDict.Clear();
    }
    private void BurnPlayer(CCSPlayerPawn pawn)
    {
        if (!Enabled || !pawn.IsValid || pawn.Health <= 0) return;
        pawn.Health -= BurnDamage;
        pawn.ApplyStressDamage = true;
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        if (pawn.Health <= 0)
        {
            pawn.CommitSuicide(true,true);
        }
    }

    private void ReplacePlayerTimer(FunMatchPlugin plugin, int userId, CCSPlayerPawn pawn)
    {
        if (playerTimersDict.Remove(userId, out var oldTimer)) oldTimer.timer?.Kill();
        playerTimersDict[userId] = new playerTimer(plugin.AddTimer(BurnAfterSecond, () => BurnPlayer(pawn), TimerFlags.REPEAT));
    }
}
