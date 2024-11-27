// <copyright file="GlobalSuppressions.cs" company="Paul Layne">
// Copyright (c) Paul Layne. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MyDU.BlueprintValidator.Tests")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "NameSpaces", Scope = "namespaceanddescendants", Target = "~N:MyDU.BlueprintValidator")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "Allow underscores in consts", Scope = "member", Target = "~F:myDU.BlueprintValidator.Processors.VoxelData.Extensions.SerializationExtensions.COMPRESSED_MAGIC")]
