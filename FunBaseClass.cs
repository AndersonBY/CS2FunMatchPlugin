using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace FunMatchPlugin;
public abstract class FunBaseClass
{
    public bool Enabled = false;
    abstract public string Decription{get;}
    public FunBaseClass(FunMatchPlugin plugin)
    {
        RegisterCommand(plugin);
    }
    public FunBaseClass(){}
    public abstract void Fun(FunMatchPlugin plugin);
    public abstract void EndFun(FunMatchPlugin plugin);
    public virtual void RegisterCommand(FunMatchPlugin plugin){}
    public virtual void DisPlayHelp()
    {
        Server.PrintToChatAll(StringExtensions.ReplaceColorTags("{RED}") + "[FunMatchPlugin] " + this.Decription);
    }

    protected static void WithCheats(Action action)
    {
        var svCheats = ConVar.Find("sv_cheats")!;
        bool wasEnabled = svCheats.GetPrimitiveValue<bool>();
        svCheats.SetValue(true);
        try
        {
            action();
        }
        finally
        {
            svCheats.SetValue(wasEnabled);
        }
    }
}
