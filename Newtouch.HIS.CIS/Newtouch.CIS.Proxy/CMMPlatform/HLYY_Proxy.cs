
using Newtouch.CIS.Proxy.CMMPlatform.DTO.HLYYRequest;

using Newtouch.CIS.Proxy.HisApiService;
using Newtouch.Infrastructure;

namespace Newtouch.CIS.Proxy.CMMPlatform
{
    
    /// </summary>
    public class HlyyProxy 
    {
        
        // 声明 client 为类的属性
        public HisApiClient Client { get; private set; }
        
        public HlyyProxy()
        {
            // 在构造函数中初始化 client
            Client = new HisApiClient();
        }

       /**
        * 处方审核状态查询
        */
        public string GetEngDataStatus(string presNo)
        {
            var request = new EngDataStatusReq()
            {
                Info = new RequestInfo
                {
                    PresNo = presNo
                }
            };
            var reqXMl = new getEngDataStatus
            {
                xml = XMLSerializer.XmlSerializeAll(request)
            };

            return Client.getEngDataStatus(reqXMl).@return;
        }
       
        /**
        * 处方审核状态查询
        */
        public EngineRes engine(EngineReq engineReq)
        {
            var engine = new engine
            {
                getxml = XMLSerializer.XmlSerializeAll(engineReq)
            };
            return XmlSerializerExt.XmlDeSerialize<EngineRes>(Client.engine(engine).@return);
        }
        
        
        /**
       * 说明书
       */
        public string GetHisSmsJson(string ypmcId)
        {
            var getHisSmsJson = new getHisSmsJson
            {
                ypmcid = ypmcId
            };
            return Client.getHisSmsJson(getHisSmsJson).@return;
        }
       


    }
}