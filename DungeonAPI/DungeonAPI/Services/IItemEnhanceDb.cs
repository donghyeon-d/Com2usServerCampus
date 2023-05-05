using System;
using DungeonAPI.ModelDB;
namespace DungeonAPI.Services;

public interface IItemEnhanceDb
{
    Task<ErrorCode> EnhancePlayerItem(Int32 itemId);
}

