// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         ArgumentResult.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 4:25 PM
//    Created Date:     11/08/2023 4:04 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

using System;
using Interfaces;

#pragma warning disable CS8618

/// <inheritdoc />
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global
public sealed class ArgumentResult<T> : ArgumentResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentResult{T}"/> class.
    /// </summary>
    internal ArgumentResult()
    {
        this.Type = typeof(T);
    }

    /// <summary>
    /// Gets the value of the type.
    /// </summary>
    public new T Value { get; init; }

    /// <summary>
    /// Gets the respective <see cref="ArgumentResult{T}"/> from an <see cref="ArgumentResult"/>.
    /// </summary>
    /// <param name="arg">The ArgumentResult provided.</param>
    /// <returns>The <see cref="ArgumentResult{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the argument isn't the right type.</exception>
    public static ArgumentResult<T> Get(ArgumentResult arg)
    {
        if (arg is not ArgumentResult<T> argOut)
        {
            throw new ArgumentNullException(nameof(arg), $"Argument cannot be applied the type of {typeof(T).FullName}.");
        }

        return argOut;
    }

    /// <summary>
    /// Gets the respective <see cref="Value"/> from an <see cref="ArgumentResult"/>.
    /// </summary>
    /// <param name="arg">The ArgumentResult provided.</param>
    /// <returns>The <see cref="Value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the argument isn't the right type.</exception>
    public static T GetValue(ArgumentResult arg)
    {
        if (arg is not ArgumentResult<T> argOut)
        {
            throw new ArgumentNullException(nameof(arg), $"Argument cannot be applied the type of {typeof(T).FullName}.");
        }

        return argOut.Value;
    }
}

/// <summary>
/// Represents a single argument from a result of a command.
/// </summary>
public class ArgumentResult : IPrefixableItem
{
    /// <summary>
    /// Gets the index of the result relative to the other parameters.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    public object Value { get; init; }

    /// <inheritdoc/>
    public string Prefix => this.Name;

    /// <inheritdoc cref="CommandArgument.Name"/>
    public string Name { get; init; }

    /// <inheritdoc cref="CommandArgument.Description"/>
    public string Description { get; init; }

    /// <inheritdoc cref="CommandArgument.IsRemainder"/>
    public bool IsRemainder { get; init; }

    /// <inheritdoc cref="CommandArgument.Type"/>
    public Type Type { get; init; }

    /// <summary>
    /// Gets the respective <see cref="Value"/> from an <see cref="ArgumentResult"/>.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <returns>The <see cref="Value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the argument isn't the right type.</exception>
    public T Result<T>()
    {
        if (this.Value is not T res)
        {
            throw new ArgumentNullException(nameof(T), $"Argument cannot be applied the type of {typeof(T).FullName}.");
        }

        return res;
    }
}