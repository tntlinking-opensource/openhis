namespace Newtouch.CIS.Proxy.CMMPlatform.DTO.HLYYRequest
{
    public class EngDataStatusRes
    {
        public ResponseInfo Info { get; set; }
    }
    
    public class ResponseInfo
    {
        public string Rest { get; set; } // 处方状态信息
        public string Message { get; set; } // 消息
        public string Suggest { get; set; } // 建议
        public string Username { get; set; } // 用户名
        public int DispenseFlag { get; set; } // 配药标志
    }
}