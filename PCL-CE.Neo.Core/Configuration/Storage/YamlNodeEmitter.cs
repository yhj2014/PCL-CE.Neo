using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class YamlNodeEmitter : IEmitter
{
    public YamlNode? SingleRootNode { get; private set; }

    public void Emit(ParsingEvent @event)
    {
        switch (@event)
        {
            case Scalar scalar:
                SingleRootNode = new YamlScalarNode(scalar.Value);
                break;
            case MappingStart:
                SingleRootNode = new YamlMappingNode();
                break;
            case SequenceStart:
                SingleRootNode = new YamlSequenceNode();
                break;
        }
    }
}