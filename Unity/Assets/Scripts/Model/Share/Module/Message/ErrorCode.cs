namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_Success = 0;

        // 1-11004 是SocketError请看SocketError定义
        //-----------------------------------
        // 100000-109999是Core层的错误

        // 110000以下的错误请看ErrorCore.cs

        // 这里配置逻辑层的错误码
        // 110000 - 200000是抛异常的错误
        // 200001以上不抛异常

        
        public const int ERR_LoginRepeatedly = 200001;
        public const int ERR_LoginInfoEmpty = 200002;
        public const int ERR_LoginPasswordError = 200003;
        public const int ERR_AccountNameFormError = 200004;
        public const int ERR_PasswordFormError = 200005;
        public const int ERR_AccountBanned = 200006;
        public const int ERR_TokenError = 200007;
        public const int ERR_CreateRoleRepeatedly = 200008;
        public const int ERR_RoleNameEmpty = 200009;
        public const int ERR_RoleDuplicate = 200010;
        public const int ERR_DeleteRepeatedly = 200011;
        public const int ERR_RoleNotFound = 200012;
        public const int ERR_GetRealmKeyRepeatedly = 200013;
        public const int ERR_LoginGateRepeatedly = 200014;

        // public const int ERR_LoginInfoEmpty = 200002;

        // public const int ERR_LoginPasswordError = 200003;

        // public const int ERR_LoginSuccessed = 200003;
    }
}