# DB 스키마

## MasterData
> MasterData를 업데이트 할 때는 모든 내용을 다 덮어씀. 그렇기 때문에 database 전체를 지웠다가 다시 생성함
```
DROP DATABASE IF EXISTS MasterDataDB;
CREATE DATABASE IF NOT EXISTS MasterDataDB;
```

### MasterData.Meta
```
DROP TABLE IF EXISTS MasterDataDB.Meta;
CREATE TABLE IF NOT EXISTS MasterDataDB.Meta
(
    Version INT PRIMARY KEY COMMENT '버전'
) COMMENT '버전 정보 테이블';
```

### MasterData.BaseItem
```
DROP TABLE IF EXISTS MasterDataDB.BaseItem;
CREATE TABLE IF NOT EXISTS MasterDataDB.BaseItem
(
    Code INT AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호',
    Name VARCHAR(50) NOT NULL UNIQUE COMMENT '아이템 이름',
    Attribute INT NOT NULL COMMENT '특성',
    Sell INT NOT NULL COMMENT '판매 금액',
    Buy INT NOT NULL COMMENT '구입 금액',
    UseLv INT NOT NULL COMMENT '사용 가능 레벨',
    Attack INT NOT NULL COMMENT '공격력',
    Defence INT NOT NULL COMMENT '방어력',
    Magic INT NOT NULL COMMENT '마법력',
    EnhanceMaxCount TINYINT NOT NULL COMMENT '최대 강화 가능 횟수',
    MaxStack INT NOT NULL DEFAULT 1 COMMENT '겹침 가능 개수'
) COMMENT '아이템 정보 테이블';
```

### MasterDataDB.ItemAttribute
```
DROP TABLE IF EXISTS MasterDataDB.ItemAttribute;
CREATE TABLE IF NOT EXISTS MasterDataDB.ItemAttribute
(
    Name VARCHAR(50) NOT NULL UNIQUE COMMENT '특성 이름',
    Code INT AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호'
) COMMENT '아이템 속성 정보 테이블';
```
### MasterDataDB.AttendanceReward
```
DROP TABLE IF EXISTS MasterDataDB.AttendanceReward;
CREATE TABLE IF NOT EXISTS MasterDataDB.AttendanceReward
(
    Code INT AUTO_INCREMENT PRIMARY KEY COMMENT '보상 번호',
    Day TINYINT NOT NULL COMMENT '날짜',
    ItemCode INT NOT NULL COMMENT '아이템 번호',
    Count INT NOT NULL COMMENT '아이템 개수'
) COMMENT '출석부 보상 정보 테이블';
```

### MasterDataDB.InAppProduct
```
DROP TABLE IF EXISTS MasterDataDB.InAppProduct;
CREATE TABLE IF NOT EXISTS MasterDataDB.InAppProduct
(
    Code INT PRIMARY KEY COMMENT '상품 번호',
    ItemCode INT NOT NULL COMMENT '아이템 번호',
    ItemName VARCHAR(50) NOT NULL COMMENT '아이템 이름',
    ItemCount INT NOT NULL COMMENT '아이템 개수'
) COMMENT '인생 삼풍 정보 테이블 묶음상품';
```

### MasterDataDB.DungeonStage
```
DROP TABLE IF EXISTS MasterDataDB.DungeonStage;
CREATE TABLE IF NOT EXISTS MasterDataDB.DungeonStage
(
    StageCode INT PRIMARY KEY COMMENT '스테이지 번호',
    Thema VARCHAR(30) NOT NULL COMMENT '던전종류',
    Stage INT NOT NULL COMMENT '스테이지 단계'
) COMMENT '스테이지에서 드롭되는 아이템';
``` 

### MasterDataDB.StageItem
```
DROP TABLE IF EXISTS MasterDataDB.StageItem;
CREATE TABLE IF NOT EXISTS MasterDataDB.StageItem
(
    StageCode INT NOT NULL COMMENT '스테이지 번호',
    ItemCode INT NOT NULL COMMENT '파밍 가능 아이템',
    Count INT NOT NULL COMMENT '아이템 개수'
) COMMENT '스테이지에서 드롭되는 아이템';
```

### MasterDataDB.StageAttackNPC
```
DROP TABLE IF EXISTS MasterDataDB.StageAttackNPC;
CREATE TABLE IF NOT EXISTS MasterDataDB.StageAttackNPC
(
    StageCode INT NOT NULL COMMENT '스테이지 번호',
    NPCCode INT NOT NULL COMMENT '공격 NPC',
    Count INT NOT NULL COMMENT '공격 NPC 개수',
    Exp INT NOT NULL COMMENT '1개당 보상 경험치'
) COMMENT '스테이지에서 나오는 npc';
```

## AccountDB
> 계정 관리를 위한 Database
```
CREATE DATABASE IF NOT EXISTS AccountDB;
```

### AccountDB.account
```
CREATE TABLE IF NOT EXISTS AccountDB.Account
(
    AccountID INT AUTO_INCREMENT PRIMARY KEY COMMENT '계정 고유번호',
    Email VARCHAR(50) UNIQUE NOT NULL COMMENT '계정 이름',
    HashedPassword VARCHAR(100) NOT NULL COMMENT '해싱된 비밀번호',
    SaltValue VARCHAR(100) NOT NULL COMMENT '솔트값',
    IsDeleted TINYINT DEFAULT 0 NOT NULL COMMENT '삭제 요청 유무'
) COMMENT '계정 정보';
```

