using DungeonAPI.Models;
using System;
namespace DungeonAPI.Services
{
	public interface IMasterDataDb
	{
        //public Task<Tuple<ErrorCode, IEnumerable<MasterData.Item>>> Load();
        public Task<Tuple<ErrorCode, MasterData>> Get();

        
    }
}



