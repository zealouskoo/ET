namespace ET.Server
{
    [MessageHandler(SceneType.LoginCenter)]
    public class G2L_RemoveLoginReocrdHandler : MessageHandler<Scene, G2L_RemoveLoginReocrd, L2G_AddLoginRecord>
    {
        protected override async ETTask Run(Scene scene, G2L_RemoveLoginReocrd request, L2G_AddLoginRecord response)
        {
            long account = request.AccountName.GetLongHashCode();
            LoginInfoRecordComponent loginInfoRecordComponent = scene.Root().GetComponent<LoginInfoRecordComponent>();
            // 取得请求账号对应的zone信息，看是否和请求的ServerId一致，是则移除请求账号
            int zone = loginInfoRecordComponent.Get(account);
            if (zone.Equals(request.ServerId))
            {
                loginInfoRecordComponent.Remove(account);
            }
            await ETTask.CompletedTask;
        }
    }
}