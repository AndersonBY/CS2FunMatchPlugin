using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using System.Text.Json;

namespace FunMatchPlugin;

public class FunMatchPlugin : BasePlugin, IPluginConfig<FunMatchPluginConfig>
{
    public override string ModuleName => "Fun Match Plugin";
    public override string ModuleVersion => "1.2.0";
    public FunMatchPluginConfig Config { get; set; } = new();
    private bool isUnloading;
    public override void Load(bool hotReload)
    {
        isUnloading = false;
        Console.WriteLine("Fun Match Plugin Load!");
        InstallFun(Config);
        InstallCustomModes();
        //FunC4EveryWhere funC4EveryWhere = new(this);
        //FunLists.Add(funC4EveryWhere);
    }

    public override void Unload(bool hotReload)
    {
        isUnloading = true;
        UnLoadFun();
    }
    public void OnConfigParsed(FunMatchPluginConfig config)
    {
        Console.WriteLine("Parsing config");
        Config = config;
    }

    private void InstallCustomModes()
    {
        var configdirectory = Path.Combine(Application.RootDirectory, "configs/plugins/FunMatchPlugin");
        if (!Path.Exists(configdirectory)) Directory.CreateDirectory(configdirectory);
        var configpath = Path.Combine(configdirectory, "CustomModes.json");
        if (!File.Exists(configpath)) return;

        var dir_addons = Directory.GetParent(Application.RootDirectory);
        var dir_csgo = Directory.GetParent(dir_addons!.FullName);
        var path_cfg = Path.Combine(dir_csgo!.FullName, "cfg");

        using (StreamReader r = new StreamReader(configpath))
        {
            string json = r.ReadToEnd();
            CustomModesConfig customModesConfig = JsonSerializer.Deserialize<CustomModesConfig>(json)
                ?? throw new InvalidDataException($"Invalid custom mode configuration: {configpath}");
            foreach (var mode in customModesConfig.Modes ?? [])
            {
                string loadFileName = ValidateConfigFileName(mode.Fun_cfgfilename);
                string unloadFileName = ValidateConfigFileName(mode.Endfun_cfgfilename);
                var fun_cfg_game = Path.Combine(path_cfg, loadFileName);
                var fun_cfg_config = Path.Combine(configdirectory, loadFileName);
                var endfun_cfg_game = Path.Combine(path_cfg, unloadFileName);
                var endfun_cfg_config = Path.Combine(configdirectory, unloadFileName);
                File.Copy(fun_cfg_config, fun_cfg_game, true);
                File.Copy(endfun_cfg_config, endfun_cfg_game, true);
                FunLists.Add(new FunCustomConsoleMode(mode.Decr, loadFileName, unloadFileName));
            }
        }
    }

