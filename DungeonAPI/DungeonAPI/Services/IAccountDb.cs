using System;
using System.Threading.Tasks;

namespace DungeonAPI.Services
{
	public interface IAccountDb : IDisposable
	{
        public Task<ErrorCode> CreateAccountAsync(String email, String pw);

        public Task<ErrorCode> VerifyAccountAsync(String email, String pw);

        // public Task<ErrorCOde> DeleteAccountAsync(String id, String pw);
    }
}

