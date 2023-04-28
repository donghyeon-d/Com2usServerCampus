using System;

// 1000 ~ 19999
public enum ErrorCode : UInt16
{
    None = 0,

    // Common 1000 ~
    UnhandleException = 1001,
    RedisFailException = 1002,
    InValidRequestHttpBody = 1003,
    AuthTokenFailWrongAuthToken = 1006,

    // Account 2000 ~
    CreateAccountFailException = 2001,
    CreateAccountFailDuplicatedEmail = 2002,
    LoginFailException = 2003,
    LoginFailUserNotExist = 2004,
    LoginFailPwNotMatch = 2005,
    LoginFailSetAuthToken = 2006,
    AuthTokenMismatch = 2007,
    AuthTokenNotFound = 2008,
    AuthTokenFailWrongKeyword = 2009,
    AuthTokenFailSetNx = 2010,
    AccountIdMismatch = 2011,
    DuplicatedLogin = 2012,
    CreateAccountFailInsert = 2013,
    LoginFailAddRedis = 2014,
    CheckAuthFailNotExist = 2015,
    CheckAuthFailNotMatch = 2016,
    CheckAuthFailException = 2017,

    // Character 3000 ~
    CreateCharacterRollbackFail = 3001,
    CreateCharacterFailNoSlot = 3002,
    CreateCharacterFailException = 3003,
    CharacterNotExist = 3004,
    CountCharactersFail = 3005,
    DeleteCharacterFail = 3006,
    GetCharacterInfoFail = 3007,
    InvalidCharacterInfo = 3008,
    GetCharacterItemsFail = 3009,
    CharacterCountOver = 3010,
    CharacterArmorTypeMisMatch = 3011,
    CharacterHelmetTypeMisMatch = 3012,
    CharacterCloakTypeMisMatch = 3012,
    CharacterDressTypeMisMatch = 3013,
    CharacterPantsTypeMisMatch = 3012,
    CharacterMustacheTypeMisMatch = 3012,
    CharacterArmorCodeMisMatch = 3013,
    CharacterHelmetCodeMisMatch = 3014,
    CharacterCloakCodeMisMatch = 3015,
    CharacterDressCodeMisMatch = 3016,
    CharacterPantsCodeMisMatch = 3017,
    CharacterMustacheCodeMisMatch = 3018,
    CharacterHairCodeMisMatch = 3019,
    CharacterCheckCodeError = 3010,
    CharacterLookTypeError = 3011,

    CharacterStatusChangeFail = 3012,
    CharacterIsExistGame = 3013,
    GetCharacterListFail = 3014,

    //GameDb 4000~ 
    GetGameDbConnectionFail = 4002,

    // MasterDataDB 5000~
    GetMasterDataDBConnectionFail = 5001
}