## GameDB
```
CREATE DATABASE IF NOT EXISTS GameDB;
```

### GameDB.Player
```
CREATE TABLE IF NOT EXISTS GameDB.Player
(
    AccountId INT NOT NULL UNIQUE COMMENT '계정DB 번호',
    PlayerId INT AUTO_INCREMENT PRIMARY KEY COMMENT '유저 고유번호',
    Exp INT NOT NULL COMMENT  '경험치',
    Level INT NOT NULL COMMENT  '레벨',
    Hp INT NOT NULL COMMENT '현재 체력',
    Mp INT NOT NULL COMMENT '현재 마력',
    Attack INT NOT NULL COMMENT '공격력',
    Defence INT NOT NULL COMMENT '방어력',
    Magic INT NOT NULL COMMENT '마법력',
    Money INT NOT NULL COMMENT '골드'
) COMMENT '유저 게임 데이터';
```

### GameDB.Item
```
CREATE TABLE IF NOT EXISTS GameDB.Item
(
    PlayerId INT NOT NULL COMMENT '유저 고유번호',
    ItemId INT AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 고유번호',
    ItemCode INT NOT NULL COMMENT '아이템 마스터데이터 번호',
    ItemCount INT NOT NULL COMMENT '아이템 개수',
    Attack INT NOT NULL COMMENT '공격력',
    Defence INT NOT NULL COMMENT '방어력',
    Magic INT NOT NULL COMMENT '마법력',
    EnhanceLevel INT NOT NULL COMMENT '강화 레벨',
    EnhanceTryCount TINYINT NOT NULL COMMENT '남은 강화 횟수',
    IsDestructed BOOL NOT NULL COMMENT '파괴 유무',
    IsDeleted BOOL NOT NULL COMMENT '삭제 유무'
) COMMENT '아이템 데이터';
```

### GameDB.Mail
```
CREATE TABLE IF NOT EXISTS GameDB.Mail
(
    PlayerId INT NOT NULL COMMENT '유저 고유번호',
    MailId INT AUTO_INCREMENT PRIMARY KEY COMMENT '메일 고유번호',
    Title VARCHAR(100) NOT NULL COMMENT '메일 제목',
    Content TEXT NOT NULL COMMENT '메일 컨텐츠 본문',
    PostDate DATETIME NOT NULL COMMENT '메일 받은 날짜',
    ExpiredDate DATETIME NOT NULL COMMENT '메일 만료 날짜',
    IsOpened BOOL NOT NULL COMMENT '열어봤는지 여부',
    IsReceivedItem BOOL NOT NULL COMMENT '보상 받았는지 여부',
    IsDeleted BOOL NOT NULL COMMENT '삭제 여부',
    CanDelete BOOL NOT NULL COMMENT '삭제 가능 여부',
    Sender VARCHAR(50) NOT NULL COMMENT '보낸 사람',
    ItemCode INT NOT NULL COMMENT '아이템 마스터데이터 번호',
    ItemCount INT NOT NULL COMMENT '아이템 개수',
) COMMENT '메일 데이터';
```

### GameDb.AttendanceBook
```
CREATE TABLE IF NOT EXISTS GameDB.AttendanceBook
(
    PlayerId INT PRIMARY KEY COMMENT '유저 고유번호',
    StartDate DATETIME NOT NULL COMMENT '출석 시작일',
    LastReceiveDate DATETIME NOT NULL COMMENT '최종 수령일',
    DayCount INT NOT NULL COMMENT '연속일'
) COMMENT '출석부';
```

### GameDb.InAppPurchase
```
CREATE TABLE IF NOT EXISTS GameDB.InAppPurchase
(
    PlayerId INT NOT NULL COMMENT '유저 고유번호',
    ReceiptId VARCHAR(30) PRIMARY KEY COMMENT '영수증 번호',
    ReceiveDate DATETIME NOT NULL COMMENT '수령일',
    ProductCode INT NOT NULL COMMENT '상품번호'
) COMMENT '인앱결제내역';
```

### GameDb.CompletedDungeon
```
CREATE TABLE IF NOT EXISTS GameDB.CompletedDungeon
(
    PlayerId INT PRIMARY KEY COMMENT '유저 고유번호',
    ForestThema INT NOT NULL DEFAULT 0 COMMENT 'Forest thema 완료한 최고 단계',
    BeachThema INT NOT NULL DEFAULT 0 COMMENT 'Beach thema 완료한 최고 단계',
    DesertThema INT NOT NULL DEFAULT 0 COMMENT 'Desert thema 완료한 최고 단계'
) COMMENT '완료한 던전';
```

### Redis
- Player Info
    - Data Type : String
    - Key : "P" +playerId + "Info"
    - Value : { string AuthToken, int Id, string Status, int CurrentStage }

- Notice
    - Data Type : List
    - Key : 
    - Value : [{string Title, string Content, dateTime Date}, {...} ]

- InDungeon
    - Data Type : String
    - Key : "P" +playerId + "Dungeon";
    - Value : { [{int NPCCode, int Count, int Max }, { int NPCCode, int Count, int Max }, ...], 
                [{ int ItemCode, int Count, int Max }, { int ItemCode, int Count, int Max }, ...] }

LobbyCount : RedisList (방번호 = index + 1, 그 값이 인원수}
"LobbyCount" = [{int count}, {int count}, {int count}, ...] 

"Lobby<number>" = [ {string Message, int Timestemp}, {string Message, int Timestemp}, ... }
