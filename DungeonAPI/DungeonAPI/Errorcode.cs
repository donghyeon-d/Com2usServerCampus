using SqlKata.Execution;
using System;

// 1000 ~ 19999
public enum ErrorCode : UInt16
{
    None = 0,

    // Common 1000 ~
    UnhandleException = 1001,
    RedisFailException = 1002,
    InValidRequesBody = 1003,
    EmptyRequestBody = 1004,
    InValidRequestAppVersion = 1005,
    InValidRequestMasterDataVersion = 1006,
    AuthTokenFailWrongAuthToken = 1007,

    // Account 2000 ~
    CreateAccountFailException = 2001,
    CreateAccountFailDuplicatedEmail = 2002,
    LoginFailException = 2003,
    LoginFailPlayerNotExist = 2004,
    LoginFailPwNotMatch = 2005,
    LoginFailSetAuthToken = 2006,
    AuthTokenMismatch = 2007,
    AuthTokenNotFound = 2008,
    AuthTokenFailWrongKeyword = 2009,
    AuthTokenFailException = 2010,
    AccountIdMismatch = 2011,
    DuplicatedLogin = 2012,
    CreateAccountFailInsert = 2013,
    LoginFailAddRedis = 2014,
    CheckAuthFailNotExist = 2015,
    CheckAuthFailNotMatch = 2016,
    CheckAuthFailException = 2017,
    AuthTockenFailException = 2018,
    AuthTockenCreateFailException = 2019,
    AuthTockenDeleteFail = 2020,
    AuthTockenDeleteFailException = 2021,
    DeleteAccountFail = 2022,
    DeleteAccountFailException = 2023,
    LoadAccountEmailNotMatch = 2034,
    LoadAccountFailException = 2025,
    AuthTockenCreateFail = 2026,

    // MemoryDB 2100~

    ChangUserStatusFail = 2100,
    ChangUserStatusFailException = 2101,
    ChangeUserNotFound = 2102,
    ChangUserSetStautsFail = 2103,
    GetFarmingItemListNotExist = 2104,
    GetFarmingItemListFailException = 2105,
    GetKillNPCNotExist = 2106,
    GetKillNPCFailException = 2107,
    SetFarmingItemListFail = 2108,
    SetFarmingItemListFailException = 2109,
    SetKillNPCListFail = 2110,
    SetKillNPCListFailException = 2111,
    DeleteKillNPCListFailNotExist = 2112,
    DeleteKillNPCListFailException = 2113,
    DeleteFarmingItemListNotExist = 2114,
    DeleteFarmingItemListFailException = 2115,
    GetDungeonInfoFailNotExist = 2116,
    GetDungeonInfoFailException = 2117,
    SetDungeonInfoFailNotExist = 2118,
    SetDungeonInfoFailException = 2119,
    DeleteDungeonInfoFailNotExist = 2120,
    DeleteDungeonInfoFailException = 2121,


    // Character 3000 ~
    //CreateCharacterRollbackFail = 3001,
    //CreateCharacterFailNoSlot = 3002,
    //CreateCharacterFailException = 3003,
    //CharacterNotExist = 3004,
    //CountCharactersFail = 3005,
    //DeleteCharacterFail = 3006,
    //GetCharacterInfoFail = 3007,
    //InvalidCharacterInfo = 3008,
    //GetCharacterItemsFail = 3009,
    //CharacterCountOver = 3010,
    //CharacterArmorTypeMisMatch = 3011,
    //CharacterHelmetTypeMisMatch = 3012,
    //CharacterCloakTypeMisMatch = 3012,
    //CharacterDressTypeMisMatch = 3013,
    //CharacterPantsTypeMisMatch = 3012,
    //CharacterMustacheTypeMisMatch = 3012,
    //CharacterArmorCodeMisMatch = 3013,
    //CharacterHelmetCodeMisMatch = 3014,
    //CharacterCloakCodeMisMatch = 3015,
    //CharacterDressCodeMisMatch = 3016,
    //CharacterPantsCodeMisMatch = 3017,
    //CharacterMustacheCodeMisMatch = 3018,
    //CharacterHairCodeMisMatch = 3019,
    //CharacterCheckCodeError = 3010,
    //CharacterLookTypeError = 3011,

    //CharacterStatusChangeFail = 3012,
    //CharacterIsExistGame = 3013,
    //GetCharacterListFail = 3014,

    // Player 3000~
    PlayerNotExist = 3000,
    CreatePlayerFail = 3001,
    CreatePlayerFailException = 3002,
    LoadPlayerFail = 3003,
    LoadPlayerFailException = 3004,
    UpdatePlayerFail = 3005,
    UpdatePlayerFailException = 3006,
    DeletePlayerFail = 3007,
    DeletePlayerFailException = 3008,
    AddMoneyNotFoundPlayer = 3009,
    AddMoneyFailInvalidRange = 3010,
    AddMoneyFailUpdateFail = 3011,
    AddMoneyFailFailException = 3012,
    AddExpNotFoundPlayer = 3013,
    AddExpFailFailException = 3014,
    IncreamentFactorFail = 3015,
    IncreamentFactorFailException = 3016,



    //GameDb 4000~ 
    GetGameDbConnectionFail = 4002,

