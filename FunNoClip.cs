using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace FunMatchPlugin;

using CounterStrikeSharp.API.Modules.Timers;
public class FunNoClip : FunBaseClass
{
    public override string Decription => "NoClip ON 启用飞行";
    private List<CCSPlayerController> Allplayers = new();
    public FunNoClip(FunMatchPlugin plugin) : base(plugin){}
    private Timer ?NoclipOnTimer = null;
    private bool IsNoClipON = false;
    public float interval = 2.0f;

    public override void Fun(FunMatchPlugin plugin)
    {
        if (Enabled) return;
        Enabled = true;
        IsNoClipON = false;
        NoclipOnTimer = plugin.AddTimer(interval,SetNoclip,TimerFlags.REPEAT);
    }
    public override void EndFun(FunMatchPlugin plugin)
    {
        Enabled = false;
        IsNoClipON = false;

        NoclipOnTimer?.Kill();
        NoclipOnTimer = null;

        try
        {
            WithCheats(() =>
            {
                Allplayers = Utilities.GetPlayers();
                foreach (var p in Allplayers)
                {
                    if (p.IsValid)
                    {
                        p.ExecuteClientCommandFromServer("noclip 0");
                    }
                }
            });
        }
        finally
        {
            Allplayers.Clear();
        }
    }

    private void SetNoclip()
    {
        if (!Enabled) return;

        if (IsNoClipON is false)
        {
            WithCheats(() =>
            {
                Allplayers = Utilities.GetPlayers();
                foreach (var p in Allplayers)
                {
                    if (!p.IsValid || p.IsBot) continue;
                    var pawn = p.OriginalControllerOfCurrentPawn.Get()?.PlayerPawn.Get();
                    if (pawn is null || pawn.IsDefusing || pawn.InBombZone) continue;
                    p.ExecuteClientCommandFromServer("noclip 1");
                }
            });
            IsNoClipON = true;
        }
        else
        {
            WithCheats(() =>
            {
                Allplayers = Utilities.GetPlayers();
                foreach (var p in Allplayers)
                {
                    if (!p.IsValid || p.IsBot) continue;
                    p.ExecuteClientCommandFromServer("noclip 0");
                }
            });
            IsNoClipON = false;
        }
    }
}
