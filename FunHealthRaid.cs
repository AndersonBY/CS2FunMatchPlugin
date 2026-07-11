using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace FunMatchPlugin;

public class FunHealthRaid : FunBaseClass
{
    public override string Decription => "Health Raid 攻击吸血";

    public FunHealthRaid(FunMatchPlugin plugin) : base(plugin){}
    private BasePlugin.GameEventHandler<EventPlayerHurt>? EventPlayerHurtHandler;
    public int initHP = 100;
    public int maxRaid = 100;
    public float RaidScale = 0.5F;
    private readonly Dictionary<nint, PlayerState> playerStates = new();
    private readonly record struct PlayerState(int MaxHealth, int Armor, bool HasHelmet);

    public override void EndFun(FunMatchPlugin plugin)
    {
        Enabled = false;
        var Allplayers = Utilities.GetPlayers();
        if (EventPlayerHurtHandler is not null)
        {
            plugin.DeregisterEventHandler(EventPlayerHurtHandler);
            EventPlayerHurtHandler = null;
        }
        foreach (var p in Allplayers)
        {
            if (!p.IsValid) continue;
            var oringin = p.OriginalControllerOfCurrentPawn.Get();
            if (oringin is null) continue;
            CCSPlayerPawn ?pawn = oringin.PlayerPawn.Get();
            if (pawn is null) continue;
            if (!pawn!.IsValid) continue;
            if (!playerStates.TryGetValue(pawn.Handle, out var state)) continue;
            pawn.MaxHealth = state.MaxHealth;
            pawn.Health = Math.Min(pawn.Health, state.MaxHealth);
            pawn.ArmorValue = Math.Min(pawn.ArmorValue, state.Armor);
            p.PawnHasHelmet = p.PawnHasHelmet && state.HasHelmet;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
            Utilities.SetStateChanged(p, "CCSPlayerController", "m_bPawnHasHelmet");
        }
        playerStates.Clear();
    }

    public override void Fun(FunMatchPlugin plugin)
    {
        if (Enabled) return;
        Enabled = true;
        playerStates.Clear();
        var Allplayers = Utilities.GetPlayers();
        foreach (var p in Allplayers)
        {
            if (!p.IsValid) continue;
            var oringin = p.OriginalControllerOfCurrentPawn.Get();
            if (oringin is null) continue;
            CCSPlayerPawn ?pawn = oringin.PlayerPawn.Get();
            if (pawn is null) continue;
            if (!pawn!.IsValid) continue;
            playerStates[pawn.Handle] = new PlayerState(pawn.MaxHealth, pawn.ArmorValue, p.PawnHasHelmet);
            p.GiveNamedItem(CsItem.Kevlar);
            p.GiveNamedItem(CsItem.KevlarHelmet);
            pawn!.MaxHealth = initHP;
            pawn!.Health = initHP;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }

        plugin.RegisterEventHandler<EventPlayerHurt> (EventPlayerHurtHandler = (@event, info)=>
        {
            if (Enabled == false) return HookResult.Stop;
            if (@event.Userid is null || @event.Attacker is null) return HookResult.Continue;
            if (@event.Userid == @event.Attacker) return HookResult.Continue;
            if (@event.Userid.Team == @event.Attacker.Team) return HookResult.Continue;
            var attacker = @event.Attacker.OriginalControllerOfCurrentPawn.Get()?.PlayerPawn.Get();
            if (attacker is null || !attacker.IsValid) return HookResult.Continue;
            var damage = @event.DmgHealth * RaidScale;
            if (damage > maxRaid) damage = maxRaid;
            Server.NextFrame(() =>
            {
                if (!Enabled || attacker is null || !attacker.IsValid) return;
                attacker.MaxHealth += (int)damage;
                attacker.Health += (int)damage;
                Utilities.SetStateChanged(attacker, "CBaseEntity", "m_iMaxHealth");
                Utilities.SetStateChanged(attacker, "CBaseEntity", "m_iHealth");
            });
            return HookResult.Continue;
        });
    }
}