    // Mail Table 4100~
    MailCreateFailException = 4100,
    MailLoadFailException = 4101,
    MailDeleteFailNotExist = 4102,
    MailDeleteFailException = 4103,
    MailMarkAsOpenFailNotExist = 4104,
    MailMarkAsOpenFailException = 4105,
    MailContentCreateFail = 4106,
    MailContentCreateFailException = 4107,
    MailContentLoadFail = 4108,
    MailContentLoadFailException = 4109,
    MailContentDeleteFail = 4110,
    MailContentDeleteFailException = 4111,
    MailRewardCreateFail = 4112,
    MailRewardCreateFailException = 4113,
    MailRewardLoadFail = 4114,
    MailRewardLoadFailException = 4115,
    MailRewardDeleteFail = 4116,
    MailRewardDeleteFailException = 4117,
    MailMarkAsReceivedFailNotExist = 4118,
    MailDeleteFailNotExistOrCannotDelete = 4119,
    MailRewardNotFound = 4120,
    MailRewardAlreadyReceived = 4121,
    MarkAsReceivedRewardUpdateFail = 4122,
    ReceiveRewardButSomeLoss = 4123,
    NotFoundPlayerMail = 4124,
    PlayerMailCheckException = 4125,
    MailCreateFail = 4126,
    ReadMailWrongPlayer = 4127,
    ReadMailContentWrongPlayer = 4128,
    MailMarkAsReceivedFailException = 4129,
    MarkAsNotReceivedItemFailNotExist = 4130,
    MarkAsNotReceivedItemException = 4131,
    LoadMailFailNotExist = 4132,
    LoadMailFailException = 4133,
    LoadMailWrongPlayer = 4134,


    // AttendanceBook 4200~
    LoadPlayerAttendanceBookNotExist = 4200,
    LoadPlayerAttendanceBookFailException = 4201,
    CreatePlayerAttendanceBookFail = 4202,
    CreatePlayerAttendanceBookFailException = 4203,
    DeletePlayerAttendanceBookFail = 4202,
    DeletePlayerAttendanceBookFailException = 4203,
    AlreadyReceiveAttendanceReward = 4204,
    SendToMailExceptionAtReceiveAttendanceReward = 4205,
    UpdateAttendanceBookTupleFail = 4206,
    UpdateAttendanceBookTupleFailException = 4207,
    ReceiveRewardToMailFailException = 4208,
    InvalidDayCount = 4209,


    // InAppPurchase 4300~
    WrongReceipt = 4300,
    DuplicatedReceipt = 4301,
    CheckDuplicatedReceiptFailException = 4302,
    InsertInAppPurchaseFail = 4304,
    InsertInAppPurchaseFailException = 4305,
    SendToMailExceptionAtProvidePurchasedProduct = 4306,
    DeletePurchaseInfoFail = 4307,
    DeletePurchaseInfoFailException = 4308,
    InvalidInAppProduct = 4309,
    InAppSendMailFail = 4310,

    // MasterDataDB 5000~
    MasterDataConnectionFail = 5001,
    MasterDataFailException = 5002,

    // Item 6000~
    DefaultItemCreateFailException = 6000,
    DefaultItemCreateFail = 6001,
    LoadAllItemsFailException = 6002,
    DeletePlayerAllItemsFail = 6003,
    DeletePlayerAllItemsFailException = 6004,
    LoadItemNotFound = 6005,
    LoadItemFailException = 6006,
    UpdateItemFail = 6007,
    UpdateItemFailException = 6008,
    AddOneItemFail = 6009,
    AddOntItemFailException = 6010,
    AddStackItemFail = 6011,
    AddStackItemFailException = 6012,
    AddStackItemGetFail = 6013,
    NotFoundMasterDataItemAtAddStackItem = 6014,
    DeleteItemFail = 6015,
    DeleteItemFailException = 6016,
    AcquiredItemFailException = 6017,
    InvalidItemCode = 6018,
    PushItemToListFail = 6019,
    PushItemToListException = 6020,

    // Item Enhance 6100~
    UnenhanceableItem = 6100,
    WrongItemOwner = 6101,
    TryMoreThanMaxCount = 6102,
    DestructedItem = 6103,


    // Notice 7000~
    NoticeFailException = 7000,
    NoticeAuthFail = 7001,
    NoticeDuplicatedTitile = 7002,

    // Stage 8000~
    AddCompletedDungeonFail = 8000,
    AddCompletedDungeonFailException = 8001,
    DeleteCompletedDungeonFail = 8002,
    DeleteCompletedDungeonFailException = 8003,
    CompleteListNotExist = 8004,
    ReadCompleteListFailException = 8005,
    ReadCompleteListFail = 8006,
    InvalidStageCode = 8008,
    NotCompleteBeforeStage = 8009,
    NeedToCompleteBeforeStage = 8010,

    // Stage Farming 8100~
    InvalidPlayerStatusNotPlayDungeon = 8100,
    InvalidFarmingItem = 8101,
    FarmingItemNotMatchStageItem = 8102,
    TooMuchItem = 8103,

    // Stage KillNPC 8200 ~
    InvalidStageNPC = 8200,
    TooMuchKillNPC = 8201,


    // Stage Complete 8300 ~
    StageCompleteInvalidPlayerStatus = 8300,
    PlayerDontKillAllNPC = 8301,
    SaveStageRewardItemListFail = 8302,
    SaveStageRewardMoneyFail = 8302,
}


//var notExist = _queryFactory.Query("CompletedDungeon")
//                            .WhereNotExists(q => q.From("CompletedDungeon")
//                                                   .Where("PlayerId", playerId)
//                                                   .Where("Thema", thema)
//                                                   .Where("Stage", stage)
//                                                   .FromRaw("(Values (?, ?, ?)) AS t (PlayerId, Thema, Stage)", playerId, thema, stage)
//                                                   .Select("t.PlayerId", "t.Thema", "t.Stage"));
//var insert = _queryFactory.Query("CompletedDungeon")
//    .AsInsert(new[] { "PlayerId", "Thema", "Stage" }, notExist);



//var result = await _queryFactory.ExecuteAsync(insert);