using System;
using System.Collections.Generic;
using System.IO;
using telemetry_device_main;
using telemetry_device_main.decodeor;
using telemetry_device_main.icds;

namespace telemetry_device.Core.Factory
{
    class DecoderFactory
    {
        private Dictionary<IcdTypes, string> _icdFiles;
        public DecoderFactory()
        {
            _icdFiles = new Dictionary<IcdTypes, string>();
            string FiberBoxDownJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FiberBoxDownIcd.ToString() + Consts.FILE_TYPE);
            string FiberBoxUpJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FiberBoxUpIcd.ToString() + Consts.FILE_TYPE);
            string FlightBoxDownJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FlightBoxDownIcd.ToString() + Consts.FILE_TYPE);
            string FlightBoxUpJson = File.ReadAllText(Consts.REPO_PATH + IcdTypes.FlightBoxUpIcd.ToString() + Consts.FILE_TYPE);
            _icdFiles.Add(IcdTypes.FiberBoxDownIcd, FiberBoxDownJson);
            _icdFiles.Add(IcdTypes.FiberBoxUpIcd, FiberBoxUpJson);
            _icdFiles.Add(IcdTypes.FlightBoxDownIcd, FlightBoxDownJson);
            _icdFiles.Add(IcdTypes.FlightBoxUpIcd, FlightBoxUpJson);
        }
        public IDecodePacket Create(IcdTypes icdType)
        {
            switch(icdType)
            {
                case IcdTypes.FiberBoxDownIcd:
                    return new FiberBoxdecodeor<FiberBoxDownIcd>(_icdFiles[icdType]);
                                   
                case IcdTypes.FiberBoxUpIcd:
                    return new FiberBoxdecodeor<FiberBoxUpIcd>(_icdFiles[icdType]);
                                                       
                case IcdTypes.FlightBoxDownIcd:
                    return new FlightBoxdecodeor<FlightBoxDownIcd>(_icdFiles[icdType]);
                                    
                case IcdTypes.FlightBoxUpIcd:
                    return new FlightBoxdecodeor<FlightBoxUpIcd>(_icdFiles[icdType]);
                default:
                    throw new NotSupportedException($"Decoder not found for type: {icdType}");
            }
        }
    }
}
