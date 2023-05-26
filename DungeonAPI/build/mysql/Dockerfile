FROM debian:oldstable

RUN apt-get -y update && \
	apt-get -y upgrade && \
	apt-get install -y mariadb-server

COPY ./tools /tools
RUN	 chmod 777 ./tools/init.sh
RUN bash ./tools/init.sh

# RUN	mv ./tools/init.sh ./ && chmod +x ./init.sh

# RUN mv ./tools/50-server.cnf /etc/mysql/mariadb.conf.d/

ENTRYPOINT ["mysqld", "--bind-address=0.0.0.0"]

