﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using NetCord.Services.Helpers;

namespace NetCord.Services.Interactions;

public class InteractionService<TContext>(InteractionServiceConfiguration<TContext>? configuration = null) : IService where TContext : IInteractionContext
{
    private readonly InteractionServiceConfiguration<TContext> _configuration = configuration ?? InteractionServiceConfiguration<TContext>.Default;
    private readonly Dictionary<ReadOnlyMemory<char>, InteractionInfo<TContext>> _interactions = new(ReadOnlyMemoryCharComparer.InvariantCulture);

    public IReadOnlyDictionary<ReadOnlyMemory<char>, InteractionInfo<TContext>> GetInteractions() => new Dictionary<ReadOnlyMemory<char>, InteractionInfo<TContext>>(_interactions);

    [RequiresUnreferencedCode("Types might be removed")]
    public void AddModules(Assembly assembly)
    {
        foreach (var type in ServiceHelpers.GetModules(typeof(BaseInteractionModule<TContext>), assembly))
            AddModuleCore(type);
    }

    public void AddModule([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
    {
        if (!type.IsAssignableTo(typeof(BaseInteractionModule<TContext>)))
            throw new InvalidOperationException($"Modules must inherit from '{nameof(BaseInteractionModule<TContext>)}'.");

        AddModuleCore(type);
    }

    public void AddModule<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods)] T>()
    {
        AddModule(typeof(T));
    }

    private void AddModuleCore([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
    {
        var configuration = _configuration;
        foreach (var method in type.GetMethods())
        {
            InteractionAttribute? interactionAttribute = method.GetCustomAttribute<InteractionAttribute>();
            if (interactionAttribute is null)
                continue;
            InteractionInfo<TContext> interactionInfo = new(method, type, configuration);
            _interactions.Add(interactionAttribute.CustomId.AsMemory(), interactionInfo);
        }
    }

    public void AddInteraction(string customId, Delegate handler)
    {
        InteractionInfo<TContext> interactionInfo = new(handler, _configuration);
        _interactions.Add(customId.AsMemory(), interactionInfo);
    }

    public async ValueTask<IExecutionResult> ExecuteAsync(TContext context, IServiceProvider? serviceProvider = null)
    {
        try
        {
            return await ExecuteAsyncCore(context, serviceProvider).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new ExecutionExceptionResult(ex);
        }
    }

    private async ValueTask<IExecutionResult> ExecuteAsyncCore(TContext context, IServiceProvider? serviceProvider)
    {
        var configuration = _configuration;
        var separator = configuration.ParameterSeparator;
        var content = ((ICustomIdInteractionData)context.Interaction.Data).CustomId.AsMemory();
        var index = content.Span.IndexOf(separator);
        InteractionInfo<TContext>? interactionInfo;
        ReadOnlyMemory<char> arguments;
        if (index == -1)
        {
            var customId = content;

            if (!TryGetInteractionInfo(customId, out interactionInfo))
                return new NotFoundResult("Interaction not found.");

            arguments = default;
        }
        else
        {
            var customId = content[..index];

            if (!TryGetInteractionInfo(customId, out interactionInfo))
                return new NotFoundResult("Interaction not found.");

            arguments = content[(index + 1)..];
        }

        var preconditionResult = await interactionInfo.EnsureCanExecuteAsync(context, serviceProvider).ConfigureAwait(false);

        if (preconditionResult is IFailResult)
            return preconditionResult;

        var interactionParameters = interactionInfo.Parameters;
        int interactionParametersLength = interactionParameters.Count;

        var parametersToPass = new object?[interactionParametersLength];

        var maxParamIndex = interactionParametersLength - 1;
        for (int paramIndex = 0; paramIndex <= maxParamIndex; paramIndex++)
        {
            var parameter = interactionParameters[paramIndex];
            if (!parameter.Params)
            {
                ReadOnlyMemory<char> currentArg;
                if (paramIndex == maxParamIndex)
                    currentArg = arguments;
                else
                {
                    index = arguments.Span.IndexOf(separator);
                    if (index == -1)
                        return new ParameterCountMismatchResult(ParameterCountMismatchType.TooFew);

                    currentArg = arguments[..index];
                    arguments = arguments[(index + 1)..];
                }
                object? value;
                if (parameter.HasDefaultValue && currentArg.IsEmpty)
                    value = parameter.DefaultValue;
                else
                {
                    var typeReaderResult = await parameter.ReadAsync(currentArg, context, configuration, serviceProvider).ConfigureAwait(false);

                    if (typeReaderResult is not TypeReaderSuccessResult typeReaderSuccessResult)
                        return typeReaderResult;

                    value = typeReaderSuccessResult.Value;

                    var parameterPreconditionResult = await parameter.EnsureCanExecuteAsync(value, context, serviceProvider).ConfigureAwait(false);
                    if (parameterPreconditionResult is IFailResult)
                        return parameterPreconditionResult;
                }

                parametersToPass[paramIndex] = value;
            }
            else
            {
                if (parameter.HasDefaultValue && arguments.IsEmpty)
                    parametersToPass[paramIndex] = parameter.DefaultValue;
                else
                {
                    var result = await ReadParamsAsync(context, separator, parametersToPass, arguments, paramIndex, parameter, configuration, serviceProvider).ConfigureAwait(false);
                    if (result is IFailResult)
                        return result;
                }
            }
        }

        try
        {
            await interactionInfo.InvokeAsync(parametersToPass, context, serviceProvider).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new ExecutionExceptionResult(ex);
        }

        return SuccessResult.Instance;
    }

    [UnconditionalSuppressMessage("Trimming", "IL3050:RequiresDynamicCode", Justification = "The type of the array is known to be present")]
    private static async ValueTask<IExecutionResult> ReadParamsAsync(TContext context, char separator, object?[] parametersToPass, ReadOnlyMemory<char> arguments, int paramIndex, InteractionParameter<TContext> parameter, InteractionServiceConfiguration<TContext> configuration, IServiceProvider? serviceProvider)
    {
        var ranges = Split(arguments.Span, separator);
        var count = ranges.Count;
        var array = Array.CreateInstance(parameter.ElementType, count);
        for (int i = 0; i < count; i++)
        {
            var typeReaderResult = await parameter.ReadAsync(arguments[ranges[i]], context, configuration, serviceProvider).ConfigureAwait(false);

            if (typeReaderResult is not TypeReaderSuccessResult typeReaderSuccessResult)
                return typeReaderResult;

            var value = typeReaderSuccessResult.Value;

            var preconditionResult = await parameter.EnsureCanExecuteAsync(value, context, serviceProvider).ConfigureAwait(false);
            if (preconditionResult is IFailResult)
                return preconditionResult;

            array.SetValue(value, i);
        }
        parametersToPass[paramIndex] = array;
        return SuccessResult.Instance;

        static List<Range> Split(ReadOnlySpan<char> arguments, char separator)
        {
            List<Range> result = [];

            int startIndex = 0;
            int index;

            while ((index = arguments.IndexOf(separator)) != -1)
            {
                result.Add(new(startIndex, startIndex + index));
                var move = index + 1;
                startIndex += move;
                arguments = arguments[move..];
            }
            result.Add(new(startIndex, startIndex + arguments.Length));

            return result;
        }
    }

    private bool TryGetInteractionInfo(ReadOnlyMemory<char> customId, [MaybeNullWhen(false)] out InteractionInfo<TContext> result)
    {
        return _interactions.TryGetValue(customId, out result);
    }
}
