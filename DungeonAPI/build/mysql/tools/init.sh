service mysql start

GRANT ALL PRIVILEGES ON *.* TO 'com2us'@'%' WITH GRANT OPTION;

alter user 'com2us'@'%' identified by '123123';