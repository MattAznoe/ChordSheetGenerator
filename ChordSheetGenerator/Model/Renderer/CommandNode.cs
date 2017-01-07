using System;

namespace CSGen.Model.Renderer
{
    public class CommandNode : BaseNode
    {
        private CommandType _commandType;
        private PrintRegionType _regionType;

        public CommandNode(CommandType commandType, PrintRegionType regionType = PrintRegionType.None)
        {
            _commandType = commandType;
            _regionType = regionType;
        }

        public CommandType CommandType { get { return _commandType; } }
        public PrintRegionType RegionType { get { return _regionType; } }
    }
}

