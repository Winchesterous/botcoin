sqlcmd -E -S . -d BotCoin -I -i BuildTables_Test.sql -o BuildTables_Test.log

notepad BuildTables_Test.log
