#!/bin/bash
timedatectl set-timezone Asia/Bishkek
sudo apt update
sudo apt -y install htop
sudo apt -y install gnupg2
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -
echo "deb http://apt.postgresql.org/pub/repos/apt/ `lsb_release -cs`-pgdg main" |sudo tee  /etc/apt/sources.list.d/pgdg.list
sudo apt update
sudo apt -y install postgresql-12 postgresql-client-12
echo "Дальше ручная настройка"

#Дальше ручная настройка
#sudo su - postgres
#psql -c "alter user postgres with password '$Peq=CBSxWdj3=BSt'"
#psql
#CREATE user walkman_admin with encrypted password 'f#*z8V3s8R?NR+JV';
#CREATE user walkman_sso with encrypted password '=5V5%?AcTMqp!@3G';
#Конфиги https://pgtune.leopard.in.ua/#/

#apt-get install postgresql-12-cron
