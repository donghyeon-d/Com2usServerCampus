FROM debian:oldstable

RUN apt-get -y update && \
	apt-get -y upgrade && \
	apt-get install -y mariadb-server

COPY ./tools/init.sh ./init.sh
RUN	chmod +x ./init.sh

ENTRYPOINT ["sh", "./init.sh"]
