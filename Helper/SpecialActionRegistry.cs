using skill_composer.SpecialActions;
using System.Collections.Concurrent;
using System.Reflection;

namespace skill_composer.Helper
{
    public static class SpecialActionRegistry
    {
        private static readonly ConcurrentDictionary<string, ISpecialAction> _actions;

        static SpecialActionRegistry()
        {
            _actions = new ConcurrentDictionary<string, ISpecialAction>();

            // Get the assembly containing the special actions
            var assembly = Assembly.GetExecutingAssembly();

            // Find all types in the namespace that implement ISpecialAction
            var actionTypes = assembly.GetTypes()
                .Where(type => typeof(ISpecialAction).IsAssignableFrom(type)
                               && !type.IsInterface
                               && !type.IsAbstract
                               && type.Namespace == "skill_composer.SpecialActions")
                .ToList();

            // Create an instance of each action and add it to the dictionary
            foreach (var actionType in actionTypes)
            {
                try
                {
                    var actionInstance = (ISpecialAction)Activator.CreateInstance(actionType);
                    var actionName = actionType.Name.Replace("Action", "");
                    _actions[actionName] = actionInstance;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating instance of {actionType.Name}: {ex.Message}");
                }
            }
        }

        public static ConcurrentDictionary<string, ISpecialAction> GetActions() 
        {
            return _actions;
        }

        public static ISpecialAction GetAction(string actionName)
        {
            if (actionName.Contains("TextToSpeech")) actionName = "TextToSpeech";

            if (_actions.TryGetValue(actionName, out var action))
            {
                return action;
            }

            throw new ArgumentException($"No action found for {actionName}");
        }
    }
}
