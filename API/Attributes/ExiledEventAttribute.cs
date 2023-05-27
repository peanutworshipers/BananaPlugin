namespace BananaPlugin.API.Attributes;

using System;

/// <summary>
/// Attribute responsible for automatically assigning banana feature events.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class ExiledEventAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExiledEventAttribute"/> class.
    /// </summary>
    /// <param name="handlerTypeName">The field's declaring type name.</param>
    /// <param name="fieldName">The field name.</param>
    public ExiledEventAttribute(string handlerTypeName, string fieldName)
    {
        this.TypeName = handlerTypeName;
        this.FieldName = fieldName;
    }

    /// <summary>
    /// Gets the field's declaring type name.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets the field name.
    /// </summary>
    public string FieldName { get; }
}
