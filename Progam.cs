using DiceRoller_v2;
using Velopack;

public static class Program
{
    public static void Main()
    {
        VelopackApp.Build().Run();
        DiceRoller diceRoller = new DiceRoller();
    }

    // FEATURE PLANNER
    // -> params arg for <add>, <remove> to add/remove multiple objects/faces in one command


    // Releasing a build
    // From DiceRoller_v2/
    // dotnet publish -c Release -r win-x64 -o publish
    // vpk pack --packId DiceRoller_SwishyFishy_Release --packVersion 1.0.1 --packDir publish --mainExe DiceRoller_v2.exe --packAuthors "Jonah Galloway-Fenwick" --packTitle DiceRoller
}