﻿// <copyright file="GlobalSuppressions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA0001:XML comment analysis is disabled due to project configuration", Justification = "no idea what this is")]

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "we need to document")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "we need to document")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "we need to document")]

[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000:Keywords should be spaced correctly", Justification = "new() should not be replaced by new() ")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "Standard codecleanup removes the this")]

[assembly: SuppressMessage("Interoperability", "CA1416:Check platform compatibility", Justification = "this is a long way to get platform compatibility")]

[assembly: InternalsVisibleToAttribute("SystemTrayMenu.Tests")]
