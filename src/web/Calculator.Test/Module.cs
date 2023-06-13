global using System;
global using System.Linq;
global using System.Threading.Tasks;
global using FfAdmin.Calculator.Core;
global using FfAdmin.Common;
global using FluentAssertions;
global using VerifyMSTest;
global using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Runtime.CompilerServices;
using VerifyTests;

namespace FfAdmin.Calculator.Test;

static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() =>
        VerifierSettings.SortPropertiesAlphabetically();
}
