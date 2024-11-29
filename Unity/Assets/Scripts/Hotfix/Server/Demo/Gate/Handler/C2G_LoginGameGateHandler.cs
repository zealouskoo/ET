namespace ET.Server
{
    [MessageSessionHandler(SceneType.Gate)]
    public class C2G_LoginGameGateHandler : MessageSessionHandler<C2G_LoginGameGate, G2C_LoginGameGate>
    {
        protected override async ETTask Run(Session session, C2G_LoginGameGate request, G2C_LoginGameGate response)
        {
            Scene root = session.Root();
            // 出现自组件后续会有异步逻辑判断
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_LoginGateRepeatedly;
                session.Disconnect().Coroutine();
                return;
            }

            // GateSessionKeyComponent
            string account = root.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            // 1. 检查用户是否已经存在，不存在就代表登录的 key 是无效的
            if (string.IsNullOrEmpty(account))
            {
                response.Error = ErrorCode.ERR_ConnectGateKeyError;
                Log.Error("Gate key 验证失败！");
                session.Disconnect().Coroutine();
                return;
            }
            
            // 2. 移除key，防止重复被利用
            root.GetComponent<GateSessionKeyComponent>().Remove(request.Key);
            // 3. 移除 SessionAcceptTimeoutComponent，防止 session 5秒后被移除
            session.RemoveComponent<SessionAcceptTimeoutComponent>();
            // 4. 异步判定 session instanceId 是否相同，不相同则 session 出问题（登录无效）
            long instanceId = session.InstanceId;
            CoroutineLockComponent coroutineLockComponent = root.GetComponent<CoroutineLockComponent>();
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.LoginGate, request.AccountName.GetLongHashCode()))
                {
                    // 异步过程中 session 发生了改变，可能已经断开
                    if (!instanceId.Equals(session.InstanceId))
                    {
                        response.Error = ErrorCode.ERR_GateSessionNotSync;
                        return;
                    }
                    
                    G2L_AddLoginRecord g2LAddLoginRecord = G2L_AddLoginRecord.Create();
                    g2LAddLoginRecord.AccountName = request.AccountName;
                    L2G_AddLoginRecord l2GAddLoginRecord = 
                            await root.GetComponent<MessageSender>().Call(StartSceneConfigCategory.Instance.LoginCenterConfig.ActorId, g2LAddLoginRecord)
                                    as L2G_AddLoginRecord;

                    if (l2GAddLoginRecord.Error != ErrorCode.ERR_Success)
                    {
                        response.Error = l2GAddLoginRecord.Error;
                        session.Disconnect().Coroutine();
                        return;
                    }

                    var playerComponent = root.GetComponent<PlayerComponent>();
                    Player player = playerComponent.GetByAccount(request.AccountName);

                    if (player == null)
                    {
                        player = playerComponent.AddChildWithId<Player,string>(request.RoleId, request.AccountName);
                        long id = player.Parent.InstanceId;
                        player.UnitId = request.RoleId;

                        playerComponent.AddChild(player);
                        Log.Debug($"Do AddChildWithId and AddChild are same parent? : {player.Parent.InstanceId.Equals(id)}");
                        PlayerSessionComponent playerSessionComponent = player.AddComponent<PlayerSessionComponent>();
                        playerSessionComponent.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.GateSession);
                        await playerSessionComponent.AddLocation(LocationType.GateSession);

                        player.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
                        await player.AddLocation(LocationType.Player);
                        
                        playerSessionComponent.Session = session;
                        SessionPlayerComponent sessionPlayerComponent = session.AddComponent<SessionPlayerComponent>();
                        sessionPlayerComponent.Player = player;

                        player.State = PlayerState.Gate;
                    }
                    else
                    {
                        
                        player.RemoveComponent<PlayerOfflineTimeoutComponent>();
                        session.AddComponent<SessionPlayerComponent>().Player = player;
                        player.GetComponent<PlayerSessionComponent>().Session = session;
                    }

                    response.PlayerId = player.Id;
                }
            }
            // 4.1  发送处理消息 G2L_AddLoginRecord 通知登录中心服，记录本次登录的服务器Zone
            // 4.2  接收回复消息 L2G_AddLoginRecord，检查是否添加记录成功
            // 5. 获取 PlayerComponent 组件, Player 组件
            // 5.1  获取 Player, 为空就添加Player
            
        }
    }
}