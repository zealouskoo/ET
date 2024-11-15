namespace ET.Server {

    [ChildOf(typeof(AccountInfoComponent))]
    public class AccountInfo : Entity, IAwake {
        public string Account;

        public string Password;
    } 
}