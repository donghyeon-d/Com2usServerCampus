using System;
namespace DungeonAPI.Services
{
	public interface IMasterDataDb
	{
		public Task<ErrorCode> Load();
	}
}



