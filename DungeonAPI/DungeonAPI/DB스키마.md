# DB 스키마

## MasterData
> MasterData를 업데이트 할 때는 모든 내용을 다 덮어씀. 그렇기 때문에 database 전체를 지웠다가 다시 생성함
    ``` sql
    DROP DATABASE IF EXISTS MasterDataDB;
    CREATE DATABASE IF NOT EXISTS MasterDataDB;
    ```

### MasterData.Meta
    ``` sql
    DROP TABLE IF EXISTS MasterDataDB.Meta;
    CREATE TABLE IF NOT EXISTS MasterDataDB.Meta
    (
        Version INT NOT NULL PRIMARY KEY COMMENT '버전'
    ) COMMENT '버전 정보 테이블';
    ```

### MasterData.Item
    ``` sql
    DROP TABLE IF EXISTS MasterDataDB.Item;
    CREATE TABLE IF NOT EXISTS MasterDataDB.Item
    (
        Code INT AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호',
        Name VARCHAR(50) NOT NULL UNIQUE COMMENT '아이템 이름',
        Attribute INT NOT NULL COMMENT '특성',
        Sell BIGINT NOT NULL COMMENT '판매 금액',
        Buy BIGINT NOT NULL COMMENT '구입 금액',
        UseLv INT NOT NULL COMMENT '사용 가능 레벨',
        Attack BIGINT NOT NULL COMMENT '공격력',
        Defence BIGINT NOT NULL COMMENT '방어력',
        Magic BIGINT NOT NULL COMMENT '마법력',
        EnhanceMaxCount TINYINT NOT NULL COMMENT '최대 강화 가능 횟수',
        MaxStack INT NOT NULL DEFAULT 1 COMMENT '겹침 가능 개수'
    ) COMMENT '아이템 정보 테이블';
    ```

### MasterDataDB.ItemAttribute
    ``` sql
    USE MasterDataDB;

    DROP TABLE IF EXISTS MasterDataDB.item_attribute;
    CREATE TABLE IF NOT EXISTS MasterDataDB.item_attribute
    (
        Name VARCHAR(50) NOT NULL UNIQUE COMMENT '특성 이름',
        Code INT AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호'
    ) COMMENT '아이템 속성 정보 테이블';
    ```

### MasterDataDB.attendance_reward
    ``` sql
    USE `MasterDataDB`;

    DROP TABLE IF EXISTS MasterDataDB.attendance_reward;
    CREATE TABLE IF NOT EXISTS MasterDataDB.attendance_reward
    (
        Code INT AUTO_INCREMENT PRIMARY KEY COMMENT '보상 번호',
        Day TINYINT NOT NULL COMMENT '날짜',
        ItemCode INT NOT NULL COMMENT '아이템 번호',
        Count INT NOT NULL COMMENT '아이템 개수'
    ) COMMENT '출석부 보상 정보 테이블';
    ```

### MasterDataDB.in_app_product
    **InAppProduct**
    ``` sql
    USE `MasterDataDB`;

    DROP TABLE IF EXISTS MasterDataDB.in_app_product;
    CREATE TABLE IF NOT EXISTS MasterDataDB.in_app_product
    (
        Code INT NOT NULL COMMENT '상품 번호',
        ItemCode INT NOT NULL COMMENT '아이템 번호',
        ItemName VARCHAR(50) NOT NULL COMMENT '아이템 이름',
        ItemCount INT NOT NULL COMMENT '아이템 개수'
    ) COMMENT '인생 삼풍 정보 테이블 묶음상품';
    ```

### MasterDataDB.stage_item
    ``` sql
    USE `MasterDataDB`;

    DROP TABLE IF EXISTS MasterDataDB.stage_item;
    CREATE TABLE IF NOT EXISTS MasterDataDB.stage_item
    (
        Code INT NOT NULL COMMENT '스테이지 단계',
        ItemCode INT NOT NULL COMMENT '파밍 가능 아이템'
    )
    ```

### MasterDataDB.stage_attack_npc
    ``` sql
    USE `MasterDataDB`;

    DROP TABLE IF EXISTS MasterDataDB.stage_attack_npc;
    CREATE TABLE IF NOT EXISTS MasterDataDB.stage_attack_npc
    (
        Code INT NOT NULL COMMENT '스테이지 단계',
        NPCCode INT NOT NULL COMMENT '공격 NPC',
        ItemCount INT NOT NULL COMMENT '공격 NPC 개수',
        Exp INT NOT NULL COMMENT '1개당 보상 경험치'
    )
    ```


## AccountDB
> 계정 관리를 위한 Database
    ``` sql
    CREATE DATABASE IF NOT EXISTS AccountDB;
    ```

