using System.Collections.Immutable;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Conversion;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Implementation.PathFinding;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Implementation;

internal class UnitConversionEngine : IUnitConversionEngine
{
    private readonly ConversionMap _conversionMap;
    private readonly IImmutableDictionary<(string From, string To), IUnitConversion> _conversions;
    private readonly IImmutableDictionary<
        (string From, string To),
        (string, string)[]
    > _proxyConversions;

    public UnitConversionEngine(
        Dictionary<(string From, string To), IUnitConversion> conversions,
        Dictionary<(string From, string To), (string From, string To)[]> proxyConversions,
        IEnumerable<DTOs.Unit> registeredUnits
    )
    {
        _conversions = conversions.ToImmutableDictionary();
        _conversionMap = ConversionMapBuilder.BuildMap(registeredUnits, _conversions);
        _proxyConversions = proxyConversions.ToImmutableDictionary();
    }

    public IImmutableDictionary<(string From, string To), IUnitConversion> RegisteredConversions =>
        _conversions;

    public bool HasDirectConversion(string from, string to) => _conversions.ContainsKey((from, to));

    public bool HasInvertedConversion(string from, string to) =>
        _conversions.ContainsKey((to, from));

    public IUnitConversion GetDirectConversion(string from, string to) => _conversions[(from, to)];

    public IUnitConversion GetInvertedConversion(string from, string to) =>
        new InvertedUnitConversion(GetDirectConversion(to, from));

    public IUnitConversion? FindConversion(string from, string to) =>
        FindConversion(from, to, true, true, false, true);

    public bool HasProxyConversion(string from, string to) =>
        _proxyConversions.ContainsKey((from, to));

    public bool HasInvertedProxyConversion(string from, string to) =>
        _proxyConversions.ContainsKey((to, from));

    public IUnitConversion GetProxyConversion(string from, string to) =>
        new ChainedUnitConversion(
            GetProxyConversionSteps(from, to, out var steps),
            $"Proxy[{string.Join('>', steps)}]"
        );

    public IUnitConversion GetInvertedProxyConversion(string from, string to) =>
        new ChainedUnitConversion(
            GetProxyConversionSteps(to, from, out var steps)
                .Reverse()
                .Select(i => new InvertedUnitConversion(i)),
            $"Proxy[{string.Join('>', steps.Reverse())}]"
        );

    public IEnumerable<(string From, string To)> GetConversionsForUnit(string unit) =>
        _conversions.Keys.Where(i => i.From == unit || i.To == unit);

    private IEnumerable<IUnitConversion> GetProxyConversionSteps(
        string from,
        string to,
        out string[] steps
    )
    {
        var proxyConversion = _proxyConversions[(from, to)];
        var conversionSteps = new List<IUnitConversion>();
        var stepList = new List<string>();
        string lastStepTo = null;
        foreach (var (stepFrom, stepTo) in proxyConversion)
        {
            var conversion = FindSafeConversion(stepFrom, stepTo);
            if (conversion == null)
            {
                throw new InvalidOperationException(
                    $"Could not find suitable conversion from {stepFrom} to {stepTo}"
                );
            }

            stepList.Add(stepFrom);
            conversionSteps.Add(conversion);
            lastStepTo = stepTo;
        }

        if (lastStepTo != null)
        {
            stepList.Add(lastStepTo);
        }

        steps = stepList.ToArray();
        return conversionSteps;
    }

    private IUnitConversion? FindSafeConversion(string from, string to) =>
        FindConversion(from, to, false, false, false, false);

