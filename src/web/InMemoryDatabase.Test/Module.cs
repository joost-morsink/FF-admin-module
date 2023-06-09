global using System;

global using FluentAssertions;
global using Microsoft.VisualStudio.TestTools.UnitTesting;

global using FfAdmin.Common;
global using FfAdmin.InMemoryDatabase;
global using VerifyTests;
using System.Runtime.CompilerServices;


static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() =>
        VerifierSettings.SortPropertiesAlphabetically();
}
