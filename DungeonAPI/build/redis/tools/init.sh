#!/bin/bash

echo "bind 0.0.0.0 ::" >> /etc/redis/redis.conf

exec redis-server /etc/redis/redis.conf --daemonize no --protected-mode no