    private IUnitConversion? FindConversion(
        string from,
        string to,
        bool useProxyConversions,
        bool usePartialProxyConversions,
        bool useCompoundProxyConversions,
        bool useConversionMap
    )
    {
        if (from == to)
        {
            return new NoneConversion();
        }

        if (HasDirectConversion(from, to))
        {
            return GetDirectConversion(from, to);
        }

        if (HasInvertedConversion(from, to))
        {
            return GetInvertedConversion(from, to);
        }

        if (useProxyConversions)
        {
            if (HasProxyConversion(from, to))
            {
                return GetProxyConversion(from, to);
            }

            if (HasInvertedProxyConversion(from, to))
            {
                return GetInvertedProxyConversion(from, to);
            }
        }

        if (
            usePartialProxyConversions
            && TryFindAndConstructProxyConversion(from, to, out var proxyConversion)
        )
        {
            return proxyConversion;
        }

        if (useCompoundProxyConversions)
        {
            if (
                TryFindAndConstructCompoundProxyConversion(
                    from,
                    to,
                    out var compoundProxyConversion
                )
            )
            {
                return compoundProxyConversion;
            }

            if (TryFindUnitWithSameConversion(from, out var matchingSourceUnits))
            {
                foreach (var matchingUnit in matchingSourceUnits)
                {
                    if (
                        TryFindAndConstructCompoundProxyConversion(
                            matchingUnit,
                            to,
                            out var compoundProxyConversionWithEquivalentUnit
                        )
                    )
                    {
                        return compoundProxyConversionWithEquivalentUnit;
                    }
                }
            }

            if (TryFindAndConstructProxyConversion(to, from, out var revCompoundProxyConversion))
            {
                return revCompoundProxyConversion;
            }

            if (TryFindUnitWithSameConversion(to, out var matchingTargetUnits))
            {
                foreach (var matchingUnit in matchingTargetUnits)
                {
                    if (
                        TryFindAndConstructCompoundProxyConversion(
                            from,
                            matchingUnit,
                            out var compoundProxyConversionWithEquivalentUnit
                        )
                    )
                    {
                        return compoundProxyConversionWithEquivalentUnit;
                    }
                }
            }
        }

        if (useConversionMap && _conversionMap.IsNodeReachable(from, to, out var path))
        {
            var steps = path.ToArray().Reverse().ToArray();
            var conversions = new List<IUnitConversion>();
            string self,
                next;
            for (var i = 0; i < steps.Length; i++)
            {
                self = steps[i].Name;
                next = (i + 1 < steps.Length) ? steps[i + 1].Name : to;
                if (HasDirectConversion(self, next))
                {
                    conversions.Add(GetDirectConversion(self, next));
                }
                else if (HasInvertedConversion(self, next))
                {
                    conversions.Add(GetInvertedConversion(self, next));
                }
                else
                {
                    break;
                }
            }

            return new ChainedUnitConversion(conversions, path.ToString());
        }

        return null;
    }

