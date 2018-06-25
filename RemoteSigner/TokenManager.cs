using System;
using System.Collections.Generic;
using System.Linq;
using RemoteSigner.Log;
using RemoteSigner.Models;

namespace RemoteSigner {
    public static class TokenManager {

        static readonly List<LoginToken> currentTokens;
        static readonly Dictionary<string, LoginToken> valueToToken;

        static TokenManager() {
            currentTokens = new List<LoginToken>();
            valueToToken = new Dictionary<string, LoginToken>();
        }

        public static LoginToken GenerateToken(string keyFingerprint) {
            var tkn = new LoginToken {
                Id = Guid.NewGuid().ToString(),
                Expiration = DateTime.UtcNow.AddSeconds(Configuration.DefaultExpirationSeconds),
                Keyfingerprint = keyFingerprint,
                LoggedSince = DateTime.UtcNow
            };

            lock (currentTokens) {
                currentTokens.Add(tkn);
                RefreshCaches();
            }

            return tkn;
        }

        public static bool CheckToken(string tokenValue) {
            var token = valueToToken.ContainsKey(tokenValue) ? valueToToken[tokenValue] : null;
            return token != null && !token.IsExpired;
        }

        public static string GetTokenFingerprint(string tokenValue) {
            var token = valueToToken.ContainsKey(tokenValue) ? valueToToken[tokenValue] : null;
            return token?.Keyfingerprint;
        }

        static void RefreshCaches() {
            valueToToken.Clear();
            currentTokens.ForEach(tkn => {
                valueToToken.Add(tkn.Id, tkn);
            });
        }

        public static void CleanExpiredTokens() {
            Logger.Log("TokenManager", "Cleaning expired tokens");
            lock (currentTokens) {
                var tksToRemove = currentTokens.Where(tkn => tkn.IsExpired).ToList();
                tksToRemove.ForEach(tkn => { currentTokens.Remove(tkn); });
                Logger.Log("TokenManager", $"{tksToRemove.Count} expired tokens removed.");
                RefreshCaches();
            }
        }
    }
}
