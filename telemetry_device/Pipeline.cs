using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using telemetry_device.compactCollection;

namespace telemetry_device
{
    class PipeLine
    {
        private BufferBlock<BufferBlockItem> _pullerBlock;
        private TransformBlock<BufferBlockItem, Dictionary<string, (int, bool)>> _decryptBlock;
        public PipeLine()
        {
            _pullerBlock = new BufferBlock<BufferBlockItem>();
            _decryptBlock = new TransformBlock<BufferBlockItem, Dictionary<string, (int, bool)>>(ProccessPackets);
            _pullerBlock.LinkTo(_decryptBlock);
        }

        public Dictionary<string, (int, bool)> ProccessPackets(BufferBlockItem bufferItem)
        {
            try
            {
                Dictionary<string, (int, bool)> decryptedParamDict = bufferItem.IcdDecryptObject.DecryptPacket(bufferItem.PacketData);
                return decryptedParamDict;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void PushToBuffer(BufferBlockItem blockItem)
        {
            this._pullerBlock.Post(blockItem);
        }
    }
}
