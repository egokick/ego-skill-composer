namespace skill_composer.Helper
{
    /// <summary>
    /// Attribute to mark properties that should be ignored by the GetParameters method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreParameterAttribute : Attribute
    {
    }
}
