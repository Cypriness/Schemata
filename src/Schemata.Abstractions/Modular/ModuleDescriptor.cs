using System;
using System.Reflection;

namespace Schemata.Abstractions.Modular;

public class ModuleDescriptor(
    string   name,
    Assembly assembly,
    Type     entry,
    Type     provider,
    string?  display     = null,
    string?  description = null,
    string?  company     = null,
    string?  copyright   = null,
    string?  version     = null)
{
    public string Name { get; private set; } = name;

    public string DisplayName { get; private set; } = display ?? name;

    public string? Description { get; private set; } = description;

    public string? Company { get; private set; } = company;

    public string? Copyright { get; private set; } = copyright ?? $"\u00a9 {DateTime.Now.Year} {company}";

    public string? Version { get; private set; } = version;

    public Assembly Assembly { get; private set; } = assembly;

    public Type EntryType { get; private set; } = entry;

    public Type ProviderType { get; private set; } = provider;
}