    /// <summary>
    /// Attempts to find and construct a proxy conversion from Unit <see cref="from"/> to Unit <see cref="to"/>.
    /// Returns true and emits the built conversion on <see cref="conversion"/> if successful.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="conversion"></param>
    /// <returns></returns>
    private bool TryFindAndConstructProxyConversion(
        string from,
        string to,
        out IUnitConversion conversion
    )
    {
        conversion = null;
        // Attempt regular conversion
        if (TryFindMatchingProxyConversion(from, to, out var matchingChains))
        {
            foreach (var matchingChain in matchingChains)
            {
                if (TryConstructProxyConversion(from, to, matchingChain, out conversion))
                {
                    return true;
                }
            }
        }

        // Attempt reverse conversion
        if (TryFindMatchingProxyConversion(to, from, out var reversedMatchingChain))
        {
            foreach (var matchingChain in reversedMatchingChain)
            {
                if (TryConstructProxyConversion(to, from, matchingChain, out conversion, true))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to find existing proxy conversions that contain both Units <see cref="from"/> and <see cref="to"/>
    /// in that order. If found, returns true and emits the found conversion keys on
    /// <see cref="conversionKeys"/>.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="conversionKeys"></param>
    /// <returns></returns>
    private bool TryFindMatchingProxyConversion(
        string from,
        string to,
        out (string From, string To)[] conversionKeys
    ) => TryFindMatchingProxyConversion(from, to, _proxyConversions, out conversionKeys);

    private bool TryFindReverseMatchingProxyConversion(
        string from,
        string to,
        out (string From, string To)[] conversionKeys
    )
    {
        var reversedConversionKeys = _proxyConversions.ToImmutableDictionary(
            i => i.Key,
            i => i.Value.Reverse().Select(s => (s.Item2, s.Item1)).ToArray()
        );
        return TryFindMatchingProxyConversion(from, to, reversedConversionKeys, out conversionKeys);
    }

    private bool TryFindMatchingProxyConversion(
        string from,
        string to,
        IImmutableDictionary<(string From, string To), (string, string)[]> proxyConversions,
        out (string From, string To)[] conversionKeys
    )
    {
        conversionKeys = proxyConversions
            .Where(i => i.Value.Any(p => p.Item1 == from) && i.Value.Any(p => p.Item2 == to))
            .Select(i => i.Key)
            .ToArray();
        return conversionKeys.Any();
    }

    /// <summary>
    /// Attempts the construction of a partial proxy conversion using the given <see cref="usingBaseChain"/>
    /// base chain. If successful, returns true and emits the constructed chain on <see cref="conversion"/>.
    /// A reversed chain can also be constructed by enabling <see cref="reverse"/>. This reads the provided
    /// chain and the parameters <see cref="from"/> and <see cref="to"/> in reverse order.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="usingBaseChain"></param>
    /// <param name="conversion"></param>
    /// <param name="reverse"></param>
    /// <returns></returns>
    private bool TryConstructProxyConversion(
        string from,
        string to,
        (string From, string To) usingBaseChain,
        out IUnitConversion conversion,
        bool reverse = false
    )
    {
        conversion = default;

        if (reverse)
        {
            var tmp = to;
            to = from;
            from = tmp;
        }

        var baseChain = _proxyConversions[usingBaseChain];
        if (reverse)
        {
            baseChain = baseChain.Select(i => (i.Item2, i.Item1)).Reverse().ToArray();
        }

        var derivedChain = new List<(string, string)>();
        var readingProxyChain = false;
        foreach (var (stepFrom, stepTo) in baseChain)
        {
            if (!readingProxyChain)
            {
                if (stepFrom == from)
                {
                    readingProxyChain = true;
                }
                else
                {
                    continue;
                }
            }

            derivedChain.Add((stepFrom, stepTo));

            if (stepTo == to)
            {
                break;
            }
        }

        if (!derivedChain.Any())
        {
            return false;
        }

        var derivedChainSteps = new List<IUnitConversion>();
        foreach (var (stepFrom, stepTo) in derivedChain)
        {
            var step = FindSafeConversion(stepFrom, stepTo);
            if (step == null)
            {
                return false;
            }

            derivedChainSteps.Add(step);
        }

        var chainStr = string.Join(
            reverse ? '<' : '>',
            derivedChain.Select(i => i.Item1).Concat(new[] { derivedChain.Last().Item2 })
        );
        var chainName = $"Partial{(reverse ? "Rev" : string.Empty)}Proxy[{chainStr}]";
        conversion = new ChainedUnitConversion(derivedChainSteps, chainName);
        return true;
    }

    /// <summary>
    /// Attempts to find and construct a compound conversion where existing conversions between units are used.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="conversion"></param>
    /// <returns></returns>
    private bool TryFindAndConstructCompoundProxyConversion(
        string from,
        string to,
        out IUnitConversion conversion
    )
    {
        conversion = null;
        var conversionsForSource = GetConversionsForUnit(from).ToArray();
        if (conversionsForSource.Any())
        {
            foreach (var sourceConversionPair in conversionsForSource)
            {
                if (
                    TryFindMatchingProxyConversion(
                        sourceConversionPair.To,
                        to,
                        out var foreignProxyChains
                    )
                )
                {
                    foreach (var foreignProxyChain in foreignProxyChains)
                    {
                        if (
                            TryConstructProxyConversion(
                                foreignProxyChain.To,
                                to,
                                foreignProxyChain,
                                out var partialChain
                            )
                        )
                        {
                            var bridgeConversion = FindSafeConversion(
                                from,
                                sourceConversionPair.To
                            );
                            if (bridgeConversion == null)
                            {
                                continue;
                            }

                            conversion = new ChainedUnitConversion(bridgeConversion, partialChain);
                            return true;
                        }
                    }
                }
            }
        }

        var conversionsForTarget = GetConversionsForUnit(to).ToArray();
        if (conversionsForTarget.Any())
        {
            foreach (var targetConversionPair in conversionsForTarget)
            {
                if (
                    TryFindMatchingProxyConversion(
                        from,
                        targetConversionPair.From,
                        out var foreignProxyChains
                    )
                )
                {
                    foreach (var foreignProxyChain in foreignProxyChains)
                    {
                        if (
                            TryConstructProxyConversion(
                                from,
                                foreignProxyChain.From,
                                foreignProxyChain,
                                out var partialChain
                            )
                        )
                        {
                            var bridgeConversion = FindSafeConversion(
                                targetConversionPair.From,
                                to
                            );
                            if (bridgeConversion == null)
                            {
                                continue;
                            }

                            conversion = new ChainedUnitConversion(partialChain, bridgeConversion);
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to find "unit synonyms" (units that have a <see cref="NoneConversion"/>>.
    /// Returns true, if any have been found and outputs a list of matching units (excluding itself) using
    /// <see cref="matchingUnits"/>.
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="matchingUnits"></param>
    /// <returns></returns>
    private bool TryFindUnitWithSameConversion(string unit, out string[] matchingUnits)
    {
        matchingUnits = _conversions
            .Where(i => i.Value is NoneConversion && (i.Key.From == unit || i.Key.To == unit))
            .SelectMany(i => new[] { i.Key.From, i.Key.To })
            .Where(i => i != unit)
            .ToArray();
        return matchingUnits.Any();
    }
}
