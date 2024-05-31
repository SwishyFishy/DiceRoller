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
}