CREATE DATABASE IF NOT EXISTS GameDB;

CREATE TABLE IF NOT EXISTS GameDB.Player
(
    AccountId INT NOT NULL COMMENT '계정DB 번호',
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

CREATE TABLE IF NOT EXISTS GameDB.Item
(
    PlayerId INT NOT NULL COMMENT '유저 고유번호',
    ItemId INT AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 고유번호',
    ItemCode INT NOT NULL COMMENT '아이템 마스터데이터 번호',
    ItemCount INT NOT NULL COMMENT '아이템 개수',
    Attack BIGINT NOT NULL COMMENT '공격력',
    Defence BIGINT NOT NULL COMMENT '방어력',
    Magic BIGINT NOT NULL COMMENT '마법력',
    EnhanceLevel INT NOT NULL COMMENT '강화 레벨',
    EnhanceTryCount TINYINT NOT NULL COMMENT '남은 강화 횟수',
    IsDestructed BOOL NOT NULL COMMENT '파괴 유무',
    IsDeleted BOOL NOT NULL COMMENT '삭제 유무'
) COMMENT '아이템 데이터';

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
    ItemCount INT NOT NULL COMMENT '아이템 개수'
) COMMENT '메일 데이터';

CREATE TABLE IF NOT EXISTS GameDB.AttendanceBook
(
    PlayerId INT PRIMARY KEY COMMENT '유저 고유번호',
    StartDate DATETIME NOT NULL COMMENT '출석 시작일',
    LastReceiveDate DATETIME NOT NULL COMMENT '최종 수령일',
    DayCount INT NOT NULL COMMENT '연속일'
) COMMENT '출석부';

CREATE TABLE IF NOT EXISTS GameDB.InAppPurchase
(
    PlayerId INT NOT NULL COMMENT '유저 고유번호',
    ReceiptId VARCHAR(30) PRIMARY KEY COMMENT '영수증 번호',
    ReceiveDate DATETIME NOT NULL COMMENT '수령일',
    ProductCode INT NOT NULL COMMENT '상품번호'
) COMMENT '인앱결제내역';

CREATE TABLE IF NOT EXISTS GameDB.CompletedDungeon
(
    PlayerId INT NOT NULL COMMENT '유저 고유번호',
    StageCode INT NOT NULL COMMENT '스테이지 고유번호',
    PRIMARY KEY(PlayerId, StageCode)
) COMMENT '완료한 던전';