
USE MasterDataDB;

INSERT into Meta (Version)
VALUES (1);

INSERT into BaseItem (Name, Attribute, Sell, Buy, UseLv, Attack, Defence, Magic, EnhanceMaxCount, MaxStack)
VALUES 
('돈', 5, 0, 0, 0, 0, 0, 0, 0, 1000000000),
('작은 칼', 1, 10, 20, 1, 10, 5, 1, 10, 1),
('도금 칼', 1, 100, 200, 5, 29, 12, 10, 10, 1),
('나무 방패', 2, 7, 15, 1, 3, 10, 1, 10, 1),
('보통 모자', 3, 5, 8, 1, 1, 1, 1, 10, 1),
('포션', 4, 3, 6, 1, 0, 0, 0, 0, 100);

INSERT INTO ItemAttribute (Name) 
VALUES ("Weapon"), ("Armor"), ("Clothing"), ("MagicTool"), ("Money");

INSERT INTO AttendanceReward (Day, ItemCode, Count)
VALUES (1, 1, 100),
(2, 1, 100),
(3, 1, 100),
(4, 1, 200),
(5, 1, 200),
(6, 1, 200),
(7, 2, 1),
(8, 1, 100),
(9, 1, 100),
(10, 1, 100),
(11, 6, 5),
(12, 1, 150),
(13, 1, 150),
(14, 1, 150),
(15, 1, 150),
(16, 1, 150),
(17, 1, 150),
(18, 4, 1),
(19, 1, 200),
(20, 1, 200),
(21, 1, 200),
(22, 1, 200),
(23, 1, 200),
(24, 5, 1),
(25, 1, 250),
(26, 1, 250),
(27, 1, 250),
(28, 1, 250),
(29, 1, 250),
(30, 3, 1);

INSERT INTO InAppProduct (Code, ItemCode, ItemName, ItemCount)
VALUES
(1, 1, '돈', 1000),
(1, 2, '작은 칼', 1),
(1, 3, '도금 칼', 1),
(2, 4, '나무 방패', 1),
(2, 5, '보통 모자', 1),
(2, 6, '포션', 10),
(3, 1, '돈', 2000),
(3, 2, '작은 칼', 1),
(3, 3, '나무 방패', 1),
(3, 5, '보통 모자', 1);

INSERT INTO DungeonStage (StageCode, Thema, Stage)
VALUES
(101, 'forest', 1),
(102, 'forest', 2),
(103, 'forest', 3),
(201, 'beach', 1),
(202, 'beach', 2),
(203, 'beach', 3),
(301, 'desert', 1),
(302, 'desert', 2),
(303, 'desert', 3);

INSERT INTO StageItem (StageCode , ItemCode , Count)
VALUES
(101,	1,	100),
(101,	2,	1),
(102,	3,	2),
(201,	4,	1),
(202,	5,	1),
(203,	6,	2),
(301,	1,	100),
(302,	2,	1),
(303,	3,	2);

INSERT INTO StageAttackNPC (StageCode , NPCCode , Count , Exp)
VALUES
(101,	101,	10,	10),
(101,	110,	12,	15),
(102,	111,	10,	20),
(201,	201,	40,	20),
(202,	211,	20,	35),
(203,	221,	1,	50),
(301,	301,	10,	30),
(302, 311,	20,	30),
(303,320,	10,	35);