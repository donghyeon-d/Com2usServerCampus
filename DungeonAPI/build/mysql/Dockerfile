FROM debian:oldstable

RUN apt-get -y update && \
	apt-get -y upgrade && \
	apt-get install -y mariadb-server

COPY ./tools/* /
RUN	 chmod 777 ./mysql_account.sql
RUN	 chmod 777 ./mysql_master.sql
RUN	 chmod 777 ./mysql_master_data.sql
RUN	 chmod 777 ./mysql_game.sql
RUN	 chmod 777 ./init.sh
RUN bash ./init.sh

# RUN	mv ./tools/init.sh ./ && chmod +x ./init.sh

# RUN mv ./tools/50-server.cnf /etc/mysql/mariadb.conf.d/

ENTRYPOINT ["mysqld", "--bind-address=0.0.0.0"]
