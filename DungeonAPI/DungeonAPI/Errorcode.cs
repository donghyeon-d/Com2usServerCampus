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


    //GameDb 4000~ 
    GetGameDbConnectionFail = 4002,

    // Mail Table 4100~
    MailCreateFailException = 4100,
    MailLoadFailException = 4101,
    MailDeleteFailNotExist = 4102,
    MailDeleteFailException = 4103,
    MailReceivedFailNotExist = 4104,
    MailReceivedFailException = 4105,
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


    // MasterDataDB 5000~
    MasterDataConnectionFail = 5001,
    MasterDataFailException = 5002,

    // Item 6000~
    DefaultItemCreateFailException = 6000,
    DefaultItemCreateFail = 6001,
    LoadAllItemsFailException = 6002,
    DeletePlayerAllItemsFail = 6003,
    DeletePlayerAllItemsFailException = 6004,

    // Notice 7000~
    NoticeFailException = 7000,
    NoticeAuthFail = 7001,
    NoticeDuplicatedTitile = 7002
}