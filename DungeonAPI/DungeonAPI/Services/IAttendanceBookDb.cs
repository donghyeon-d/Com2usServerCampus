using System;
using DungeonAPI.ModelDB;

namespace DungeonAPI.Services;

public interface IAttendanceBookDb
{
    // 출석정보 가져오기 - Controller에서 response에 isNeededRestart 보내주기
    public Task<Tuple<ErrorCode, AttendanceBook>> LoadAttandanceBookInfoByPlayerId(Int32 playerId);

    // 보상 받기 (우편함으로 보내). 이미 받았는지 확인
    public Task<ErrorCode> ReceiveRewardToMail(Int32 playerId);

    // Player 튜플 추가하기 - 계정 생성하고 기본 데이터 생성할 때 생성하기
    public Task<ErrorCode> CreatePlayerAttendanceBook(Int32 playerId);

    // Player 튜플 삭제하기 - 롤백용
    public Task<ErrorCode> DeletePlayerAttendanceBook(Int32 playerId);
}