    private static string ValidateConfigFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetExtension(fileName), ".cfg", StringComparison.OrdinalIgnoreCase) ||
            fileName.Any(character => !char.IsLetterOrDigit(character) && character is not '_' and not '-' and not '.'))
        {
            throw new InvalidDataException($"Invalid custom mode cfg filename: {fileName}");
        }
        return fileName;
    }

    private void InstallFun(FunMatchPluginConfig config)
    {
        if (config.IsFunBulletTeleportOn)
        {
            FunBulletTeleport funBulletTeleport = new(this);
            FunLists.Add(funBulletTeleport);
        }
        if (config.IsFunHealTeammatesOn)
        {
            FunHealTeammates funHealTeammates = new(this)
            {
                BurnAfterSecond = Math.Max(0.1f, config.FunHealTeammatesBurnAfterSecond),
                BurnDamage = Math.Max(0, config.FunHealTeammatesBurnDamage),
                HealValue = Math.Max(0, config.FunHealTeammatesHealValue),
            };
            FunLists.Add(funHealTeammates);
        }
        if (config.IsFunHealthRaidOn)
        {
            FunHealthRaid funHealthRaid = new(this)
            {
                initHP = Math.Max(1, config.FunHealthRaidinitHP),
                maxRaid = Math.Max(0, config.FunHealthRaidmaxRaid),
                RaidScale = Math.Max(0, config.FunHealthRaidScale),
            };
            FunLists.Add(funHealthRaid);
        }
        if (config.IsFunHighHPOn)
        {
            FunHighHP funHighHP = new(this)
            {
                maxHP = Math.Max(1, config.FunHighHPmaxHP),
                armor = Math.Max(0, config.FunHighHParmor),
            };
            FunLists.Add(funHighHP);
        }
        if (config.IsFunInfiniteGrenadeOn)
        {
            FunInfiniteGrenade funFunInfiniteGrenade = new(this);
            FunLists.Add(funFunInfiniteGrenade);
        }
        if (config.IsFunJumpOrDieOn)
        {
            FunJumpOrDie funJumpOrDie = new(this)
            {
                BurnAfterSecond = Math.Max(0.1f, config.FunJumpOrDieBurnAfterSecond),
                BurnDamage = Math.Max(0, config.FunJumpOrDieBurnDamage),
            };
            FunLists.Add(funJumpOrDie);
        }
        if (config.IsFunNoClipOn)
        {
            FunNoClip funNoClip = new(this)
            {
                interval = Math.Max(0.1f, config.FunNoClipinterval),
            };
            FunLists.Add(funNoClip);
        }
        if (config.IsFunPlayerShootExChangeOn)
        {
            FunPlayerShootExChange funPlayerShootExChange = new(this);
            FunLists.Add(funPlayerShootExChange);
        }
        if (config.IsFunToTheMoonOn)
        {
            FunToTheMoon funToTheMoon = new(this)
            {
                gravity = Math.Max(1, config.FunToTheMoongravity),
                BulletGiveAbsV = Math.Max(0, config.FunToTheMoonBulletGiveAbsV),
            };
            FunLists.Add(funToTheMoon);
        }
        if (config.IsFunWNoStopOn)
        {
            FunWNoStop funWNoStop = new(this)
            {
                BurnAfterSecond = Math.Max(0.1f, config.FunWNoStopBurnAfterSecond),
                BurnDamage = Math.Max(0, config.FunWNoStopBurnDamage),
            };
            FunLists.Add(funWNoStop);
        }
        if (config.IsFunDropWeaponOnShootOn)
        {
            FunDropWeaponOnShoot funDropWeaponOnShoot = new();
            FunLists.Add(funDropWeaponOnShoot);
        }
        if (config.IsFunChangeWeaponOnShootOn)
        {
            FunChangeWeaponOnShoot funChangeWeaponOnShoot = new();
            FunLists.Add(funChangeWeaponOnShoot);
        }
        if (config.IsFunFootBallOn)
        {
            FunFootBall funFootBall = new();
            FunLists.Add(funFootBall);
            RegisterListener<Listeners.OnServerPrecacheResources>((manifest) =>
            {
                manifest.AddResource("models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl");
                manifest.AddResource("models/props_fairgrounds/fairgrounds_flagpole01.vmdl");
            });
        }
    }

    private Random rd = new Random();
    private List<int> playedFunIndices = new List<int>();
    private int lastPlayedIndex = -1;
    private int currentFunOrderIndex = 0;

    public void LoadRandomFun()
    {
        if (CurrentActiceFunIndex >= 0) return;

        if (FunLists.Count == 0)
        {
            Console.WriteLine("[FunMatchPlugin] No fun modes are enabled.");
            return;
        }

        if (FunLists.Count == 1)
        {
            LoadFunByIndex(0);
            return;
        }

        if (playedFunIndices.Count >= FunLists.Count)
        {
            playedFunIndices.Clear();
        }

        int newIndex;
        do
        {
            newIndex = rd.Next(0, FunLists.Count);
        } while (playedFunIndices.Contains(newIndex) || newIndex == lastPlayedIndex);

        playedFunIndices.Add(newIndex);
        lastPlayedIndex = newIndex;
        LoadFunByIndex(newIndex);
    }

    public void LoadFunByOrderOrRandom()
    {
        if (CurrentActiceFunIndex >= 0) return;

        while (currentFunOrderIndex < Config.FunOrder.Count)
        {
            int funIndex = Config.FunOrder[currentFunOrderIndex] - 1; // Convert to zero-based index
            currentFunOrderIndex++;
            if (funIndex >= 0 && funIndex < FunLists.Count)
            {
                LoadFunByIndex(funIndex);
                return;
            }
            Console.WriteLine($"[FunMatchPlugin] Ignoring invalid FunOrder entry: {funIndex + 1}");
        }

        LoadRandomFun();
    }

    public void UnLoadFun()
    {
        if (CurrentActiceFunIndex < 0) return;
        int index = CurrentActiceFunIndex;
        CurrentActiceFunIndex = -1;
        FunLists[index].EndFun(this);
    }

    public void LoadFunByIndex(int index)
    {
        if (index < 0 || index >= FunLists.Count || CurrentActiceFunIndex >= 0) return;

        CurrentActiceFunIndex = index;
        try
        {
            FunLists[index].Fun(this);
            if (DisPlayHelp) FunLists[index].DisPlayHelp();
        }
        catch
        {
            try
            {
                FunLists[index].EndFun(this);
            }
            finally
            {
                CurrentActiceFunIndex = -1;
            }
            throw;
        }
    }

    public void UnLoadFunByIndex(int index)
    {
        if (index != CurrentActiceFunIndex) return;
        UnLoadFun();
    }


    public void LoadFunByName(string name)
    {

    }
    public bool DisPlayHelp = true;
    private int CurrentActiceFunIndex = -1;
    private List<FunBaseClass> FunLists = new List<FunBaseClass>();
    private bool EnableRandom = true;

    private int ManualLoadIndex = -1;

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        UnLoadFun();
        Server.NextFrame(() =>
        {
            if (isUnloading) return;
            if (ManualLoadIndex >= 0)
            {
                LoadFunByIndex(ManualLoadIndex);
            }
            else if (EnableRandom)
            {
                LoadFunByOrderOrRandom();
            }
        });
        return HookResult.Continue;
    }

    [ConsoleCommand("fun_load", "Load fun by num")]
    [CommandHelper(minArgs: 1, usage: "[num]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/root")]
    public void OnLoadFunCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {

        int num;
        int.TryParse(commandInfo.GetArg(1), out num);
        if (num <= 0 || num > FunLists.Count)
        {
            commandInfo.ReplyToCommand($"Invalid num. pls input num from {1} - {FunLists.Count}");
            return;
        }
        if (ManualLoadIndex >= 0)
        {
            commandInfo.ReplyToCommand($"Alraedy loaded {ManualLoadIndex + 1} Manually, Pls !fun_load first");
            return;
        }

        UnLoadFun();
        ManualLoadIndex = num - 1;
        try
        {
            LoadFunByIndex(ManualLoadIndex);
        }
        catch
        {
            ManualLoadIndex = -1;
            throw;
        }
    }

    [ConsoleCommand("!fun_load", "UnLoad fun Manually")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/root")]
    public void OnUnLoadFunCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (ManualLoadIndex < 0)
        {
            commandInfo.ReplyToCommand("No manually loaded fun mode.");
            return;
        }

        int unloadedIndex = ManualLoadIndex;
        UnLoadFun();
        ManualLoadIndex = -1;
        commandInfo.ReplyToCommand($"Unloaded {unloadedIndex + 1}");
    }

    [ConsoleCommand("fun_lists", "Lists Avaliable Fun")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnListFunCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        for (int i = 0; i < FunLists.Count; i++)
        {
            commandInfo.ReplyToCommand($"{i + 1} {FunLists[i].Decription}");
        }
    }

    [ConsoleCommand("fun_displayhelp", "DisPlay Help Fun")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/root")]
    public void OnDisPlayHelpCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        DisPlayHelp = true;
    }

    [ConsoleCommand("!fun_displayhelp", "Don't DisPlay Help Fun")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/root")]
    public void OnDontDisPlayHelpCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        DisPlayHelp = false;
    }

    [ConsoleCommand("!fun_random", "Don't random Fun everyround")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/root")]
    public void OnDontRandomCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        EnableRandom = false;
        if (ManualLoadIndex < 0) UnLoadFun();
    }

    [ConsoleCommand("fun_random", "random Fun everyround")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/root")]
    public void OnRandomCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        EnableRandom = true;
    }
}
