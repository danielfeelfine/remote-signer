using System;
namespace RemoteSigner.Models {
    public class LoginToken {
        public string Id { get; set; }
        public DateTime Expiration { get; set; }
        public string Keyfingerprint { get; set; }
        public DateTime LoggedSince { get; set; }
        public bool IsExpired {
            get {
                return DateTime.UtcNow > Expiration;
            }
        }
    }
}