### AccountDB.account
    ``` sql
    CREATE TABLE IF NOT EXISTS AccountDB.account
    (
        AccountID INT AUTO_INCREMENT PRIMARY KEY COMMENT '계정 고유번호',
        Email VARCHAR(50) UNIQUE COMMENT '계정 이름',
        HashedPassword VARCHAR(100) NOT NULL COMMENT '해싱된 비밀번호',
        SaltValue VARCHAR(100) NOT NULL COMMENT '솔트값',
        IsDeleted TINYINT DEFAULT 0 NOT NULL COMMENT '삭제 요청 유무'
    )
    ```

## GameDB
### GameDB.Player
CREATE TABLE IF NOT EXISTS GameDB.player
(
    AccountId INT NOT NULL COMMENT '계정DB 번호',
    PlayerId INT AUTO_INCREMENT PRIMARY KEY COMMENT '유저 고유번호',
    Exp INT NOT NULL COMMENT  '경험치',
    Level INT NOT NULL COMMENT  '레벨',
    Hp INT NOT NULL COMMENT '현재 체력',
    Mp INT NOT NULL COMMENT '현재 마력',
    Attack INT NOT NULL COMMENT '공격력',
    Defence INT NOT NULL COMMENT '방어력',
    Magic INT NOT NULL COMMENT '마법력'
) COMMENT '유저 게임 데이터';

### GameDB.item
CREATE TABLE IF NOT EXISTS GameDB.item
(
    PlayerId INT NOT NULL COMMENT '유저 고유번호',
    ItemId INT AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 고유번호',
    ItemMasterDataCode INT NOT NULL COMMENT '아이템 마스터데이터 번호',
    ItemCount INT NOT NULL COMMENT '아이템 개수',
    Attack BIGINT NOT NULL COMMENT '공격력',
    Defence BIGINT NOT NULL COMMENT '방어력',
    Magic BIGINT NOT NULL COMMENT '마법력',
    EnhanceLevel INT NOT NULL COMMENT '강화 레벨',
    RemainingEnhanceCount TINYINT NOT NULL COMMENT '남은 강화 횟수',
    IsDestructed TINYINT COMMENT '파괴 유무'
) COMMENT '아이템 데이터';

### GameDB.Mail
CREATE TABLE IF NOT EXISTS GameDB.Mail
{
    PlayerId INT NOT NULL COMMENT '유저 고유번호',
    MailId INT AUTO_INCREMENT PRIMARY KEY COMMENT '메일 고유번호',
    Title VARCHAR(100) NOT NULL COMMENT '메일 제목',
    PostDate DATETIME NOT NULL COMMENT '메일 받은 날짜',
    ExpiredDate DATETIME NOT NULL COMMENT '메일 만료 날짜',
    IsOpened TINYINT NOT NULL COMMENT '열어봤는지 여부',
    IsReceivedReward TINYINT NOT NULL COMMENT '보상 받았는지 여부',
    IsDeleted TINYINT NOT NULL COMMENT '삭제 여부',
    CanDelete TINYINT NOT NULL COMMENT '삭제 가능 여부',
    Sender VARCHAR(50) NOT NULL COMMENT '보낸 사람'
} COMMENT '메일 데이터';

### GameDB.MailContent
CREATE TABLE IF NOT EXISTS GameDB.MailContent
{
    MailId INT PRIMARY KEY NOT NULL COMMENT '메일 컨텐츠 고유번호',
    Content TEXT NOT NULL COMMENT '메일 컨텐츠 본문'
} COMMENT '메일 컨텐츠 본문'

### GameDB.MailReward
CREATE TABLE IF NOT EXISTS GameDB.MailReward
{
    MailId INT NOT NULL COMMENT '메일 보상 고유번호',
    BaseItemCode는 INT NOT NULL COMMENT '아이템 마스터데이터 번호',
    ItemCount INT NOT NULL COMMENT '아이템 개수'
} COMMENT '메일 보상'

### GameDb.AttendanceBook
CREATE TABLE IF NOT EXISTS GameDB.AttendanceBook
{
    PlayerId INT PRIMARY KEY COMMENT COMMENT '유저 고유번호',
    StartDate DATETIME NOT NULL COMMENT '출석 시작일',
    LastReceiveDate DATETIME NOT NULL COMMENT '최종 수령일',
    ConsecutiveDays INT NOT NULL COMMENT '연속일'
} COMMENT '출석부'

### GameDb.InAppPurchase
CREATE TABLE IF NOT EXISTS GameDB.InAppPurchase
{
    PlayerId INT PRIMARY KEY COMMENT COMMENT '유저 고유번호',
    ReceiptId VARCHAR(30) NOT NULL UNIQUE COMMENT '영수증 번홒',
    ReceiveDate DATETIME NOT NULL COMMENT '수령일',
    ProductCode INT NOT NULL COMMENT '상품번호'
} COMMENT '인앱결제내역'