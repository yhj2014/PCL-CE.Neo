using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Framing;

public static class ProtocolReader
{
    private const int MaxTypeLength = 128;
    private const int MaxBodyLength = 65536;

    public static async Task<ScaffoldingResponse> ReadResponseAsync(PipeReader reader, CancellationToken ct)
    {
        while (true)
        {
            var result = await reader.ReadAsync(ct).ConfigureAwait(false);
            var buffer = result.Buffer;

            try
            {
                if (_TryParseResponse(in buffer, out var response, out var consumed))
                {
                    reader.AdvanceTo(consumed, buffer.End);
                    return response;
                }

                if (result.IsCompleted)
                {
                    throw new InvalidOperationException("Connection closed while reading response.");
                }
            }
            finally
            {
                reader.AdvanceTo(buffer.Start, buffer.End);
            }
        }
    }

    private static bool _TryParseResponse(
        in ReadOnlySequence<byte> buffer, out ScaffoldingResponse response, out SequencePosition consumed)
    {
        response = default;
        consumed = buffer.Start;

        if (buffer.Length < 5) return false;

        var reader = new SequenceReader<byte>(buffer);

        if (!reader.TryRead(out var status)) return false;
        if (!reader.TryReadBigEndian(out int bodyLength32)) return false;

        var bodyLength = (uint)bodyLength32;
        if (bodyLength > MaxBodyLength)
            throw new InvalidDataException($"Response body length {bodyLength} exceeds maximum of {MaxBodyLength}.");

        if (reader.Remaining < bodyLength) return false;

        var bodyBuffer = reader.Sequence.Slice(reader.Position, bodyLength);
        var body = bodyBuffer.ToArray();

        response = new ScaffoldingResponse(status, body);
        reader.Advance(bodyLength);
        consumed = reader.Position;

        return true;
    }
}