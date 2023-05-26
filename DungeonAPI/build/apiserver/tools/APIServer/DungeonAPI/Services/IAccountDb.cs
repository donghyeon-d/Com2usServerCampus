using System;
using System.Threading.Tasks;

namespace DungeonAPI.Services
{
	public interface IAccountDb : IDisposable
	{
        public Task<Tuple<ErrorCode, Int32>> CreateAccountAsync(String email, String pw);

        public Task<Tuple<ErrorCode, Int32>> VerifyAccountAsync(String email, String pw);

        public Task<ErrorCode> DeleteAccountAsync(String email);

        public Task<Tuple<ErrorCode, Int32>> LoadAccountIdByEmail(String email);
    }
}

