// See https://aka.ms/new-console-template for more information
using skill_composer.Helper;
using test.SpecialActionsTestData;

Console.WriteLine("Select special action to test:");

var actions = SpecialActionRegistry.GetActions();

int i = 0;
foreach(var action in actions.OrderByDescending(x=>x.Key))
{
    i++;
    Console.WriteLine($"{i} - {action.Key}");
}

Console.WriteLine();

var selection = Console.ReadLine();

int si = 0;
int.TryParse(selection, out si);

// This is used to overwrite 
var selectedSkill = actions.OrderByDescending(x => x.Key).Take(si).Last();

Console.WriteLine($"Selected Skill: {selectedSkill.Key}");

var (task, setting) = new QRCodeCreate().GetTestData();

var action = 
task = await action.ExecuteAsync(task, selectedSkill, settings);



Console.ReadLine();