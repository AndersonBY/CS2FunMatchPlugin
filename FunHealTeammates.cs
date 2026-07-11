namespace FunMatchPlugin;

using System.Data.Common;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;



public class FunHealTeammates : FunBaseClass
{
    public override string Decription => "Heal Teammates 治疗队友";

    public FunHealTeammates(FunMatchPlugin plugin) : base(plugin){}
    public float BurnAfterSecond = 1.0F;
    public int BurnDamage = 5;
    public int HealValue = 10;
    private bool previousFriendlyFire;
    private bool previousAutoKick;
    private float previousFriendlyFireDamageReduction;
    private bool hasConVarSnapshot;
    private BasePlugin.GameEventHandler<EventRoundFreezeEnd>? EventRoundFreezeEndHandler;
    private BasePlugin.GameEventHandler<EventPlayerDisconnect>? EventPlayerDisconnectHandler;
    private BasePlugin.GameEventHandler<EventPlayerConnectFull>? EventPlayerConnectFullHandler;
    private BasePlugin.GameEventHandler<EventPlayerHurt>? EventPlayerHurtHandler;
    private Dictionary<int,Timer> playerTimersDict = new ();
    private void BurnPlayer(CCSPlayerPawn? pawn)
    {
        if (!Enabled || pawn is null || !pawn.IsValid || pawn.Health <= 0) return;
        pawn.Health -= BurnDamage;
        pawn.ApplyStressDamage = true;
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        if (pawn.Health <= 0)
        {
            pawn.CommitSuicide(true,true);
        }
    }
    public override void EndFun(FunMatchPlugin plugin)
    {
        Enabled = false;
        if (hasConVarSnapshot)
        {
            ConVar.Find("mp_autokick")!.SetValue(previousAutoKick);
            ConVar.Find("ff_damage_reduction_bullets")!.SetValue(previousFriendlyFireDamageReduction);
            ConVar.Find("mp_friendlyfire")!.SetValue(previousFriendlyFire);
            hasConVarSnapshot = false;
        }
        foreach (var value in playerTimersDict.Values)
        {
            if (value is not null)
            value.Kill();
        }
        playerTimersDict.Clear();
        if (EventRoundFreezeEndHandler is not null) plugin.DeregisterEventHandler(EventRoundFreezeEndHandler);
        if (EventPlayerDisconnectHandler is not null) plugin.DeregisterEventHandler(EventPlayerDisconnectHandler);
        if (EventPlayerConnectFullHandler is not null) plugin.DeregisterEventHandler(EventPlayerConnectFullHandler);
        if (EventPlayerHurtHandler is not null) plugin.DeregisterEventHandler(EventPlayerHurtHandler);
        EventRoundFreezeEndHandler = null;
        EventPlayerDisconnectHandler = null;
        EventPlayerConnectFullHandler = null;
        EventPlayerHurtHandler = null;
    }

    public override void Fun(FunMatchPlugin plugin)
    {
        if (Enabled) return;
        Enabled = true;

        previousAutoKick = ConVar.Find("mp_autokick")!.GetPrimitiveValue<bool>();
        previousFriendlyFire = ConVar.Find("mp_friendlyfire")!.GetPrimitiveValue<bool>();
        previousFriendlyFireDamageReduction = ConVar.Find("ff_damage_reduction_bullets")!.GetPrimitiveValue<float>();
        hasConVarSnapshot = true;
        ConVar.Find("mp_autokick")!.SetValue(false);
        ConVar.Find("mp_friendlyfire")!.SetValue(true);
        ConVar.Find("ff_damage_reduction_bullets")!.SetValue(0.0f);
        plugin.RegisterEventHandler <EventRoundFreezeEnd>(EventRoundFreezeEndHandler = (@event, info) =>
        {
            
            if (Enabled == false) return HookResult.Stop;
            var Allplayers = Utilities.GetPlayers();
            foreach (var p in Allplayers)
            {
                if (p.UserId is null || (int)p.UserId < 0 || !p.PawnIsAlive || p.OriginalControllerOfCurrentPawn is null) continue;
                //if (p.IsBot) continue;
                var pawn = p.OriginalControllerOfCurrentPawn.Get()?.PlayerPawn.Get();
                if (pawn is null) continue;
                ReplacePlayerTimer(plugin, (int)p.UserId, pawn);
            }
            return HookResult.Stop;
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

        plugin.RegisterEventHandler <EventPlayerDisconnect> (EventPlayerDisconnectHandler = (@event, info) =>
        {
            
            if (Enabled == false) return HookResult.Stop;
            Timer ?playerTimer;
            if (@event.Userid?.UserId is null) return HookResult.Continue;
            playerTimersDict.TryGetValue((int)@event.Userid.UserId,out playerTimer);
            if (playerTimer is null) return HookResult.Continue;
            playerTimer.Kill();
            playerTimersDict.Remove((int)@event.Userid.UserId);
            return HookResult.Continue;
        });

        //EventPlayerAvengedTeammate not working using playerhurtinstead
        plugin.RegisterEventHandler <EventPlayerHurt> (EventPlayerHurtHandler = (@event , info)=>
        {
            if (!Enabled || @event.Attacker is null || @event.Userid is null) return HookResult.Continue;
            if (@event.Attacker == @event.Userid) return HookResult.Continue;
            if (@event.Attacker.Team != @event.Userid.Team) return HookResult.Continue;
            CCSPlayerPawn? pawn = @event.Userid.OriginalControllerOfCurrentPawn.Get()?.PlayerPawn.Get();
            if (pawn is null || !pawn.IsValid) return HookResult.Continue;
            pawn.Health += HealValue;
            if (pawn.Health >= pawn.MaxHealth) pawn.Health = pawn.MaxHealth;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
            return HookResult.Continue;
        });

    }

    private void ReplacePlayerTimer(FunMatchPlugin plugin, int userId, CCSPlayerPawn pawn)
    {
        if (playerTimersDict.Remove(userId, out var oldTimer)) oldTimer.Kill();
        playerTimersDict[userId] = plugin.AddTimer(BurnAfterSecond, () => BurnPlayer(pawn), TimerFlags.REPEAT);
    }
}
