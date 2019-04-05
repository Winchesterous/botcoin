sqlcmd -E -S . -d BotCoin -I -i BuildTables_Prod.sql -o BuildTables_Prod.log

notepad BuildTables_Prod.log
