CREATE DATABASE IF NOT EXISTS MasterDataDB;

USE MasterDataDB;

DROP TABLE IF EXISTS MasterDataDB.Meta;
CREATE TABLE IF NOT EXISTS MasterDataDB.Meta
(
    Version INT NOT NULL PRIMARY KEY COMMENT '버전'
) COMMENT '버전 정보 테이블';

DROP TABLE IF EXISTS MasterDataDB.`BaseItem`;
CREATE TABLE IF NOT EXISTS MasterDataDB.`BaseItem`
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

DROP TABLE IF EXISTS MasterDataDB.ItemAttribute;
CREATE TABLE IF NOT EXISTS MasterDataDB.ItemAttribute
(
    Name VARCHAR(50) NOT NULL UNIQUE COMMENT '특성 이름',
    Code INT AUTO_INCREMENT PRIMARY KEY COMMENT '아이템 번호'
) COMMENT '아이템 속성 정보 테이블';

DROP TABLE IF EXISTS MasterDataDB.AttendanceReward;
CREATE TABLE IF NOT EXISTS MasterDataDB.AttendanceReward
(
    Code INT AUTO_INCREMENT PRIMARY KEY COMMENT '보상 번호',
    Day TINYINT NOT NULL COMMENT '날짜',
    ItemCode INT NOT NULL COMMENT '아이템 번호',
    Count INT NOT NULL COMMENT '아이템 개수'
) COMMENT '출석부 보상 정보 테이블';

DROP TABLE IF EXISTS MasterDataDB.InAppProduct;
CREATE TABLE IF NOT EXISTS MasterDataDB.InAppProduct
(
    Code INT NOT NULL COMMENT '상품 번호',
    ItemCode INT NOT NULL COMMENT '아이템 번호',
    ItemName VARCHAR(50) NOT NULL COMMENT '아이템 이름',
    ItemCount INT NOT NULL COMMENT '아이템 개수'
) COMMENT '인생 삼풍 정보 테이블 묶음상품';

DROP TABLE IF EXISTS MasterDataDB.DungeonStage;
CREATE TABLE IF NOT EXISTS MasterDataDB.DungeonStage
(
    StageCode INT NOT NULL COMMENT '스테이지 번호',
    Thema VARCHAR(30) NOT NULL COMMENT '던전종류',
    Stage INT NOT NULL COMMENT '스테이지 단계'
) COMMENT '스테이지에서 드롭되는 아이템';

DROP TABLE IF EXISTS MasterDataDB.StageItem;
CREATE TABLE IF NOT EXISTS MasterDataDB.StageItem
(
    StageCode INT NOT NULL COMMENT '스테이지 번호',
    ItemCode INT NOT NULL COMMENT '파밍 가능 아이템',
    Count INT NOT NULL COMMENT '아이템 개수'
) COMMENT '스테이지에서 드롭되는 아이템';

DROP TABLE IF EXISTS MasterDataDB.StageAttackNPC;
CREATE TABLE IF NOT EXISTS MasterDataDB.StageAttackNPC
(
    StageCode INT NOT NULL COMMENT '스테이지 번호',
    NPCCode INT NOT NULL COMMENT '공격 NPC',
    Count INT NOT NULL COMMENT '공격 NPC 개수',
    Exp INT NOT NULL COMMENT '1개당 보상 경험치'
) COMMENT '스테이지에서 나오는 npc';
