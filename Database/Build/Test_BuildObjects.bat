sqlcmd -E -S . -d TradeData -I -i BuildObjects.sql -o BuildObjects.log

notepad BuildObjects.log
