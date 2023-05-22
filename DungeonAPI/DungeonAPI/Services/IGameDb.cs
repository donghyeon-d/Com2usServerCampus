using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface IGameDb
{
    // Player Table
    public Task<Tuple<ErrorCode, Int32>> CreatePlayerAsync(Int32 accountId);
    public Task<Tuple<ErrorCode, Player?>> LoadPlayerByPlayerIdAsync(Int32 playerId);
    public Task<Tuple<ErrorCode, Player?>> LoadPlayerByAccountAsync(Int32 accountId);
    public Task<ErrorCode> UpdatePlayerAsync(Player player);
    public Task<ErrorCode> DeletePlayerAsync(Int32 accountId);
    public Task<Tuple<ErrorCode, Int32>> LoadPlayerIdByAccountIdAsync(Int32 accountId);
    public Task<ErrorCode> AddMoney(Int32 playerId, Int32 amount);
    public Task<ErrorCode> AddExp(Int32 playerId, Int32 amount);

    // Item Talbe
    public Task<ErrorCode> CreateDefaltItemsAsync(Int32 playerId);
    public Task<Tuple<ErrorCode, List<Item>?>> LoadPlayerItemListAsync(Int32 playerId);
    public Task<ErrorCode> UpdateItemAsync(Item item);
    public Task<ErrorCode> DeleteItem(Int32 itemId);
    public Task<ErrorCode> DeletePlayerAllItemsAsync(Int32 playerId);
    public Task<Tuple<ErrorCode, Item?>> LoadItemByItemId(Int32 itemId);
    public Task<Tuple<ErrorCode, Int32>> AddItemToPlayerItemList(Int32 playerId, Item item);

    // Mail Table
    public Task<Tuple<ErrorCode, Int32>> SendMail(Mail mail);
    public Task<Tuple<ErrorCode, Mail?>> LoadMail(Int32 mailId);
    public Task<Tuple<ErrorCode, List<MailInfo>?>> ReadMailListAtPage(Int32 playerId, Int32 pageNumber);
    public Task<Tuple<ErrorCode, MailContent?>> ReadMailContent(Int32 playerId, Int32 mailId);
    public Task<ErrorCode> MarkAsDeleteMail(Int32 mailId, Int32 playerId);
    public Task<ErrorCode> MarkAsOpenMail(Int32 MailId, Int32 playerId);
    public Task<ErrorCode> MarkAsReceivedItem(Int32 MailId, Int32 playerId);
    public Task<ErrorCode> MarkAsNotReceivedItem(Int32 MailId, Int32 playerId);
    public Task<ErrorCode> DeleteMail(Int32 mailId);

    // AttandanceBook Table
    public Task<Tuple<ErrorCode, AttendanceBook?>> LoadAttandanceBookInfo(Int32 playerId);
    public Task<ErrorCode> UpdateAttendanceBook(AttendanceBook attendanceBook);
    public Task<ErrorCode> CreatePlayerAttendanceBook(Int32 playerId);
    public Task<ErrorCode> DeletePlayerAttendanceBook(Int32 playerId);

    // InApp Purchase Tables
    public Task<ErrorCode> RegistReceipt(Int32 playerId, String receiptId, Int32 productCode);
    public Task<ErrorCode> DeleteReceipt(String receiptId);

    // CompleteDungeon Table
    public Task<ErrorCode> UpdateCompleteDungeon(Int32 playerId, Int32 stageCode);
    public Task<ErrorCode> AddCompletedDungeon(Int32 playerId, Int32 stageCode);
    public Task<ErrorCode> DeleteWhenFail(Int32 playerId, Int32 stageCode);
    public Task<Tuple<ErrorCode, CompletedDungeon?>> ReadCompleteList(Int32 playerId);
    public Task<ErrorCode> CreatePlayerCompletedDungeon(Int32 playerId);
    public Task<ErrorCode> CheckCanEnterStage(Int32 playerId, Int32 stageCode);

}

