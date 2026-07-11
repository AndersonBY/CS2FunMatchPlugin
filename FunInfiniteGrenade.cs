using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace FunMatchPlugin;

public class FunInfiniteGrenade : FunBaseClass
{
    public override string Decription => "Infinite Grenade 无限火力";

    public FunInfiniteGrenade(FunMatchPlugin plugin) : base(plugin){}
    private bool previousAutoKick;
    private int previousInfiniteAmmo;
    private int previousGrenadeLimitTotal;
    private int previousGrenadeLimitDefault;
    private int previousRiflesAllowed;
    private int previousPistolsAllowed;
    private int previousSmgsAllowed;
    private int previousHeavyAllowed;
    private bool hasConVarSnapshot;

    public override void Fun(FunMatchPlugin plugin)
    {
        if (Enabled) return;
        Enabled = true;
        previousAutoKick = ConVar.Find("mp_autokick")!.GetPrimitiveValue<bool>();
        previousInfiniteAmmo = ConVar.Find("sv_infinite_ammo")!.GetPrimitiveValue<int>();
        previousGrenadeLimitTotal = ConVar.Find("ammo_grenade_limit_total")!.GetPrimitiveValue<int>();
        previousGrenadeLimitDefault = ConVar.Find("ammo_grenade_limit_default")!.GetPrimitiveValue<int>();
        previousRiflesAllowed = ConVar.Find("mp_weapons_allow_rifles")!.GetPrimitiveValue<int>();
        previousPistolsAllowed = ConVar.Find("mp_weapons_allow_pistols")!.GetPrimitiveValue<int>();
        previousSmgsAllowed = ConVar.Find("mp_weapons_allow_smgs")!.GetPrimitiveValue<int>();
        previousHeavyAllowed = ConVar.Find("mp_weapons_allow_heavy")!.GetPrimitiveValue<int>();
        hasConVarSnapshot = true;
        //ConVar.Find("mp_weapons_allow_heavyassaultsuit").SetValue(true);
        WithCheats(() =>
        {
            ConVar.Find("mp_autokick")!.SetValue(false);
            ConVar.Find("sv_infinite_ammo")!.SetValue(1);
            ConVar.Find("ammo_grenade_limit_total")!.SetValue(10);
            ConVar.Find("ammo_grenade_limit_default")!.SetValue(2);
            ConVar.Find("mp_weapons_allow_rifles")!.SetValue(0);
            ConVar.Find("mp_weapons_allow_pistols")!.SetValue(0);
            ConVar.Find("mp_weapons_allow_smgs")!.SetValue(0);
            ConVar.Find("mp_weapons_allow_heavy")!.SetValue(0);
        });
        var Allplayers = Utilities.GetPlayers();
        bool BombHasGiven = false;
        foreach (var p in Allplayers)
        {
            if (!p.IsValid) continue;
            p.RemoveWeapons();
            p.GiveNamedItem(CsItem.HE);
            p.GiveNamedItem(CsItem.Knife);
            p.GiveNamedItem(CsItem.Kevlar);
            p.GiveNamedItem(CsItem.KevlarHelmet);
            if (p.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.Terrorist && !BombHasGiven)
            {
                p.GiveNamedItem(CsItem.C4);
                BombHasGiven = true;
            }
        }
    }
    public override void EndFun(FunMatchPlugin plugin)
    {
        Enabled = false;
        if (!hasConVarSnapshot) return;
        WithCheats(() =>
        {
            ConVar.Find("mp_autokick")!.SetValue(previousAutoKick);
            ConVar.Find("sv_infinite_ammo")!.SetValue(previousInfiniteAmmo);
            ConVar.Find("ammo_grenade_limit_total")!.SetValue(previousGrenadeLimitTotal);
            ConVar.Find("ammo_grenade_limit_default")!.SetValue(previousGrenadeLimitDefault);
            ConVar.Find("mp_weapons_allow_rifles")!.SetValue(previousRiflesAllowed);
            ConVar.Find("mp_weapons_allow_pistols")!.SetValue(previousPistolsAllowed);
            ConVar.Find("mp_weapons_allow_smgs")!.SetValue(previousSmgsAllowed);
            ConVar.Find("mp_weapons_allow_heavy")!.SetValue(previousHeavyAllowed);
        });
        hasConVarSnapshot = false;
    }
}
