using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FunMatchPlugin;

public class FunHighHP : FunBaseClass
{
    public override string Decription => "1000 HP 1000生命值";
    private List<CCSPlayerController> Allplayers = new();
    public FunHighHP(FunMatchPlugin plugin) : base(plugin){}
    public int maxHP = 1000;
    public int armor = 200;
    private readonly Dictionary<nint, PlayerState> playerStates = new();
    private readonly record struct PlayerState(int MaxHealth, int Armor, bool HasHelmet);
    public override void Fun(FunMatchPlugin plugin)
    {
        if (Enabled) return;
        Enabled = true;
        playerStates.Clear();
        Allplayers = Utilities.GetPlayers();
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
            pawn!.MaxHealth = maxHP;
            pawn!.Health = maxHP;
            pawn!.ArmorValue = armor;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");
        }
    }
    public override void EndFun(FunMatchPlugin plugin)
    {
        Enabled = false;
        Allplayers = Utilities.GetPlayers();
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
        Allplayers.Clear();
    }
}
