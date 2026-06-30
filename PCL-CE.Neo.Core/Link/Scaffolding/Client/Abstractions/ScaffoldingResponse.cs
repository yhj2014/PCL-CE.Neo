namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

public record ScaffoldingResponse(byte Status, ReadOnlyMemory<byte> Body);